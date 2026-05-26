"""Converts FBX files to GLB using ufbx for parsing and bpy for export.
NOTE: The FBX files need to have embedded textures.

Uses ufbx instead of Blender's FBX importer. The reason is because 
the bpy importer doesn't convert all FBX files correctly 
(e.g. incorrect normals and face orientation, broken UV transforms, broken textures etc.)

ufbx works quite well and has been tested with many FBX files. It is the FBX importer
of choice for this project.
"""

from BPYExportConverter import BPYExportConverter
import logging
import bpy
import ufbx
import os
import math
import mathutils
from utils import get_gamma_corrected_color
from environment_management import get_environment_var, EnvironmentVariableName


class UFBXDataContainers:
    """Defines containers for ufbx data.

    NOTE: This class is used because at early development stages ufbx data was being freed from memory and causing crashes.
    By defining Python lists these data were prevented from being freed since now there were references to the data.
    Now, the crashes might be fixed since we are disabling the Python garbage collector. However this is not tested and the
    implementation still uses this class.
    """

    def __init__(self):
        self.textures = []
        self.texture_objects = []
        self.materials: list[ufbx.Material] = []
        self.mesh_materials = {}
        self.mesh_instances = {}
        self.mesh_vertices = {}
        self.mesh_faces = {}
        self.meshes: list[ufbx.Mesh] = []
        self.scene: ufbx.Scene = None
        self.nodes: list[ufbx.Node] = []


class FBXConverter(BPYExportConverter):
    """Uses ufbx to import the FBX data to memory, exports to GLB using bpy.
    NOTE: The FBX files need to have embedded textures."""

    BLEND_MAP = {
        ufbx.BlendMode.TRANSLUCENT: 'MIX',
        ufbx.BlendMode.ADDITIVE: 'ADD',
        ufbx.BlendMode.MULTIPLY: 'MULTIPLY',
        ufbx.BlendMode.SCREEN: 'SCREEN',
    }

    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__)
        self.containers = None
        self.temp_files = []

    def import_model(self):
        """Loads the FBX file with ufbx and copies all scene data into UFBXDataContainers.

        ufbx is configured with target_axes and handedness_conversion_axis so that
        the loaded coordinate system already matches Blender's Y-up, right-handed
        convention.
        """

        self.logger.info("Clearing blender scene default cube...")
        bpy.ops.wm.read_factory_settings(use_empty=True)

        self.containers = UFBXDataContainers()

        self.logger.info("Importing FBX model with ufbx.")

        # Maps FBX right-handed Z-up to Blender Y-up.
        target_axes = ufbx.CoordinateAxes(
            ufbx.CoordinateAxis.POSITIVE_X,
            ufbx.CoordinateAxis.POSITIVE_Z,
            ufbx.CoordinateAxis.NEGATIVE_Y
        )

        fbx_path = get_environment_var(EnvironmentVariableName.MODEL_FILE_PATH)

        scene = ufbx.load_file(fbx_path,
                               target_axes=target_axes,
                               handedness_conversion_axis=ufbx.MirrorAxis.Y,
                               skip_skin_vertices=True)

        self.logger.info("Populating data containers...")

        self.containers.scene = scene
        for tex in self.containers.scene.texture_files:
            self.containers.textures.append(tex)
        for node in self.containers.scene.nodes:
            self.containers.nodes.append(node)

        for mesh in self.containers.scene.meshes:
            self.containers.meshes.append(mesh)
            self.containers.mesh_instances[mesh.typed_id] = mesh.instances
            self.containers.mesh_vertices[mesh.typed_id] = mesh.vertices
            self.containers.mesh_faces[mesh.typed_id] = mesh.faces

            for mat in mesh.materials:
                self.containers.materials.append(mat)
                if mesh.typed_id not in self.containers.mesh_materials:
                    self.containers.mesh_materials[mesh.typed_id] = []
                self.containers.mesh_materials[mesh.typed_id].append(mat.typed_id)

        self.logger.success("Scene data initialized.")

    def process_model(self):
        """Creates bpy images, materials and meshes from the loaded ufbx container data."""
        script_dir = os.path.dirname(os.path.abspath(__file__))
        os.chdir(script_dir)

        tex_map = self._create_bpy_images()
        mat_map = self._create_bpy_materials(tex_map)
        self._create_bpy_meshes(mat_map)

        bpy.ops.object.select_all(action='DESELECT')
        for obj in bpy.data.objects:
            if obj.type == 'MESH':
                obj.select_set(True)
                bpy.context.view_layer.objects.active = obj

        bpy.context.view_layer.update()

    def _create_bpy_images(self) -> dict:
        """Writes embedded FBX textures to disk and loads them as bpy images.

        Returns:
            dict mapping ufbx texture index to bpy.types.Image.
        """
        tex_map = {}
        self.logger.info("Extracting model textures...")
        for tex in self.containers.textures:
            if tex.content:
                _, fmt = os.path.splitext(tex.absolute_filename)
                if not fmt:
                    fmt = ".png"

                temp_path = os.path.abspath(f"{tex.index}{fmt}")
                with open(temp_path, "wb") as f:
                    f.write(tex.content)

                img = bpy.data.images.load(temp_path)
                img.colorspace_settings.name = 'sRGB'
                img.pack()
                tex_map[tex.index] = img
                self.temp_files.append(temp_path)
        return tex_map

    def _create_bpy_materials(self, tex_map: dict) -> dict:
        """Creates bpy Principled BSDF materials from ufbx material data.

        Args:
            tex_map: the map returned by _create_bpy_images().

        Returns:
            dict mapping ufbx material typed_id to bpy.types.Material.
        """

        mat_map = {}
        self.logger.info("Creating bpy materials...")
        for fbx_mat in self.containers.materials:
            mat = bpy.data.materials.new(name=fbx_mat.name)
            mat.use_nodes = True
            bsdf = mat.node_tree.nodes.get("Principled BSDF")

            pbr = fbx_mat.pbr
            if pbr.base_color.has_value:
                col = pbr.base_color.value_vec4
                r, g, b, a = col[0], col[1], col[2], col[3]
                bsdf.inputs['Base Color'].default_value = get_gamma_corrected_color(r, g, b, a)

            base_tex = self._setup_texture_chain(mat, tex_map, pbr.base_color)
            if base_tex:
                mat.node_tree.links.new(base_tex, bsdf.inputs['Base Color'])

            opacity = self._get_opacity_value(fbx_mat)
            bsdf.inputs['Alpha'].default_value = opacity
            if opacity < 1.0:
                mat.blend_method = 'BLEND'

            mat_map[fbx_mat.typed_id] = mat
        return mat_map

    def _create_bpy_meshes(self, mat_map: dict) -> None:
        """Creates bpy meshes to match ufbx mesh data. Maps UVs and normals via loop indices of meshes.

        Args:
            mat_map: map returned by _create_bpy_materials().
        """
        scale = float(get_environment_var(EnvironmentVariableName.SCALE))
        fallback_mat = bpy.data.materials.new(name="Fallback_Material")
        fallback_mat.use_nodes = True

        mesh_cache = {}
        self.logger.info("Creating bpy meshes...")

        for node in self.containers.nodes:
            if not node.mesh:
                continue

            fbx_mesh = node.mesh
            blender_mesh = None

            if fbx_mesh not in mesh_cache: #If mesh was not seen. Different nodes could use the same mesh to save space (instancing)
                if len(fbx_mesh.faces) > 0:
                    blender_mesh = bpy.data.meshes.new(fbx_mesh.name)

                    verts = [(v.x * scale, v.y * scale, v.z * scale) for v in self.containers.mesh_vertices[fbx_mesh.typed_id]]
                    faces = [tuple(fbx_mesh.vertex_indices[i] for i in range(face.index_begin, face.index_begin + face.num_indices))
                             for face in self.containers.mesh_faces[fbx_mesh.typed_id]]

                    blender_mesh.from_pydata(verts, [], faces)

                    #Cheking UV mapping validity.
                    for i in range(len(faces)):
                        for j in range(len(faces[i])):
                            if faces[i][j] != blender_mesh.polygons[i].vertices[j]:
                                self.logger.warning("mismatch in vertex indices!")

                        fbx_face = self.containers.mesh_faces[fbx_mesh.typed_id][i]
                        for corner_idx, blender_loop_idx in enumerate(blender_mesh.polygons[i].loop_indices):
                            if corner_idx + fbx_face.index_begin != blender_loop_idx:
                                self.logger.warning("mismatch in loop indices!")

                    '''
                    UV MAPPING

                    Blender stores polygon corner data in a flat array called the
                    loop array. Each polygon holds a slice of that array identified
                    by loop_indices. A "loop" is essentially a polygon corner.

                    ufbx uses the same scheme: each face has a contiguous slice of
                    the loop arrays starting at face.index_begin with length face.num_indices.

                    This makes the UV mapping straight forward, the correspondance is 
                    fbx_face.index_begin + corner_idx = poly.loop_indices[corner_idx]

                    '''

                    if fbx_mesh.vertex_uv.exists:
                        uv_indices = fbx_mesh.vertex_uv.indices
                        uv_values = fbx_mesh.vertex_uv.values
                        uv_layer = blender_mesh.uv_layers.new(name="UVMap")

                        for poly_idx, poly in enumerate(blender_mesh.polygons):
                            fbx_face = self.containers.mesh_faces[fbx_mesh.typed_id][poly_idx]
                            for corner_idx, blender_loop_idx in enumerate(poly.loop_indices):
                                fbx_loop_idx = fbx_face.index_begin + corner_idx
                                if fbx_loop_idx < len(uv_indices):
                                    val_idx = uv_indices[fbx_loop_idx]
                                    if 0 <= val_idx < len(uv_values):
                                        uv = uv_values[val_idx]
                                        uv_layer.uv[blender_loop_idx].vector = (uv.x, uv.y)
                                        continue
                                uv_layer.uv[blender_loop_idx].vector = (0.0, 0.0)

                    custom_normals = [(0.0, 0.0, 1.0)] * len(blender_mesh.loops)

                    if fbx_mesh.vertex_normal.exists:
                        norm_indices = fbx_mesh.vertex_normal.indices
                        norm_values = fbx_mesh.vertex_normal.values

                        for poly_idx, poly in enumerate(blender_mesh.polygons):
                            poly.use_smooth = True
                            fbx_face = self.containers.mesh_faces[fbx_mesh.typed_id][poly_idx]
                            for corner_idx, blender_loop_idx in enumerate(poly.loop_indices):
                                fbx_loop_idx = fbx_face.index_begin + corner_idx
                                if fbx_loop_idx < len(norm_indices):
                                    val_idx = norm_indices[fbx_loop_idx]
                                    if 0 <= val_idx < len(norm_values):
                                        n = norm_values[val_idx]
                                        custom_normals[blender_loop_idx] = (n.x, n.y, n.z)

                        blender_mesh.normals_split_custom_set(custom_normals)
                    else:
                        for poly in blender_mesh.polygons:
                            poly.use_smooth = True

                    blender_mesh.validate()
                    blender_mesh.update()

                    for mat_id in self.containers.mesh_materials[fbx_mesh.typed_id]:
                        bl_mat = mat_map.get(mat_id)
                        if bl_mat:
                            blender_mesh.materials.append(bl_mat)
                        else:
                            self.logger.warning("Unknown material in mesh! Using fallback material...")
                            blender_mesh.materials.append(fallback_mat)

                    if fbx_mesh.face_material:
                        for i, poly in enumerate(blender_mesh.polygons):
                            poly.material_index = fbx_mesh.face_material[i]

                    mesh_cache[fbx_mesh] = blender_mesh
            else:
                blender_mesh = mesh_cache[fbx_mesh]

            '''
            MESH AND NODE TRANSFORMING 
            FBX has a "node_to_world" which means "node's scene transform", and a "geometry_to_node" which means
            "mesh local offset". We apply these sequentially, first node_to_world to node and then geometry_to_node to mesh of node. 
            '''
            
            node_empty = bpy.data.objects.new(node.name, None)
            node_empty.matrix_world = self._to_blender_matrix(node.node_to_world, scale)
            bpy.context.collection.objects.link(node_empty)

            if blender_mesh:
                mesh_obj = bpy.data.objects.new(node.name, blender_mesh)
                mesh_obj.matrix_local = self._to_blender_matrix(node.geometry_to_node, scale)
                mesh_obj.parent = node_empty
                bpy.context.collection.objects.link(mesh_obj)

                if not fbx_mesh.vertex_normal.exists:
                    self.logger.warning("Mesh does not have vertex normals. Applying auto smooth with 30 degree angle.")
                    bpy.context.view_layer.objects.active = mesh_obj
                    mesh_obj.select_set(True)
                    bpy.ops.object.shade_smooth_by_angle(angle=math.radians(30.0))
                    mesh_obj.select_set(False)

    def _setup_texture_chain(self, mat, tex_map: dict, fbx_prop: ufbx.MaterialMap):
        """Applies UV mapping and texture chaining to material. Essentially textures a material properly.

        For a non-layered texture the method only performs UV mapping and single texture assignment to material.
        
        For layered textures, all textures in layers are blended together. For each layer after the first
        a Mix RGBA node is inserted.

        Args:
            mat: material to be textured (texture referenced)
            tex_map: texture map used for the referencing
            fbx_prop: a "prop" from ufbx. if it is not a texture, the method exits early.

        Returns:
            The color output socket, ready to be linked to a BSDF input. Returns None if fbx_prop has no texture.
        """
        if not fbx_prop.texture:
            return None

        texture_layers = fbx_prop.texture.layers if fbx_prop.texture.type == ufbx.TextureType.LAYERED else [fbx_prop]
        last_output = None

        for layer in texture_layers:
            tex_obj = layer.texture if hasattr(layer, 'texture') else layer
            self.containers.texture_objects.append(tex_obj)
            if not tex_obj or not tex_obj.has_file:
                continue

            bl_image = tex_map.get(tex_obj.file_index)
            if not bl_image:
                continue
            
            #UV Mapping
            tex_node = mat.node_tree.nodes.new('ShaderNodeTexImage')
            tex_node.image = bl_image

            uv_node = mat.node_tree.nodes.new('ShaderNodeUVMap')
            uv_node.uv_map = "UVMap"

            #The Mapping node applies the UV transform stored in the FBX texture. 
            mapping_node = mat.node_tree.nodes.new('ShaderNodeMapping')

            if tex_obj.has_uv_transform:
                #Applying UV transform.
                u_scale = tex_obj.uv_transform.scale.x
                v_scale = tex_obj.uv_transform.scale.y
                mapping_node.inputs['Scale'].default_value = (u_scale, v_scale, 1.0)

                u_offset = tex_obj.uv_transform.translation.x
                v_offset = tex_obj.uv_transform.translation.y
                mapping_node.inputs['Location'].default_value = (u_offset, v_offset, 0.0)

                # FBX UV rotation is always a pure Z-axis rotation, extracting the Euler Z component is sufficient.
                rot = tex_obj.uv_transform.rotation
                q = mathutils.Quaternion((rot.w, rot.x, rot.y, rot.z))
                z_angle = q.to_euler().z
                mapping_node.inputs['Rotation'].default_value = (0.0, 0.0, z_angle)

            mat.node_tree.links.new(uv_node.outputs['UV'], mapping_node.inputs['Vector'])
            mat.node_tree.links.new(mapping_node.outputs['Vector'], tex_node.inputs['Vector'])

            if last_output is None:
                last_output = tex_node.outputs['Color']
            else:
                mix_node = mat.node_tree.nodes.new('ShaderNodeMix')
                mix_node.data_type = 'RGBA'
                blend_mode = getattr(layer, 'blend_mode', ufbx.BlendMode.TRANSLUCENT)
                mix_node.blend_type = self.BLEND_MAP.get(blend_mode, 'MIX')
                mix_node.inputs['Factor'].default_value = getattr(layer, 'opacity', 1.0)
                mat.node_tree.links.new(last_output, mix_node.inputs[6])
                mat.node_tree.links.new(tex_node.outputs['Color'], mix_node.inputs[7])
                last_output = mix_node.outputs[2]

        return last_output

    @staticmethod
    def _get_opacity_value(fbx_mat: ufbx.Material) -> float:
        """Searches for an opacity data in the given ufbx material.

        The method can be extended to search for more patterns.
        Since opacity is not high priority for our platform, only a simple search is done.
        """
        for prop in fbx_mat.props.props:
            if prop.name == "Opacity":
                return prop.value_vec4.x
        return 1.0

    @staticmethod
    def _to_blender_matrix(m, scale=1.0) -> mathutils.Matrix:
        """Converts a ufbx matrix to a Blender mathutils.Matrix.

        ufbx names matrix members by column: c0, c1, c2 are the basis columns and
        c3 is the translation column. Each column is a Vec3 with .x, .y, .z fields,
        so m.c0.x is row 0 col 0, m.c0.y is row 1 col 0, etc.

        The translation components (c3.x/y/z) are multiplied by scale to apply model scale.

        Args:
            m: a ufbx matrix with column fields c0–c3.
            scale: unit scale factor of model.
        Returns:
            the converted Blender mathutils.Matrix
        """

        return mathutils.Matrix([
            [m.c0.x, m.c1.x, m.c2.x, m.c3.x * scale],
            [m.c0.y, m.c1.y, m.c2.y, m.c3.y * scale],
            [m.c0.z, m.c1.z, m.c2.z, m.c3.z * scale],
            [0, 0, 0, 1]
        ])

    def convert(self):
        try:
            super().convert()
        finally:
            #Delete temp texture files written to disk during _create_bpy_images.
            for path in self.temp_files:
                os.remove(path)

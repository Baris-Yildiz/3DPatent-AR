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

    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__)
        self.containers = None
        self.temp_files = []

    def import_model(self):
        self.logger.info("Clearing blender scene default cube...")
        bpy.ops.wm.read_factory_settings(use_empty=True)

        self.containers = UFBXDataContainers()

        self.logger.info("Importing FBX model with ufbx.")

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

            base_tex = self.setup_texture_chain(mat, tex_map, pbr.base_color, self.containers)
            if base_tex:
                mat.node_tree.links.new(base_tex, bsdf.inputs['Base Color'])

            opacity = self._get_opacity_value(fbx_mat)
            bsdf.inputs['Alpha'].default_value = opacity
            if opacity < 1.0:
                mat.blend_method = 'BLEND'

            mat_map[fbx_mat.typed_id] = mat
        return mat_map

    def _create_bpy_meshes(self, mat_map: dict) -> None:
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

            if fbx_mesh not in mesh_cache:
                if len(fbx_mesh.faces) > 0:
                    blender_mesh = bpy.data.meshes.new(fbx_mesh.name)

                    verts = [(v.x * scale, v.y * scale, v.z * scale) for v in self.containers.mesh_vertices[fbx_mesh.typed_id]]
                    faces = [tuple(fbx_mesh.vertex_indices[i] for i in range(face.index_begin, face.index_begin + face.num_indices))
                             for face in self.containers.mesh_faces[fbx_mesh.typed_id]]

                    blender_mesh.from_pydata(verts, [], faces)

                    for i in range(len(faces)):
                        for j in range(len(faces[i])):
                            if faces[i][j] != blender_mesh.polygons[i].vertices[j]:
                                self.logger.warning("mismatch in vertex indices!")

                        fbx_face = self.containers.mesh_faces[fbx_mesh.typed_id][i]
                        for corner_idx, blender_loop_idx in enumerate(blender_mesh.polygons[i].loop_indices):
                            if corner_idx + fbx_face.index_begin != blender_loop_idx:
                                self.logger.warning("mismatch in loop indices!")

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

    def setup_texture_chain(self, mat, tex_map, fbx_prop: ufbx.MaterialMap, containers: UFBXDataContainers):
        if not fbx_prop.texture:
            return None

        BLEND_MAP = {
            ufbx.BlendMode.TRANSLUCENT: 'MIX',
            ufbx.BlendMode.ADDITIVE: 'ADD',
            ufbx.BlendMode.MULTIPLY: 'MULTIPLY',
            ufbx.BlendMode.SCREEN: 'SCREEN',
        }

        texture_layers = fbx_prop.texture.layers if fbx_prop.texture.type == ufbx.TextureType.LAYERED else [fbx_prop]
        last_output = None

        for layer in texture_layers:
            tex_obj = layer.texture if hasattr(layer, 'texture') else layer
            containers.texture_objects.append(tex_obj)
            if not tex_obj or not tex_obj.has_file:
                continue

            bl_image = tex_map.get(tex_obj.file_index)
            if not bl_image:
                continue

            tex_node = mat.node_tree.nodes.new('ShaderNodeTexImage')
            tex_node.image = bl_image

            uv_node = mat.node_tree.nodes.new('ShaderNodeUVMap')
            uv_node.uv_map = "UVMap"

            mapping_node = mat.node_tree.nodes.new('ShaderNodeMapping')

            if tex_obj.has_uv_transform:
                u_scale = tex_obj.uv_transform.scale.x
                v_scale = tex_obj.uv_transform.scale.y
                mapping_node.inputs['Scale'].default_value = (u_scale, v_scale, 1.0)

                u_offset = tex_obj.uv_transform.translation.x
                v_offset = tex_obj.uv_transform.translation.y
                mapping_node.inputs['Location'].default_value = (u_offset, v_offset, 0.0)

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
                mix_node.blend_type = BLEND_MAP.get(blend_mode, 'MIX')
                mix_node.inputs['Factor'].default_value = getattr(layer, 'opacity', 1.0)
                mat.node_tree.links.new(last_output, mix_node.inputs[6])
                mat.node_tree.links.new(tex_node.outputs['Color'], mix_node.inputs[7])
                last_output = mix_node.outputs[2]

        return last_output

    @staticmethod
    def _get_opacity_value(fbx_mat: ufbx.Material) -> float:
        for prop in fbx_mat.props.props:
            if prop.name == "Opacity":
                return prop.value_vec4.x
        return 1.0

    @staticmethod
    def _to_blender_matrix(m, scale=1.0) -> mathutils.Matrix:
        return mathutils.Matrix([
            [m.c0.x, m.c1.x, m.c2.x, m.c3.x * scale],
            [m.c0.y, m.c1.y, m.c2.y, m.c3.y * scale],
            [m.c0.z, m.c1.z, m.c2.z, m.c3.z * scale],
            [0, 0, 0, 1]
        ])

    def export_model(self):
        super().export_model()
        for path in self.temp_files:
            os.remove(path)

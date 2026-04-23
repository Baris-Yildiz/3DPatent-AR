import math

import bpy
import ufbx
import os
from mathutils import Matrix


#embedded FBX-GLB exporting

def setup_texture_chain(nodes, links, tex_map, fbx_prop, is_data=False):
    """
    Helper to handle ufbx texture layers and return the final output socket.
    is_data=True sets the image to 'Non-Color' (for Roughness/Normal/Metallic).
    """
    
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
        
        if not tex_obj or not tex_obj.has_file:
            continue
        

        bl_image = tex_map.get(tex_obj.file_index)
        if not bl_image:
            continue

        tex_node = nodes.new('ShaderNodeTexImage')
        tex_node.image = bl_image
        
        if last_output is None:
            last_output = tex_node.outputs['Color']
        else:
            mix_node = nodes.new('ShaderNodeMix')
            mix_node.data_type = 'RGBA'
            blend_mode = getattr(layer, 'blend_mode', ufbx.BlendMode.TRANSLUCENT)
            mix_node.blend_type = BLEND_MAP.get(blend_mode, 'MIX')
            mix_node.inputs['Factor'].default_value = getattr(layer, 'opacity', 1.0)
            
            links.new(last_output, mix_node.inputs[6])
            links.new(tex_node.outputs['Color'], mix_node.inputs[7])
            last_output = mix_node.outputs[2]
            
    return last_output

class UFBXDataContainers:
    textures = []
    materials:ufbx.MaterialList = []
    mesh_materials = {}
    mesh_instances = {}
    mesh_vertices = {}
    mesh_faces = {}
    meshes = []

def initialize_scene_data(fbx_path, containers:UFBXDataContainers):    

    # preload ufbx data to python containers. needed for avoiding memory leaks and crashes. load all needed data.
    bpy.ops.wm.read_factory_settings(use_empty=True)


    target_axes = ufbx.CoordinateAxes(
        ufbx.CoordinateAxis.POSITIVE_X,
        ufbx.CoordinateAxis.POSITIVE_Z,
        ufbx.CoordinateAxis.NEGATIVE_Y
    )
    
    #target_unit_meters = 1.0 metre için, 0.01 santimetre(0.01 m = 1cm) vs. blender, gltf metre bazında bekliyor.

    scene = ufbx.load_file(fbx_path, 
                           target_axes=target_axes, 
                           target_unit_meters=1.0,
                           skip_mesh_parts=True,
                           skip_skin_vertices=True,
                           )


    for tex in scene.texture_files:
        containers.textures.append(tex)
    for mesh in scene.meshes:
        
        containers.meshes.append(mesh)
        containers.mesh_instances[mesh.typed_id] = mesh.instances
        containers.mesh_vertices[mesh.typed_id] = mesh.vertices
        containers.mesh_faces[mesh.typed_id] = mesh.faces


        for mat in mesh.materials:    
            containers.materials.append(mat)
            if mesh.typed_id not in containers.mesh_materials:
                containers.mesh_materials[mesh.typed_id] = []
            containers.mesh_materials[mesh.typed_id].append(mat.typed_id)

def convert_fbx_to_glb(fbx_path, output_path, DRACO_COMPRESS_LEVEL, DRACO_QUANTIZATION_SETTINGS):
    print("Running FBX to GLB conversion with ufbx...")
    
    containers = UFBXDataContainers()
    initialize_scene_data(fbx_path, containers)

    tex_map = {}
    
    script_dir = os.path.dirname(os.path.abspath(__file__))    
    os.chdir(script_dir)
    
    #Creating blender image buffers
    for tex in containers.textures:
        if tex.content:
            
            temp_path = os.path.abspath(f"{tex.index}.png") #Temporary write in this path

            with open(temp_path, "wb") as f:
                f.write(tex.content)

            img = bpy.data.images.load(temp_path)
            img.pack()  
            tex_map[tex.index] = img
            
            os.remove(temp_path)
    
    mat_map = {}
    #Create blender material buffers: Get all materials in ufbx scene and convert to blender materials.
    for fbx_mat in containers.materials:
        mat = bpy.data.materials.new(name=fbx_mat.name)
        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        links = mat.node_tree.links
        bsdf = nodes.get("Principled BSDF")

        # 1. BASE COLOR 
        pbr = fbx_mat.pbr
        if pbr.base_color.has_value:
            col = pbr.base_color.value_vec4
            bsdf.inputs['Base Color'].default_value = (col[0], col[1], col[2], 1.0)
        
        base_tex = setup_texture_chain(nodes, links, tex_map, pbr.base_color, is_data=False)
        if base_tex:
            links.new(base_tex, bsdf.inputs['Base Color'])

        mat_map[fbx_mat.typed_id] = mat
    
    # Create blender meshes and assign appropriate materials.
    for fbx_mesh in containers.meshes:
        if len(fbx_mesh.faces) == 0:
            continue
        blender_mesh = bpy.data.meshes.new(fbx_mesh.name)
        
        verts = [(v.x, v.y, v.z) for v in containers.mesh_vertices[fbx_mesh.typed_id]]

        faces = [tuple(fbx_mesh.vertex_indices[i] for i in range(face.index_begin, face.index_begin + face.num_indices))
                  for face in containers.mesh_faces[fbx_mesh.typed_id]]
        
        blender_mesh.from_pydata(verts, [], faces)
        
        if fbx_mesh.vertex_uv.exists:
            uv_layer = blender_mesh.uv_layers.new(name="UVMap")
            for face in containers.mesh_faces[fbx_mesh.typed_id]:
                for i in range(face.index_begin, face.index_begin + face.num_indices):
                    # ufbx stores UVs in the same loop order as face indices
                    uv = fbx_mesh.vertex_uv.values[fbx_mesh.vertex_uv.indices[i]]
                    uv_layer.data[i].uv = (uv.x, uv.y)

        blender_mesh.validate()
        blender_mesh.update()

        for id in containers.mesh_materials[fbx_mesh.typed_id]:
            
            bl_mat = mat_map.get(id)
            
            if bl_mat:
                blender_mesh.materials.append(bl_mat)
            else:
                # Append None to keep the index order correct even if a material is missing
                blender_mesh.materials.append(None)

        # 2. Assign the faces
        if fbx_mesh.face_material:
            for i, poly in enumerate(blender_mesh.polygons):
                poly.material_index = fbx_mesh.face_material[i]


        for instance in containers.mesh_instances[fbx_mesh.typed_id]:
            obj = bpy.data.objects.new(fbx_mesh.name, blender_mesh)
            
            #Apply transformation to fix rotation + scale.
            m = instance.node_to_world
            
            obj.matrix_world = Matrix([
                [m.c0.x, m.c1.x , m.c2.x , m.c3.x ],
                [m.c0.y , m.c1.y , m.c2.y , m.c3.y ],
                [m.c0.z , m.c1.z , m.c2.z , m.c3.z ],
                [0, 0, 0, 1]
            ])

            
            bpy.context.collection.objects.link(obj)

    # Select all mesh objects
    bpy.ops.object.select_all(action='DESELECT')
    for obj in bpy.data.objects:
        if obj.type == 'MESH':
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj

    # Join them into a single primitive
    
    if len(bpy.context.selected_objects) > 1:
        bpy.ops.object.join()
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(30.0))
        joined_obj = bpy.context.active_object
        joined_obj.data.validate()
        joined_obj.data.update()

    bpy.context.view_layer.update()
    print("Finished constructing GLTF, now exporting...")
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT', export_normals=True,
                              export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3],
                                export_yup=True)

    #Memory cleanup
    for obj in bpy.data.objects:
        bpy.data.objects.remove(obj, do_unlink=True)

    bpy.ops.outliner.orphans_purge(do_local_ids=True, do_recursive=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    print("FBX to GLB conversion via ufbx completed successfully.")

#TODO: bazı materyallerin değerleri yanlış olabilir: örneğin opak bir cam
#TODO: vray corona materyallerini ya alma ya da bir logic yaz
#TODO: her materyal pbr olmayabilir diffuse e fallback ekle.

'''
FILE = "fbx/coupe.fbx"
scene_data(FILE)
scene = build_blender_scene_from_ufbx(FILE)
'''
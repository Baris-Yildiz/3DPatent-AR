import bpy
import ufbx
import os
import time
import gc
from mathutils import Matrix


#embedded FBX-GLB exporting

def setup_texture_chain(nodes, links, tex_map, fbx_prop, is_data=False):
    """
    Helper to handle ufbx texture layers and return the final output socket.
    is_data=True sets the image to 'Non-Color' (crucial for Roughness/Normal/Metallic).
    """
    
    if not fbx_prop.texture:
        return None
    print("tex")
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
        
        print(tex_obj.file_index)
        bl_image = tex_map.get(tex_obj.file_index)
        if not bl_image:
            continue
        
        # CRITICAL: Data maps (Normal/Roughness) must not use sRGB color management
        if is_data:
            bl_image.colorspace_settings.name = 'Non-Color'

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

textures = []
materials:ufbx.MaterialList = []
mesh_materials = {}
mesh_instances = {}
mesh_vertices = {}
mesh_faces = {}
meshes = []

def scene_data(fbx_path):    
    # ufbx bufferları unstable olduğu için onları önce python listlerine çevirmek memory errorlerini ortadan kaldırıyor.
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = ufbx.load_file(fbx_path)

    for tex in scene.texture_files:
        textures.append(tex)
    
    for mesh in scene.meshes:
        meshes.append(mesh)
        mesh_instances[mesh.typed_id] = mesh.instances
        mesh_vertices[mesh.typed_id] = mesh.vertices
        mesh_faces[mesh.typed_id] = mesh.faces


        for mat in mesh.materials:              #Mat objeleri aynı memory adresinde ise (ikisi art arda, nadir) o zaman typed_id ye erişim crash verdiriyor.
            materials.append(mat)
            if mesh.typed_id not in mesh_materials:
                mesh_materials[mesh.typed_id] = []
            mesh_materials[mesh.typed_id].append(mat.typed_id)

def build_blender_scene_from_ufbx(fbx_path):
    scene = ufbx.load_file(fbx_path)
    
    # FBX default is often 1.0 (cm). Blender/GLTF want 0.01 (m).
    # ufbx provides settings.unit_meters to help with this.
    unit_scale = scene.settings.unit_meters 

    # If unit_meters is 1.0, and the file is in cm, 
    # you usually need to multiply by 0.01.
    if unit_scale == 0: # Fallback if not defined
        unit_scale = 0.01

    tex_map = {}
    

    #Creating blender image buffers
    for tex in textures:
        if tex.content:
            
            temp_path = os.path.abspath(f"{tex.index}.jpg") #TODO: change path later
            with open(temp_path, "wb") as f:
                f.write(tex.content)

            img = bpy.data.images.load(temp_path)
            img.pack()  
            tex_map[tex.index] = img
            
            print(tex.index)
            os.remove(temp_path)

    mat_map = {}
    #Create blender material buffers
    for fbx_mat in materials:
        mat = bpy.data.materials.new(name=fbx_mat.name)
        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        links = mat.node_tree.links
        bsdf = nodes.get("Principled BSDF")
        for prop in fbx_mat.props.props:
            # This will print things like 'VRay_Diffuse', 'Corona_Glossiness', etc.
            print(f"Property found: {prop.name}") 

        # 1. BASE COLOR (Your existing logic)
        pbr = fbx_mat.pbr
        if pbr.base_color.has_value:
            col = pbr.base_color.value_vec4
            bsdf.inputs['Base Color'].default_value = (col[0], col[1], col[2], 1.0)
        
        base_tex = setup_texture_chain(nodes, links, tex_map, pbr.base_color, is_data=False)
        if base_tex:
            links.new(base_tex, bsdf.inputs['Base Color'])
        

        # 2. METALLIC
        if pbr.metalness.has_value:
            bsdf.inputs['Metallic'].default_value = pbr.metalness.value_int
        met_tex = setup_texture_chain(nodes, links, tex_map, pbr.metalness, is_data=True)
        if met_tex:
            links.new(met_tex, bsdf.inputs['Metallic'])

        # 3. ROUGHNESS
        if pbr.roughness.has_value:
            bsdf.inputs['Roughness'].default_value = pbr.roughness.value_int
        rough_tex = setup_texture_chain(nodes, links, tex_map, pbr.roughness, is_data=True)
        if rough_tex:
            links.new(rough_tex, bsdf.inputs['Roughness'])

        # 4. NORMAL MAP (Special Handling)
        norm_tex = setup_texture_chain(nodes, links, tex_map, pbr.normal_map, is_data=True)
        if norm_tex:
            normal_map_node = nodes.new('ShaderNodeNormalMap')
            links.new(norm_tex, normal_map_node.inputs['Color'])
            links.new(normal_map_node.outputs['Normal'], bsdf.inputs['Normal'])

        # 5. EMISSION
        if pbr.emission_color.has_value:
            e_col = pbr.emission_color.value_vec4
            bsdf.inputs['Emission Color'].default_value = (e_col[0], e_col[1], e_col[2], 1.0)
        em_tex = setup_texture_chain(nodes, links, tex_map, pbr.emission_color, is_data=False)
        if em_tex:
            links.new(em_tex, bsdf.inputs['Emission Color'])

        mat_map[fbx_mat.typed_id] = mat
    print(mat_map)
        
    for fbx_mesh in meshes:
        if len(fbx_mesh.faces) == 0:
            continue
        blender_mesh = bpy.data.meshes.new(fbx_mesh.name)
        
        verts = [(v.x, v.y, v.z) for v in mesh_vertices[fbx_mesh.typed_id]]

        faces = [tuple(fbx_mesh.vertex_indices[i] for i in range(face.index_begin, face.index_begin + face.num_indices))
                  for face in mesh_faces[fbx_mesh.typed_id]]
        
        blender_mesh.from_pydata(verts, [], faces)
        
        if fbx_mesh.vertex_uv.exists:
            uv_layer = blender_mesh.uv_layers.new(name="UVMap")
            for face in mesh_faces[fbx_mesh.typed_id]:
                for i in range(face.index_begin, face.index_begin + face.num_indices):
                    # ufbx stores UVs in the same loop order as face indices
                    uv = fbx_mesh.vertex_uv.values[fbx_mesh.vertex_uv.indices[i]]
                    uv_layer.data[i].uv = (uv.x, uv.y)

        blender_mesh.validate()
        blender_mesh.update()

        # 1. Fill the slots in the EXACT order the FBX mesh expects
        for id in mesh_materials[fbx_mesh.typed_id]:
            
            bl_mat = mat_map.get(id)
            
            if bl_mat:
                blender_mesh.materials.append(bl_mat)
            else:
                    # Append None to keep the index order correct even if a material is missing
                blender_mesh.materials.append(None)

        # 2. Assign the faces (now the indices will match perfectly)
        if fbx_mesh.face_material:
            for i, poly in enumerate(blender_mesh.polygons):
                    # This index (0, 1, 2...) now corresponds to the slots we just filled
                poly.material_index = fbx_mesh.face_material[i]


        for instance in mesh_instances[fbx_mesh.typed_id]:
            # Create a new Object container for this specific instance
            obj = bpy.data.objects.new(fbx_mesh.name, blender_mesh)
            
            # 3. APPLY THE TRANSFORM (The 'Lumping' Fix)
            # Even though we are looping through meshes, 
            # the 'instance' tells us the correct node_to_world matrix.
            m = instance.node_to_world
            
            # Using the column-based access (most common in ufbx Python)
            obj.matrix_world = Matrix([
                [m.c0.x * unit_scale, m.c1.x * unit_scale, m.c2.x * unit_scale, m.c3.x * unit_scale],
                [m.c0.y * unit_scale, m.c1.y * unit_scale, m.c2.y * unit_scale, m.c3.y * unit_scale],
                [m.c0.z * unit_scale, m.c1.z * unit_scale, m.c2.z * unit_scale, m.c3.z * unit_scale],
                [0, 0, 0, 1]
            ])
            bpy.context.collection.objects.link(obj)

    # Select all mesh objects
    bpy.ops.object.select_all(action='DESELECT')
    for obj in bpy.data.objects:
        if obj.type == 'MESH':
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj

    print(bpy.data.images)
    # Join them into a single primitive
    if len(bpy.context.selected_objects) > 1:
        bpy.ops.object.join()

    bpy.context.view_layer.update()
    bpy.ops.export_scene.gltf(filepath="final_output", export_format='GLB', export_materials='EXPORT', export_normals=True)

    for obj in bpy.data.objects:
        bpy.data.objects.remove(obj, do_unlink=True)

    bpy.ops.outliner.orphans_purge(do_local_ids=True, do_recursive=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    return scene


#TODO: bazı materyallerin değerleri yanlış olabilir: örneğin opak bir cam
#TODO: vray corona materyallerini ya alma ya da bir logic yaz
#TODO: her materyal pbr olmayabilir diffuse e fallback ekle.
FILE = "fbx/Checkpoint.fbx"
scene_data(FILE)
scene = build_blender_scene_from_ufbx(FILE)

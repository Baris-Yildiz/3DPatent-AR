import bpy
import os
import convert_fbx

DRACO_COMPRESS_LEVEL = 6
DRACO_QUANTIZATION_SETTINGS = (16,12,12,12)

#empty scene to prevent loading old stuff.
def empty_blender_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)

def apply_draco_compression(output_path):
    empty_blender_scene()
    bpy.ops.import_scene.gltf(filepath=output_path)
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT', export_normals=True,
                              export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3])
def srgb_to_linear(c):
    if c <= 0.04045:
        return c / 12.92
    else:
        return ((c + 0.055) / 1.055) ** 2.4
    
#converts .obj files to .glb.
def convert_obj(file_path, output_path):
    empty_blender_scene()
    bpy.ops.wm.obj_import(filepath=file_path)

    #Gamma 2.2 fix for colors.
    for mat in bpy.data.materials:
        if not mat.use_nodes:
            continue
            
        nodes = mat.node_tree.nodes
        principled = next((n for n in nodes if n.type == 'BSDF_PRINCIPLED'), None)
        
        if principled:
            base_color_socket = principled.inputs.get("Base Color")
            
            if base_color_socket and not base_color_socket.is_linked:
                
                #Take r g b a values from bsdf socket and perform gamma 2.2 correction.
                r, g, b, a = base_color_socket.default_value
                
                linear_r = srgb_to_linear(r)
                linear_g = srgb_to_linear(g)
                linear_b = srgb_to_linear(b)
                
                base_color_socket.default_value = (linear_r, linear_g, linear_b, a)
    
        
    #TODO: error handling : missing texture durumunda renksiz materyaller ile export ediliyor.
    bpy.ops.export_scene.gltf(  filepath=output_path, export_format='GLB', export_normals=True,
                                export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3]
                              )

#converts .stl files to .glb.
def convert_stl(file_path, output_path):
    empty_blender_scene()
    bpy.ops.wm.stl_import(filepath=file_path)
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT', export_normals=True,
                              export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3])

def convert_non_cad(file_path, file_type):
    try:
        output_path = os.environ.get("OUTPUT_PATH")
        if file_type == ".obj":
            convert_obj(file_path, output_path)
        elif file_type == ".fbx":
            convert_fbx.convert_fbx_to_glb(file_path, output_path, DRACO_COMPRESS_LEVEL, DRACO_QUANTIZATION_SETTINGS)
        elif file_type == ".stl":
            convert_stl(file_path, output_path)
        else:
            print("Error: Unsupported 3D model type!")
    finally:
        empty_blender_scene()

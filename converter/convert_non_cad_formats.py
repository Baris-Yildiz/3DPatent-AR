import bpy
import os
import convert_fbx
import logging

DRACO_COMPRESS_LEVEL = 6
DRACO_QUANTIZATION_SETTINGS = (16,12,12,12)

logger = logging.getLogger(__name__)

#empty scene to prevent loading old stuff.
def empty_blender_scene():
    logger.info("Clearing bpy scene...")
    #Memory cleanup
    for obj in bpy.data.objects:
        bpy.data.objects.remove(obj, do_unlink=True)

    bpy.ops.outliner.orphans_purge(do_local_ids=True, do_recursive=True)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    logger.success("Clearing finished.")

def apply_draco_compression(output_path):
    logger.info("Applying DRACO compression")
    empty_blender_scene()

    logger.info("Importing GLB file to scene...")
    bpy.ops.import_scene.gltf(filepath=output_path)

    logger.info("Exporting with DRACO compression...")
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT', export_normals=True,
                              export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3])
    logger.success("Exporting with DRACO compression finished.")
    
#converts .obj files to .glb.
def convert_obj(file_path, output_path):
    logger.info("Starting OBJ to GLB converter.")
    empty_blender_scene()

    logger.info("Importing OBJ file to scene...")
    bpy.ops.wm.obj_import(filepath=file_path)

    gamma_correction = lambda x: (x / 12.92) if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4

    logger.info("Iterating over model materials...")
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
                
                linear_r = gamma_correction(r)
                linear_g = gamma_correction(g)
                linear_b = gamma_correction(b)
                
                base_color_socket.default_value = (linear_r, linear_g, linear_b, a)
    
    logger.info("Exporting to GLB...")
    #TODO: error handling : missing texture durumunda renksiz materyaller ile export ediliyor.
    bpy.ops.export_scene.gltf(  filepath=output_path, export_format='GLB', export_normals=True,
                                export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3]
                              )
    logger.success("Exporting to GLB finished.")

#converts .stl files to .glb.
def convert_stl(file_path, output_path):
    logger.info("Starting STL to GLB converter.")
    empty_blender_scene()

    logger.info("Importing STL model to scene...")
    bpy.ops.wm.stl_import(filepath=file_path)

    logger.info("Exporting to GLB...")
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT', export_normals=True,
                              export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3])
    logger.success("Export to GLB finished.")

def convert_non_cad(file_path, file_type):
    
    try:
        output_path = os.environ.get("OUTPUT_PATH")
        if file_type == ".obj":
            convert_obj(file_path, output_path)
        elif file_type == ".fbx":
            logger.info("Starting FBX to GLB converter.")

            logger.info("Clearing blender scene default cube...")
            bpy.ops.wm.read_factory_settings(use_empty=True) #Remove blender default cube
            
            convert_fbx.convert_fbx_to_glb(file_path, output_path, DRACO_COMPRESS_LEVEL, DRACO_QUANTIZATION_SETTINGS)
        elif file_type == ".stl":
            convert_stl(file_path, output_path)
        else:
            logger.error("Unsupported 3D model type!")
    finally:
        empty_blender_scene()

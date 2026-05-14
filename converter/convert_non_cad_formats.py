import bpy
import os
import convert_fbx
import logging
import math
from bpy_fetch_warnings import capture_bpy_import_warnings, capture_bpy_export_warnings
from handlers import ImportLogHandler, ExportLogHandler

DRACO_COMPRESS_LEVEL = 6
DRACO_QUANTIZATION_SETTINGS = (16,12,20,12)

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

def generate_custom_normals_for_object_if_needed(blender_obj):
    if blender_obj.type != 'MESH':
        return
    
    blender_mesh = blender_obj.data
    if not blender_mesh.has_custom_normals:
        logger.warning(f"Blender Mesh {blender_obj.name} does not have custom normals. Generating default normals for it.")

        bpy.context.view_layer.objects.active = blender_obj
        blender_obj.select_set(True)

        for poly in blender_mesh.polygons:
            poly.use_smooth = True
                    
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(30.0))
        blender_obj.select_set(False)

def apply_gamma_correction(blender_mat, gamma_correction):
    if not blender_mat.use_nodes:
        return
    
    nodes = blender_mat.node_tree.nodes
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

def resize_model():
    logger.info("Resizing model according to scale...")
    bpy.ops.object.select_all(action='SELECT')

    scale = float(os.environ.get("SCALE"))
    
    bpy.ops.transform.resize(value=(scale, scale, scale))
    
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    bpy.ops.object.select_all(action='DESELECT')


#converts .obj files to .glb.
def convert_obj(file_path, output_path):
    logger.info("Starting OBJ to GLB converter.")
    empty_blender_scene()

    logger.info("Importing OBJ file to scene...")
    with capture_bpy_import_warnings() as log_temp:
        bpy.ops.wm.obj_import(filepath=file_path)
        import_log_handler = ImportLogHandler(log_temp)
        import_log_handler.export_user_logs_to_json()

    logger.info("Iterating over model meshes...")
    for obj in bpy.data.objects:
        generate_custom_normals_for_object_if_needed(obj)
                     
    gamma_correction = lambda x: (x / 12.92) if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4

    logger.info("Iterating over model materials...")
    #Gamma 2.2 fix for colors.
    for mat in bpy.data.materials:
        apply_gamma_correction(mat, gamma_correction)
    
    resize_model()
    
    logger.info("Exporting to GLB...")

    with capture_bpy_export_warnings() as log_file:

        #TODO: error handling : missing texture durumunda renksiz materyaller ile export ediliyor. (örneğin ARABA1 OBJ CM)
        bpy.ops.export_scene.gltf(  filepath=output_path, export_format='GLB', export_normals=True,
                                export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3]
                              )
        export_log_handler = ExportLogHandler(log_file)
        export_log_handler.export_user_logs_to_json()

    logger.success("Exporting to GLB finished.")

#converts .stl files to .glb.
def convert_stl(file_path, output_path):
    logger.info("Starting STL to GLB converter.")
    empty_blender_scene()

    logger.info("Importing STL model to scene...")
    with capture_bpy_import_warnings() as log_temp:
        bpy.ops.wm.stl_import(filepath=file_path)
        import_log_handler = ImportLogHandler()
        import_log_handler.process_logs(log_temp)
        import_log_handler.export_user_logs_to_json()
    
    resize_model()

    logger.info("Exporting to GLB...")
    with capture_bpy_export_warnings() as log_file:

        bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT', export_normals=True,
                              export_draco_mesh_compression_enable=True,
                                export_draco_mesh_compression_level=DRACO_COMPRESS_LEVEL,
                                export_draco_position_quantization=DRACO_QUANTIZATION_SETTINGS[0], 
                                export_draco_normal_quantization=DRACO_QUANTIZATION_SETTINGS[1],
                                export_draco_texcoord_quantization=DRACO_QUANTIZATION_SETTINGS[2],
                                export_draco_generic_quantization=DRACO_QUANTIZATION_SETTINGS[3])
    
        export_log_handler = ExportLogHandler()
        export_log_handler.process_logs(log_file)
        export_log_handler.export_user_logs_to_json()

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

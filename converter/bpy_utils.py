import math
import bpy
import logging
from environment_management import get_environment_var, EnvironmentVariableName
from utils import get_gamma_corrected_color

def generate_custom_normals_for_object_if_needed(blender_obj):
    if blender_obj.type != 'MESH':
        return
    
    blender_mesh = blender_obj.data
    
    if not blender_mesh.has_custom_normals:
        logger = logging.getLogger(__name__)
        logger.warning(f"Blender Mesh {blender_obj.name} does not have custom normals. Generating default normals for it.")

        bpy.context.view_layer.objects.active = blender_obj
        blender_obj.select_set(True)

        for poly in blender_mesh.polygons:
            poly.use_smooth = True
                    
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(30.0))
        blender_obj.select_set(False)


def apply_gamma_correction(blender_mat):

    if not blender_mat.use_nodes:
        return
    
    nodes = blender_mat.node_tree.nodes
    principled = next((n for n in nodes if n.type == 'BSDF_PRINCIPLED'), None)
        
    gamma_correction = lambda x: (x / 12.92) if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4

    if principled:
        base_color_socket = principled.inputs.get("Base Color")
            
        if base_color_socket and not base_color_socket.is_linked:
                
            #Take r g b a values from bsdf socket and perform gamma 2.2 correction.
            r, g, b, a = base_color_socket.default_value
            base_color_socket.default_value = get_gamma_corrected_color(r,g,b,a)

def resize_model():
    bpy.ops.object.select_all(action='SELECT')

    scale = float(get_environment_var(EnvironmentVariableName.SCALE))
    bpy.ops.transform.resize(value=(scale, scale, scale))    
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    bpy.ops.object.select_all(action='DESELECT')
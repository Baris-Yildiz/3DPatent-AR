"""Defines utility functions for bpy model processing."""

import math
import bpy
import logging
from environment_management import get_environment_var, EnvironmentVariableName
from utils import get_gamma_corrected_color

def generate_custom_normals_for_object_if_needed(blender_obj: bpy.types.Object):
    """Generates smooth normals for a Blender object if it has no custom normals.

    Uses bpy.ops.object.shade_smooth_by_angle with a 30-degree threshold.

    Args:
        blender_obj: the Blender object to generate normals for.
    """
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


def apply_gamma_correction(blender_mat: bpy.types.Material):
    """Applies gamma correction to a material's base color, converting from sRGB to linear space.

    bpy expects imported material colors to be in linear space. OBJ file colors are in sRGB
    and bpy does not automatically convert them, so this must be called manually after import.

    Only affects unlinked base color sockets on Principled BSDF nodes.

    Args:
        blender_mat: the Blender material to apply gamma correction to.
    """
    if not blender_mat.use_nodes:
        return

    nodes = blender_mat.node_tree.nodes
    principled = next((n for n in nodes if n.type == 'BSDF_PRINCIPLED'), None)

    if principled:
        base_color_socket = principled.inputs.get("Base Color")

        if base_color_socket and not base_color_socket.is_linked:
            r, g, b, a = base_color_socket.default_value
            base_color_socket.default_value = get_gamma_corrected_color(r, g, b, a)


def resize_model():
    """Resizes all objects in the scene according to the SCALE environment variable.

    Applies the scale transform to vertices (not the object transform).

    Handles object selection internally, no need to pre-select objects before calling.
    """
    bpy.ops.object.select_all(action='SELECT')

    scale = float(get_environment_var(EnvironmentVariableName.SCALE))
    bpy.ops.transform.resize(value=(scale, scale, scale))
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    bpy.ops.object.select_all(action='DESELECT')

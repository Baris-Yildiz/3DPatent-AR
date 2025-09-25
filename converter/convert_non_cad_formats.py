import bpy
import os

#empty scene to prevent loading old stuff.
def empty_blender_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)

#converts .obj files to .glb.
def convert_obj():
    empty_blender_scene()
    bpy.ops.wm.obj_import(filepath=file_path)
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB')

#converts .fbx files to .glb.
def convert_fbx():
    empty_blender_scene()
    bpy.ops.wm.fbx_import(filepath=file_path)
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT')

#converts .stl files to .glb.
def convert_stl():
    empty_blender_scene()
    bpy.ops.wm.stl_import(filepath=file_path)
    bpy.ops.export_scene.gltf(filepath=output_path, export_format='GLB', export_materials='EXPORT')

file_path = os.environ.get("MODEL_PATH")
output_path = f"out.glb"
file_type = os.environ.get("MODEL_TYPE")

if file_type == ".obj":
    convert_obj()
elif file_type == ".fbx":
    convert_fbx()
elif file_type == ".stl":
    convert_stl()
else:
    print("Error: Unsupported 3D model type!")

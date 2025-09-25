#code to run either non-cad or cad depending on file format... use for loop to iterate over dir and determine extension.
import os
import subprocess
import sys

CAD_FILE_LIST = [".iges", ".igs", ".stp", ".step"]
NONCAD_FILE_LIST = [".obj", ".stl", ".fbx"]

FREECAD_PYTHON_PATH = r"C:\Program Files\FreeCAD 1.0\bin\python.exe"
CAD_CONVERTER_SCRIPT_NAME = "convert_cad_formats.py"
NON_CAD_CONVERTER_SCRIPT_NAME = "convert_non_cad_formats.py"

input_file_path = sys.argv[1]

#converts a single file
def convert_file_to_glb(abs_input_file_path):

    ext = (os.path.splitext(abs_input_file_path)[1]).lower()

    os.environ["MODEL_PATH"] = abs_input_file_path 
    os.environ["MODEL_TYPE"] = ext
    
    if (ext in NONCAD_FILE_LIST):
        subprocess.run(r'venv\Scripts\activate && python ' + NON_CAD_CONVERTER_SCRIPT_NAME ,shell=True)
    elif (ext in CAD_FILE_LIST):
        script_path = os.path.abspath(CAD_CONVERTER_SCRIPT_NAME)
        cmd = [FREECAD_PYTHON_PATH, script_path]
        subprocess.run(cmd, capture_output=True, text=True)
    else:
        print("Error: Unsupported 3D model path!")

convert_file_to_glb(input_file_path)



'''
#batch converts supported file formats to .glb.
def convert_files_to_glb(folder_path):
    try:
        for root, dirs, files in os.walk(folder_path):
            print(f"Scanning: {root}")

            for file in files:
                print(f"Scanning file: {file}")
            
                filename, ext = os.path.splitext(file)
                input_path = os.path.join(root, file)
                output_path = os.path.join(root, f"{filename}_out.glb")
            
                if (ext.lower() == ".obj"):
                    print(f"Converting .OBJ to .GLB: {file}")
                    cncf.convert_obj(input_path, output_path)
                elif (ext.lower() == ".fbx"):
                    print(f"Converting .FBX to .GLB: {file}")
                    convert_fbx(input_path, output_path)
                elif (ext.lower() == ".stl"):
                    print(f"Converting .STL to .GLB: {file}")
                    convert_stl(input_path, output_path)
                elif (ext.lower() in CAD_FILE_LIST):
                    ccf.run_converter_macro()

    finally:
        empty_blender_scene()
'''
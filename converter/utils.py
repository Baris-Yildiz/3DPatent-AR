import os
import sys
import logging
sys.path.append("/usr/local/lib")

from convert_cad_formats import convert_cad
from convert_non_cad_formats import convert_non_cad, apply_draco_compression

CAD_FILE_LIST = [".iges", ".igs", ".stp", ".step"]
NONCAD_FILE_LIST = [".obj", ".stl", ".fbx"]

#Converts a single file
def convert_file_to_glb(abs_input_file_path):
    logger = logging.getLogger(__name__)
    ext = (os.path.splitext(abs_input_file_path)[1]).lower()
    logger.info(f"Model file extension: {ext}")
    try:
        if (ext in NONCAD_FILE_LIST):
            logger.info(f"Extension found in NON-CAD FORMATS list: {ext}")
            convert_non_cad(abs_input_file_path, ext)
            logger.success(f"Conversion of NON-CAD format model successful.")

        elif (ext in CAD_FILE_LIST):
            logger.info(f"Extension found in CAD FORMATS list: {ext}")
            os.environ["MODEL_PATH"] = abs_input_file_path 
            logger.info(f"Initiating conversion using FreeCAD.")
            convert_cad()
            apply_draco_compression(os.environ["OUTPUT_PATH"])
            logger.success(f"Conversion of CAD format model successful.")

        else:
            logger.error(f"Unsupported 3D model extension: {ext}")
    except Exception as e:
        logger.fatal(f"An Error Occured: {str(e)}")
        sys.exit(1)
    


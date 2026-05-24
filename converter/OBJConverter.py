from BPYImportExportConverter import BPYImportExportConverter
import logging
from utils import ModelType
import bpy
from bpy_utils import generate_custom_normals_for_object_if_needed, apply_gamma_correction, resize_model
from environment_management import get_environment_var, EnvironmentVariableName

class OBJConverter(BPYImportExportConverter):
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__) 

    def import_model(self):
        import_path = get_environment_var(EnvironmentVariableName.MODEL_FILE_PATH)
        super().import_model(import_path, ModelType.OBJ)
    
    def process_model(self):
        self.logger.info("Iterating over model meshes...")
        for obj in bpy.data.objects:
            generate_custom_normals_for_object_if_needed(obj)

        self.logger.info("Iterating over model materials...")
        for mat in bpy.data.materials:
            apply_gamma_correction(mat)

        self.logger.info("Resizing model according to scale...")
        resize_model()

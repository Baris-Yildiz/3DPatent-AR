from BPYImportExportConverter import BPYImportExportConverter
import logging
from utils import ModelType
from bpy_utils import resize_model
from environment_management import get_environment_var, EnvironmentVariableName

class STLConverter(BPYImportExportConverter):
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__) 

    def import_model(self):
        import_path = get_environment_var(EnvironmentVariableName.MODEL_FILE_PATH)
        super().import_model(import_path, ModelType.STL)
    
    def process_model(self):
        self.logger.info("Resizing model according to scale...")
        resize_model()
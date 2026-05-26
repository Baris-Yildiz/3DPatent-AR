from BPYImportExportConverter import BPYImportExportConverter
import logging
from utils import ModelType
from environment_management import get_environment_var, EnvironmentVariableName

class GLBConverter(BPYImportExportConverter):
    """Adds DRACO compression to an existing GLB/GLTF model.

    Implemented for completeness. The platform needs to handle GLB inputs
    even though no format conversion is required.
    """
    
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__) 

    def import_model(self):
        import_path = get_environment_var(EnvironmentVariableName.MODEL_FILE_PATH)
        super().import_model(import_path, ModelType.GLB)
    
    def process_model(self):
        self.logger.info("Not applying further processing to GLB model.")

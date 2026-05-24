from BPYImportExportConverter import BPYImportExportConverter
import logging
import os
from environment_management import get_environment_var, EnvironmentVariableName
from utils import ModelType

import FreeCAD
import FreeCADGui

class CADConverter(BPYImportExportConverter):
    MACRO_SCRIPT_NAME = "freecad_converter.py"
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__) 

    def import_model(self):
        FreeCADGui.showMainWindow()

        macro_script_path = os.path.abspath(self.MACRO_SCRIPT_NAME)
        macro_name = FreeCADGui.Command.createCustomCommand(macro_script_path)

        FreeCADGui.runCommand(macro_name)

        import_path = get_environment_var(EnvironmentVariableName.OUTPUT_FILE_PATH)
        super().import_model(import_path, ModelType.GLB)

    def process_model(self):
        self.logger.info("No further processing for the CAD format.")

    
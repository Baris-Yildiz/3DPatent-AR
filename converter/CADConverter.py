from BPYImportExportConverter import BPYImportExportConverter
import logging
import os
from environment_management import get_environment_var, EnvironmentVariableName
from utils import ModelType

import sys
sys.path.append("/usr/local/lib") #Path configuration for the "import FreeCAD" and "import FreeCADGui" lines to work
import FreeCAD
import FreeCADGui


class CADConverter(BPYImportExportConverter):
    """Converter that extends the import_model to handle CAD-supported formats: IGS/IGES and STP/STEP.

    Conversion of IGS/IGES and STP/STEP files utilize this class.
    """

    # Macro to be run inside FreeCAD. 
    # FreeCAD executes this to export intermediate GLB files. 
    # These intermediate GLB files get imported back with bpy to apply DRACO compression.
    MACRO_SCRIPT_NAME = "freecad_converter.py"

    
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__) 

    
    def import_model(self):
        """Imports CAD-supported formats using FreeCAD.
    
        Opens FreeCAD and executes the macro inside it to obtain .glb files.
        Then imports the GLBs back with bpy to apply DRACO compression.
        """
        FreeCADGui.showMainWindow()

        macro_script_path = os.path.abspath(self.MACRO_SCRIPT_NAME)
        macro_name = FreeCADGui.Command.createCustomCommand(macro_script_path)

        FreeCADGui.runCommand(macro_name)

        import_path = get_environment_var(EnvironmentVariableName.OUTPUT_FILE_PATH)
        super().import_model(import_path, ModelType.GLB)

    def process_model(self):
        self.logger.info("No further processing for the CAD format.")

    
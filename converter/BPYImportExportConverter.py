from BPYExportConverter import BPYExportConverter
import logging
from bpy_fetch_warnings import capture_bpy_import_warnings
from environment_management import get_environment_var, EnvironmentVariableName
import bpy
from LogHandler import ImportLogHandler 
from utils import ModelType

#Converter that defines the shared bpy import / export stage for OBJ, STL, GLB/GLTF models. 
class BPYImportExportConverter(BPYExportConverter):
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__)

    def import_model(self, import_path, model_type):

        self._empty_blender_scene()

        self.logger.info(f"Importing {model_type} file to Blender scene.")
        
        with capture_bpy_import_warnings() as log_file:
            import_func = self._get_model_import_func(model_type, import_path)
            import_func()
            import_log_handler = ImportLogHandler(log_file)
            import_log_handler.export_user_logs_to_json()
        self.logger.success("Import finished.")

    def _empty_blender_scene(self):
        self.logger.info("Clearing bpy scene...")

        for obj in bpy.data.objects:
            bpy.data.objects.remove(obj, do_unlink=True)

        bpy.ops.outliner.orphans_purge(do_local_ids=True, do_recursive=True)

        bpy.ops.wm.read_factory_settings(use_empty=True)
        self.logger.success("Clearing finished.")

    def _get_model_import_func(self, model_type, file_path):
        if model_type == ModelType.OBJ:
            return lambda: bpy.ops.wm.obj_import(filepath=file_path)
        elif model_type == ModelType.STL:
            return lambda: bpy.ops.wm.stl_import(filepath=file_path)
        elif model_type == ModelType.GLTF or model_type == ModelType.GLB:
            return lambda: bpy.ops.import_scene.gltf(filepath=file_path)
        else:
            error_msg = f"Model type {model_type} not supported for conversion!"
            self.logger.fatal(error_msg)
            raise ValueError(error_msg)
        
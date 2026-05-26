from BPYExportConverter import BPYExportConverter
import logging
from bpy_fetch_log_messages import capture_bpy_import_messages
from environment_management import get_environment_var, EnvironmentVariableName
import bpy
from LogHandler import ImportLogHandler
from utils import ModelType

class BPYImportExportConverter(BPYExportConverter):
    """Converter that defines the import_model method to import the model using bpy importers. Extends BPYExportConverter.

    Converters of OBJ, STL, GLB and GLTF formats extend this class. Since the bpy importer is sufficient in importing
    these file formats, bpy's pipeline is enough.
    """

    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__)

    def import_model(self, import_path: str, model_type: ModelType):
        """Imports a model to the bpy scene. Cleans the scene before importing to remove stuff like Blender's default cube.

        Also captures any import logs that are relevant to a temp file.

        Args:
            import_path: path of the model to be imported.
            model_type: type of model, either one of OBJ, STL, GLB, GLTF is supported.
        """
        self._empty_blender_scene()

        self.logger.info(f"Importing {model_type} file to Blender scene.")

        with capture_bpy_import_messages() as log_file:
            import_func = self._get_model_import_func(model_type, import_path)
            import_func()
            import_log_handler = ImportLogHandler(log_file)
            import_log_handler.export_user_logs_to_json()
        self.logger.success("Import finished.")

    def _empty_blender_scene(self):
        """Empties the bpy scene, readies it for model import."""
        self.logger.info("Clearing bpy scene...")

        for obj in bpy.data.objects:
            bpy.data.objects.remove(obj, do_unlink=True)

        bpy.ops.outliner.orphans_purge(do_local_ids=True, do_recursive=True)

        bpy.ops.wm.read_factory_settings(use_empty=True)
        self.logger.success("Clearing finished.")

    def _get_model_import_func(self, model_type: ModelType, file_path: str):
        """Returns the bpy import function for the given model type.

        Args:
            model_type: Either one of OBJ, STL, GLTF or GLB.
            file_path: path of the imported model.
        """
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

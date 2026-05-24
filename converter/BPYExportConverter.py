from AbstractConverter import AbstractConverter

import bpy
import logging

from bpy_fetch_warnings import capture_bpy_export_warnings
from LogHandler import ExportLogHandler

from environment_management import get_environment_var, EnvironmentVariableName

#Converter that defines the shared bpy export stage with DRACO compression. 
class BPYExportConverter(AbstractConverter):
    DRACO_COMPRESS_LEVEL = 6
    DRACO_QUANTIZATION_SETTINGS = (16,12,20,12)

    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__)

    def export_model(self):
        self.logger.info("Finished constructing model in GLB, now exporting...")
        with capture_bpy_export_warnings() as log_file:
            out_file_path = get_environment_var(EnvironmentVariableName.OUTPUT_FILE_PATH)
            bpy.ops.export_scene.gltf(filepath=out_file_path, export_format='GLB', export_materials='EXPORT', export_normals=True,
                                export_draco_mesh_compression_enable=True,
                                    export_draco_mesh_compression_level=self.DRACO_COMPRESS_LEVEL,
                                    export_draco_position_quantization=self.DRACO_QUANTIZATION_SETTINGS[0], 
                                    export_draco_normal_quantization=self.DRACO_QUANTIZATION_SETTINGS[1],
                                    export_draco_texcoord_quantization=self.DRACO_QUANTIZATION_SETTINGS[2],
                                    export_draco_generic_quantization=self.DRACO_QUANTIZATION_SETTINGS[3],
                                    export_yup=True)
            
            export_log_handler = ExportLogHandler(log_file)
            export_log_handler.export_user_logs_to_json()        
        self.logger.success("Export finished.")

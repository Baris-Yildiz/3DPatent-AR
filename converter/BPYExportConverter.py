from AbstractConverter import AbstractConverter

import bpy
import logging

from bpy_fetch_log_messages import capture_bpy_export_messages
from LogHandler import ExportLogHandler

from environment_management import get_environment_var, EnvironmentVariableName

class BPYExportConverter(AbstractConverter):
    """Converter that defines the export_model method to export the model using bpy and DRACO compression.

    All converter classes extend this subclass since the output model is always a .glb file with DRACO compression.
    This is needed to ensure standardization of uploaded models. A standard .glb format allows for AR-viewing of all model types.
    """

    DRACO_COMPRESS_LEVEL = 6  # lossless compression level (0 = most speed, 6 = most compression; 6 is the max bpy 5.0.0 supports)
    DRACO_QUANTIZATION_SETTINGS = (16, 12, 20, 12)  # position, normal, texcoord, generic quantization in bits (0-30)

    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__)

    def export_model(self):
        """Exports the model as .glb with DRACO compression using bpy.ops.export_scene.gltf.

        Uses DRACO_COMPRESS_LEVEL and DRACO_QUANTIZATION_SETTINGS for optimal compression.

        If some models appear wrong, these values — especially DRACO_QUANTIZATION_SETTINGS — might be the cause,
        since quantization reduces precision of internal model data. For example, if textures appear sampled
        in wrong places, low UV quantization settings may be the cause.

        Also captures any relevant export logs and exports a JSON file containing them.
        """
        self.logger.info("Finished constructing model in GLB, now exporting...")
        with capture_bpy_export_messages() as log_file:
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

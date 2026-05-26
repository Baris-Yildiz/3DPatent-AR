from environment_management import get_environment_var, EnvironmentVariableName
import logging
import os
from utils import ModelType

from AbstractConverter import AbstractConverter
from OBJConverter import OBJConverter
from STLConverter import STLConverter
from GLBConverter import GLBConverter
from FBXConverter import FBXConverter
from CADConverter import CADConverter


def get_converter() -> AbstractConverter:
    """Returns the appropriate converter based on the MODEL_FILE_PATH environment variable.

    Reads the file extension from MODEL_FILE_PATH and returns the appropriate converter.
    
    Raises ValueError for unsupported extensions.
    """
    abs_input_file_path = get_environment_var(EnvironmentVariableName.MODEL_FILE_PATH)

    ext = (os.path.splitext(abs_input_file_path)[1]).lower()
    logger = logging.getLogger(__name__)
    logger.info(f"Model file extension: {ext}")

    if ext == ModelType.OBJ.value:
        return OBJConverter()
    elif ext == ModelType.STL.value:
        return STLConverter()
    elif ext == ModelType.GLB.value or ext == ModelType.GLTF.value:
        return GLBConverter()
    elif ext == ModelType.FBX.value:
        return FBXConverter()
    elif ext == ModelType.STP.value or ext == ModelType.IGS.value or ext == ModelType.STEP.value or ext == ModelType.IGES.value:
        return CADConverter()
    else:
        raise ValueError(f"Unsupported file extension: {ext}")

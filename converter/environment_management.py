"""Manages cross-module state by storing converter arguments in os.environ.
Environment variables are used instead of simple system arguments.

The reason for this: the FreeCAD macro "freecad_converter.py" is pasted in the FreeCAD terminal and executed and doesn't take external arguments.
That is why it has to use environment variables.
"""

import os
from enum import Enum

class EnvironmentVariableName(Enum):
    """Helper enum to be used when accessing os.environ variables."""
    SCALE = "SCALE"
    OUTPUT_FILE_PATH = "OUTPUT_FILE_PATH"
    MODEL_FILE_PATH = "MODEL_FILE_PATH"

class ConverterArgs:
    """Used to initialize values for environment variables.

    Construct the object, then call set() to push
    the values into os.environ.
    """

    def __init__(self, input_file_path: str, output_file_path: str, scale_str: str):
        """
        Args:
            input_file_path: path of model to be converted.
            output_file_path: path which converted/output model is written.
            scale_str: either one of "mm", "cm", "in" or "m". Defines the modeling scale.
        """
        self._scale_dict = {"mm": 0.001, "cm": 0.01, "in": 0.0254, "m": 1.0}

        self.input_file_path = os.path.abspath(input_file_path)
        self.output_file_path = os.path.abspath(output_file_path)
        self.scale = self._scale_from_str(scale_str)

    def _scale_from_str(self, scale_str: str):
        """Converts string scale to float scale factor according to _scale_dict.

        Args:
            scale_str: either one of "mm", "cm", "in" or "m". Defines the modeling scale.
        """
        if scale_str not in self._scale_dict:
            raise ValueError(f"Unsupported scale provided: {scale_str}. Supported scales: {self._scale_dict.keys()}")
        return str(self._scale_dict[scale_str])

    def _set_environment_variables(self):
        """Sets os.environ variables from the parsed converter arguments."""
        os.environ[EnvironmentVariableName.SCALE.value] = self.scale
        os.environ[EnvironmentVariableName.OUTPUT_FILE_PATH.value] = self.output_file_path
        os.environ[EnvironmentVariableName.MODEL_FILE_PATH.value] = self.input_file_path

    def set(self):
        """Pushes parsed values to global os.environ variables. These are accessed by other modules to perform conversion."""
        self._set_environment_variables()

def get_environment_var(env_var: EnvironmentVariableName):
    """Fetches an environment variable by enum key.

    Other modules access environment variables with this method rather than
    os.environ directly.

    Args:
        env_var: either one of SCALE, OUTPUT_FILE_PATH, MODEL_FILE_PATH.
    """
    return os.environ.get(env_var.value)

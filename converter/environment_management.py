import os
from enum import Enum

#Helper enum to be used when accessing os.environ variables.
class EnvironmentVariableName(Enum):
    SCALE = "SCALE"
    OUTPUT_FILE_PATH = "OUTPUT_FILE_PATH"
    MODEL_FILE_PATH = "MODEL_FILE_PATH"

class ConverterArgs:

    def __init__(self, input_file_path, output_file_path, scale_str:str):
        self._scale_dict = {"mm":0.001, "cm":0.01, "in":0.0254, "m":1.0}

        self.input_file_path = os.path.abspath(input_file_path)
        self.output_file_path = os.path.abspath(output_file_path)
        self.scale = self._scale_from_str(scale_str)
    
    #Converts string scale to float scale factor.
    def _scale_from_str(self, scale_str:str):
        if scale_str not in self._scale_dict:
            raise ValueError(f"Unsupported scale provided: {scale_str}. Supported scales: {self._scale_dict.keys()}")
        return str(self._scale_dict[scale_str])
    
    #Sets up os.environ variables.
    def _set_environment_variables(self):
        os.environ[EnvironmentVariableName.SCALE.value] = self.scale
        os.environ[EnvironmentVariableName.OUTPUT_FILE_PATH.value] = self.output_file_path
        os.environ[EnvironmentVariableName.MODEL_FILE_PATH.value] = self.input_file_path

    #Outside access to setting os.environ variables.
    def set(self):
        self._set_environment_variables()

def get_environment_var(env_var:EnvironmentVariableName):
    return os.environ.get(env_var.value)
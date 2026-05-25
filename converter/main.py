import sys
import os
import faulthandler
import logging

from converter_decider import get_converter
from environment_management import ConverterArgs
from ConverterLogger import ConverterLogger

logging.setLoggerClass(ConverterLogger)

logging.getLogger("glTFImporter_errors").propagate = False

logger = logging.getLogger(__name__)

#Starting point of the converter.
if __name__ == "__main__":
    logger.info("Starting converter.")
    
    logger.info("Enabling faulthandler for crash info.")
    faulthandler.enable()

    import gc
    logger.info("Disabling automatic garbage collection.")
    gc.disable()

    input_file_path = sys.argv[1]
    scale = sys.argv[2].lower().lstrip().rstrip()
    output_file_path = "output.glb"
    
    converter_args = ConverterArgs(input_file_path=input_file_path, output_file_path=output_file_path, scale_str=scale)
    converter_args.set()

    logger.info(f"Starting converter for model at input path: {converter_args.input_file_path} and output path: {converter_args.output_file_path}")
    
    converter = get_converter()
    converter.convert()

    logger.info("Exiting converter.")
    os._exit(0)

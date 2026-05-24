#Code to test converter system
'''
import os
import logging
from converter_decider import get_converter

def run_test_cases():
    logger = logging.getLogger(__name__)

    test_file_folder = "test_models"
    test_output_folder = "test_outputs"
    
    logger.info("Running test cases...")

    walk = os.walk(test_file_folder)
    count = len(next(walk)[1])
    curr = 1
    
    os.makedirs(test_output_folder,exist_ok=True)

    try:
        for root, _, files in walk:    
            for file in files:

                input_file_path = sys.argv[1]
                scale = sys.argv[2].lower().lstrip().rstrip()
                output_file_path = "output.glb"
                
                converter_args = ConverterArgs(input_file_path=input_file_path, output_file_path=output_file_path, scale_str=scale)
                converter_args.set()

                converter = get_converter()
                converter.convert()

                name, ext = os.path.splitext(file)
                ext = ext.lower()
                if(ext in NONCAD_FILE_LIST):
                    logger.info(f"Running test case {curr}/{count} ({file}):")
                    abspath = os.path.abspath(os.path.join(root, file))
                    os.environ["OUTPUT_PATH"] = f"{test_output_folder}/{name}_out.glb"
                    os.environ["SCALE"] = "1.0"
                    convert_file_to_glb(abspath)
                curr = curr + 1
        logger.info("All test cases passed!")
    except Exception as e:
        logger.critical(f"Test case {curr}/{count} failed with error message: {e}")

run_test_cases()
'''

import sys
import os
import faulthandler
from utils import convert_file_to_glb
 
faulthandler.enable()
import gc
gc.disable()

input_file_path = os.path.abspath(sys.argv[1])
os.environ["OUTPUT_PATH"] = os.path.abspath("output.glb")
convert_file_to_glb(input_file_path)

print("end")
os._exit(0)
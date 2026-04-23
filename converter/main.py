
import sys
import os
from utils import convert_file_to_glb

input_file_path = os.path.abspath(sys.argv[1])
os.environ["OUTPUT_PATH"] = os.path.abspath("output.glb")
convert_file_to_glb(input_file_path)
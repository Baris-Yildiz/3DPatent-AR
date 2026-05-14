import sys
import os
import faulthandler
import logging
from rich.logging import RichHandler
from rich.console import Console
from rich.theme import Theme
from utils import convert_file_to_glb

LOGFORMAT = '[%(asctime)s] %(name)s: %(message)s'

SUCCESS = 25
logging.addLevelName(SUCCESS, "SUCCESS")

def success(self, message, *args, **kws):
    if self.isEnabledFor(SUCCESS):
        self._log(SUCCESS, message, args, **kws)

logging.Logger.success = success

custom_theme = Theme({
    "logging.level.success": "bold green", 
})

custom_console = Console(theme=custom_theme)

logging.basicConfig(
    level=logging.DEBUG,
    format=LOGFORMAT,
    handlers=[RichHandler(console=custom_console, rich_tracebacks=True)]
)

logger = logging.getLogger(__name__)


#Starting point of the converter.
if __name__ == "__main__":
    logger.info("Starting converter.")
    
    logger.info("Enabling faulthandler for crash info.")
    faulthandler.enable()

    import gc
    logger.info("Disabling automatic garbage collection.")
    gc.disable()

    input_file_path = os.path.abspath(sys.argv[1])

    scale = sys.argv[2].lower().lstrip().rstrip()
    scale_factor = 1.0

    if scale == "mm":
        scale_factor = 0.001
    elif scale == "cm":
        scale_factor = 0.01
    elif scale == "in":
        scale_factor = 0.0254
    elif scale == "m":
        scale_factor = 1.0
    else:
        logger.warning(f"Unsupported scale provided: {scale}. Defaulting to meters.")

    os.environ["SCALE"] = str(scale_factor)

    os.environ["OUTPUT_PATH"] = os.path.abspath("output.glb")
    output_path = os.environ["OUTPUT_PATH"]

    logger.info(f"Starting converter for model at input path: {input_file_path} and output path: {output_path}")
    convert_file_to_glb(input_file_path)

    logger.info("Exiting converter.")
    os._exit(0)
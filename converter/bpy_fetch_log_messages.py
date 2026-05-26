"""
This module contains methods to catch logged messages during
model importing and exporting.

Messages are logged to the console as a result of calling 
bpy methods such as bpy.ops.wm.obj_import (for importing) and 
bpy.ops.export_scene.gltf (for exporting). Methods in this module
write the relevant logs to temp files. 
"""

import os
import sys
import tempfile
import logging
from contextlib import contextmanager
from typing import Generator, IO

_STDOUT_FD = 1
_STDERR_FD = 2

@contextmanager
def capture_bpy_import_messages() -> Generator[IO[str], None, None]:
    """Captures model import logs. Designed to be used with 
    bpy.ops.wm.obj_import, bpy.ops.wm.stl_import and 
    bpy.ops.import_scene.gltf

    Above bpy import methods log messages directly to fd 1 and 2. This 
    context manager redirects fd 1 and 2 to a temp file so that they are 
    reachable for further use. 

    Yields:
        IO[str]: The seekable temp log file containing all import messages.
    """
    sys.stdout.flush()
    sys.stderr.flush()

    old_stdout = sys.stdout
    old_stderr = sys.stderr
    old_stdout_fd = os.dup(_STDOUT_FD)
    old_stderr_fd = os.dup(_STDERR_FD)

    with tempfile.TemporaryFile(mode='w+t') as log_temp:
        os.dup2(log_temp.fileno(), _STDOUT_FD)
        os.dup2(log_temp.fileno(), _STDERR_FD)
        sys.stdout = log_temp
        sys.stderr = log_temp

        try:
            yield log_temp
        finally:
            sys.stdout = old_stdout
            sys.stderr = old_stderr
            os.dup2(old_stdout_fd, _STDOUT_FD)
            os.dup2(old_stderr_fd, _STDERR_FD)
            os.close(old_stdout_fd)
            os.close(old_stderr_fd)

@contextmanager
def capture_bpy_export_messages() -> Generator[IO[str], None, None]:
    """Captures model export logs. Designed to be used with 
    bpy.ops.export_scene.gltf

    bpy.ops.export_scene.gltf method logs messages using the logging module.
    The logger used is named "glTFImporter". This method makes the logs also
    flow to the temp file by utilizing logging.StreamHandler. 
    
    Yields:
        IO[str]: The seekable temp log file containing all export messages.
    """
    with tempfile.TemporaryFile(mode='w+t') as log_temp:
        export_logger = logging.getLogger("glTFImporter")
        export_handler = logging.StreamHandler(log_temp)
        export_handler.setLevel(logging.WARNING)
        export_logger.addHandler(export_handler)

        try:
            yield log_temp
        finally:
            export_logger.removeHandler(export_handler)
            export_handler.flush()
            export_handler.close()

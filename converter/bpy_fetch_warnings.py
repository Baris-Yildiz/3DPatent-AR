import os
import sys
import tempfile
import logging
from contextlib import contextmanager

@contextmanager
def capture_bpy_import_warnings():

    old_stdout_fd = os.dup(sys.stdout.fileno()) #Get OS console output connection
    old_stderr_fd = os.dup(sys.stderr.fileno()) #Get OS console error connection

    #Temp files to capture the Blender logs
    with tempfile.TemporaryFile(mode='w+t') as log_temp:

        #Write Blender logs to temp files.
        os.dup2(log_temp.fileno(), sys.stdout.fileno())
        os.dup2(log_temp.fileno(), sys.stderr.fileno())
        
        try:
            yield log_temp
        finally:
            #Restore console connections
            os.dup2(old_stdout_fd, sys.stdout.fileno())
            os.dup2(old_stderr_fd, sys.stderr.fileno())
            os.close(old_stdout_fd)
            os.close(old_stderr_fd)

@contextmanager
def capture_bpy_export_warnings():
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

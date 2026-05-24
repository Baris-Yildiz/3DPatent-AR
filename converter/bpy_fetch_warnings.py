import os
import sys
import tempfile
import logging
from contextlib import contextmanager

_STDOUT_FD = 1
_STDERR_FD = 2

@contextmanager
def capture_bpy_import_warnings():
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

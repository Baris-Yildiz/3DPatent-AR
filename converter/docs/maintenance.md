# Maintenance notes

This page documents intentional design decisions that look like bugs or omissions but are not. Read this before making changes.

## `gc.disable()` is intentional

```python
# main.py
import gc
gc.disable()
```

During the FBX Conversion, `ufbx` loads model data into memory and we frequently access them. The problem is that Python's garbage collector can also decide to free space at this time and free some of the data that the procedure plans to use later. The actual cause of this might be memory limitations or a conflict between `ufbx` and the garbage collector. 

Disabling the garbage collector with `gc.disable()` fixes this problem. 

## `os._exit(0)` is intentional

```python
# main.py
os._exit(0)
```

Letting the script shutdown gracefully causes to program to print warnings about memory leakage and segfaults. It is worth noting that these warnings are encountered inside the Docker container and not in the development environment (Windows 11). Also worth noting is that this doesn't prevent the module from creating the output GLB and seems to only be an exit bug. Forcefully closing with `os._exit(0)` prevents logs in the Docker container while raising them in the development environment. 

## `faulthandler` is enabled at startup

```python
# main.py
faulthandler.enable()
```

This prints a stack trace if the process crashes (segfault, abort). Can be useful while debugging, has no effect on output.

## DRACO compression settings

DRACO settings are defined as class variables on `BPYExportConverter`:

```python
DRACO_COMPRESS_LEVEL = 6 # between 0-6, max compression is 6
DRACO_QUANTIZATION_SETTINGS = (16, 12, 20, 12)  # position, normal, texcoord, generic
```

These apply to **all** exported GLB's.

If some models look wrong, especially if the UV mapping seems wrong and texture parts are scattered in the output model, see if increasing the texcoord quantization `DRACO_QUANTIZATION_SETTINGS[2]` fixes the issue. Increasing this value means more precision, potentially getting rid of truncation loss.

## FreeCAD install path is hardcoded

`CADConverter` appends a hardcoded path to `sys.path` before importing FreeCAD:

```python
sys.path.append("/usr/local/lib")
```

This matches the `cmake -DCMAKE_INSTALL_PREFIX=/usr/local` install prefix in the Dockerfile and is needed for importing `FreeCAD` and `FreeCADGui`.

## Running with `xvfb-run -a` 

The module is ran using 

```cmd
xvfb-run -a python main.py <file> <scale>
```

`xvfb-run -a ` is for FreeCAD to run in headless mode. The Docker container doesn't support GUI, so this is essential. While Blender is not run in headless mode and is compiled to Python with `bpy`, we still run the whole module with `xvfb-run -a` for simplicity. 

## Error messages are in Turkish

`ImportLogHandler` and `ExportLogHandler` write user-facing error messages to in Turkish. This is intentional since out platform is in Turkish.
# Documentation Plan

## Overview

Four steps:
1. Add docstrings to the source code
2. Write `docs/architecture.md` (data flow + class hierarchy)
3. Write the remaining `.md` pages under `docs/`
4. Build with MkDocs → HTML site + PDF

---

## Step 1 — Docstrings in source code

> **Syntax rule (important):** A docstring only works if it is the **first statement inside** the `def` or `class` body, using `"""`. Placing `'''...'''` *before* `def`/`class` makes it a floating string literal that all tooling ignores. Use `"""` throughout for consistency with PEP 257.
>
> ```python
> # WRONG — floating string, not a docstring
> '''Does something.'''
> def my_method(self):
>     pass
>
> # CORRECT
> def my_method(self):
>     """Does something."""
>     pass
> ```

### Module-level docstrings (top of every `.py` file)

| File | Status |
|---|---|
| `bpy_fetch_log_messages.py` | ✅ Done |
| `LogHandler.py` | ✅ Done |
| `bpy_utils.py` | ✅ Done |
| `environment_management.py` | ✅ Done |
| `FBXConverter.py` | ✅ Done |
| `main.py` | ✅ Done |
| `freecad_converter.py` | ✅ Done |
| `ConverterLogger.py` | — (class docstring serves this purpose) |
| `BPYExportConverter.py` | — (class docstring serves this purpose) |
| `BPYImportExportConverter.py` | — (class docstring serves this purpose) |
| `CADConverter.py` | — (class docstring serves this purpose) |
| `OBJConverter.py` | — (class docstring serves this purpose) |
| `STLConverter.py` | — (class docstring serves this purpose) |
| `GLBConverter.py` | — (class docstring serves this purpose) |
| `converter_decider.py` | — (function docstring serves this purpose) |
| `utils.py` | — (class docstring serves this purpose) |

### Class docstrings

| Class | File | Status |
|---|---|---|
| `AbstractConverter` | `AbstractConverter.py` | ✅ Done |
| `UFBXDataContainers` | `FBXConverter.py` | ✅ Done |
| `FBXConverter` | `FBXConverter.py` | ✅ Done |
| `BPYExportConverter` | `BPYExportConverter.py` | ✅ Done |
| `BPYImportExportConverter` | `BPYImportExportConverter.py` | ✅ Done |
| `CADConverter` | `CADConverter.py` | ✅ Done |
| `OBJConverter` | `OBJConverter.py` | ✅ Done |
| `STLConverter` | `STLConverter.py` | ✅ Done |
| `GLBConverter` | `GLBConverter.py` | ✅ Done |
| `ConverterLogger` | `ConverterLogger.py` | ✅ Done |
| `LogHandler` | `LogHandler.py` | ✅ Done |
| `ImportLogHandler` | `LogHandler.py` | ✅ Done |
| `ExportLogHandler` | `LogHandler.py` | ✅ Done |
| `ConverterArgs` | `environment_management.py` | ✅ Done |

### Method-level docstrings

| Method | File | Status |
|---|---|---|
| `capture_bpy_import_messages()` | `bpy_fetch_log_messages.py` | ✅ Done |
| `capture_bpy_export_messages()` | `bpy_fetch_log_messages.py` | ✅ Done |
| `process_logs()` | `LogHandler.py` | ✅ Done |
| `export_model()` | `BPYExportConverter.py` | ✅ Done |
| `import_model()` | `BPYImportExportConverter.py` | ✅ Done |
| `_empty_blender_scene()` | `BPYImportExportConverter.py` | ✅ Done |
| `_get_model_import_func()` | `BPYImportExportConverter.py` | ✅ Done |
| `get_environment_var()` | `environment_management.py` | ✅ Done |
| `generate_custom_normals_for_object_if_needed()` | `bpy_utils.py` | ✅ Done |
| `apply_gamma_correction()` | `bpy_utils.py` | ✅ Done |
| `resize_model()` | `bpy_utils.py` | ✅ Done |
| `get_gamma_corrected_color()` | `utils.py` | ✅ Done |
| `AbstractConverter` methods | `AbstractConverter.py` | ✅ Done |
| `import_model()` | `FBXConverter.py` | ✅ Done |
| `process_model()` | `FBXConverter.py` | ✅ Done |
| `_create_bpy_images()` | `FBXConverter.py` | ✅ Done |
| `_create_bpy_materials()` | `FBXConverter.py` | ✅ Done |
| `_create_bpy_meshes()` | `FBXConverter.py` | ✅ Done |
| `_setup_texture_chain()` | `FBXConverter.py` | ✅ Done |
| `_get_opacity_value()` | `FBXConverter.py` | ✅ Done |
| `_to_blender_matrix()` | `FBXConverter.py` | ✅ Done |
| `import_model()` | `CADConverter.py` | ✅ Done |

### Remaining `'''` issues

| File | Issue |
|---|---|
| `CADConverter.py` | `"""..."""` floating before `MACRO_SCRIPT_NAME` class variable — not a real docstring, should be a `#` comment |
| `test.py` | ✅ Done |

### Use Google-style format with `"""`
```python
def capture_bpy_import_messages():
    """Captures Blender stdout/stderr at the OS file-descriptor level.

    Blender's C layer writes import warnings directly to fd 1 and fd 2,
    bypassing Python's sys.stdout. This context manager redirects those fds
    to a temp file so warnings can be parsed by ImportLogHandler.

    Yields:
        A seekable text file containing all output written during import.
    """
```
## Step 3 — Docs pages

### File structure
```
docs/
  index.md
  architecture.md
  formats/
    fbx.md
    cad.md
    obj-stl-glb.md
  modules/
    environment.md
    logging.md
    log-handlers.md
  deployment.md
  maintenance.md
mkdocs.yml
```

### Page contents guide

**`index.md`**
- What this tool does in 3 sentences
- Supported input formats → GLB output
- Quick-start: single command example
- Link to architecture page

**`architecture.md`** ← Step 2 above

**`formats/fbx.md`** (most important format page)
- Why `ufbx` is used instead of Blender's native FBX importer
- Why `UFBXDataContainers` exists (memory safety)
- Pipeline: load → pre-load containers → images → materials → meshes → export
- Coordinate system conversion (`target_axes` + `handedness_conversion_axis`)
- UV mapping: how ufbx face/loop indices map to Blender loop indices
- Texture layers and the shader node chain

**`formats/cad.md`**
- Linux/Docker only — explain the FreeCAD constraint
- Two-stage pipeline: FreeCAD macro → intermediate GLB → re-import with DRACO
- The `sys.path.append("/usr/local/lib")` and why

**`formats/obj-stl-glb.md`**
- Uses Blender's built-in importers
- OBJ-specific: gamma correction (sRGB → linear) and why it's applied manually
- STL-specific: no material data, just geometry + scale
- GLB/GLTF: passthrough — re-export applies DRACO compression

**`modules/environment.md`**
- `ConverterArgs` lifecycle (construct → `.set()`)
- Scale string → float conversion table (mm, cm, in, m)
- Why `get_environment_var()` instead of `os.environ` directly
- Env var table (same as architecture.md)

**`modules/logging.md`**
- `ConverterLogger` and the `SUCCESS` level (numeric 25, between INFO and WARNING)
- Why `logging.setLoggerClass()` must be called before any `getLogger()`
- The two capture context managers in `bpy_fetch_log_messages.py` and when each is used
- `faulthandler` — enabled at startup for crash stack traces

**`modules/log-handlers.md`**
- `LogHandler` base class: regex pattern matching on captured log text
- `ImportLogHandler`: OBJ/MTL-specific patterns
- `ExportLogHandler`: GLTF export warning patterns
- Output format: `import_errors.json` / `export_errors.json`
- Error messages are in Turkish by design (end-user facing)

**`deployment.md`**
- Local dev setup (venv, `bpy` pip package, no separate Blender needed)
- Docker build (builds Blender + FreeCAD from source, long build time)
- `xvfb-run` for headless rendering
- `docker_python_requirements.txt` vs `local_python_requirements.txt` difference

**`maintenance.md`** ← most important for future maintainers
- `gc.disable()` is intentional — `bpy` leaks with Python GC enabled
- `os._exit(0)` is intentional — normal Python exit causes Blender to segfault
- DRACO settings (`DRACO_COMPRESS_LEVEL`, `DRACO_QUANTIZATION_SETTINGS`) live in `BPYExportConverter` and apply to all formats
- ufbx `target_axes` / `handedness_conversion_axis` — do not change without testing across multiple FBX exporters (Maya, 3ds Max, Blender)
- FreeCAD path `/usr/local/lib` is hardcoded in `CADConverter.py` for Docker
- In-progress OOP refactoring: `Converter.py`, `CADConverter.py`, `NONCADConverter.py`, `ConverterDecider.py` are stubs not yet wired in

---

## Step 4 — MkDocs setup

### Install
```bash
pip install mkdocs mkdocs-material mkdocs-with-pdf
```

### `mkdocs.yml`
```yaml
site_name: 3D Model Converter
theme:
  name: material
  features:
    - navigation.tabs
    - navigation.sections
    - content.code.annotate

plugins:
  - search
  - with-pdf:
      output_path: converter-docs.pdf

markdown_extensions:
  - admonition
  - pymdownx.details
  - pymdownx.superfences:
      custom_fences:
        - name: mermaid
          class: mermaid
          format: !!python/name:pymdownx.superfences.fence_code_format
  - pymdownx.tabbed:
      alternate_style: true

nav:
  - Home: index.md
  - Architecture: architecture.md
  - Formats:
    - FBX: formats/fbx.md
    - CAD (STEP/IGES): formats/cad.md
    - OBJ / STL / GLB: formats/obj-stl-glb.md
  - Modules:
    - Environment: modules/environment.md
    - Logging: modules/logging.md
    - Log Handlers: modules/log-handlers.md
  - Deployment: deployment.md
  - Maintenance: maintenance.md
```

### Commands
```bash
mkdocs serve          # local preview at http://127.0.0.1:8000
mkdocs build          # builds site/ + converter-docs.pdf
mkdocs gh-deploy      # publishes to GitHub Pages
```

---

## Inspiration

- [Material for MkDocs docs](https://squidfunk.github.io/mkdocs-material/) — see what the output looks like
- [Pillow docs](https://pillow.readthedocs.io/) — good model for a file-format processing tool
- [Google Python Style Guide — docstrings](https://google.github.io/styleguide/pyguide.html#38-comments-and-docstrings) — the format used above
- [Python `contextlib` stdlib source](https://github.com/python/cpython/blob/main/Lib/contextlib.py) — good reference for documenting context managers

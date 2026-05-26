# Deployment

## Docker

Our web platform runs the module in Docker containers spawned per uploaded model file. The `Dockerfile` is present in the repository and produces the Docker container when it is run. 

The Docker container includes compiled `Blender 5.0.0` and `FreeCAD 1.0.0` sources and the source code of the module. `Blender` is compiled as the `bpy` module and as the software, while `FreeCAD` is compiled as the software. This is why in order to utilize `FreeCAD`, we need to run in headless mode, which `xvfb-run` allows for.

Currently, the Docker image takes about **1 hour and 40 minutes** assuming uncached instructions. The time depends on CPU and caching.

### Build

Build `Dockerfile` into an image with:

```bash
# after changing into directory which contains Dockerfile:
docker build -t 3dpatent-converter .
```

The `Dockerfile`:

1. Installs build toolchain (`gcc-14`, `g++-14`, `cmake`, Blender/FreeCAD dependencies)
2. Clones and builds Blender at a pinned commit, then packages it as a `.whl`
3. Installs the Blender wheel into a Python 3.11 venv
4. Clones and builds FreeCAD 1.0.0, installs to `/usr/local`
5. Installs Python dependencies from `docker_python_requirements.txt`
6. Copies the converter source code

### Run

Run the created image into a container with: 
```bash
docker run 3dpatent-converter
```

## Local development

The project configuration is currently lacking in terms of a development environment. For example, testing model conversion that requires FreeCAD is not covered in the local setup shown in this section. During the development, WSL and the Docker container was used to test changes in this context. The current local development configuration lacks support of **xvfb-run**.

Despite this, following the below steps produces a local environment that allows for code editing, running **without xvfb-run** and testing non-STEP/IGES formats.

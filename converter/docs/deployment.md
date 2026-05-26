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

!!! warning "xvfb-run is not available on Windows"
    `xvfb-run` is a Linux-only tool with no Windows equivalent. STEP/IGES conversion requires FreeCAD to run in headless mode via `xvfb-run`, so it cannot be tested locally on Windows. Use WSL or the Docker container for that.

* Download Python 3.11.9

* Set up a Python Virtual Environment (Below steps is for Windows only)
```bash
python -m venv venv
./venv/scripts/activate    
```

* Install **local** dependencies
```bash
pip install -r local_python_requirements.txt    
```

* Install **bpy** (For Windows)

    You need to build Blender's bpy module from their Github repository. Also, since the project uses a specific version of bpy 5.0.0, make sure to follow the commands below to build from the correct version. You can use Powershell to run these commands.

    See this [link](https://developer.blender.org/docs/handbook/building_blender/python_module/) for further information about building the `bpy` module.

    ```bash
    #download specific version of Blender
    git clone https://projects.blender.org/blender/blender.git
    cd blender
    git checkout $(git rev-list -n 1 --before="2025-10-7 20:33:41" main) #checkout bpy 5.0.0 
    make update
    make bpy
    pip install setuptools

    # The command below changes depending on your platform , for example:
    python ./build_files/utils/make_bpy_wheel.py ../build_windows_Bpy_x64_vc17_Release/bin/Release --build-dir ../build_windows_Bpy_x64_vc17_Release --output-dir ./

    # The wheel name also changes depending on the platform, for example:
    pip install bpy-5.0.0a0-cp311-cp311-win_amd64.whl #pip install on the venv python!
    ```

* Install **FreeCAD** (for Windows)

    Download and install FreeCAD 1.0.0 from the [FreeCAD releases page](https://github.com/FreeCAD/FreeCAD/releases/tag/1.0.0). Use the Windows installer (`FreeCAD_1.0.0-conda-Windows-x86_64-installer-1.exe`). The default install path is `C:\Program Files\FreeCAD 1.0`.

    After installing, create a `freecad.pth` file inside your venv's `site-packages` folder so Python can find FreeCAD at import time. The content of `freecad.pth` should be the installation path:

    ```bash
    echo C:\Program Files\FreeCAD 1.0\bin > venv\Lib\site-packages\freecad.pth
    ```
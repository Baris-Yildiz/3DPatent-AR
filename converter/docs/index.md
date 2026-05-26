# 3D Model Converter Module

Python module that converts 3D model files to the GLB format. Also applies DRACO compression. Used to convert
model files uploaded to our **platform** so that they are suitable for viewing from our AR mobile application. 

## Supported formats

| Input format | Extension(s) | Conversion Pipeline |
|---|---|---|
| OBJ | `.obj` | `bpy` Python library |
| STL | `.stl` | `bpy` Python library |
| FBX | `.fbx` | `ufbx` , `bpy` Python libraries |
| GLB / GLTF | `.glb`, `.gltf` | `bpy` library |
| STEP | `.stp`, `.step` | headless FreeCAD + `FreeCAD` , `bpy` Python libraries |
| IGES | `.igs`, `.iges` | headless FreeCAD + `FreeCAD`, `bpy` Python libraries |

!!! note "GLB format "
    Native GLB format files are still taken in for several reasons:

    * **For completeness:** Our web platform sends uploaded files to this module. By also providing a GLB endpoint we decouple file format related decisions from the web platform and make this module the only responsible structure for conversion. We also make the web platform more simple by not introducing GLB-specific storing logic.   
    * **For flexibility:** Later we may need to also modify these files instead of just storing them. For this purpose the module is easily extendable. 
    * **For DRACO compression:** Since all other files are exported with DRACO compression, applying a DRACO compression to a native GLB file is needed for completeness.   

## Usage in the Platform

The module is run inside a Docker Container, managed by the web platform. The Docker Container is a Ubuntu 24.04 OS that includes source code of the module, dependencies of the source code and installations of Blender 5.0.0 and FreeCAD 1.0.0. The module is mainly designed to be a running process in the container and **may not be configured properly for local usage**. More details in the [Deployment](deployment.md) section.

## Next steps

- [Architecture](architecture.md) — data flow, conversion pipelines, and class hierarchy
- [Deployment](deployment.md) — local setup and Docker build
- [Maintenance](maintenance.md) — intentional design decisions for future maintainers

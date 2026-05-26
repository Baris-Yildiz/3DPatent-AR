from enum import Enum

class ModelType(Enum):
    """Supported input model formats."""
    OBJ = ".obj"
    STL = ".stl"
    FBX = ".fbx"
    GLB = ".glb"
    GLTF = ".gltf"
    IGES = ".iges"
    IGS = ".igs"
    STP = ".stp"
    STEP = ".step"

def get_gamma_corrected_color(r: float, g: float, b: float, a: float):
    """Performs gamma correction, converting a color from sRGB to linear space.

    Args:
        r: red component of color in range [0, 1].
        g: green component of color in range [0, 1].
        b: blue component of color in range [0, 1].
        a: alpha component of color in range [0, 1].
    """
    gamma_function = lambda x: (x / 12.92) if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4

    linear_r = gamma_function(r)
    linear_g = gamma_function(g)
    linear_b = gamma_function(b)
    return (linear_r, linear_g, linear_b, a)

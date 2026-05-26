'''Macro for FreeCAD environment to import a STP/STEP or IGS/IGES and export to GLB.'''
import FreeCAD
import FreeCADGui
import ImportGui # type: ignore
import os
from environment_management import get_environment_var, EnvironmentVariableName

def convert_step_iges():

    try:
        doc = FreeCAD.newDocument("ImportScene") 

        model_path = get_environment_var(EnvironmentVariableName.MODEL_FILE_PATH)
        
        #Import model to scene.
        ImportGui.insert(model_path, "ImportScene")
        
        #Export root objects, exporting all objects causes problems.
        ImportGui.export(doc.RootObjects, get_environment_var(EnvironmentVariableName.OUTPUT_FILE_PATH))

        FreeCAD.closeDocument(doc.Name)
    except Exception as e:
        print(f"An Error Occured: {str(e)}")
    
convert_step_iges()
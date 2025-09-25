import FreeCAD
import FreeCADGui
import ImportGui # type: ignore
import os

#convert cad formats .iges and .stp to .glb
def convert_step_iges():

    try:
        doc = FreeCAD.newDocument("ImportScene")

        model_path = os.environ.get("MODEL_PATH")
        #import step to scene
        ImportGui.insert(model_path, "ImportScene")
        
        #export root objects, exporting all objects causes problems
        ImportGui.export(doc.RootObjects, u"out.glb")

        FreeCAD.closeDocument(doc.Name)
    except Exception as e:
        print("An Error Occured.")
    
convert_step_iges()
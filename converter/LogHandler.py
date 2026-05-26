"""
    This module contains classes to handle log messages received in log files.

    In the project, firstly the context managers in the "bpy_fetch_log_messages" module are 
    used to populate temp log files, and then the log files are given to handler classes 
    in this module.

    Handler classes in this module process the log files by searching for logs that match to 
    certain warning/error message Regular Expression patterns. These patterns essentially define
    log messages that are worth communicating to the end user.
     
    The relevant log messages are extracted in JSON files. These JSON files are then used by
    the server to notify the user of warnings/errors encoutered during model import/export. 
"""

import regex as re
import tempfile
import os
import json

class LogHandler:
    """
    The parent class that defines process_logs and export_logs_to_json methods.
    
    The class is extended by subclasses ImportLogHandler and ExportLogHandler that define patterns to be used for handling.
    """

    
    def __init__(self, name, logfile, patterns):  
        """
        Args:
            name (str): Name of the handler. Also used to name the output JSON file.
            logfile (IO[str]): The file which contains the logs line by line. Can be any seekable and iterable file.
            patterns (dict[str, re.Pattern[str]]): Dict with keys as strings (str) and values as Regular Expression patterns (regex.Pattern). The keys represent main messages surfaced to the end user. The values represent patterns to catch log messages.
        """  
        self.name = name
        self.logfile = logfile
        self.logfile.seek(0)
        self.patterns = patterns

    
    def process_logs(self) -> list[dict]:
        """
        Reads the log file line by line, uses self.patterns.

        Returns:
            A list of dicts with keys "message" and "details". 
            
            "message" is the main message surfaced to the user, in self.patterns.keys().
            "details" is any important details in the original log, the caught group. 
        """
        user_logs = []
        for line in self.logfile:
            for key in self.patterns.keys():
                match = re.search(self.patterns[key], line)
                if match:
                    data = {"message":key, "details": match.group(1)}
                    user_logs.append(data)
        return user_logs
    
    
    def export_user_logs_to_json(self):
        """
        Calls process_logs() and writes the returned dict to the output JSON file.   
        """
        errors = self.process_logs()
        
        if errors:
            path = os.path.abspath(f"{self.name}.json")
            with open(f"{path}", encoding="utf-8", mode="w") as f:
                json.dump(errors, f, ensure_ascii=False, indent=4)
        

class ImportLogHandler(LogHandler):
    """
    Defines self.patterns for import messages.
    The surfaced messages are in Turkish and are exported in the JSON file "import_errors.json" .
    """


    def __init__(self, logfile):
        self.patterns = {
            "Yüklediğiniz MTL dosyasındaki bu satır desteklenmiyor": re.compile(r"MTL\stexture\smap\stype\snot\ssupported:\s(.*?$)"),
            "MTL dosyası bulunamamıştır. Modeliniz rengi/materyali eksik görünebilir": re.compile(r"parse_and_store:\sOBJ\simport:\scannot\sread\sfrom\sMTL\sfile:\s\'.*\\(.*?\.mtl)\'"),
            "Yüklediğiniz modeldeki bu UV indeksi geçerli değil.": re.compile(r"Invalid\sUV\sindex\s(\d*?)\s\(valid\srange\s\[\d*?\,\s\d*?\)\)\,\signoring\sface"),
            "Yüklediğiniz modeldeki bu normal indeksi geçerli değil.": re.compile(r"Invalid\snormal\sindex\s(\d*?)\s\(valid\srange\s\[\d*?\,\s\d*?\)\)\,\signoring\sface"),
            "Yüklediğiniz modeldeki bu vertex/köşe indeksi geçerli değil.":re.compile(r"Invalid\svertex\sindex\s(\d*?)\s\(valid\srange\s\[\d*?\,\s\d*?\)\)\,\signoring\sface"),
            "Yüklediğiniz modelde çok uzun bir satır var.":re.compile(r"OBJ\sfile\scontains\sa\sline\s\#(\d*?)\sthat\sis\stoo\slong"),
            "Yüklediğiniz MTL dosyasındaki şu 'illum' değeri desteklenmiyor":re.compile(r"Material\sillum\svalue\s\'(\d*?)\'\sis\snot\ssupported\sby\sthe\sPrincipled\sBSDF\sshader."),
        }
        super().__init__("import_errors", logfile, self.patterns)
    

class ExportLogHandler(LogHandler):
    """
    Defines self.patterns for export messages.
    The surfaced messages are in Turkish and are exported in the JSON file "export_errors.json" .
    """

    
    def __init__(self, logfile):
        self.patterns = {
            "Yüklediğiniz modelde bu texture dosyası bulunamamıştır": re.compile(r"Image\s\'<bpy_struct,\sImage\(\"(.*?)\"\)\sat(.*?')\shas\sno\ssize\sand\scannot\sbe\sexported."),
            "Yüklediğiniz modeldeki bir texture verisi boş, export edilemedi": re.compile(r"Image\sdata\sis\sempty,\snot\sexporting\simage(.*)"),
            "Yüklediğiniz modeldeki şu mesh çıkarılamadı. Modeliniz yanlış görünebilir": re.compile(r"Mesh\s'(.*?)'\shas\sno\sprimitives\sand\swill\sbe\somitted\."),
            "Yüklediğiniz modeldeki şu mesh geçersiz ve hatalı görünebilir": re.compile(r"Mesh\s(.*?)\sis\snot\svalid,\sand\smay\sbe\sexported\swrongly"),
        }
        super().__init__("export_errors", logfile, self.patterns)

# https://projects.blender.org/blender/blender/src/commit/60325c7a9c889a6683f177d10daed8f54b020b8f/source/blender/io/wavefront_obj/importer/obj_import_file_reader.cc
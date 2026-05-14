import regex as re
import tempfile
import os
import json

class LogHandler:
    def __init__(self, name, logfile, patterns):    
        self.name = name
        self.logfile = logfile
        self.logfile.seek(0)
        self.patterns = patterns

    def process_logs(self):
        user_logs = []
        for line in self.logfile:
            for key in self.patterns.keys():
                match = re.search(self.patterns[key], line)
                if match:
                    data = {"message":key, "details": match.group(1)}
                    user_logs.append(data)
        return user_logs

    def export_user_logs_to_json(self):
        errors = self.process_logs()
        
        if errors:
            path = os.path.abspath(f"{self.name}.json")
            with open(f"{path}", encoding="utf-8", mode="w") as f:
                json.dump(errors, f, ensure_ascii=False, indent=4)
        

class ImportLogHandler(LogHandler):
    def __init__(self, logfile):
        self.patterns = {
            "Yüklediğiniz MTL dosyasındaki bu satır desteklenmiyor": re.compile(r"MTL\stexture\smap\stype\snot\ssupported:\s(.*?$)")
        }
        super().__init__("import_errors", logfile, self.patterns)
    

class ExportLogHandler(LogHandler):
    def __init__(self, logfile):
        self.patterns = {
            "Yüklediğiniz modelde bu texture dosyası bulunamamıştır": re.compile(r"Image\s\'<bpy_struct,\sImage\(\"(.*?)\"\)\sat(.*?')\shas\sno\ssize\sand\scannot\sbe\sexported.")
        }
        super().__init__("export_errors", logfile, self.patterns)
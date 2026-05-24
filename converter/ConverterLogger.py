import logging

from rich.logging import RichHandler
from rich.console import Console
from rich.theme import Theme

SUCCESS_LEVEL = 25
SUCCESS_NAME = "SUCCESS"

if SUCCESS_NAME not in logging.getLevelNamesMapping():
    logging.addLevelName(SUCCESS_LEVEL, SUCCESS_NAME)
    
#Custom logger class for this converter.
class ConverterLogger(logging.Logger):
    
    def __init__(self, name, level=logging.DEBUG):
        super().__init__(name, level)

        if not self.handlers:
            log_format = '[%(asctime)s] %(name)s: %(message)s'

            formatter = logging.Formatter(log_format)

            custom_theme = Theme({
                "logging.level.success": "bold green", 
            })

            custom_console = Console(theme=custom_theme)

            rich_handler = RichHandler(console=custom_console, rich_tracebacks=True)
            rich_handler.setLevel(level)
            rich_handler.setFormatter(formatter)

            self.addHandler(rich_handler)

    def success(self, message, *args, **kws):
        if self.isEnabledFor(SUCCESS_LEVEL):
            self._log(SUCCESS_LEVEL, message, args, **kws)
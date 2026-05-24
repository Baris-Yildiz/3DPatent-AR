from abc import ABC, abstractmethod

#Abstract Base Converter class to define conversion
class AbstractConverter(ABC):

    @abstractmethod
    def import_model(self) -> None:
        pass
    
    @abstractmethod
    def process_model(self) -> None:
        pass

    @abstractmethod
    def export_model(self) -> None:
        pass

    def convert(self) -> None:
        self.import_model()
        self.process_model()
        self.export_model()
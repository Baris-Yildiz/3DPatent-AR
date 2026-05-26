from abc import ABC, abstractmethod

class AbstractConverter(ABC):
    """Abstract base class that defines the converter pipeline.

    convert() calls import_model(), process_model(), export_model() in that order. 
    """

    @abstractmethod
    def import_model(self) -> None:
        """Imports the model / loads the model into memory.

        Although there is no enforcement, the convention is to define this method as an "importer" in the subclasses.
        """
        pass

    @abstractmethod
    def process_model(self) -> None:
        """Creates materials and meshes for GLB conversion. Does any processing other than importing or exporting.

        Although there is no enforcement, the convention is to define this method as a "model processor" in the subclasses.
        """
        pass

    @abstractmethod
    def export_model(self) -> None:
        """Exports the model as a GLB file.

        Although there is no enforcement, the convention is to define this method as an "exporter" in the subclasses.
        """
        pass

    def convert(self) -> None:
        """Runs the full conversion pipeline: import_model -> process_model -> export_model."""
        self.import_model()
        self.process_model()
        self.export_model()

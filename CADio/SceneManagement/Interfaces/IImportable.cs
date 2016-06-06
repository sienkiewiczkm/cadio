using CADio.SceneManagement.Serialization;

namespace CADio.SceneManagement.Interfaces
{
    public interface IImportable
    {
        void Import(SceneDataImporter importer);
    }
}
using CADio.SceneManagement.Serialization;

namespace CADio.SceneManagement.Interfaces
{
    public interface ISaveable
    {
        void Save(SceneDataSaver saver);
    }
}
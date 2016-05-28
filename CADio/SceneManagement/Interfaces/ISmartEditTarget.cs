namespace CADio.SceneManagement.Interfaces
{
    public interface ISmartEditTarget
    {
        void RegisterNewObject(WorldObject worldObject);
        void NotifyAboutStateChange();
    }
}
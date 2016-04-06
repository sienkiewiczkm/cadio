namespace CADio.SceneManagement
{
    public interface ISmartEditTarget
    {
        void RegisterNewObject(WorldObject worldObject);
        void NotifyAboutStateChange();
    }
}
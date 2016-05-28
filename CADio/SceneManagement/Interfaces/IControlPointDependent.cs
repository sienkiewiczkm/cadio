namespace CADio.SceneManagement.Interfaces
{
    public interface IControlPointDependent
    {
        void AttachObject(WorldObject worldObject);
        void DetachControlPoint(ControlPoint controlPoint);
        void DetachObjectReferences(WorldObject worldObject);
    }
}
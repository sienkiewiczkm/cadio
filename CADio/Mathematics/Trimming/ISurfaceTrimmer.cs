namespace CADio.Mathematics.Trimming
{
    public enum TrimMode
    {
        Disabled,
        Inside,
        Outside,
    }

    public interface ISurfaceTrimmer
    {
        TrimMode TrimMode { get; set; } 
        bool VerifyParametrisation(double u, double v);
    }
}
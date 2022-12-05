namespace AssetStudio;

public class BuildType
{
    private readonly string buildType;

    public bool IsAlpha => buildType == "a";
    public bool IsPatch => buildType == "p";

    public BuildType(string type) => buildType = type;
}

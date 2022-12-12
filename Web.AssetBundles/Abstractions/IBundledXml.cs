namespace Web.AssetBundles.Abstractions;

public interface IBundledXml
{
    public string BundleName { get; }

    public void LoadBundle(string xml);
}

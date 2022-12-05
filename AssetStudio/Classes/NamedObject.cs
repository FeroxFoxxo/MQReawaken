namespace AssetStudio;

public class NamedObject : EditorExtension
{
    public string m_Name;

    protected NamedObject(ObjectReader reader) : base(reader) => m_Name = reader.ReadAlignedString();
}

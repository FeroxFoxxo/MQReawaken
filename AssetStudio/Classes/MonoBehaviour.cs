namespace AssetStudio;

public sealed class MonoBehaviour : Behaviour
{
    public string m_Name;
    public PPtr<MonoScript> m_Script;

    public MonoBehaviour(ObjectReader reader) : base(reader)
    {
        m_Script = new PPtr<MonoScript>(reader);
        m_Name = reader.ReadAlignedString();
    }
}

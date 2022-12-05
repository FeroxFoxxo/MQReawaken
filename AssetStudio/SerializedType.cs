namespace AssetStudio;

public class SerializedType
{
    public int classID;
    public string m_AsmName;
    public bool m_IsStrippedType;
    public string m_KlassName;
    public string m_NameSpace;
    public byte[] m_OldTypeHash; //Hash128
    public byte[] m_ScriptID; //Hash128
    public short m_ScriptTypeIndex = -1;
    public TypeTree m_Type;
    public int[] m_TypeDependencies;
}

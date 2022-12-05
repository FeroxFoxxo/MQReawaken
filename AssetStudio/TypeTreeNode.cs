namespace AssetStudio;

public class TypeTreeNode
{
    public int m_ByteSize;
    public int m_Index;
    public int m_Level;
    public int m_MetaFlag;
    public string m_Name;
    public uint m_NameStrOffset;
    public ulong m_RefTypeHash;
    public string m_Type;
    public int m_TypeFlags; //m_IsArray
    public uint m_TypeStrOffset;
    public int m_Version;

    public TypeTreeNode()
    {
    }

    public TypeTreeNode(string type, string name, int level, bool align)
    {
        m_Type = type;
        m_Name = name;
        m_Level = level;
        m_MetaFlag = align ? 0x4000 : 0;
    }
}

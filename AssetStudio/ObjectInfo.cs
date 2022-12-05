namespace AssetStudio;

public class ObjectInfo
{
    public uint byteSize;
    public long byteStart;
    public int classID;
    public ushort isDestroyed;

    public long m_PathID;
    public SerializedType serializedType;
    public byte stripped;
    public int typeID;
}

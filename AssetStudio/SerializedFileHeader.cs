namespace AssetStudio;

public class SerializedFileHeader
{
    public long m_DataOffset;
    public byte m_Endianess;
    public long m_FileSize;
    public uint m_MetadataSize;
    public byte[] m_Reserved;
    public SerializedFileFormatVersion m_Version;
}

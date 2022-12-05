using System;

namespace AssetStudio;

public class ObjectReader : EndianBinaryReader
{
    public SerializedFile assetsFile;
    public uint byteSize;
    public long byteStart;
    public long m_PathID;
    public SerializedFileFormatVersion m_Version;
    public BuildTarget platform;
    public SerializedType serializedType;
    public ClassIDType type;

    public int[] version => assetsFile.version;
    public BuildType buildType => assetsFile.buildType;

    public ObjectReader(EndianBinaryReader reader, SerializedFile assetsFile, ObjectInfo objectInfo) : base(
        reader.BaseStream, reader.Endian)
    {
        this.assetsFile = assetsFile;
        m_PathID = objectInfo.m_PathID;
        byteStart = objectInfo.byteStart;
        byteSize = objectInfo.byteSize;
        if (Enum.IsDefined(typeof(ClassIDType), objectInfo.classID))
            type = (ClassIDType)objectInfo.classID;
        else
            type = ClassIDType.UnknownType;
        serializedType = objectInfo.serializedType;
        platform = assetsFile.m_TargetPlatform;
        m_Version = assetsFile.header.m_Version;
    }

    public void Reset() => Position = byteStart;
}

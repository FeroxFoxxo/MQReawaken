using System;

namespace AssetStudio;

public class FileIdentifier
{
    //custom
    public string fileName;
    public Guid guid;
    public string pathName;

    public int
        type; //enum { kNonAssetType = 0, kDeprecatedCachedAssetType = 1, kSerializedAssetType = 2, kMetaAssetType = 3 };
}

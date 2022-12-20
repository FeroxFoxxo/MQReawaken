using AssetStudio;
using Web.AssetBundles.Models;

using static AssetStudio.BundleFile;

namespace Web.AssetBundles.Extensions;

public static class FixAssetBundle
{
    public static byte[] ApplyFixes(this InternalAssetInfo asset) =>
        asset.GetHeader().Concat(asset.GetBundle()).ToArray();

    private static byte[] GetHeader(this InternalAssetInfo asset)
    {
        var header = new Header()
        {
            signature = "UnityRaw",
            unityRevision = asset.UnityVersion
        };

        var versionInfo = asset.UnityVersion.Split('.').Select(x => x.Split('f'))
            .SelectMany(x => x).Select(x => Convert.ToDouble(x)).ToArray();

        header.unityVersion =
            versionInfo[0] == 2 ? "2.x.x" :
            versionInfo[0] is 3 or 4 ? "3.x.x" :
            throw new InvalidDataException();

        var d = versionInfo[0] + versionInfo[1] / 10;

        header.version =
            d is >= 1 and <= 2.5 ? 1u :
            d is >= 2.6 and <= 3.4 ? 2u :
            d is >= 3.5 and <= 4 ? 3u :
            throw new InvalidDataException();

        using var memStream = new MemoryStream();
        var binWriter = new BinaryWriter(memStream);

        binWriter.WriteStringTillNull(header.signature);
        binWriter.WriteUInt32(header.version, EndianType.BigEndian);
        binWriter.WriteStringTillNull(header.unityVersion);
        binWriter.WriteStringTillNull(header.unityRevision);

        var headerArray = memStream.ToArray();

        return headerArray;
    }

    private static byte[] GetBundle(this InternalAssetInfo asset)
    {
        var assetArray = File.ReadAllBytes(asset.Path);

        using var memStream = new MemoryStream();
        var binWriter = new BinaryWriter(memStream);

        binWriter.WriteStringTillNull(asset.UnityVersion);
        binWriter.Write((byte)0x06);

        var seq = memStream.ToArray();

        var charIndex = assetArray.GetIndexOfByteSequence(seq);

        assetArray[charIndex + seq.Length - 1] = 0x13;

        return assetArray;
    }

    private static int GetIndexOfByteSequence(this byte[] array, byte[] subSeq)
    {
        for(var i = 0; i < array.Length - subSeq.Length; i++)
        {
            var isOfSequence = true;

            for (var j = 0; j < subSeq.Length; j++)
                if (subSeq[j] != array[i + j])
                {
                    isOfSequence = false;
                    break;
                }

            if (isOfSequence)
                return i;
        }
        throw new InvalidDataException();
    }

    public static int[] GetUnityVersionArray(this string version) =>
        version.Split('.').Select(x => x.Split('f'))
            .SelectMany(x => x).Select(x => Convert.ToInt32(x)).ToArray();

    public static double GetUnityVersionDouble(this string version) =>
        GetUnityVersionArray(version).GetDoubleFromArray();

    private static double GetDoubleFromArray(this int[] array) =>
        array
            .Select((t, i) => t * Convert.ToDouble(Math.Pow(-10, array.Length - i - 1)))
            .Sum();
}

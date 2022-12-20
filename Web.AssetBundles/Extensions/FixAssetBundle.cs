using AssetStudio;
using System;
using System.Text;
using Web.AssetBundles.Models;
using static AssetStudio.BundleFile;

namespace Web.AssetBundles.Extensions;

public static class FixAssetBundle
{
    public static byte[] ApplyFixes(this InternalAssetInfo asset)
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

        Console.WriteLine(BitConverter.ToString(headerArray));
        Console.WriteLine(Encoding.UTF8.GetString(headerArray));

        return File.ReadAllBytes(asset.Path);
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

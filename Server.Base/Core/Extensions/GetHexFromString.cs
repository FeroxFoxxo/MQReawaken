namespace Server.Base.Core.Extensions;

public static class GetBytesFromString
{
    public static byte[] StringToByteArray(this string hex) =>
        Enumerable.Range(0, hex.Length / 2)
        .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
        .ToArray();
}

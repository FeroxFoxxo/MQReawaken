using AssetStudio;
using System.Text;

namespace Web.AssetBundles.Extensions;

public static class BinaryWriterExtensions
{
    public static void WriteStringTillNull(this BinaryWriter writer, string value) =>
        writer.Write(Encoding.UTF8.GetBytes($"{value}\0"));
    
    public static void WriteUInt32(this BinaryWriter writer, uint value, EndianType endian)
    {
        if (endian == EndianType.BigEndian)
        {
            var buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            writer.Write(buffer);
        }
        else
        {
            writer.Write(value);
        }
    }
}

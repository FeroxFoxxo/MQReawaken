using System.Buffers;

namespace AssetStudio;

public static class BigArrayPool<T>
{
    public static ArrayPool<T> Shared { get; } = ArrayPool<T>.Create(64 * 1024 * 1024, 3);
}

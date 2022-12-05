/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.IO;

namespace Org.Brotli.Dec;

/// <summary>
///     <see cref="System.IO.Stream" />
///     decorator that decompresses brotli data.
///     <p> Not thread-safe.
/// </summary>
public class BrotliInputStream : Stream
{
    public const int DefaultInternalBufferSize = 16384;

    /// <summary>Decoder state.</summary>
    private readonly State state = new();

    /// <summary>Internal buffer used for efficient byte-by-byte reading.</summary>
    private readonly byte[] buffer;

    /// <summary>Next unused byte offset.</summary>
    private int bufferOffset;

    /// <summary>Number of decoded but still unused bytes in internal buffer.</summary>
    private int remainingBufferBytes;

    // <{[INJECTED CODE]}>
    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override bool CanWrite => false;

    /// <summary>
    ///     Creates a
    ///     <see cref="System.IO.Stream" />
    ///     wrapper that decompresses brotli data.
    ///     <p>
    ///         For byte-by-byte reading (
    ///         <see cref="ReadByte()" />
    ///         ) internal buffer with
    ///         <see cref="DefaultInternalBufferSize" />
    ///         size is allocated and used.
    ///         <p> Will block the thread until first kilobyte of data of source is available.
    /// </summary>
    /// <param name="source">underlying data source</param>
    /// <exception cref="System.IO.IOException">in case of corrupted data or source stream problems</exception>
    public BrotliInputStream(Stream source)
        : this(source, DefaultInternalBufferSize, null)
    {
    }

    /// <summary>
    ///     Creates a
    ///     <see cref="System.IO.Stream" />
    ///     wrapper that decompresses brotli data.
    ///     <p>
    ///         For byte-by-byte reading (
    ///         <see cref="ReadByte()" />
    ///         ) internal buffer of specified size is
    ///         allocated and used.
    ///         <p> Will block the thread until first kilobyte of data of source is available.
    /// </summary>
    /// <param name="source">compressed data source</param>
    /// <param name="byteReadBufferSize">
    ///     size of internal buffer used in case of
    ///     byte-by-byte reading
    /// </param>
    /// <exception cref="System.IO.IOException">in case of corrupted data or source stream problems</exception>
    public BrotliInputStream(Stream source, int byteReadBufferSize)
        : this(source, byteReadBufferSize, null)
    {
    }

    /// <summary>
    ///     Creates a
    ///     <see cref="System.IO.Stream" />
    ///     wrapper that decompresses brotli data.
    ///     <p>
    ///         For byte-by-byte reading (
    ///         <see cref="ReadByte()" />
    ///         ) internal buffer of specified size is
    ///         allocated and used.
    ///         <p> Will block the thread until first kilobyte of data of source is available.
    /// </summary>
    /// <param name="source">compressed data source</param>
    /// <param name="byteReadBufferSize">
    ///     size of internal buffer used in case of
    ///     byte-by-byte reading
    /// </param>
    /// <param name="customDictionary">
    ///     custom dictionary data;
    ///     <see langword="null" />
    ///     if not used
    /// </param>
    /// <exception cref="System.IO.IOException">in case of corrupted data or source stream problems</exception>
    public BrotliInputStream(Stream source, int byteReadBufferSize, byte[] customDictionary)
    {
        if (byteReadBufferSize <= 0)
            throw new ArgumentException("Bad buffer size:" + byteReadBufferSize);
        if (source == null)
            throw new ArgumentException("source is null");
        buffer = new byte[byteReadBufferSize];
        remainingBufferBytes = 0;
        bufferOffset = 0;
        try
        {
            State.SetInput(state, source);
        }
        catch (BrotliRuntimeException ex)
        {
            throw new IOException("Brotli decoder initialization failed", ex);
        }

        if (customDictionary != null)
            Decode.SetCustomDictionary(state, customDictionary);
    }

    /// <summary>
    ///     <inheritDoc />
    /// </summary>
    /// <exception cref="System.IO.IOException" />
    public override void Close() => State.Close(state);

    /// <summary>
    ///     <inheritDoc />
    /// </summary>
    /// <exception cref="System.IO.IOException" />
    public override int ReadByte()
    {
        if (bufferOffset >= remainingBufferBytes)
        {
            remainingBufferBytes = Read(buffer, 0, buffer.Length);
            bufferOffset = 0;
            if (remainingBufferBytes == -1)
                return -1;
        }

        return buffer[bufferOffset++] & 0xFF;
    }

    /// <summary>
    ///     <inheritDoc />
    /// </summary>
    /// <exception cref="System.IO.IOException" />
    public override int Read(byte[] destBuffer, int destOffset, int destLen)
    {
        if (destOffset < 0)
            throw new ArgumentException("Bad offset: " + destOffset);
        if (destLen < 0)
            throw new ArgumentException("Bad length: " + destLen);
        if (destOffset + destLen > destBuffer.Length)
            throw new ArgumentException("Buffer overflow: " + (destOffset + destLen) + " > " + destBuffer.Length);
        if (destLen == 0)
            return 0;
        var copyLen = Math.Max(remainingBufferBytes - bufferOffset, 0);
        if (copyLen != 0)
        {
            copyLen = Math.Min(copyLen, destLen);
            Array.Copy(buffer, bufferOffset, destBuffer, destOffset, copyLen);
            bufferOffset += copyLen;
            destOffset += copyLen;
            destLen -= copyLen;
            if (destLen == 0)
                return copyLen;
        }

        try
        {
            state.output = destBuffer;
            state.outputOffset = destOffset;
            state.outputLength = destLen;
            state.outputUsed = 0;
            Decode.Decompress(state);
            if (state.outputUsed == 0)
                return 0;
            return state.outputUsed + copyLen;
        }
        catch (BrotliRuntimeException ex)
        {
            throw new IOException("Brotli stream decoding failed", ex);
        }
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override IAsyncResult BeginWrite(byte[] buffer, int offset,
        int count, AsyncCallback callback, object state) =>
        throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Flush()
    {
    }
}

/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.IO;

namespace Org.Brotli.Dec;

internal sealed class State
{
    internal readonly int[] blockLength = new int[3];

    internal readonly int[] blockLenTrees = new int[3 * Huffman.HuffmanMaxTableSize];

    internal readonly int[] blockTypeRb = new int[6];

    internal readonly int[] blockTypeTrees = new int[3 * Huffman.HuffmanMaxTableSize];

    internal readonly BitReader br = new();

    internal readonly int[] distRb = { 16, 15, 11, 4 };

    internal readonly HuffmanTreeGroup hGroup0 = new();

    internal readonly HuffmanTreeGroup hGroup1 = new();

    internal readonly HuffmanTreeGroup hGroup2 = new();

    internal readonly int[] numBlockTypes = new int[3];

    internal int bytesToIgnore = 0;

    internal int bytesToWrite;

    internal int bytesWritten;

    internal int contextLookupOffset1;

    internal int contextLookupOffset2;

    internal byte[] contextMap;

    internal int contextMapSlice;

    internal byte[] contextModes;

    internal int copyDst;

    internal int copyLength;

    internal byte[] customDictionary = new byte[0];

    internal int distance;

    internal int distanceCode;

    internal int distancePostfixBits;

    internal int distancePostfixMask;

    internal byte[] distContextMap;

    internal int distContextMapSlice;

    internal int distRbIdx = 0;

    internal long expectedTotalSize = 0;

    internal bool inputEnd;

    internal int insertLength;

    internal bool isMetadata;

    internal bool isUncompressed;

    internal int j;

    internal int literalTree;

    internal int literalTreeIndex = 0;

    internal int maxBackwardDistance;

    internal int maxDistance = 0;

    internal int maxRingBufferSize;

    internal int metaBlockLength;

    internal int nextRunningState;

    internal int numDirectDistanceCodes;

    internal byte[] output;

    internal int outputLength;

    internal int outputOffset;

    internal int outputUsed;

    internal int pos = 0;

    internal byte[] ringBuffer;

    internal int ringBufferSize = 0;
    internal int runningState = RunningState.Uninitialized;

    internal int treeCommandOffset;

    internal bool trivialLiteralContext = false;

    // Current meta-block header information.
    // TODO: Update to current spec.
    private static int DecodeWindowBits(BitReader br)
    {
        if (BitReader.ReadBits(br, 1) == 0)
            return 16;
        var n = BitReader.ReadBits(br, 3);
        if (n != 0)
            return 17 + n;
        n = BitReader.ReadBits(br, 3);
        if (n != 0)
            return 8 + n;
        return 17;
    }

    /// <summary>Associate input with decoder state.</summary>
    /// <param name="state">uninitialized state without associated input</param>
    /// <param name="input">compressed data source</param>
    internal static void SetInput(State state, Stream input)
    {
        if (state.runningState != RunningState.Uninitialized)
            throw new InvalidOperationException("State MUST be uninitialized");
        BitReader.Init(state.br, input);
        var windowBits = DecodeWindowBits(state.br);
        if (windowBits == 9)
            /* Reserved case for future expansion. */
            throw new BrotliRuntimeException("Invalid 'windowBits' code");
        state.maxRingBufferSize = 1 << windowBits;
        state.maxBackwardDistance = state.maxRingBufferSize - 16;
        state.runningState = RunningState.BlockStart;
    }

    /// <exception cref="System.IO.IOException" />
    internal static void Close(State state)
    {
        if (state.runningState == RunningState.Uninitialized)
            throw new InvalidOperationException("State MUST be initialized");
        if (state.runningState == RunningState.Closed)
            return;
        state.runningState = RunningState.Closed;
        BitReader.Close(state.br);
    }
}

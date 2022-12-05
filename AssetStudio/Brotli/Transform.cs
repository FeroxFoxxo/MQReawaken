/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Org.Brotli.Dec;

/// <summary>Transformations on dictionary words.</summary>
internal sealed class Transform
{
    internal static readonly Transform[] Transforms =
    {
        new(string.Empty, WordTransformType.Identity, string.Empty), new(string.Empty,
            WordTransformType.Identity, " "),
        new(" ", WordTransformType.Identity, " "), new(string.Empty, WordTransformType.OmitFirst1, string.Empty),
        new(string.Empty, WordTransformType.UppercaseFirst, " "),
        new(string.Empty, WordTransformType.Identity, " the "), new(" ", WordTransformType.Identity
            , string.Empty),
        new("s ", WordTransformType.Identity, " "), new(string.Empty, WordTransformType.Identity, " of "), new(
            string.Empty, WordTransformType
                .UppercaseFirst, string.Empty),
        new(string.Empty, WordTransformType.Identity, " and "),
        new(string.Empty, WordTransformType.OmitFirst2, string.Empty),
        new(string.Empty, WordTransformType.OmitLast1, string.Empty), new(", ", WordTransformType.Identity, " "), new(
            string.Empty, WordTransformType.Identity
            , ", "),
        new(" ", WordTransformType.UppercaseFirst, " "), new(string.Empty, WordTransformType.Identity, " in "), new(
            string.Empty, WordTransformType
                .Identity, " to "),
        new("e ", WordTransformType.Identity, " "), new(string.Empty, WordTransformType.Identity, "\""), new(
            string.Empty,
            WordTransformType.Identity, "."),
        new(string.Empty, WordTransformType.Identity, "\">"), new(string.Empty, WordTransformType.Identity, "\n"),
        new(string.Empty, WordTransformType.OmitLast3, string.Empty),
        new(string.Empty, WordTransformType.Identity, "]"), new(string.Empty, WordTransformType
            .Identity, " for "),
        new(string.Empty, WordTransformType.OmitFirst3, string.Empty),
        new(string.Empty, WordTransformType.OmitLast2, string.Empty),
        new(string.Empty, WordTransformType.Identity, " a "), new(string.Empty, WordTransformType.Identity, " that "),
        new(" ", WordTransformType.UppercaseFirst
            , string.Empty),
        new(string.Empty, WordTransformType.Identity, ". "), new(".", WordTransformType.Identity, string.Empty), new(
            " ", WordTransformType
                .Identity, ", "),
        new(string.Empty, WordTransformType.OmitFirst4, string.Empty),
        new(string.Empty, WordTransformType.Identity, " with "), new(string.Empty, WordTransformType.Identity, "'"),
        new(string.Empty, WordTransformType.Identity, " from "), new(string.Empty, WordTransformType.Identity
            , " by "),
        new(string.Empty, WordTransformType.OmitFirst5, string.Empty),
        new(string.Empty, WordTransformType.OmitFirst6, string.Empty),
        new(" the ", WordTransformType.Identity, string.Empty),
        new(string.Empty, WordTransformType.OmitLast4, string.Empty), new(string.Empty, WordTransformType
            .Identity, ". The "),
        new(string.Empty, WordTransformType.UppercaseAll, string.Empty),
        new(string.Empty, WordTransformType.Identity, " on "), new(string.Empty, WordTransformType.Identity, " as "),
        new(string.Empty, WordTransformType.Identity, " is "), new(string.Empty, WordTransformType.OmitLast7
            , string.Empty),
        new(string.Empty, WordTransformType.OmitLast1, "ing "), new(string.Empty, WordTransformType.Identity, "\n\t"),
        new(string.Empty
            , WordTransformType.Identity, ":"),
        new(" ", WordTransformType.Identity, ". "), new(string.Empty, WordTransformType.Identity, "ed "),
        new(string.Empty, WordTransformType.OmitFirst9, string.Empty),
        new(string.Empty, WordTransformType.OmitFirst7, string.Empty), new(string.Empty, WordTransformType
            .OmitLast6, string.Empty),
        new(string.Empty, WordTransformType.Identity, "("), new(string.Empty, WordTransformType.UppercaseFirst, ", "),
        new(string.Empty, WordTransformType.OmitLast8, string.Empty),
        new(string.Empty, WordTransformType.Identity, " at "), new(string.Empty, WordTransformType
            .Identity, "ly "),
        new(" the ", WordTransformType.Identity, " of "), new(string.Empty, WordTransformType.OmitLast5, string.Empty),
        new(
            string.Empty, WordTransformType.OmitLast9, string.Empty),
        new(" ", WordTransformType.UppercaseFirst, ", "), new(string.Empty, WordTransformType.UppercaseFirst
            , "\""),
        new(".", WordTransformType.Identity, "("), new(string.Empty, WordTransformType.UppercaseAll, " "), new(
            string.Empty, WordTransformType
                .UppercaseFirst, "\">"),
        new(string.Empty, WordTransformType.Identity, "=\""), new(" ", WordTransformType.Identity, "."), new(".com/",
            WordTransformType.Identity, string.Empty),
        new(" the ", WordTransformType.Identity, " of the "), new(string.Empty, WordTransformType.UppercaseFirst
            , "'"),
        new(string.Empty, WordTransformType.Identity, ". This "), new(string.Empty, WordTransformType.Identity, ","),
        new(".", WordTransformType
            .Identity, " "),
        new(string.Empty, WordTransformType.UppercaseFirst, "("),
        new(string.Empty, WordTransformType.UppercaseFirst, "."),
        new(string.Empty, WordTransformType.Identity, " not "), new(" ", WordTransformType.Identity, "=\""), new(
            string.Empty, WordTransformType.Identity, "er "
        ),
        new(" ", WordTransformType.UppercaseAll, " "), new(string.Empty, WordTransformType.Identity, "al "), new(" ",
            WordTransformType
                .UppercaseAll, string.Empty),
        new(string.Empty, WordTransformType.Identity, "='"), new(string.Empty, WordTransformType.UppercaseAll, "\""),
        new(string.Empty, WordTransformType.UppercaseFirst, ". "), new(" ", WordTransformType.Identity, "("), new(
            string.Empty, WordTransformType.Identity,
            "ful "),
        new(" ", WordTransformType.UppercaseFirst, ". "), new(string.Empty, WordTransformType.Identity, "ive "), new(
            string.Empty, WordTransformType
                .Identity, "less "),
        new(string.Empty, WordTransformType.UppercaseAll, "'"), new(string.Empty, WordTransformType.Identity, "est "),
        new(" ", WordTransformType.UppercaseFirst, "."), new(string.Empty, WordTransformType.UppercaseAll, "\">"), new(
            " ", WordTransformType.Identity, "='"
        ),
        new(string.Empty, WordTransformType.UppercaseFirst, ","), new(string.Empty, WordTransformType.Identity, "ize "),
        new(string.Empty, WordTransformType
            .UppercaseAll, "."),
        new("\u00c2\u00a0", WordTransformType.Identity, string.Empty), new(" ", WordTransformType.Identity, ","), new(
            string.Empty
            , WordTransformType.UppercaseFirst, "=\""),
        new(string.Empty, WordTransformType.UppercaseAll, "=\""), new(string.Empty, WordTransformType.Identity
            , "ous "),
        new(string.Empty, WordTransformType.UppercaseAll, ", "),
        new(string.Empty, WordTransformType.UppercaseFirst, "='"), new(" ",
            WordTransformType.UppercaseFirst, ","),
        new(" ", WordTransformType.UppercaseAll, "=\""), new(" ", WordTransformType.UppercaseAll, ", "),
        new(string.Empty, WordTransformType.UppercaseAll, ","), new(string.Empty, WordTransformType.UppercaseAll, "("),
        new(string.Empty, WordTransformType.UppercaseAll, ". "), new(" ", WordTransformType.UppercaseAll, "."),
        new(string.Empty, WordTransformType.UppercaseAll, "='"), new(" ", WordTransformType
            .UppercaseAll, ". "),
        new(" ", WordTransformType.UppercaseFirst, "=\""), new(" ", WordTransformType.UppercaseAll, "='"), new(" ",
            WordTransformType
                .UppercaseFirst, "='")
    };

    private readonly byte[] prefix;

    private readonly byte[] suffix;

    private readonly int type;

    internal Transform(string prefix, int type, string suffix)
    {
        this.prefix = ReadUniBytes(prefix);
        this.type = type;
        this.suffix = ReadUniBytes(suffix);
    }

    internal static byte[] ReadUniBytes(string uniBytes)
    {
        var result = new byte[uniBytes.Length];
        for (var i = 0; i < result.Length; ++i)
            result[i] = unchecked((byte)uniBytes[i]);
        return result;
    }

    internal static int TransformDictionaryWord(byte[] dst, int dstOffset, byte[] word, int wordOffset, int len,
        Transform transform)
    {
        var offset = dstOffset;
        // Copy prefix.
        var @string = transform.prefix;
        var tmp = @string.Length;
        var i = 0;
        // In most cases tmp < 10 -> no benefits from System.arrayCopy
        while (i < tmp)
            dst[offset++] = @string[i++];
        // Copy trimmed word.
        var op = transform.type;
        tmp = WordTransformType.GetOmitFirst(op);
        if (tmp > len)
            tmp = len;
        wordOffset += tmp;
        len -= tmp;
        len -= WordTransformType.GetOmitLast(op);
        i = len;
        while (i > 0)
        {
            dst[offset++] = word[wordOffset++];
            i--;
        }

        if (op == WordTransformType.UppercaseAll || op == WordTransformType.UppercaseFirst)
        {
            var uppercaseOffset = offset - len;
            if (op == WordTransformType.UppercaseFirst)
                len = 1;
            while (len > 0)
            {
                tmp = dst[uppercaseOffset] & 0xFF;
                if (tmp < 0xc0)
                {
                    if (tmp >= 'a' && tmp <= 'z')
                        dst[uppercaseOffset] ^= 32;
                    uppercaseOffset += 1;
                    len -= 1;
                }
                else if (tmp < 0xe0)
                {
                    dst[uppercaseOffset + 1] ^= 32;
                    uppercaseOffset += 2;
                    len -= 2;
                }
                else
                {
                    dst[uppercaseOffset + 2] ^= 5;
                    uppercaseOffset += 3;
                    len -= 3;
                }
            }
        }

        // Copy suffix.
        @string = transform.suffix;
        tmp = @string.Length;
        i = 0;
        while (i < tmp)
            dst[offset++] = @string[i++];
        return offset - dstOffset;
    }
}

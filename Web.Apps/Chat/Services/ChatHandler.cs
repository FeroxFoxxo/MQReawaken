using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using System.Security.Cryptography;
using System.Text;
using Web.Apps.Chat.Models;

namespace Web.Apps.Chat.Services;

public class ChatHandler : IService
{
    private readonly ChatConfig _chatConfig;
    private readonly EventSink _eventSink;

    public byte[] EncryptedWordList { get; private set; }

    public ChatHandler(EventSink eventSink, ChatConfig chatConfig)
    {
        _eventSink = eventSink;
        _chatConfig = chatConfig;
    }

    public void Initialize() => _eventSink.WorldLoad += GenerateChat;

    private void GenerateChat() => EncryptedWordList =
        Encrypt(string.Join("", _chatConfig.Words.Select(x => $"{x}{_chatConfig.TerminationCharacter}")));

    public byte[] Encrypt(string data)
    {
        var dataArray = Encoding.UTF8.GetBytes(data);

        var key = _chatConfig.CrispKey;
        var keyArray = new byte[key.Length / 2];

        for (var i = 0; i < key.Length; i += 2)
            keyArray[i / 2] = (byte)Convert.ToInt32(key.Substring(i, 2), 16);

        using var aes = Aes.Create();

        aes.KeySize = 128;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = keyArray;

        using var cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);

        cryptoStream.Write(dataArray, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        return ms.ToArray();
    }
}

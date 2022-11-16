using System.Security.Cryptography;
using System.Text;
using Server.Base.Accounts.Modals;

namespace Server.Base.Accounts.Helpers;

public class PasswordHasher
{
    private readonly byte[] _hashBuffer;
    private readonly SHA512 _sha512HashProvider;

    public PasswordHasher()
    {
        _sha512HashProvider = SHA512.Create();
        _hashBuffer = new byte[256];
    }

    public string HashSha512(string phrase)
    {
        var length = Encoding.ASCII.GetBytes(phrase, 0, phrase.Length > 256 ? 256 : phrase.Length, _hashBuffer, 0);
        var hashed = _sha512HashProvider.ComputeHash(_hashBuffer, 0, length);

        return BitConverter.ToString(hashed);
    }

    public bool CheckPassword(Account account, string plainPassword) =>
        account.Password == GetPassword(account.Username, plainPassword);

    public string GetPassword(string username, string password) =>
        HashSha512(username + password);
}

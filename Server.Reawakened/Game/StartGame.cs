using Server.Base.Core.Abstractions;
using System.Runtime.InteropServices;

namespace Server.Reawakened.Game;

public class StartGame : IService
{
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    private static string ShowDialog()
    {
        var ofn = new OpenFileName()
        {
            lpstrFilter = "Game Executable (*.exe)\0",
            lpstrFile = new string(new char[256]),
            lpstrFileTitle = new string(new char[64]),
            lpstrTitle = "Get Game Executable"
        };

        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;

        return GetOpenFileName(ref ofn) ? ofn.lpstrFile : string.Empty;
    }

    public void Initialize()
    {
    }
}

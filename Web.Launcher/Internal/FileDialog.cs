using System.Runtime.InteropServices;

namespace Web.Launcher.Internal;

internal class FileDialog
{
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    public static string GetFile(string title, string filter)
    {
        var ofn = new OpenFileName
        {
            lpstrFilter = filter,
            lpstrFile = new string(new char[256]),
            lpstrFileTitle = new string(new char[64]),
            lpstrTitle = title
        };

        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;

        return GetOpenFileName(ref ofn) ? ofn.lpstrFile : string.Empty;
    }
}

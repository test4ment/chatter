public static class UnicodeReader
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    static extern bool ReadConsoleW(IntPtr hConsoleInput, [Out] byte[]
       lpBuffer, uint nNumberOfCharsToRead, out uint lpNumberOfCharsRead,
       IntPtr lpReserved);

    public static IntPtr GetWin32InputHandle()
    {
        const int STD_INPUT_HANDLE = -10;
        IntPtr inHandle = GetStdHandle(STD_INPUT_HANDLE);
        return inHandle;
    }

    public static string ReadLine()
    {
        const int bufferSize = 1024;
        var buffer = new byte[bufferSize];

        uint charsRead;

        ReadConsoleW(GetWin32InputHandle(), buffer, bufferSize, out charsRead, (IntPtr)0);
        // -2 to remove ending \n\r
        int nc = ((int)charsRead - 2) * 2;
        
        var b = new byte[nc];
        
        for (var i = 0; i < nc; i++)
            try{
                b[i] = buffer[i];
            }
            catch(IndexOutOfRangeException){
                break;
            }
            
        var utf8enc = Encoding.UTF8;
        var unicodeenc = Encoding.Unicode;
        return utf8enc.GetString(Encoding.Convert(unicodeenc, utf8enc, b));
    }
}
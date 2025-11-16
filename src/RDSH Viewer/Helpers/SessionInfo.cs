using System.Runtime.InteropServices;

namespace RDSH_Viewer.Helpers
{
    /// <summary>
    /// RDP session data transfer object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SessionInfo
    {
        public int SessionID;
        [MarshalAs(UnmanagedType.LPStr)]
        public string pWinStationName;
        public SessionState State;
    }
}

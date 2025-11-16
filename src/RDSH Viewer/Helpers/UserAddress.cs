using System.Runtime.InteropServices;

namespace RDSH_Viewer.Helpers
{
    /// <summary>
    /// IP address data transfer object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UserAddress
    {
        public int AddressFamily;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Address;

        public string ToIP() => AddressFamily == 2 ? $"{Address[2]}.{Address[3]}.{Address[4]}.{Address[5]}" : string.Empty;
    }
}

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.DirectoryServices.AccountManagement;

namespace RDSH_Viewer.Helpers
{
    /// <summary>
    /// Implements logic for working with RDP sessions.
    /// </summary>
    public static class RdpHelper
    {
        /// <summary>
        /// Get RDP session data from RDSH.
        /// </summary>
        /// <param name="servers">RDS hosts list.</param>
        public static List<RdpSession> GetRdpSessions(List<string> servers)
        {
            var sessions = new List<RdpSession>();

            foreach (string server in servers)
            {
                var serverHandle = IntPtr.Zero;
                serverHandle = OpenServer(server);

                try
                {
                    var SessionInfoPtr = IntPtr.Zero;
                    var userPtr = IntPtr.Zero;
                    var clientNamePtr = IntPtr.Zero;
                    var clientAddressPtr = IntPtr.Zero;
                    int sessionCount = 0;
                    int retVal = WTSEnumerateSessions(serverHandle, 0, 1, ref SessionInfoPtr, ref sessionCount);
                    int dataSize = Marshal.SizeOf(typeof(SessionInfo));
                    int currentSession = (int)SessionInfoPtr;
                    uint bytes = 0;

                    if (retVal != 0)
                    {
                        for (int i = 0; i < sessionCount; i++)
                        {
                            var sessionInfo = (SessionInfo)Marshal.PtrToStructure((IntPtr)currentSession, typeof(SessionInfo));
                            currentSession += dataSize;

                            WTSQuerySessionInformation(serverHandle, sessionInfo.SessionID, RdpInfo.ClientAddress, out clientAddressPtr, out bytes);
                            WTSQuerySessionInformation(serverHandle, sessionInfo.SessionID, RdpInfo.ClientName, out clientNamePtr, out bytes);
                            WTSQuerySessionInformation(serverHandle, sessionInfo.SessionID, RdpInfo.UserName, out userPtr, out bytes);

                            if (Marshal.PtrToStringAnsi(userPtr).Length > 0)
                            {
                                var clientAddress = (UserAddress)Marshal.PtrToStructure(clientAddressPtr, typeof(UserAddress));
                                var clientIp = clientAddress.ToIP();
                                var computerName = Marshal.PtrToStringAnsi(clientNamePtr);
                                var userLogonName = Marshal.PtrToStringAnsi(userPtr);
                                var userDisplayName = GetUserDisplayName(userLogonName);
                                var rdpSession = new RdpSession(rdsHost: server, sessionId: sessionInfo.SessionID, userDisplayName: userDisplayName,
                                    userLogonName: userLogonName, sessionState: sessionInfo.State, workstation: computerName, ipAddress: clientIp);
                                sessions.Add(rdpSession);
                            }

                            WTSFreeMemory(clientAddressPtr);
                            WTSFreeMemory(clientNamePtr);
                            WTSFreeMemory(userPtr);
                        }

                        WTSFreeMemory(SessionInfoPtr);
                    }
                }

                catch
                {
                    // Do nothing.
                }

                finally
                {
                    CloseServer(serverHandle);
                }
            }

            return sessions;
        }

        private static string GetUserDisplayName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return string.Empty;
            }

            var principal = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain), userName);
            return principal?.DisplayName ?? string.Empty;
        }

        private static IntPtr OpenServer(string Name)
        {
            var server = WTSOpenServer(Name);
            return server;
        }

        private static void CloseServer(IntPtr ServerHandle) => WTSCloseServer(ServerHandle);

        [DllImport("wtsapi32.dll")]
        private static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll")]
        private static extern int WTSEnumerateSessions(IntPtr hServer, [MarshalAs(UnmanagedType.U4)] int Reserved, [MarshalAs(UnmanagedType.U4)] int Version, ref IntPtr ppSessionInfo, [MarshalAs(UnmanagedType.U4)] ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, RdpInfo rdpInfo, out IntPtr ppBuffer, out uint pBytesReturned);
    }
}

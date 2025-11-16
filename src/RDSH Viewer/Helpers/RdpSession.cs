namespace RDSH_Viewer.Helpers
{
    /// <summary>
    /// RDP session data transfer object.
    /// </summary>
    public class RdpSession
    {
        public RdpSession(string rdsHost, int sessionId, string userDisplayName, string userLogonName, SessionState sessionState, string workstation, string ipAddress)
        {
            RDSHost = rdsHost.ToLower();
            SessionId = sessionId;
            UserDisplayName = userDisplayName;
            UserLogonName = userLogonName;
            SessionState = sessionState;
            Workstation = workstation.ToLower();
            IpAddress = ipAddress;
        }

        /// <summary>
        /// Get remote desktop session host name.
        /// </summary>
        public string RDSHost { get; }

        /// <summary>
        /// Get session id.
        /// </summary>
        public int SessionId { get; }

        /// <summary>
        /// Get user display name.
        /// </summary>
        public string UserDisplayName { get; }

        /// <summary>
        /// Get user logon name.
        /// </summary>
        public string UserLogonName { get; }

        /// <summary>
        /// Get rdp session state.
        /// </summary>
        public SessionState SessionState { get; }

        /// <summary>
        /// Get user workstation name.
        /// </summary>
        public string Workstation { get; }

        /// <summary>
        /// Get user logged ip address.
        /// </summary>
        public string IpAddress { get; }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using RDSH_Viewer.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System;

namespace RDSH_Viewer.VM
{
    /// <summary>
    /// Implements the Views Model logic.
    /// </summary>
    public class ViewModel : ObservableObject
    {
        private bool openSettingsFlyout;
        private bool sessionActive = false;
        private bool showLoadingPanel = false;
        private int sessionIndex = -1;
        private int themeIndex = Properties.Settings.Default.ThemeIndex;
        private ObservableCollection<RdpSession> rdpSessions = new ObservableCollection<RdpSession>();
        private readonly ObservableCollection<string> rdpServers = new ObservableCollection<string>(Properties.Settings.Default.RdpServers?.Cast<string>() ?? Enumerable.Empty<string>());
        private readonly string[] themes = new string[46] { "Light.Amber", "Light.Blue", "Light.Brown", "Light.Cobalt", "Light.Crimson", "Light.Cyan",
            "Light.Emerald", "Light.Green", "Light.Indigo", "Light.Lime", "Light.Magenta", "Light.Mauve", "Light.Olive", "Light.Orange", "Light.Pink", "Light.Purple",
            "Light.Red", "Light.Sienna", "Light.Steel", "Light.Taupe", "Light.Teal", "Light.Violet", "Light.Yellow", "Dark.Amber", "Dark.Blue", "Dark.Brown",
            "Dark.Cobalt", "Dark.Crimson", "Dark.Cyan", "Dark.Emerald", "Dark.Green", "Dark.Indigo", "Dark.Lime", "Dark.Magenta", "Dark.Mauve", "Dark.Olive",
            "Dark.Orange", "Dark.Pink", "Dark.Purple", "Dark.Red", "Dark.Sienna", "Dark.Steel", "Dark.Taupe", "Dark.Teal", "Dark.Violet", "Dark.Yellow" };

        /// <summary>
        /// Get "Add RDP Server" button clicked.
        /// </summary>
        public IRelayCommand<FrameworkElement> Command_AddServerClicked { get; private set; }

        /// <summary>
        /// Get context menu "Copy" clicked.
        /// </summary>
        public IRelayCommand Command_ContextMenuCopyClicked { get; private set; }

        /// <summary>
        /// Get context menu "Connect" clicked.
        /// </summary>
        public IRelayCommand<bool> Command_ContextMenuConnectClicked { get; private set; }

        /// <summary>
        /// Get "View Sessions" button clicked.
        /// </summary>
        public IAsyncRelayCommand Command_GetSessionsClicked { get; private set; }

        /// <summary>
        /// Get GitHub hyperlink clicked.
        /// </summary>
        public IRelayCommand<string> Command_HyperlinkClicked { get; private set; }

        /// <summary>
        /// Get "Remove RDP Server" button clicked.
        /// </summary>
        public IRelayCommand<int> Command_RemoveServerClicked { get; private set; }

        /// <summary>
        /// Get "Settings" button clicked.
        /// </summary>
        public IRelayCommand Command_SettingsClicked { get; private set; }

        /// <summary>
        /// Get app theme.
        /// </summary>
        public string[] Themes => themes;

        /// <summary>
        /// Get or set selected app theme index.
        /// </summary>
        public int ThemeIndex
        {
            get => themeIndex;
            set
            {
                if (themeIndex != value)
                {
                    themeIndex = value;
                    Properties.Settings.Default.ThemeIndex = themeIndex;
                    _ = ThemeManager.Current.ChangeTheme(Application.Current, themes[themeIndex]);
                }
            }
        }

        /// <summary>
        /// Get selected session has active state.
        /// </summary>
        public bool SessionActive
        {
            get => sessionActive;
            private set => SetProperty(ref sessionActive, value);
        }

        /// <summary>
        /// Get or set selected RDP session index.
        /// </summary>
        public int SessionIndex
        {
            get => sessionIndex;
            set
            {
                if (sessionIndex != value)
                {
                    sessionIndex = value;
                    SessionActive = sessionIndex != -1 && RdpSessions[sessionIndex].SessionState == SessionState.Active;
                }
            }
        }

        /// <summary>
        /// Get RDSH sessions information.
        /// </summary>
        public ObservableCollection<RdpSession> RdpSessions
        {
            get => rdpSessions;
            private set => SetProperty(ref rdpSessions, value);
        }

        /// <summary>
        /// Get RDS hosts.
        /// </summary>
        public ObservableCollection<string> RdpServers => rdpServers;

        /// <summary>
        /// Get settings flyout open state.
        /// </summary>
        public bool OpenSettingsFlyout
        {
            get => openSettingsFlyout;
            private set => SetProperty(ref openSettingsFlyout, value);
        }

        /// <summary>
        /// Get loading panel visibility state.
        /// </summary>
        public bool ShowLoadingPanel
        {
            get => showLoadingPanel;
            private set => SetProperty(ref showLoadingPanel, value);
        }

        /// <summary>
        /// Execute View Model.
        /// </summary>
        public ViewModel Execute()
        {
            if (Properties.Settings.Default.RdpServers is null)
            {
                Properties.Settings.Default.RdpServers = new StringCollection();
            }

            if (RdpServers.Count > 0)
            {
                _ = Task.Run(async () => await GetSessionsClickedAsync());
            }
            else
            {
                OpenSettingsFlyout = true;
            }

            return this;
        }

        /// <summary>
        /// Initialize app relay command.
        /// </summary>
        public ViewModel InitializeCommand()
        {
            Command_AddServerClicked = new RelayCommand<FrameworkElement>(AddServerClicked);
            Command_ContextMenuConnectClicked = new RelayCommand<bool>(ContextMenuConnectClicked);
            Command_ContextMenuCopyClicked = new RelayCommand(ContextMenuCopyClicked);
            Command_GetSessionsClicked = new AsyncRelayCommand(GetSessionsClickedAsync);
            Command_HyperlinkClicked = new RelayCommand<string>(HyperlinkClicked);
            Command_RemoveServerClicked = new RelayCommand<int>(RemoveServerClicked);
            Command_SettingsClicked = new RelayCommand(SettingsClicked);
            return this;
        }

        /// <summary>
        /// Set app theme.
        /// </summary>
        /// <returns></returns>
        public ViewModel SetTheme()
        {
            _ = ThemeManager.Current.ChangeTheme(Application.Current, themes[themeIndex]);
            return this;
        }

        /// <summary>
        /// Sets app localization, English is default.
        /// </summary>
        public ViewModel SetLocalization()
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            var localizedUri = new Uri(currentCulture.Equals("RU", StringComparison.InvariantCultureIgnoreCase) ? "pack://application:,,,/Strings/ru-RU.xaml" : "pack://application:,,,/Strings/en-US.xaml", UriKind.Absolute);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = localizedUri });
            return this;
        }

        private void AddServerClicked(FrameworkElement element)
        {
            var textBox = element as TextBox;
            RdpServers.Add(textBox.Text);
            Properties.Settings.Default.RdpServers.Add(textBox.Text);
            textBox.Text = string.Empty;
        }

        private void ContextMenuConnectClicked(bool viewOnly = true)
        {
            var session = RdpSessions[SessionIndex];
            var viewOnlyArgs = $"/v:{session.RDSHost} /shadow:{session.SessionId}";
            var controlArgs = $"/v:{session.RDSHost} /shadow:{session.SessionId} /control";
            _ = Process.Start("mstsc.exe", viewOnly ? viewOnlyArgs : controlArgs);
        }

        private void ContextMenuCopyClicked()
        {
            var session = RdpSessions[SessionIndex];
            Clipboard.SetText($"{session.RDSHost}, {session.SessionId}, {session.UserDisplayName}, {session.UserLogonName}, {session.SessionState}, {session.Workstation}, {session.IpAddress}");
        }

        private async Task GetSessionsClickedAsync()
        {
            if (OpenSettingsFlyout)
            {
                SettingsClicked();
            }

            RdpSessions.Clear();
            ShowLoadingPanel = true;
            var sessions = await Task.Run(() => RdpHelper.GetRdpSessions(RdpServers.ToList()));
            RdpSessions = new ObservableCollection<RdpSession>(sessions);
            ShowLoadingPanel = false;
        }

        private void RemoveServerClicked(int index)
        {
            RdpServers.RemoveAt(index);
            Properties.Settings.Default.RdpServers.RemoveAt(index);
        }

        private void HyperlinkClicked(string link) => Process.Start(link);

        private void SettingsClicked() => OpenSettingsFlyout = !OpenSettingsFlyout;
    }
}

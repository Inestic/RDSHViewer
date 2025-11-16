using MahApps.Metro.Controls;
using RDSH_Viewer.VM;
using System.Windows.Controls;

namespace RDSH_Viewer
{
    /// <summary>
    /// Implements the MainWindow.xaml logic.
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel()
                .SetLocalization()
                    .SetTheme()
                        .InitializeCommand()
                            .Execute();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Properties.Settings.Default.Save();
        private void ListView_Sessions_ContextMenuOpening(object sender, ContextMenuEventArgs e) => e.Handled = (e.Source as ListView).SelectedIndex == -1;
    }
}

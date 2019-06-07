using System.Windows;

namespace VaultExportUI
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private VaultConfig config = new VaultConfig();

        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            config.BaseRepositories = baseRepositories.Text;
            config.Vaultuser = vaultuser.Text;
            config.Vaultpass = vaultpass.Text;
            config.Vaultserveraddr = vaultserveraddr.Text;
            config.Vaultserver = vaultserver.Text;
            config.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vaultuser.Text = config.Vaultuser;
            vaultpass.Text = config.Vaultpass;
            vaultserveraddr.Text = config.Vaultserveraddr;
            vaultserver.Text = config.Vaultserver;
            baseRepositories.Text = config.BaseRepositories;
        }
    }
}

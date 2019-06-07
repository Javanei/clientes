using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace VaultExportUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> desenhosSelecionados = new ObservableCollection<string>();
        private VaultConfig config = new VaultConfig();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lsDesenhos.ItemsSource = desenhosSelecionados;
        }

        private void addDesenho()
        {
            if (tbDesenho.Text != null
                && tbDesenho.Text.Trim().Length > 0
                && !desenhosSelecionados.Contains(tbDesenho.Text.Trim()))
            {
                desenhosSelecionados.Add(tbDesenho.Text.Trim());
            }
            tbDesenho.Text = "";
            tbDesenho.Focus();
        }

        private void BtAddDesenho_Click(object sender, RoutedEventArgs e)
        {
            addDesenho();
        }

        private void BtLimparLista_Click(object sender, RoutedEventArgs e)
        {
            desenhosSelecionados.Clear();
        }

        private void BtExecute_Click(object sender, RoutedEventArgs e)
        {
            if (desenhosSelecionados.Count > 0)
            {
                string executable = "VaultExport.exe -vaultuser=" + config.Vaultuser
                    + " -vaultpass=\"" + config.Vaultpass + "\""
                    + " -vaultserver=\"" + config.Vaultserver + "\""
                    + " -vaultserveraddr=\"" + config.Vaultserveraddr + "\""
                    ;
                foreach (string s in desenhosSelecionados)
                {
                    executable += " \"" + s + "\"";
                }
                System.Console.WriteLine(executable);
            }
        }

        private void TbDesenho_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.Return)
            {
                addDesenho();
            }
        }

        private void BtConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            configWindow.ShowDialog();
        }
    }
}

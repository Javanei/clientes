using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace VaultExportUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> desenhosSelecionados = new ObservableCollection<string>();
        private VaultConfig config = new VaultConfig();

        private const string EXPORT_LOG_FILE = "exportlog.txt";

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
                string execParams = "-vaultuser=" + config.Vaultuser
                    + " -vaultpass=\"" + config.Vaultpass + "\""
                    + " -vaultserver=\"" + config.Vaultserver + "\""
                    + " -vaultserveraddr=\"" + config.Vaultserveraddr + "\""
                    + " -exportfile=\"" + config.Exportfile + "\""
                    ;
                foreach (string s in desenhosSelecionados)
                {
                    execParams += " \"" + s + "\"";
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo("VaultExport.exe ", execParams)
                {
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    //RedirectStandardOutput = false,
                    //RedirectStandardError = false,
                    //UseShellExecute = true
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                Process process = Process.Start(processStartInfo);
                bool result = process.WaitForExit(60000);
                StreamWriter writer = File.CreateText(EXPORT_LOG_FILE);
                writer.WriteLine(process.StandardOutput.ReadToEnd());
                writer.WriteLine(process.StandardError.ReadToEnd());
                writer.Flush();
                writer.Close();

                ProcessStartInfo logfile = new ProcessStartInfo(EXPORT_LOG_FILE)
                {
                    Verb = "Open",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                };
                Process.Start(logfile);

                if (result)
                {
                    ProcessStartInfo sheet = new ProcessStartInfo(config.Exportfile)
                    {
                        Verb = "Open",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Normal,
                    };
                    Process.Start(sheet);
                }
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

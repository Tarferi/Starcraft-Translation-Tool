using QChkUI;
using System.Windows;

namespace TranslatorUI {


    public partial class AboutWindow : Window {

        public RelLanguage RelLanguage { get; set; }

        public AboutWindow(RelLanguage RelLanguage) {
            this.RelLanguage = RelLanguage;
            InitializeComponent();
            txtVersion.Text = WpfApplication1.MainWindow.Version;
            checkAllowAutoUpdate.IsChecked = History.storage.autoUpdate;
        }

        private void checkAllowAutoUpdate_Checked(object sender, RoutedEventArgs e) {
            History.storage.autoUpdate = true;
            History.storage.push();
        }

        private void checkAllowAutoUpdate_Unchecked(object sender, RoutedEventArgs e) {
            History.storage.autoUpdate = false;
            History.storage.push();
        }

        private void txtLbl_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Secret:", "Input seret", "", 0, 0);
            byte[] res = UpdateWindow.readHTTP(@"https://rion.cz/epd/smt/update.php?rv=4&sk=" + input.ToString());
            if (res != null) {
                string r = UpdateWindow.toString(res);
                if(r == "OK") {
                    History.storage.secretKey = input;
                    History.storage.push();
                    MessageBox.Show("Key accepted", "Secret", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            History.storage.secretKey = "";
            History.storage.push();
            MessageBox.Show("Key rejected", "Secret", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    }
}

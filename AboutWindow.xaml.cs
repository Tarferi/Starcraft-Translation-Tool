using QChkUI;
using System.Windows;

namespace TranslatorUI {


    public partial class AboutWindow : Window {

        public RelLanguage RelLanguage { get; set; }

        public AboutWindow(RelLanguage RelLanguage) {
            this.RelLanguage = RelLanguage;
            InitializeComponent();
            txtVersion.Text = WpfApplication1.MainWindow.Version;
            checkAllowAutoUpdate.IsChecked = History.autoUpdate;
        }

        private void checkAllowAutoUpdate_Checked(object sender, RoutedEventArgs e) {
            History.autoUpdate = true;
            History.save();
        }

        private void checkAllowAutoUpdate_Unchecked(object sender, RoutedEventArgs e) {
            History.autoUpdate = false;
            History.save();
        }
    }
}

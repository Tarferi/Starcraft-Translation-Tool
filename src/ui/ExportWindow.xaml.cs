using Microsoft.Win32;
using System;
using System.Windows;
using TranslatorData;

namespace TranslatorUI {

    public partial class ExportWindow : Window {

        public dynamic RelLanguage { get { return MainWindow.GetLanguage(); } }

        private Action<string, string, bool, bool> callback;

        public ExportWindow(String[] strs, Action<string, string, bool, bool> callback) {
            InitializeComponent();
            this.DataContext = this;
            this.callback = callback;
            foreach(String str in strs) {
                cmbDefs.Items.Add(str);
            }
            checkEscapeColors.IsChecked = History.storage.exportColorCodes;
            checkEscapeNewLines.IsChecked = History.storage.exportEscapedLineBreaks;
            cmbDefs.SelectedIndex = 0;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            String copyOf = cmbDefs.SelectedItem == null ? null : cmbDefs.SelectedItem.ToString();
            if (copyOf != null) {
                SaveFileDialog openFileDialog = new SaveFileDialog();
                String type = RelLanguage.ExcelFile + " (*.xlsx)|*.xlsx";
                openFileDialog.Filter = type;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                bool? b = openFileDialog.ShowDialog();
                if (b == true) {
                    String filePath = openFileDialog.FileName;
                    History.storage.exportColorCodes = checkEscapeColors.IsChecked == true;
                    History.storage.exportEscapedLineBreaks = checkEscapeNewLines.IsChecked == true;
                    History.storage.push();
                    callback(copyOf, filePath, checkEscapeColors.IsChecked == true, checkEscapeNewLines.IsChecked == true);
                    this.Close();
                }
            }
        }
    }
}

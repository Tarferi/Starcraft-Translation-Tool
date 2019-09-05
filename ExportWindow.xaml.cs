using Microsoft.Win32;
using System;
using System.Windows;

namespace TranslatorUI {
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window {

        public RelLanguage RelLanguage { get; set; }

        private Action<string, string> callback;

        public ExportWindow(RelLanguage RelLanguage, String[] strs, Action<string, string> callback) {
            this.RelLanguage = RelLanguage;
            InitializeComponent();
            this.DataContext = this;
            this.callback = callback;
            foreach(String str in strs) {
                cmbDefs.Items.Add(str);
            }
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
                    callback(copyOf, filePath);
                    this.Close();
                }
            }
        }
    }
}

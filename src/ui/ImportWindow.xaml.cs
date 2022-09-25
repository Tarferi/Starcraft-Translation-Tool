using OfficeOpenXml;
using System;
using System.IO;
using System.Windows;

namespace TranslatorUI {
    public partial class ImportWindow : Window {

        public dynamic RelLanguage { get { return MainWindow.GetLanguage(); } }

        private String tryLoadingSheetName(String filePath) {
            try {
                FileInfo existingFile = new FileInfo(filePath);
                using (ExcelPackage package = new ExcelPackage(existingFile)) {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    return worksheet.Name;
                }
                } catch (Exception) {
            }
            return "";
        }

        public bool isGood { get { return filePath != null; } }

        public ImportWindow(Action<String, String> callback, String[] languages) {
            InitializeComponent();
            this.DataContext = this;
            this.callback = callback;
            this.languages = languages;
            filePath = MainWindow.getFile(RelLanguage.WinImport, new String[] { "xlsx" }, new String[] { RelLanguage.ExcelFile });
            if(filePath == null) {
                this.Close();
                return;
            }
            if(filePath == "") {
                this.Close();
                return;
            }
            if (!System.IO.File.Exists(filePath)) {
                this.Close();
                return;
            }
            txtLng.Text = tryLoadingSheetName(filePath);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            String language = txtLng.Text.Trim();
            if (language.Length > 0) {
                foreach (String lang in languages) {
                    if (lang == language) {
                        if (MessageBox.Show(String.Format(RelLanguage.LblConfirmOverWrite, language), RelLanguage.WinImport, MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
                            return;
                        }
                    }
                }
                callback(txtLng.Text, filePath);
                this.Close();
            }
        }

        private Action<String, String> callback;
        private String[] languages;
        private String filePath;
    }
}

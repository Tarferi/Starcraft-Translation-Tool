using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TranslatorUI {


    public partial class ImportWindow : Window {

        public RelLanguage RelLanguage { get; set; }

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

        public ImportWindow(RelLanguage RelLanguage, Action<String, String> callback, String[] languages) {
            this.RelLanguage = RelLanguage;
            InitializeComponent();
            this.DataContext = this;
            this.callback = callback;
            this.languages = languages;
            String filePath = WpfApplication1.MainWindow.getFile(RelLanguage.WinImport, new String[] { "xlsx" }, new String[] { RelLanguage.ExcelFile });
            if(filePath == null) {
                this.Close();
            }
            if(filePath == "") {
                this.Close();
            }
            if (!System.IO.File.Exists(filePath)) {
                this.Close();
            }
            txtLng.Text = tryLoadingSheetName(filePath);
            this.filePath = filePath;
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
                    callback(txtLng.Text, filePath);
                    this.Close();
                }
            }
        }

        private Action<String, String> callback;
        private String[] languages;
        private String filePath;
    }
}

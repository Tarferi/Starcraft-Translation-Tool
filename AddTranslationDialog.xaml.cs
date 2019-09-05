using System;
using System.Windows;

namespace TranslatorUI {

    public partial class AddTranslationDialog : Window {

        public RelLanguage RelLanguage { get; set; }

        public AddTranslationDialog(RelLanguage RelLanguage, String[] existingTranslations, Action<String, String> callback) {
            this.RelLanguage = RelLanguage;
            InitializeComponent();
            cmbDefs.Items.Clear();
            cmbDefs.Items.Add(RelLanguage.LblDefault);
            foreach (String str in existingTranslations) {
                cmbDefs.Items.Add(str);
            }
            cmbDefs.SelectedIndex = 0;
            this.callback = callback;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            String language = txtLng.Text.Trim();
            String copyOf = cmbDefs.SelectedItem == null ? null : cmbDefs.SelectedItem.ToString();
            if(language != RelLanguage.LblDefault) {
                if(copyOf == RelLanguage.LblDefault) {
                    copyOf = null;
                }

                if (language.Length > 0) {
                    callback(txtLng.Text, copyOf);
                    this.Close();
                }
            }
        }

        private Action<String, String> callback;
    }
}

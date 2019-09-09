using System;
using System.Windows;

namespace TranslatorUI {

    public partial class DeleteTranslationDialog : Window {

        public dynamic RelLanguage { get { return WpfApplication1.MainWindow.GetLanguage(); } }

        public DeleteTranslationDialog(String[] existingTranslations, Action<String> callback) {
            InitializeComponent();
            cmbDefs.Items.Clear();
            foreach (String str in existingTranslations) {
                cmbDefs.Items.Add(str);
            }
            this.callback = callback;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            String copyOf = cmbDefs.SelectedItem == null ? null : cmbDefs.SelectedItem.ToString();
            if(copyOf != null) {
                if (MessageBox.Show(String.Format(RelLanguage.LblConfirmDeletion, copyOf), RelLanguage.WinConfirmDeletion, MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    callback(copyOf);
                    this.Close();
                }
            }
        }

        private Action<String> callback;
    }
}

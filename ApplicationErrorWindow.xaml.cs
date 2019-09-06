using System;
using System.Windows;

namespace TranslatorUI {

    public partial class ApplicationErrorWindow : Window {

        RelLanguage RelLanguage { get; set; }

        public ApplicationErrorWindow(RelLanguage RelLanguage, Exception exception) {
            this.RelLanguage = RelLanguage;
            InitializeComponent();
            this.DataContext = this;

            // Set labels manually
            lblTitle.Text = RelLanguage.LblErrorUncaughtException;
            lblExceptionName.Text = exception.GetType().ToString();
            lblDetails.Text = RelLanguage.LblErrorCallstackDetails;
            lblExit.Text = RelLanguage.LblErrorCrash;
            btnOk.Content = RelLanguage.BtnExit;
            txtErrText.Text = exception.ToString();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}

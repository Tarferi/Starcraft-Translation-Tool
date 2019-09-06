using System.Windows;
using System.Windows.Threading;
using TranslatorUI;

namespace WpfApplication1 {
    
    public partial class App : Application {

        public App() : base() {
            this.DispatcherUnhandledException += aApp_DispatcherUnhandledException;
        }

        private static RelLanguage RelLanguage { get { return WpfApplication1.MainWindow.GetLanguage(); } }

        void aApp_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            e.Handled = true;
            ApplicationErrorWindow errDialog = new ApplicationErrorWindow(RelLanguage, e.Exception);
            errDialog.ShowDialog();
            Shutdown();
        }
    }
}

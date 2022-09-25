using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using TranslatorData;

namespace TranslatorUI {
    public partial class App : Application {

        public App() : base() {
            this.DispatcherUnhandledException += aApp_DispatcherUnhandledException;
#if !DEBUG
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveResources);
#endif
        }

        private static RelLanguage RelLanguage { get { return TranslatorUI.MainWindow.GetLanguage(); } }

        void aApp_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            e.Handled = true;
            ApplicationErrorWindow errDialog = new ApplicationErrorWindow(RelLanguage, e.Exception);
            errDialog.ShowDialog();
            Shutdown();
        }
        
#if !DEBUG
        Assembly ResolveResources(object sender, ResolveEventArgs args) {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            if (dllName == "EPPlus") {
                return Assembly.Load(TranslatorUI.Properties.Resources.EPPlus);
            } else {
                return null;
            }
        }
#endif
    }
}

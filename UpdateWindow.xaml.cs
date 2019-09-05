using QChkUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TranslatorUI {

    public partial class UpdateWindow : Window {

        public RelLanguage RelLanguage { get; set; }

        private static AsyncWorker aw = new AsyncWorker();

        private enum State {
            Started,
            CheckingRemoteVersion,
            CurrentVersion,
            NeedsUpdate,
            Updating,
            AwaitingRestart,
            FailedToResolveRemoteVersion,
            FailedUpdating
        }

        private State state = State.Started;

        private void setState(State state) {
            this.state = state;
            disableEverything();
            btnGet.Visibility = Visibility.Collapsed;
            progress.Visibility = Visibility.Collapsed;
            textBlock2_Copy.Visibility = Visibility.Collapsed;
            txtRemoteVersion.Foreground = Brushes.Black;
            if (state == State.Started) {
                txtLocalVersion.IsEnabled = true;
            } else if (state == State.CheckingRemoteVersion) {
                txtLocalVersion.IsEnabled = true;
                progress.Visibility = Visibility.Visible;
                textBlock2_Copy.Visibility = Visibility.Visible;
            } else if (state == State.CurrentVersion) {
                txtLocalVersion.IsEnabled = true;
                txtRemoteVersion.IsEnabled = true;
            } else if (state == State.NeedsUpdate) {
                txtLocalVersion.IsEnabled = true;
                txtRemoteVersion.IsEnabled = true;
                btnGet.Visibility = Visibility.Visible;
                btnGet.Content = RelLanguage.BtnUpdate;
                btnGet.IsEnabled = true;
            } else if (state == State.Updating) {
                txtLocalVersion.IsEnabled = true;
                txtRemoteVersion.IsEnabled = true;
                progress.Visibility = Visibility.Visible;
                textBlock2_Copy.Visibility = Visibility.Visible;
            } else if (state == State.AwaitingRestart) {
                txtLocalVersion.IsEnabled = true;
                txtRemoteVersion.IsEnabled = true;
                btnGet.Visibility = Visibility.Visible;
                btnGet.Content = RelLanguage.BtnRestart;
                btnGet.IsEnabled = true;
            } else if(state == State.FailedToResolveRemoteVersion) {
                txtLocalVersion.IsEnabled = true;
                txtRemoteVersion.IsEnabled = true;
                txtRemoteVersion.Foreground = Brushes.Red;
                txtRemoteVersion.Text = RelLanguage.ErrorUpdateGetRemoteVersion;
            } else if (state == State.FailedUpdating) {
                setState(State.NeedsUpdate);
            }
        }

        private void disableEverything() {
            txtLocalVersion.IsEnabled = false;
            txtRemoteVersion.IsEnabled = false;
            btnGet.IsEnabled = false;
            progress.IsEnabled = false;
            txtLocalVersion.Text = WpfApplication1.MainWindow.Version;
        }

        public UpdateWindow(RelLanguage RelLanguage) {
            this.RelLanguage = RelLanguage;
            InitializeComponent();
            setState(State.Started);
            checkRemoteVersion();
        }

        private static byte[] ReadAllBytes(BinaryReader reader) {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream()) {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) {
                    ms.Write(buffer, 0, count);
                }
                return ms.ToArray();
            }
        }

        public static byte[] readHTTP(String url) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (BinaryReader reader = new BinaryReader(stream)) {
                            return ReadAllBytes(reader);
                        }
                    }
                }
            } catch (Exception) {
                return null;
            }

        }

        public static byte[] readHTTP(String url, String fileName, String fileBase64Contents) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                byte[] data = Encoding.ASCII.GetBytes(fileName + "=" + System.Net.WebUtility.UrlEncode(fileBase64Contents));
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (Stream stream = request.GetRequestStream()) {
                    stream.Write(data, 0, data.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    using (Stream stream = response.GetResponseStream()) {
                        using (BinaryReader reader = new BinaryReader(stream)) {
                            return ReadAllBytes(reader);
                        }
                    }
                }
            } catch (Exception) {
                return null;
            }
        }

        private static object getRemoteVersion(object input) {
            byte[] htmlB = readHTTP(@"https://rion.cz/epd/smt/update.php?rv=1&ver=" + input.ToString());
            if (htmlB == null) {
                return null;
            }
            String html = toString(htmlB);
        
            if (!html.StartsWith("OK:")) {
                return null;
            }
            return html.Substring(3);
        }

        private void setRemoteVersion(object input) {
            if (input == null) { // Error
                MessageBox.Show(RelLanguage.ErrorUpdateGetRemoteVersion, RelLanguage.WinUpdateTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                setState(State.FailedToResolveRemoteVersion);
            } else {
                String str = (String)input;
                txtRemoteVersion.Text = str;
                setState(txtRemoteVersion.Text == txtLocalVersion.Text ? State.CurrentVersion : State.NeedsUpdate);
            }
        }

        private void checkRemoteVersion() {
            setState(State.CheckingRemoteVersion);
            aw.addJob(getRemoteVersion, txtLocalVersion.Text, setRemoteVersion);
        }

        public static String toString(byte[] data) {
            StringBuilder sb = new StringBuilder();
            foreach(byte d in data) {
                sb.Append((char)d);
            }
            return sb.ToString();
        }

        private object downloadRemoteVersion(object o) {

            // Get MD5 of result
            byte[] checksumB = readHTTP(@"https://rion.cz/epd/smt/update.php?rv=3");
            if (checksumB == null) {
                return null;
            }
            String checksum = toString(checksumB);
            if (!checksum.StartsWith("OK:")) {
                return null;
            }
            checksum = checksum.Substring(3).ToLower();

            // Download result
            byte[] rawData = readHTTP(@"https://rion.cz/epd/smt/update.php?rv=2");
            if (rawData == null) {
                return null;
            }

            // Verify result
            String md5Hash;
            using (var md5 = MD5.Create()) {
                byte[] hash = md5.ComputeHash(rawData);
                md5Hash = BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }

            if(checksum != md5Hash) {
                return null;
            }

            return rawData;
        }

        private byte[] rawFile = null;

        private void finishedDownloadingRemoteVersion(object data) {
            if (data == null) {
                MessageBox.Show(RelLanguage.ErrorUpdateInvalidDownload, RelLanguage.WinUpdateTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                setState(State.FailedUpdating);
            }
            this.rawFile = (byte[])data;
            setState(State.AwaitingRestart);
        }

        private bool writeFile(String path, byte[] data) {
            try {
                if (File.Exists(path)) {
                    File.Delete(path);
                }
                File.WriteAllBytes(path, data);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private bool writeTempBatch(String batchFile, String oldFile, String newFile) {
            String contents = ":begin\r\nmove /y \"{0}\" \"{1}\"\r\nIF ERRORLEVEL 1 GOTO begin\r\ndel /q \"{2}\" & \"{1}\" & exit";
            contents = String.Format(contents, newFile, oldFile, batchFile);
            return writeFile(batchFile, ASCIIEncoding.ASCII.GetBytes(contents));
        }

        private void restart() {

            // Extract new version
            string tmpNewName = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "translator_newVer.tmp");
            if (!writeFile(tmpNewName, rawFile)) {
                MessageBox.Show(RelLanguage.ErrorFailedToRunUpdator, RelLanguage.WinUpdateTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                setState(State.FailedUpdating);
            }

            string currentExeFile = getCurrentFileName();

            string tmpBatchFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "translator_newVer_updator.bat");
            if (!writeTempBatch(tmpBatchFile, currentExeFile, tmpNewName)) {
                MessageBox.Show(RelLanguage.ErrorFailedToRunUpdator, RelLanguage.WinUpdateTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                setState(State.FailedUpdating);
            }
           
            // Execute updator
            try {
                using (Process myProcess = new Process()) {
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.FileName = "cmd";
                    myProcess.StartInfo.Arguments = "/c \"" + tmpBatchFile + "\"";
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.Start();
                }
            } catch (Exception) {
                MessageBox.Show(RelLanguage.ErrorFailedToRunUpdator, RelLanguage.WinUpdateTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                setState(State.FailedUpdating);
            }
            Application.Current.Shutdown();
        }

        private void beginUpdate() {
            setState(State.Updating);
            aw.addJob(downloadRemoteVersion, null, finishedDownloadingRemoteVersion);
        }

        private void btnGet_Click(object sender, RoutedEventArgs e) {
            if (state == State.AwaitingRestart) { // Restart
                restart();
            } else if(state == State.NeedsUpdate) { // Update
                beginUpdate();
            }
        }

        private static object asyncHasUpdate(object o) {
            String localVer = (String)o;
            object remoteVer = getRemoteVersion(localVer);
            if(remoteVer == null) {
                return false;
            }
            return ((String)remoteVer) != localVer;
        }

        private static void asyncHasUpdateResp(WpfApplication1.MainWindow instance, bool hasUpdate) {
            if (hasUpdate) {
                instance.asyncFoundUpdate();
            }
        }

        public static void checkForAutoUpdatesOnBackground(WpfApplication1.MainWindow instance) {
            long lastCheckedTime = History.storage.lastCheckForUpdate;
            long now = (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
            if (lastCheckedTime != 0) {
                long second = 1;
                long minute = 60 * second;
                long hour = 60 * minute;
                long day = 24 * hour;
                long diff = now - lastCheckedTime;
                if (diff < day) {
                    return;
                }
            }
            History.storage.lastCheckForUpdate = now;
            History.storage.push();
            aw.addJob(asyncHasUpdate, WpfApplication1.MainWindow.Version, (object o) => { asyncHasUpdateResp(instance, (bool)o); });
        }

        private static String readFileAsBase64(String path) {
            try {
                byte[] bytes = File.ReadAllBytes(path);
                return System.Convert.ToBase64String(bytes);
            } catch (Exception) {
            }
            return null;
        }
    
        private static String getCurrentFileName() {
            String fname = Process.GetCurrentProcess().MainModule.FileName;
            if (fname.EndsWith(".vshost.exe")) {
                fname = fname.Substring(0, fname.Length - ".vshost.exe".Length) + ".exe";
            }
            return fname;
        }

        private void txtLocalVersion_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            String key = History.storage.secretKey;
            if(key != null) {
                if(key.Length > 0) {
                    String rawData = readFileAsBase64(getCurrentFileName());
                    if (rawData == null) {
                        MessageBox.Show("Failed to read local file", "Remote update", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    byte[] res = UpdateWindow.readHTTP(@"https://rion.cz/epd/smt/update.php?rv=5&sk=" + key + "&nv=" + txtLocalVersion.Text, "bin", rawData);
                    if (res != null) {
                        string r = UpdateWindow.toString(res);
                        if (r == "OK") {
                            MessageBox.Show("Updated", "Remote update", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    MessageBox.Show("Key rejected", "Remote update", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}

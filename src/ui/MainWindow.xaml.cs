using Microsoft.Win32;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls.Primitives;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using static TranslatorData.StarcraftColors;
using TranslatorData;
using Settings = TranslatorData.Settings;
using TranslatorUI.Properties;
#if DEBUG_STR_REMAP
using System.Diagnostics;
#endif

namespace TranslatorUI {

    enum AppState {
        START,
        PICK_MAP,
        LOADING_MAP,
        READY,
        PROCESSING
    }

    public partial class MainWindow : Window {

        public static readonly string Version = Assembly.GetEntryAssembly().GetName().Version.Major.ToString() + "." + Assembly.GetEntryAssembly().GetName().Version.Minor.ToString() + "." + Assembly.GetEntryAssembly().GetName().Version.Build.ToString() + "." + Assembly.GetEntryAssembly().GetName().Version.Revision.ToString() + " BETA";

        private static dynamic __lng;
        private StarcraftColors colors;

        public static dynamic GetLanguage() {
            return __lng;
        }

        public dynamic RelLanguage { get { return __lng; } set { __lng = value; } }

        public class NamedEncoding {
            public readonly Encoding Encoding;
            public NamedEncoding(Encoding enc) {
                this.Encoding = enc;
            }
            public override string ToString() {
                return this.Encoding.EncodingName;
            }

            private static List<NamedEncoding> encodingList;
            public static List<NamedEncoding> EncodingList {
                get {
                    if (encodingList == null) {
                        encodingList = new List<NamedEncoding>();
                        foreach (EncodingInfo info in Encoding.GetEncodings()) {
                            encodingList.Add(new NamedEncoding(info.GetEncoding()));
                        }
                    }
                    return encodingList;
                }
            }
            public static NamedEncoding GetForName(string name) {
                foreach(NamedEncoding e in EncodingList) {
                    if(e.Encoding.EncodingName == name) {
                        return e;
                    }
                }
                try {
                    Encoding enc = Encoding.GetEncoding(name);
                    if (enc != null) {
                        foreach (NamedEncoding e in EncodingList) {
                            if (e.Encoding == enc) {
                                return e;
                            }
                        }
                    }
                } catch (Exception) { }
                throw new Exception("Unknown encoding: " + name);
            }
        }

        public List<NamedEncoding> EncodingList { get => NamedEncoding.EncodingList; }

        public NamedEncoding InputEncoding {
            get {
                if (this.settings == null) {
                    return NamedEncoding.GetForName(Settings.defaultEncoding.EncodingName);
                } else {
                    return NamedEncoding.GetForName(this.settings.originalEncoding.EncodingName);
                }
            }
            set {
                if (this.settings != null && value != null) {
                    this.settings.originalEncoding = value.Encoding;
                }
            }
        }
        
        public NamedEncoding OutputEncoding {
            get {
                if (this.settings == null) {
                    return null;
                } else {
                    int index = comboRunLanguage.SelectedIndex;
                    if (index < 0) {
                        return null;
                    }
                    return NamedEncoding.GetForName(this.settings.encodings[index].EncodingName);
                }
            }
            set {
                if (this.settings != null && value != null) {
                    int index = comboRunLanguage.SelectedIndex;
                    if (index >= 0) {
                        this.settings.encodings[index] = value.Encoding;
                    }
                }
            }
        }
        

        private LanguageManager lngs;

        public MainWindow() {
            lngs = new LanguageManager();
            if (!lngs.Valid) {
                showErrorMessageBox("Critical error", "Failed to load language manager.");
                return;
            }
            RelLanguage = lngs.DefaultLanguage;

            InitializeComponent();

            setLanguage(lngs.DefaultLanguage);
            populateLanguages();

            tblTrans.EnableColumnVirtualization = true;
            tblTrans.EnableRowVirtualization = true;

            setState(AppState.START);
            History.open(txtSet.Name, txtSet);
            History.open(txtInMap.Name, txtInMap);
            History.open(txtOutMap.Name, txtOutMap);

            tblTrans.ItemsSource = Values;
            tblTrans.AutoGenerateColumns = false;

            if (txtSet.Items.Count > 0) {
                txtSet.SelectedIndex = 0;
            }
            support = SupportType.Unsupported;
            initCombos();
            UpdateWindow.checkForAutoUpdatesOnBackground(this);

            colors = StarcraftColors.load();
            if (colors == null) {
                showErrorMessageBox(RelLanguage.ResColorErrTitle, RelLanguage.ResColorErr);
            }
        }

        private void populateLanguages() {
            String preferredLanguage = History.storage.language;
            RelLanguage preferredLang = null;
            RoutedEventHandler preferredLangItm = null;

            try {
                List<MenuItem> allItems = new List<MenuItem>();
                foreach (RelLanguage lng in lngs.Languages) {
                    MenuItem itm = new MenuItem();
                    itm.IsCheckable = true;
                    itm.IsChecked = lng == lngs.DefaultLanguage;
                    itm.Header = lng.__LanguageName;
                    RoutedEventHandler clickHandle = (object sender, RoutedEventArgs e) => {
                        foreach (MenuItem itmm in allItems) {
                            itmm.IsChecked = false;
                        }
                        itm.IsChecked = true;
                        setLanguage(lng);
                        History.storage.language = lng.__LanguageName;
                        History.storage.push();
                    };
                    itm.Click += clickHandle;
                    allItems.Add(itm);
                    menuLng.Items.Add(itm);
                    if (lng.__LanguageName == preferredLanguage && preferredLanguage.Length > 0) {
                        preferredLang = lng;
                        preferredLangItm = clickHandle;
                    }
                }
                if (preferredLang != null) {
                    preferredLangItm(null, null);
                }
            } catch (Exception) { }
        }

        public void asyncFoundUpdate() {
            new UpdateWindow(RelLanguage).ShowDialog();
        }

        private void setLanguage(RelLanguage lng) {
            RelLanguage = lng;
            setState(state);
            this.DataContext = null;
            this.DataContext = this;
        }

        private void initCombos() {
            for (int i = 1; i < 13; i++) {
                cmbDeathsP.Items.Add(String.Format(RelLanguage.LblPlayer, i));
            }
            cmbDeathsP.SelectedIndex = 0;

            for (int i = 1; i < 256; i++) {
                cmbSwitches.Items.Add(String.Format(RelLanguage.LblSwitchName, i));
            }
            cmbSwitches.SelectedIndex = 0;

            for (int i = 0; i < TranslateString.unitNames.Length; i++) {
                cmbDeathsU.Items.Add(TranslateString.unitNames[i]);
            }
            cmbDeathsU.SelectedIndex = 0;
        }

        public bool showConfirmWarning(String title, String contents) {
            return MessageBox.Show(contents, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        public void showErrorMessageBox(String title, String contents) {
            MessageBox.Show(contents, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private AppState state = AppState.START;
        private TranslatorData.Settings settings;
        private AsyncWorker async = new AsyncWorker();
        private SupportType _support = SupportType.Unsupported;
        private SupportType support {
            get { return _support; }
            set {
                _support = value;
                grdCheckCond.Visibility = value == SupportType.EUDEditor ? Visibility.Visible : Visibility.Collapsed;

            }
        }

        private void setState(AppState state) {
            this.state = state;
            disableEverything();
            if (state == AppState.START) {
                brwsBtnSaveSettings.IsEnabled = true;
                brwsBtnSettings.IsEnabled = true;
                txtSet.IsEnabled = true;
                brwsBtnSaveSettings.Content = RelLanguage.BtnOpen;

                checkComments.IsEnabled = true;
                checkDisplayMessages.IsEnabled = true;
                checkLocations.IsEnabled = true;
                checkMapDetails.IsEnabled = true;
                checkSwitches.IsEnabled = true;
                checkUnitNames.IsEnabled = true;

                clearTable();
            } else if (state == AppState.PICK_MAP) {
                brwsBtnSaveSettings.Content = RelLanguage.BtnClose;
                brwsBtnSaveSettings.IsEnabled = true;
                brwsBtnInMap.IsEnabled = true;
                txtInMap.IsEnabled = true;
                btnOpenMap.IsEnabled = true;

                checkComments.IsEnabled = true;
                checkDisplayMessages.IsEnabled = true;
                checkLocations.IsEnabled = true;
                checkMapDetails.IsEnabled = true;
                checkSwitches.IsEnabled = true;
                checkUnitNames.IsEnabled = true;

                clearTable();
            } else if (state == AppState.LOADING_MAP) {
                clearTable();
                progress.IsEnabled = true;
                progress.Visibility = Visibility.Visible;
            } else if (state == AppState.PROCESSING) {
                btnOpenMap.Content = RelLanguage.BtnUnload;
                brwsBtnSaveSettings.Content = RelLanguage.BtnClose;
                brwsBtnSettings.Content = RelLanguage.BtnSave;
                progress.IsEnabled = true;
                progress.Visibility = Visibility.Visible;

            } else if (state == AppState.READY) {
                btnOpenMap.IsEnabled = true;
                btnOpenMap.Content = RelLanguage.BtnUnload;
                brwsBtnSaveSettings.Content = RelLanguage.BtnClose;
                brwsBtnSettings.Content = RelLanguage.BtnSave;
                txtOutMap.IsEnabled = true;
                brwsBtnOutMap.IsEnabled = true;

                brwsBtnSaveSettings.IsEnabled = true;
                brwsBtnSettings.IsEnabled = true;

                checkComments.IsEnabled = true;
                checkDisplayMessages.IsEnabled = true;
                checkLocations.IsEnabled = true;
                checkMapDetails.IsEnabled = true;
                checkSwitches.IsEnabled = true;
                checkUnitNames.IsEnabled = true;

                btnAddTranslation.IsEnabled = true;
                btnDeleteTranslation.IsEnabled = true;

                btnRun.IsEnabled = true;
                comboRunLanguage.IsEnabled = true;

                cmbDeathsP.IsEnabled = true;
                cmbDeathsU.IsEnabled = true;
                cmbSwitches.IsEnabled = true;
                rdDeaths.IsEnabled = true;
                rdSwitches.IsEnabled = true;

                btnCheckCond.IsEnabled = true;

                btnExportTranslation.IsEnabled = true;
                btnImportTranslation.IsEnabled = true;

                checkRepack.IsEnabled = true;
            }
        }

        private void disableEverything() {
            btnOpenMap.Content = RelLanguage.BtnLoad;
            brwsBtnSaveSettings.Content = RelLanguage.BtnOpen;
            brwsBtnSettings.Content = RelLanguage.BtnBrowse;
            txtInMap.IsEnabled = false;
            txtOutMap.IsEnabled = false;
            txtSet.IsEnabled = false;
            brwsBtnInMap.IsEnabled = false;
            brwsBtnOutMap.IsEnabled = false;
            btnOpenMap.IsEnabled = false;
            brwsBtnSaveSettings.IsEnabled = false;
            brwsBtnSettings.IsEnabled = false;

            checkComments.IsEnabled = false;
            checkDisplayMessages.IsEnabled = false;
            checkLocations.IsEnabled = false;
            checkMapDetails.IsEnabled = false;
            checkSwitches.IsEnabled = false;
            checkUnitNames.IsEnabled = false;

            btnAddTranslation.IsEnabled = false;
            btnDeleteTranslation.IsEnabled = false;

            btnRun.IsEnabled = false;
            comboRunLanguage.IsEnabled = false;

            cmbDeathsP.IsEnabled = false;
            cmbDeathsU.IsEnabled = false;
            cmbSwitches.IsEnabled = false;
            rdDeaths.IsEnabled = false;
            rdSwitches.IsEnabled = false;

            btnCheckCond.IsEnabled = false;

            progress.IsEnabled = false;
            progress.Visibility = Visibility.Collapsed;


            btnExportTranslation.IsEnabled = false;
            btnImportTranslation.IsEnabled = false;

            checkRepack.IsEnabled = false;
        }

        private void clearTable() {
            bool update = tblTrans.Columns.Count > 0 || Values.Count > 0;
            tblTrans.Columns.Clear();
            Values.Clear();
            if (update) {
                fireTableUpdate();
            }
        }

        private void fireTableUpdate() {
            tblTrans.ItemsSource = null;
            tblTrans.ItemsSource = Values;
        }

        private void setSettings(TranslatorData.Settings settings) {

            this.settings = settings;
            setComboText(txtSet, settings.settingsPath);
            setComboText(txtInMap, settings.inpuPath);
            setComboText(txtOutMap, settings.outputPath);

            clearTable();

            // Project strings back
            setStrings(settings);

            // Project languages on combobox
            object prevSelected = comboRunLanguage.SelectedItem;
            btnRun.IsEnabled = settings.langauges.Length > 0;
            comboRunLanguage.IsEnabled = btnRun.IsEnabled;
            comboRunLanguage.Items.Clear();
            for (int i = 0; i < settings.langauges.Length; i++) {
                comboRunLanguage.Items.Add(settings.langauges[i]);
            }
            if (prevSelected != null) {
                comboRunLanguage.SelectedItem = prevSelected;
            } else if (settings.langauges.Length > 0) {
                comboRunLanguage.SelectedIndex = 0;
            }
            checkRepack.IsChecked = settings.repack;

            txtInputEncoding.SelectedValue = null;
            txtInputEncoding.SelectedValue = InputEncoding;
        }

        private static int getInt(String str) {
            try {
                return Int32.Parse(str);
            } catch (Exception) {
                return 0;
            }
        }

        private TranslateString[] getCurrentStrings() {
            // Backup actual strings
            TranslateString[] strings = new TranslateString[Values.Count];
            for (int i = 0; i < strings.Length; i++) {
                strings[i] = Values[i];
            }
            // Re-order by index
            bool changed = true;
            while (changed) {
                changed = false;
                for (int i = 1; i < strings.Length; i++) {
                    TranslateString p0 = strings[i - 1];
                    TranslateString p1 = strings[i];
                    if (p0.StringIndex > p1.StringIndex) {
                        changed = true;
                        strings[i - 1] = p1;
                        strings[i] = p0;
                    }
                }
            }
            return strings;
        }

        private Settings getSettings() {
            Settings settings = this.settings;

            settings.settingsPath = getComboBox(txtSet);
            settings.inpuPath = getComboBox(txtInMap);
            settings.outputPath = getComboBox(txtOutMap);

            settings.repack = (bool) checkRepack.IsChecked;

            TranslateString[] strings = getCurrentStrings();

            if ((bool) rdDeaths.IsChecked) {
                int playerID = cmbDeathsP.SelectedIndex;
                int unitID = cmbDeathsU.SelectedIndex;
                settings.useCondition = (1 << 30) | (playerID << 8) | (unitID << 16);
            } else {
                settings.useCondition = cmbSwitches.SelectedIndex;
            }


            return settings;
        }

        private List<TranslateString> value = new List<TranslateString>();

        public List<TranslateString> Values {
            get {
                return this.value;
            }
            set {
                this.value = value;
            }
        }

        public bool checkSessionStrings(Settings settings, MapString[] strings) {

            // Check indexes
            int[] stored = settings.lastKnownMapping;
            String[] storedS = settings.originalStrings;
            String[] mapS = new String[strings.Length];
            int[] map = new int[strings.Length]; // Storage to map indexes of map strings
            //bool[] mapU = new bool[strings.Length]; // Which string was used
            List<int> mapU = new List<int>(); // Which strings were used
            for (int i = 0; i < strings.Length; i++) {
                map[i] = strings[i].mapIndex;
                mapS[i] = strings[i].str;
#if DEBUG_STR_REMAP
                Debug.WriteLine("Map string [" + i + "] index: " + map[i] + " contents: \"" + mapS[i] + "\"");
#endif
            }
            if (stored == null) {
                stored = new int[0];
            }
            settings.ts = new TranslateString[strings.Length];
            if (stored.Length == 0) { // Nothing stored create everything from scratch
                int newStringsArrayLength = strings.Length;
                String[] newOriginalStrings = new String[newStringsArrayLength];
                String[][] newTranslationStrings = new String[settings.langauges.Length][];
                int[] newLastKnownMapping = new int[newStringsArrayLength];
                for (int i = 0; i < newStringsArrayLength; i++) {
                    newOriginalStrings[i] = strings[i].str;
                    newLastKnownMapping[i] = strings[i].mapIndex;
                    for (int lI = 0; lI < settings.langauges.Length; lI++) {
                        newTranslationStrings[lI][i] = newOriginalStrings[i]; // Default translation is none
                    }
                }
                settings.lastKnownMapping = newLastKnownMapping;
                settings.strings = newTranslationStrings;
                settings.originalStrings = newOriginalStrings;
            } else {
                Dictionary<int, string> missingStoredStrings = new Dictionary<int, string>(); // Stored strings not found in map
                List<int> missingStoredStringsI = new List<int>();
                Dictionary<int, string> missingMapStrings = new Dictionary<int, string>(); // Map string not found in stored strings
                List<int> missingMapStringsI = new List<int>();
                Dictionary<int, int> remapping = new Dictionary<int, int>(); // Key is stored index, value is map index

                // Try to locate existing strings in map
                for (int i = 0; i < storedS.Length; i++) {
                    String storedString = storedS[i];
                    int storedIndex = stored[i];
#if DEBUG_STR_REMAP
                    Debug.WriteLine("Stored string [" + i + "] index: " + storedIndex + " contents: \"" + storedString + "\""); 
#endif

                    if (i < mapS.Length) { // Quick way
                        String mapString = mapS[i];
                        int mapIndex = map[i];
                        if (!mapU.Contains(mapIndex)) {
                            if (mapString == storedString) { // Naive match
                                if (storedIndex == mapIndex) { // Exact match
                                    mapU.Add(mapIndex);
#if DEBUG_STR_REMAP
                                    Debug.WriteLine("Short exact match\r\n");
#endif
                                    continue;
                                } else { // Remap index
                                    mapU.Add(mapIndex);
#if DEBUG_STR_REMAP
                                    Debug.WriteLine("Short matched map string[" + i + "] index " + mapIndex + "\r\n");
#endif
                                    remapping.Add(storedIndex, mapIndex);
                                    continue;
                                }
                            }
                        }
                    }

                    // No match, string could be misplaced
                    bool found = false;
                    for (int o = 0; o < mapS.Length; o++) {
                        if (!mapU.Contains(map[o])) { // Not used yet
                            if (mapS[o] == storedString) { // Naive match
                                mapU.Add(map[o]);
                                remapping.Add(storedIndex, map[o]);
                                found = true;
#if DEBUG_STR_REMAP
                                Debug.WriteLine("Long matched map string[" + o + "] index " + map[o] + "\r\n");
#endif
                                break;
                            }
                        }
                    }
                    if (!found) { // Stored string was not found in map
                        missingStoredStrings.Add(storedIndex, storedString);
                        missingStoredStringsI.Add(storedIndex);
#if DEBUG_STR_REMAP
                        Debug.WriteLine("No match found\r\n");
#endif
                    }
                }

                // Collect unused strings
                for (int o = 0; o < mapS.Length; o++) {
                    if (!mapU.Contains(map[o])) { // Not used yet
                        missingMapStrings.Add(map[o], mapS[o]);
                        missingMapStringsI.Add(map[o]);
#if DEBUG_STR_REMAP
                        Debug.WriteLine("Map string [" + o + "] index: " + map[o] + " contents: \"" + mapS[o] + "\" not found in storage\r\n");
#endif
                    }
                }

                // Warn user
                List<String> wa = new List<String>();

                if (missingMapStrings.Count > 0) {
                    wa.Add(String.Format(RelLanguage.ErrMissingStrings, missingMapStrings.Count));
                }
                if (missingStoredStrings.Count > 0) {
                    wa.Add(String.Format(RelLanguage.ErrMissingMapStrings, missingStoredStrings.Count));
                }
                if (remapping.Count > 0) {
                    wa.Add(String.Format(RelLanguage.ErrRemapNeeded, remapping.Count));
                }
                if (wa.Count > 0) {
                    StringBuilder sb = new StringBuilder();
                    foreach (string str in wa) {
                        if (sb.Length > 0) {
                            sb.Append("\r\n\r\n");
                        }
                        sb.Append(str);
                    }
                    sb.Append("\r\n\r\n" + RelLanguage.ErrConfirmTranslationUpdate);
                    if (MessageBox.Show(sb.ToString(), RelLanguage.WinTranslation, MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) {
                        return false;
                    }

                    if (missingMapStrings.Count > 0 || missingStoredStrings.Count > 0 || remapping.Count > 0) { // Add/Remove
                        int newStringsArrayLength = settings.originalStrings.Length + missingMapStrings.Count - missingStoredStrings.Count;
                        String[] newOriginalStrings = new String[newStringsArrayLength];
                        String[][] newTranslationStrings = new String[settings.langauges.Length][];
                        int[] newLastKnownMapping = new int[newStringsArrayLength];

                        // Copy current values without removed and remap existing
                        for (int lI = 0; lI < settings.langauges.Length; lI++) {
                            newTranslationStrings[lI] = new String[newStringsArrayLength];
                        }
                        for (int i = 0, realI = 0, remI = 0, remMax = missingStoredStrings.Count; i < settings.originalStrings.Length; i++) {
                            bool addCurrent = false;
                            if (remI == remMax) { // Nothing to remove
                                addCurrent = true;
                            } else { // Check if current index is to be removed
                                addCurrent = missingStoredStringsI[remI] != settings.lastKnownMapping[i];
                            }
                            if (!addCurrent) {
                                remI++;
                                continue;
                            }
                            if (remapping.ContainsKey(settings.lastKnownMapping[i])) { // Remap
                                newLastKnownMapping[realI] = remapping[settings.lastKnownMapping[i]];
                            } else { // Use original index
                                newLastKnownMapping[realI] = settings.lastKnownMapping[i];
                            }
                            newOriginalStrings[realI] = settings.originalStrings[i];
                            for (int lI = 0; lI < settings.langauges.Length; lI++) {
                                newTranslationStrings[lI][realI] = settings.strings[lI][i];
                            }
                            realI++;
                        }

                        // Add new values
                        for (int i = 0, iMax = missingMapStrings.Count; i < iMax; i++) {
                            int realI = i + newStringsArrayLength - missingMapStrings.Count;
                            newOriginalStrings[realI] = missingMapStrings[missingMapStringsI[i]];
                            newLastKnownMapping[realI] = missingMapStringsI[i];
                            for (int lI = 0; lI < settings.langauges.Length; lI++) {
                                newTranslationStrings[lI][realI] = newOriginalStrings[realI]; // Default translation is none
                            }
                        }
                        settings.lastKnownMapping = newLastKnownMapping;
                        settings.strings = newTranslationStrings;
                        settings.originalStrings = newOriginalStrings;
                    } // End of add / remove
                }
            }

            // Sort
            bool changed = true;
            while (changed) {
                changed = false;
                for (int i = 1; i < settings.lastKnownMapping.Length; i++) {
                    int index0 = settings.lastKnownMapping[i - 1];
                    int index1 = settings.lastKnownMapping[i];
                    if (index0 > index1) {
                        changed = true;

                        // Swap indexes
                        settings.lastKnownMapping[i] = index0;
                        settings.lastKnownMapping[i - 1] = index1;

                        // Swap original strings
                        String tmpS = settings.originalStrings[i - 1];
                        settings.originalStrings[i - 1] = settings.originalStrings[i]; // Default translation is none
                        settings.originalStrings[i] = tmpS;

                        // Swap translations
                        for (int lI = 0; lI < settings.langauges.Length; lI++) {
                            String tmpL = settings.strings[lI][i - 1];
                            settings.strings[lI][i - 1] = settings.strings[lI][i]; // Default translation is none
                            settings.strings[lI][i] = tmpL;
                        }
                    }
                }
            } // Sort while


            // Create translated strings
            for (int i = 0; i < strings.Length; i++) {
                settings.ts[i] = new TranslateString(RelLanguage, settings, strings[i], i);
            }
            return true; // All good
        }

        private void setStrings(Settings settings) {

            tblTrans.ItemsSource = null;
            using (Dispatcher.DisableProcessing()) {
                clearTable();

                int width = (int) tblTrans.ActualWidth;

                Style RightAlignStyle = new Style(typeof(DataGridCell)) {
                    Setters = {
                    new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right)
                }
                };
                Style RightAlignStyleH = new Style(typeof(DataGridColumnHeader)) {
                    Setters = {
                    new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Right)
                }
                };
                Style CenterAlignStyleH = new Style(typeof(DataGridColumnHeader)) {
                    Setters = {
                    new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center)
                }
                };

                {
                    DataGridNumericColumn c = new DataGridNumericColumn();
                    c.Header = "#";
                    c.Binding = new Binding("StringIndex");
                    c.Width = 60;
                    c.HeaderStyle = RightAlignStyleH;
                    c.IsReadOnly = true;
                    c.CellStyle = RightAlignStyle;
                    tblTrans.Columns.Add(c);
                }

                {
                    DataGridTextColumn c = new DataGridTextColumn();
                    c.Header = RelLanguage.TableUsage;
                    c.Binding = new Binding("description");

                    c.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    c.IsReadOnly = true;
                    c.HeaderStyle = CenterAlignStyleH;

                    Style textStyle = new Style(typeof(TextBlock));
                    textStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                    c.ElementStyle = textStyle;
                    Style textEditStyle = new Style(typeof(TextBox));
                    textEditStyle.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
                    c.EditingElementStyle = textEditStyle;

                    tblTrans.Columns.Add(c);
                }

                {
                    DataGridTextColumn c = new DataGridTextColumn();
                    c.Header = RelLanguage.TableOriginal;
                    c.Binding = new Binding("OriginalContents");

                    c.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    c.IsReadOnly = true;
                    c.HeaderStyle = CenterAlignStyleH;

                    Style textStyle = new Style(typeof(TextBlock));
                    textStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                    c.ElementStyle = textStyle;
                    Style textEditStyle = new Style(typeof(TextBox));
                    textEditStyle.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
                    c.EditingElementStyle = textEditStyle;

                    tblTrans.Columns.Add(c);
                }

                for (int i = 0; i < settings.langauges.Length; i++) {
                    DataGridTextColumn c = new DataGridTextColumn();
                    c.Header = settings.langauges[i];
                    c.Binding = new Binding("translations[" + i + "]");
                    c.IsReadOnly = false;
                    ((Binding) c.Binding).Mode = BindingMode.TwoWay;
                    c.HeaderStyle = CenterAlignStyleH;


                    c.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);

                    Style textStyle = new Style(typeof(TextBlock));
                    textStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                    c.ElementStyle = textStyle;
                    Style textEditStyle = new Style(typeof(TextBox));
                    textEditStyle.Setters.Add(new Setter(TextBox.AcceptsReturnProperty, true));
                    textEditStyle.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
                    c.EditingElementStyle = textEditStyle;

                    tblTrans.Columns.Add(c);

                }

                for (int i = 0; i < settings.ts.Length; i++) {
                    TranslateString ms = settings.ts[i];
                    Values.Add(ms);
                }

                if (settings.useCondition < 256) { // Switch
                    rdSwitches.IsChecked = true;
                    rdDeaths.IsChecked = false;
                    cmbSwitches.SelectedIndex = settings.useCondition;
                } else { // Deaths
                    rdDeaths.IsChecked = true;
                    rdSwitches.IsChecked = false;
                    int playerID = (settings.useCondition & 0xffff) >> 8;
                    int unitID = (settings.useCondition >> 16) & 0xff;
                    cmbDeathsP.SelectedIndex = playerID;
                    cmbDeathsU.SelectedIndex = unitID;
                }

                //d.Dispose();
                applyFilters();
                fireTableUpdate();
            }
        }

        private void applyFilters() {
            //IDisposable d = Dispatcher.DisableProcessing();

            ICollectionView Itemlist = CollectionViewSource.GetDefaultView(tblTrans);
            Predicate<object> predicate;
            bool showMap = checkMapDetails == null ? true : (bool) checkMapDetails.IsChecked;
            bool showMessages = checkDisplayMessages == null ? true : (bool) checkDisplayMessages.IsChecked;
            bool showComments = checkComments == null ? true : (bool) checkComments.IsChecked;
            bool showLocations = checkLocations == null ? true : (bool) checkLocations.IsChecked;
            bool showSwitches = checkSwitches == null ? true : (bool) checkSwitches.IsChecked;
            bool showUnits = checkUnitNames == null ? true : (bool) checkUnitNames.IsChecked;


            predicate = new Predicate<object>(item => {
                TranslateString ms = (TranslateString) item;
                if (showMap && (ms.isMapDescription || ms.isMapName || ms.forceNamesIndexes.Length > 0)) {
                    return true;
                } else if (showMessages && (ms.triggerActionIndexes.Length > 0 || ms.briefingActionIndexes.Length > 0)) {
                    return true;
                } else if (showComments && (ms.triggerCommentIndexes.Length > 0 || ms.briefingCommentsIndexes.Length > 0)) {
                    return true;
                } else if (showLocations && ms.locationIndexes.Length > 0) {
                    return true;
                } else if (showSwitches && ms.switchIndexes.Length > 0) {
                    return true;
                } else if (showUnits && ms.unitIndexs.Length > 0) {
                    return true;
                }
                return false;
            });
            tblTrans.Items.Filter = predicate;
            //d.Dispose();
            fireTableUpdate();
        }

        private bool tryLoadingGivenMap(Settings set, bool errorOnFail, AppState stateOnError) {
            setState(AppState.LOADING_MAP);

            // Load support
            try {
                if (set.inpuPath == "") {
                    if (errorOnFail) {
                        setState(stateOnError);
                    }
                    return false;
                }
                SupportType us = TheLib.getSupportType(set);
                if (us == SupportType.Unsupported) {
                    if (errorOnFail) {
                        showErrorMessageBox(RelLanguage.WinMapErr, RelLanguage.ErrUnsupportedMap);
                        setState(stateOnError);
                    }
                    return false;
                }
                this.support = us;
            } catch (Exception) {
                if (errorOnFail) {
                    showErrorMessageBox(RelLanguage.WinMapErr, RelLanguage.ErrMapFailedToLoad);
                    setState(stateOnError);
                }
                return false;
            }

            // Load data
            try {
                if (set.inpuPath == "") {
                    if (errorOnFail) {
                        setState(stateOnError);
                    }
                    return false;
                }
                MapString[] us = TheLib.getStrings(set.inpuPath, set.originalEncoding);
                if (us == null) {
                    if (errorOnFail) {
                        showErrorMessageBox(RelLanguage.WinMapErr, RelLanguage.ErrUnsupportedMap);
                        setState(stateOnError);
                    }
                    return false;
                }
                if (!checkSessionStrings(set, us)) {
                    setState(AppState.PICK_MAP);
                    return false;
                }
                setStrings(set);
            } catch (NotImplementedException) {
                if (errorOnFail) {
                    showErrorMessageBox(RelLanguage.WinMapErr, RelLanguage.ErrMapFailedToLoad);
                    setState(stateOnError);
                }
                return false;
            }

            setState(AppState.READY);
            setSettings(set);
            return true;
        }

        private void setComboText(ComboBox box, String text) {
            for (int i = 0; i < box.Items.Count; i++) {
                String item = box.Items[i].ToString();
                if (item == text) {
                    var tmp = box.Items[0];
                    box.Items[0] = box.Items[i];
                    box.Items[i] = tmp;
                    box.SelectedIndex = 0;
                    History.save(box.Name, box);
                    return;
                }
            }
            box.Items.Insert(0, text);
            box.SelectedIndex = 0;
            History.save(box.Name, box);
        }

        private String getComboBox(ComboBox box) {
            return (box.SelectedIndex >= 0 && box.SelectedIndex < box.Items.Count) ? box.Items[box.SelectedIndex].ToString() : box.Text;
        }

        private void _syncTryLoadBegin() {
            String fileName = getComboBox(txtSet);
            Settings set = Settings.loadFromFile(fileName);
            if (set == null) { // Failed to load
                if (showConfirmWarning(RelLanguage.WinLoadSettings, RelLanguage.LblNewSettingsCreate)) {
                    set = Settings.getBlank(fileName, InputEncoding.Encoding);
                    if (!set.saveToFile(fileName)) {
                        showErrorMessageBox(RelLanguage.WinSaveSettings, RelLanguage.ErrFailedToCreateSettings);
                    } else {
                        this.setSettings(set);
                        setState(AppState.PICK_MAP);
                    }
                }
            } else {
                // Patch strings
                for (int languageID = 0; languageID < set.langauges.Length; languageID++) {
                    for (int stringID = 0; stringID < set.strings[languageID].Length; stringID++) {
                        String str = set.strings[languageID][stringID];
                        str = TranslateString.unescape(str);
                        set.strings[languageID][stringID] = str;
                    }
                }

                setSettings(set);
                Func<object, object> asyncLoader = (object o) => {
                    return tryLoadingGivenMap(set, false, AppState.PICK_MAP);
                };
                Action<object> syncLoaderEnd = (object o) => { bool b = (bool) o; if (!b) { setState(AppState.PICK_MAP); } };
                setState(AppState.PROCESSING);
                //async.addJob(asyncLoader, null, syncLoaderEnd);
                syncLoaderEnd(asyncLoader(null));
            }
        }

        private void brwsBtnSaveSettings_Click(object sender, RoutedEventArgs e) { // Open settings clicked
                                                                                   // using (Dispatcher.DisableProcessing()) {
            if (state == AppState.START) { // First load, open
                _syncTryLoadBegin();
            } else if (state == AppState.PICK_MAP) { // Picking map, close
                setState(AppState.START);
            } else if (state == AppState.READY) { // Awaiting input, close
                setState(AppState.START);
            }
            // }
        }

        private static readonly Regex _regex = new Regex("^-?[0-9]+$"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text) {
            return !_regex.IsMatch(text);
        }

        private void NumericInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !_regex.IsMatch(e.Text);
        }

        private void btnOpenMap_Click(object sender, RoutedEventArgs e) { // Load map click button (can also be close map)
                                                                          //using ( Dispatcher.DisableProcessing()) {
            if (state == AppState.PICK_MAP) { // Load
                String map = getComboBox(txtInMap);
                if (map.Length > 0) {
                    tryLoadingGivenMap(getSettings(), true, AppState.PICK_MAP);
                }
            } else if (state == AppState.READY) { // Unload
                setState(AppState.PICK_MAP);
            }
            //}
        }

        private String getSettingsFile() {
            return getFile(RelLanguage.WinOpenSettingsDialogTitle, new String[] { "smt" }, new String[] { RelLanguage.FormatName });
        }

        private String getMapFile() {
            return getFile(RelLanguage.WinOpenMapDialogTitle, new String[] { "scx", "scm" }, new String[] { RelLanguage.BwMapName, RelLanguage.ScMapName });
        }

        public static String getFile(String title, String[] extension, String[] extensionDescriptions) {
            OpenFileDialog openFileDialog = new OpenFileDialog();


            String[] res = new String[extension.Length];
            for (int i = 0; i < extension.Length; i++) {
                StringBuilder sb = new StringBuilder();
                res[i] = extensionDescriptions[i] + " (*." + extension[i] + ")|*." + extension[i];
            }
            String type = string.Join("|", res);


            openFileDialog.Filter = type;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            bool? b = openFileDialog.ShowDialog();
            if (b == true) {
                String filePath = openFileDialog.FileName;
                return filePath;
            }
            return null;
        }

        private void brwsBtnSettings_Click(object sender, RoutedEventArgs e) {

            if (state == AppState.READY) { // Save
                Settings set = getSettings(); // Scrap settings from UI
                                              //using (Dispatcher.DisableProcessing()) {
                set.saveToFile(set.settingsPath);
                //}
                setSettings(set);
                MessageBox.Show(RelLanguage.LblSettingsSaved, RelLanguage.WinSaveSettings, MessageBoxButton.OK, MessageBoxImage.Information);
            } else {
                String f = getSettingsFile();
                if (f != null) {
                    setComboText(txtSet, f);
                    brwsBtnSaveSettings_Click(sender, null);
                }
            }
        }

        private void brwsBtnInMap_Click(object sender, RoutedEventArgs e) {
            //using (Dispatcher.DisableProcessing()) {
            String f = getMapFile();
            if (f != null) {
                setComboText(txtInMap, f);
            }
            //}
            btnOpenMap_Click(sender, null);
        }

        private void brwsBtnOutMap_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog openFileDialog = new SaveFileDialog();

            String type = RelLanguage.ScMapFile + " (*.scx)|*.scx";


            openFileDialog.Filter = type;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            bool? b = openFileDialog.ShowDialog();
            if (b == true) {
                String filePath = openFileDialog.FileName;
                setComboText(txtOutMap, filePath);
            }

        }

        private void checkLocations_Checked(object sender, RoutedEventArgs e) {
            applyFilters();
        }

        private void checkSwitches_Checked(object sender, RoutedEventArgs e) {
            applyFilters();
        }

        private void checkUnitNames_Checked(object sender, RoutedEventArgs e) {
            applyFilters();
        }

        private void checkMapDetails_Checked(object sender, RoutedEventArgs e) {
            applyFilters();
        }

        private void checkDisplayMessages_Checked(object sender, RoutedEventArgs e) {
            applyFilters();
        }

        private void checkComments_Checked(object sender, RoutedEventArgs e) {
            applyFilters();
        }

        private void addNewTranslationToSettings(String languageName, String[] translationData, Encoding encoding) {
            String[] newLanguages = new String[settings.langauges.Length + 1];
            Encoding[] newEncodings = new Encoding[settings.encodings.Length + 1];
            for (int i = 0; i < settings.langauges.Length; i++) {
                newLanguages[i] = settings.langauges[i];
                newEncodings[i] = settings.encodings[i];
            }
            newLanguages[newLanguages.Length - 1] = languageName;
            newEncodings[newEncodings.Length - 1] = encoding;
            settings.langauges = newLanguages;
            settings.encodings = newEncodings;

            // Update string array
            String[][] newStrings = new String[settings.strings.Length + 1][];
            for (int i = 0; i < settings.strings.Length; i++) {
                newStrings[i] = settings.strings[i];
            }
            newStrings[newStrings.Length - 1] = translationData;
            settings.strings = newStrings;

            setSettings(settings);
        }

        private void createNewLangaugeCB(String languageName, String copyOf) {
            Settings settings = getSettings();
            Encoding newEncoding = Settings.defaultEncoding;

            // Check langauge unuqie
            for (int i = 0; i < settings.langauges.Length; i++) {
                String existingLanguage = settings.langauges[i];
                if (existingLanguage == languageName) {
                    showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrFailedToCreateTranslationLanguageExists);
                    return;
                }
            }

            // Find copy of langauges
            String[] translationData = null;
            for (int i = 0; i < settings.langauges.Length; i++) {
                String existingLanguage = settings.langauges[i];
                if (existingLanguage == copyOf && copyOf != null) {
                    String[] languageData = settings.strings[i];
                    translationData = new String[languageData.Length];
                    for (int a = 0; a < translationData.Length; a++) {
                        translationData[a] = languageData[a];
                    }
                    newEncoding = settings.encodings[i];
                    break;
                }
            }
            if (translationData == null && copyOf == null) { // Use defaults

                TranslateString[] strings = getCurrentStrings();

                translationData = new String[strings.Length];
                for (int a = 0; a < translationData.Length; a++) {
                    translationData[a] = strings[a].str;
                }
            } else if (translationData == null) {
                showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrFailedToCreateTranslation);
                return;
            }

            // Have translation data, store in settings and reload
            // Update languages array
            addNewTranslationToSettings(languageName, translationData, newEncoding);
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            Settings settings = getSettings();
            String[] translations = new String[settings.langauges.Length];
            //using (Dispatcher.DisableProcessing()) {
            for (int i = 0; i < settings.langauges.Length; i++) {
                translations[i] = settings.langauges[i];
            }
            //}
            AddTranslationDialog dialog = new AddTranslationDialog(translations, createNewLangaugeCB);
            dialog.ShowDialog();

        }

        private void deleteLangaugeCB(String languageName) {
            if (languageName != null) {
                Settings settings = getSettings();

                // Check langauge unuqie
                for (int i = 0; i < settings.langauges.Length; i++) {
                    String existingLanguage = settings.langauges[i];
                    if (existingLanguage == languageName) { // Found index, update

                        // First update language array
                        String[] newLanguages = new String[settings.langauges.Length - 1];
                        Encoding[] newEncodings = new Encoding[settings.encodings.Length - 1];
                        for (int o = 0; o < i; o++) {
                            newLanguages[o] = settings.langauges[o];
                            newEncodings[o] = settings.encodings[o];
                        }
                        for (int o = i + 1; o < settings.langauges.Length; o++) {
                            newLanguages[o - 1] = settings.langauges[o];
                            newEncodings[o - 1] = settings.encodings[o];
                        }
                        settings.langauges = newLanguages;
                        settings.encodings = newEncodings;

                        // Update string array
                        String[][] newStrings = new String[settings.strings.Length - 1][];
                        for (int o = 0; o < i; o++) {
                            newStrings[o] = settings.strings[o];
                        }
                        for (int o = i + 1; o < settings.strings.Length; o++) {
                            newStrings[o - 1] = settings.strings[o];
                        }
                        settings.strings = newStrings;

                        setSettings(settings);
                        return;
                    }
                }
            }
        }

        private void btnDeleteTranslation_Click(object sender, RoutedEventArgs e) {
            Settings settings = getSettings();
            String[] translations = new String[settings.langauges.Length];
            //using (Dispatcher.DisableProcessing()) {
            for (int i = 0; i < settings.langauges.Length; i++) {
                translations[i] = settings.langauges[i];
            }
            //}
            DeleteTranslationDialog dialog = new DeleteTranslationDialog(translations, deleteLangaugeCB);
            dialog.ShowDialog();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e) {
            int index = comboRunLanguage.SelectedIndex;
            Settings settings = getSettings();
            if (settings.outputPath.Length == 0) {
                showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrNoOutputMap);
                return;
            }

            if (index >= 0 && index < settings.langauges.Length) {
                if (TheLib.process(settings, index)) {
                    MessageBox.Show(RelLanguage.LblTranslationDone, RelLanguage.WinTranslation, MessageBoxButton.OK, MessageBoxImage.Information);
                } else {
                    showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrTranslationFailed);
                }
            } else {
                showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrInvalidTargetLanguage);
            }
        }

        private void btnCheckCond_Click(object sender, RoutedEventArgs e) {
            //using (Dispatcher.DisableProcessing()) {
            Settings set = getSettings();
            if (!TheLib.checkUsedCondition(set)) {
                showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrGuardCannotBeUsed);
            } else {
                MessageBox.Show(RelLanguage.LblGuardCanBeUsed, RelLanguage.WinTranslation, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //}
        }

        private void tblTrans_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DataGrid grid = sender as DataGrid;
                TextBox col = (TextBox) grid.CurrentCell.Column.GetCellContent(grid.CurrentCell.Item);
                int caret = col.CaretIndex;
                String text = col.Text;
                col.Text = text.Substring(0, caret) + "\r\n" + text.Substring(caret);
                col.CaretIndex = caret + 1;
                e.Handled = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.staredit.net/topic/17892/");
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
            new UpdateWindow(RelLanguage).ShowDialog();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e) {
            new AboutWindow(RelLanguage).ShowDialog();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e) {
            this.Close();
        }

        enum StringOrigin {
            Title,
            Game,
            Briefing,
            Unused
        }

        private Func<TranslateString, StringOrigin> originResolver = (TranslateString str) => {
            if (str.briefingActionIndexes.Length > 0) {
                return StringOrigin.Briefing;
            } else if (str.triggerActionIndexes.Length > 0 || str.unitIndexs.Length > 0) {
                return StringOrigin.Game;
            } else if (str.isMapName) {
                return StringOrigin.Title;
            }
            return StringOrigin.Unused;
        };

        private void exportCB(Settings settings, String translation, String filePath, bool escapeColors, bool escapeNewline) {
            try {
                using (var package = new ExcelPackage()) {
                    // Add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(translation);

                    Func<int, string> numToAlphaC = (int val) => { return "" + (char) (((byte) 'A') + val); };
                    Func<int, string> numToAlphaT = null;
                    numToAlphaT = (int val) => { return val < 9 ? numToAlphaC(val) : numToAlphaT(val / 10) + numToAlphaC(val % 10); };
                    Func<int, string> numToAlpha = (int val) => { return numToAlphaT(val); };

                    Func<int, int, string> cell = (int x, int y) => numToAlpha(x) + y.ToString();

                    Action<int, int, int, int> borderAround = (int left, int top, int right, int bottom) => {
                        for (int i = left; i <= right; i++) { // Top and bottom borders
                            worksheet.Cells[cell(i, top)].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                            worksheet.Cells[cell(i, bottom)].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        }
                        for (int i = top; i <= bottom; i++) { // Left and right borders
                            worksheet.Cells[cell(left, i)].Style.Border.Left.Style = ExcelBorderStyle.Thick;
                            worksheet.Cells[cell(right, i)].Style.Border.Right.Style = ExcelBorderStyle.Thick;
                        }
                    };

                    //Add the headers
                    worksheet.Cells[cell(1, 1)].Value = "String ID";
                    worksheet.Cells[cell(2, 1)].Value = "Original String";
                    worksheet.Cells[cell(3, 1)].Value = "Usage";
                    worksheet.Cells[cell(4, 1)].Value = "Translated String";

                    ExcelRange headerCells = worksheet.Cells[cell(1, 1) + ":" + cell(4, 1)];
                    headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    headerCells.Style.Font.Bold = true;
                    headerCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    borderAround(1, 1, 4, 1); // Header border
                    borderAround(1, 1, 4, 1 + settings.ts.Length); // Entire table border
                    borderAround(1, 1, 1, 1 + settings.ts.Length); // Column borders
                    borderAround(2, 1, 2, 1 + settings.ts.Length); // Column borders
                    borderAround(3, 1, 3, 1 + settings.ts.Length); // Column borders
                    borderAround(4, 1, 4, 1 + settings.ts.Length); // Column borders

                    // Find language ID
                    int languageID = -1;
                    for (int i = 0; i < settings.langauges.Length; i++) {
                        if (settings.langauges[i] == translation) {
                            languageID = i;
                        }
                    }
                    if (languageID == -1) {
                        showErrorMessageBox(RelLanguage.WinExport, RelLanguage.ErrorExportFailed);
                    }


                    Func<int, StringOrigin, Color?> colorMapping = (int code, StringOrigin origin) => {
                        if (origin == StringOrigin.Title) {
                            if (code < colors.TitleColors.Length) {
                                StarcraftColor color = colors.TitleColors[code];
                                if (color.Valid) {
                                    return color.Color;
                                }
                            }
                            return null;
                        } else if (origin == StringOrigin.Briefing) {
                            if (code < colors.BriefingColors.Length) {
                                StarcraftColor color = colors.BriefingColors[code];
                                if (color.Valid) {
                                    return color.Color;
                                }
                            }
                            return null;
                        } else if (origin == StringOrigin.Game) {
                            if (code < colors.GameColors.Length) {
                                StarcraftColor color = colors.GameColors[code];
                                if (color.Valid) {
                                    return color.Color;
                                }
                            }
                            return null;
                        } else if (origin == StringOrigin.Unused) {
                            return null;
                        }
                        return null;
                    };

                    Action<ExcelRange, StringOrigin, String> colorizer = (ExcelRange cells, StringOrigin origin, String str) => {
                        String unescaped = TranslateString.unescape(str);
                        StringBuilder continuous = new StringBuilder();
                        Color currentColor = Color.Black;
                        cells.IsRichText = true;
                        for (int i = 0; i < unescaped.Length; i++) {
                            char c = unescaped[i];
                            Color? clr = colorMapping(c, origin);
                            if (clr != null) {
                                if (continuous.Length > 0) {
                                    ExcelRichText txt = cells.RichText.Add(continuous.ToString());
                                    txt.Color = currentColor;
                                    continuous.Clear();
                                }
                                currentColor = clr.Value;
                            } else {
                                if (c == '\r') {
                                } else if (c == '\n') {
                                    if (escapeNewline) {
                                        continuous.Append("<13>");
                                    } else {
                                        continuous.Append(c);
                                    }
                                } else if (Char.IsControl(c)) {
                                    continuous.Append(TranslateString.escape(c.ToString()));
                                } else {
                                    continuous.Append(c);
                                }
                            }
                        }
                        if (continuous.Length > 0) {
                            ExcelRichText txt = cells.RichText.Add(continuous.ToString());
                            txt.Color = currentColor;
                            continuous.Clear();
                        }
                    };

                    Action<ExcelRange, StringOrigin, String> notColorizer = (ExcelRange cells, StringOrigin o, String str) => {
                        cells.Value = str;
                    };

                    Action<ExcelRange, StringOrigin, String> cellSetter = escapeColors ? colorizer : notColorizer;

                    for (int i = 0; i < settings.originalStrings.Length; i++) {
                        TranslateString str = settings.ts[i];

                        StringOrigin origin = originResolver(str);

                        notColorizer(worksheet.Cells[cell(1, 2 + i)], origin, str.StringIndex.ToString());
                        notColorizer(worksheet.Cells[cell(2, 2 + i)], origin, str.OriginalContents);
                        notColorizer(worksheet.Cells[cell(3, 2 + i)], origin, str.description);
                        cellSetter(worksheet.Cells[cell(4, 2 + i)], origin, str.translations[languageID]);

                        try {
                            worksheet.Cells[cell(1, 2 + i)].AutoFitColumns();
                            worksheet.Cells[cell(2, 2 + i)].AutoFitColumns();
                            worksheet.Cells[cell(3, 2 + i)].AutoFitColumns();
                            worksheet.Cells[cell(4, 2 + i)].AutoFitColumns();
                        } catch (Exception) { // Welp...

                        }
                    }

                    worksheet.Calculate();
                    try {
                        worksheet.Cells.AutoFitColumns(0);  //Autofit columns for all cells
                    } catch (Exception) { // Welp...

                    }

                    double w1 = worksheet.Column(1).Width;
                    double w2 = worksheet.Column(2).Width;
                    double w3 = worksheet.Column(3).Width;
                    double w4 = worksheet.Column(4).Width;


                    for (int i = 0; i < settings.originalStrings.Length; i++) {
                        TranslateString str = settings.ts[i];

                        worksheet.Cells[cell(1, 2 + i)].Style.WrapText = true;
                        worksheet.Cells[cell(2, 2 + i)].Style.WrapText = true;
                        worksheet.Cells[cell(3, 2 + i)].Style.WrapText = true;
                        worksheet.Cells[cell(4, 2 + i)].Style.WrapText = true;

                    }

                    worksheet.Column(1).Width = w1;
                    worksheet.Column(2).Width = w2;
                    worksheet.Column(3).Width = w3;
                    worksheet.Column(4).Width = w4;


                    // set some document properties
                    package.Workbook.Properties.Title = "Starcraft Map Translation File";
                    package.Workbook.Properties.Author = "Starcraft Map Translation Tool By Tarferi";
                    package.Workbook.Properties.Comments = "This file contains translation strings for a starcraft map";


                    System.IO.FileInfo xlFile = new System.IO.FileInfo(filePath);

                    package.SaveAs(xlFile);

                    MessageBox.Show(RelLanguage.LblExportFinish, RelLanguage.WinExport, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } catch (Exception) {
                showErrorMessageBox(RelLanguage.WinExport, RelLanguage.ErrorExportFailed);
            }
        }

        private void btnExportTranslation_Click(object sender, RoutedEventArgs e) {
            Settings settings = getSettings();
            String[] translations = new String[settings.langauges.Length];
            //using (Dispatcher.DisableProcessing()) {
            for (int i = 0; i < settings.langauges.Length; i++) {
                translations[i] = settings.langauges[i];
            }
            //}
            ExportWindow dialog = new ExportWindow(translations, (String translation, String filePath, bool escapeColors, bool escapeNewline) => { exportCB(settings, translation, filePath, escapeColors, escapeNewline); });
            dialog.ShowDialog();
        }

        private void importCB(Settings settings, String translation, String filePath) {
            try {
                FileInfo existingFile = new FileInfo(filePath);
                using (ExcelPackage package = new ExcelPackage(existingFile)) {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    List<String> errors = new List<String>();

                    Func<Color, StringOrigin, StarcraftColor> colorMapping = (Color color, StringOrigin origin) => {
                        if (origin == StringOrigin.Title) {
                            return colors.getTitleColor(color);
                        } else if (origin == StringOrigin.Briefing) {
                            return colors.getBriefingColor(color);
                        } else if (origin == StringOrigin.Game) {
                            return colors.getGameColor(color);
                        } else if (origin == StringOrigin.Unused) {
                            return colors.DefaultColor;
                        }
                        errors.Add("Invalid origin: " + origin);
                        return null;
                    };

                    Func<ExcelRichTextCollection, StringOrigin, String> uncolorizer = (ExcelRichTextCollection texts, StringOrigin origin) => {
                        StringBuilder sb = new StringBuilder();
                        foreach (ExcelRichText text in texts) {
                            StarcraftColor c = colorMapping(text.Color, origin);
                            if (c == null) { // Invalid color
                                errors.Add("Invalid color: #" + (text.Color.ToArgb() & 0xffffff).ToString("X6"));
                            } else {
                                if (c.Valid) { // Not default background
                                    sb.Append((char) c.key);
                                }
                            }
                            sb.Append(text.Text.Replace("\r", "").Replace("\n", "\r\n"));
                        }
                        return sb.ToString();
                    };

                    Func<ExcelRange, StringOrigin, String> unescaper = (ExcelRange cells, StringOrigin origin) => {
                        StringBuilder result = new StringBuilder();
                        if (cells.IsRichText) {
                            String str = uncolorizer(cells.RichText, origin);
                            result.Append(TranslateString.escape(str));
                        } else {
                            result.Append(TranslateString.escape(cells.Text));
                        }
                        return result.ToString();
                    };

                    Func<int, string> numToAlphaC = (int val) => { return "" + (char) (((byte) 'A') + val); };
                    Func<int, string> numToAlphaT = null;
                    numToAlphaT = (int val) => { return val < 9 ? numToAlphaC(val) : numToAlphaT(val / 10) + numToAlphaC(val % 10); };
                    Func<int, string> numToAlpha = (int val) => { return numToAlphaT(val); };

                    Func<int, int, string> cell = (int x, int y) => numToAlpha(x) + y.ToString();

                    Func<string, string> escapeNewLines = (string str) => {
                        return str.Replace("\r", "").Replace("\n", "<13>");
                    };

                    Func<int, int, string, StringOrigin, bool> cellContains = (int x, int y, string str, StringOrigin origin) => {
                        String restr = escapeNewLines(unescaper(worksheet.Cells[cell(x, y)], origin));
                        if (restr == str) {
                            return true;
                        } else {
                            errors.Add(String.Format(RelLanguage.LblStringCorruption, str, restr));
                            return false;
                        }
                    };

                    // Verify that it's our format
                    bool isOurs = true;
                    isOurs &= cellContains(1, 1, "String ID", StringOrigin.Unused);
                    isOurs &= cellContains(2, 1, "Original String", StringOrigin.Unused);
                    isOurs &= cellContains(3, 1, "Usage", StringOrigin.Unused);
                    isOurs &= cellContains(4, 1, "Translated String", StringOrigin.Unused);
                    if (!isOurs) { errors.Add(RelLanguage.LblImportErHeaders); }

                    for (int i = 0; i < settings.originalStrings.Length; i++) {
                        TranslateString str = settings.ts[i];
                        StringOrigin origin = originResolver(str);
                        isOurs &= cellContains(1, 2 + i, str.StringIndex.ToString(), origin);
                        isOurs &= cellContains(2, 2 + i, escapeNewLines(str.OriginalContents), origin);
                        isOurs &= cellContains(3, 2 + i, escapeNewLines(str.description), origin);
                        if (!isOurs) {
                            errors.Add(RelLanguage.LblOriginalDataCorrupted);
                            break;
                        }
                    }

                    if (!isOurs) {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < errors.Count; i++) {
                            String err = errors[i];
                            if (sb.Length > 0) {
                                sb.Append("\r\n");
                            }
                            sb.Append(err);
                            if (i == 15 && errors.Count > 15) {
                                sb.Append("\r\n\r\n" + (errors.Count - 15) + " more");
                                break;
                            }
                        }
                        if (sb.Length == 0) {
                            sb.Append(RelLanguage.LblNoDetailsAvailable);
                        }
                        showErrorMessageBox(RelLanguage.WinImport, RelLanguage.ErrorImportFailed + ".\r\n" + RelLanguage.LblImportErrDetails + ":\r\n" + sb.ToString());
                        return;
                    }

                    String[] translationData = new string[settings.originalStrings.Length];
                    for (int i = 0; i < settings.originalStrings.Length; i++) {
                        StringOrigin origin = originResolver(settings.ts[i]);
                        translationData[i] = unescaper(worksheet.Cells[cell(4, 2 + i)], origin).Replace("<13>", "\r\n");
                    }

                    // Find languageID
                    int languageID = settings.langauges.Length;
                    for (int i = 0; i < settings.langauges.Length; i++) {
                        if (settings.langauges[i] == translation) {
                            languageID = i;
                        }
                    }
                    if (languageID == settings.langauges.Length) {
                        // We are not updating existing language
                        addNewTranslationToSettings(translation, translationData, settings.originalEncoding);
                    } else {
                        // We are updating existing language
                        settings.strings[languageID] = translationData;
                        setSettings(settings);
                    }
                    setStrings(settings);
                    MessageBox.Show(RelLanguage.LblImportFinished, RelLanguage.WinImport, MessageBoxButton.OK, MessageBoxImage.Information);

                }
            } catch (Exception) {
                showErrorMessageBox(RelLanguage.WinImport, RelLanguage.ErrorImportFailed);
            }
        }

        private void btnImportTranslation_Click(object sender, RoutedEventArgs e) {
            Settings settings = getSettings();
            String[] translations = new String[settings.langauges.Length];
            //using (Dispatcher.DisableProcessing()) {
            for (int i = 0; i < settings.langauges.Length; i++) {
                translations[i] = settings.langauges[i];
            }
            //}
            ImportWindow dialog = new ImportWindow((String translation, String filePath) => { importCB(settings, translation, filePath); }, translations);
            if (dialog.isGood) {
                dialog.ShowDialog();
            }
        }

        private void FilesControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            udpW1.Width = new GridLength(filesControlCol1.ActualWidth, GridUnitType.Pixel);
            udpW2.Width = new GridLength(filesControlCol2.ActualWidth, GridUnitType.Pixel);
            udpW3.Width = new GridLength(filesControlCol3.ActualWidth, GridUnitType.Pixel);
            udpW4.Width = new GridLength(filesControlCol4.ActualWidth, GridUnitType.Pixel);

        }

        private void comboRunLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            txtOutputEncoding.SelectedValue = null;
            txtOutputEncoding.SelectedValue = OutputEncoding;
        }
    }

    public class InvertedBooleanToVisibilityConverter : IValueConverter {

        private BooleanToVisibilityConverter def = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Visibility vis = (Visibility) def.Convert(value, targetType, parameter, culture);
            if (vis == Visibility.Visible) {
                return Visibility.Collapsed;
            } else {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            Visibility vis = (Visibility) def.ConvertBack(value, targetType, parameter, culture);
            if (vis == Visibility.Visible) {
                return Visibility.Collapsed;
            } else {
                return Visibility.Visible;
            }
        }
    }

    public class DataGridNumericColumn : DataGridTextColumn {
        protected override object PrepareCellForEdit(System.Windows.FrameworkElement editingElement, System.Windows.RoutedEventArgs editingEventArgs) {
            TextBox edit = editingElement as TextBox;
            edit.PreviewTextInput += OnPreviewTextInput;

            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            try {
                Convert.ToInt32(e.Text);
            } catch {
                // Show some kind of error message if you want

                // Set handled to true
                e.Handled = true;
            }
        }
    }

    public class TranslateString {

        public Settings settings;

        public int[] unitIndexs;
        public int[] switchIndexes;
        public int[] locationIndexes;
        public int[] triggerActionIndexes;
        public int[] triggerCommentIndexes;
        public int[] briefingActionIndexes;
        public int[] briefingCommentsIndexes;
        public int[] forceNamesIndexes;

        public bool isMapName;
        public bool isMapDescription;

        public String str { get { return settings.originalStrings[storageIndex]; } }
        public int storageIndex;

        dynamic RelLanguage;

        public TranslateString(dynamic RelLanguage, Settings settings, MapString ms, int index) {
            this.settings = settings;
            this.RelLanguage = RelLanguage;
            this.storageIndex = index;
            this.translations = new StringList(this);

            // Copy from map string
            this.unitIndexs = ms.unitIndexs;
            this.switchIndexes = ms.switchIndexes;
            this.locationIndexes = ms.locationIndexes;
            this.triggerActionIndexes = ms.triggerActionIndexes;
            this.triggerCommentIndexes = ms.triggerCommentIndexes;
            this.briefingActionIndexes = ms.briefingActionIndexes;
            this.briefingCommentsIndexes = ms.briefingCommentsIndexes;
            this.forceNamesIndexes = ms.forceNamesIndexes;
            this.isMapName = ms.isMapName;
            this.isMapDescription = ms.isMapDescription;
        }

        public static String escape(String input) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                char c = input[i];
                int ci = (int) c;
                String cx = ci.ToString("X");
                if (c >= 1 && c <= 8) {
                    sb.Append("<" + cx + ">");
                } else if (c >= 0x0B && c <= 0x1F && c != 0x0D) {
                    sb.Append("<" + cx + ">");
                } else {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static bool isHexDigit(char c) {
            switch (c) {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'a':
                case 'A':
                case 'b':
                case 'B':
                case 'c':
                case 'C':
                case 'd':
                case 'D':
                case 'e':
                case 'E':
                case 'f':
                case 'F':
                return true;
            }
            return false;
        }

        private static bool isHexDigit(char c1, char c2) {
            return isHexDigit(c1) && isHexDigit(c2);
        }

        private static int toHexDigit(char c) {
            switch (c) {
                case '0':
                return 0;
                case '1':
                return 1;
                case '2':
                return 2;
                case '3':
                return 3;
                case '4':
                return 4;
                case '5':
                return 5;
                case '6':
                return 6;
                case '7':
                return 7;
                case '8':
                return 8;
                case '9':
                return 9;
                case 'a':
                case 'A':
                return 10;
                case 'b':
                case 'B':
                return 11;
                case 'c':
                case 'C':
                return 12;
                case 'd':
                case 'D':
                return 13;
                case 'e':
                case 'E':
                return 14;
                case 'f':
                case 'F':
                return 15;
                default:
                return 0;
            }
        }

        private static int toHexDigit(char c1, char c2) {
            return (toHexDigit(c1) << 4) | toHexDigit(c2);
        }

        public static String unescape(String input) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                if (i + 2 < input.Length) {
                    if (input[i] == '<' && isHexDigit(input[i + 1]) && input[i + 2] == '>') {
                        sb.Append((char) toHexDigit(input[i + 1]));
                        i += 2;
                        continue;
                    }
                }
                if (i + 3 < input.Length) {
                    if (input[i] == '<' && isHexDigit(input[i + 1], input[i + 2]) && input[i + 3] == '>') {
                        sb.Append((char) toHexDigit(input[i + 1], input[i + 2]));
                        i += 3;
                        continue;
                    }
                }
                sb.Append(input[i]);
            }
            return sb.ToString();
        }

        public static readonly String[] unitNames = new String[] { "Terran Marine", "Terran Ghost", "Terran Vulture", "Terran Goliath", "Goliath Turret", "Terran Siege Tank (Tank Mode)", "Siege Tank Turret (Tank Mode)", "Terran SCV", "Terran Wraith", "Terran Science Vessel", "Gui Montag (Firebat)", "Terran Dropship", "Terran Battlecruiser", "Spider Mine", "Nuclear Missile", "Terran Civilian", "Sarah Kerrigan (Ghost)", "Alan Schezar (Goliath)", "Alan Schezar Turret", "Jim Raynor (Vulture)", "Jim Raynor (Marine)", "Tom Kazansky (Wraith)", "Magellan (Science Vessel)", "Edmund Duke (Tank Mode)", "Edmund Duke Turret (Tank Mode)", "Edmund Duke (Siege Mode)", "Edmund Duke Turret (Siege Mode)", "Arcturus Mengsk (Battlecruiser)", "Hyperion (Battlecruiser)", "Norad II (Battlecruiser)", "Terran Siege Tank (Siege Mode)", "Siege Tank Turret (Siege Mode)", "Terran Firebat", "Scanner Sweep", "Terran Medic", "Zerg Larva", "Zerg Egg", "Zerg Zergling", "Zerg Hydralisk", "Zerg Ultralisk", "Zerg Broodling", "Zerg Drone", "Zerg Overlord", "Zerg Mutalisk", "Zerg Guardian", "Zerg Queen", "Zerg Defiler", "Zerg Scourge", "Torrasque (Ultralisk)", "Matriarch (Queen)", "Infested Terran", "Infested Kerrigan (Infested Terran)", "Unclean One (Defiler)", "Hunter Killer (Hydralisk)", "Devouring One (Zergling)", "Kukulza (Mutalisk)", "Kukulza (Guardian)", "Yggdrasill (Overlord)", "Terran Valkyrie", "Mutalisk Cocoon", "Protoss Corsair", "Protoss Dark Templar (Unit)", "Zerg Devourer", "Protoss Dark Archon", "Protoss Probe", "Protoss Zealot", "Protoss Dragoon", "Protoss High Templar", "Protoss Archon", "Protoss Shuttle", "Protoss Scout", "Protoss Arbiter", "Protoss Carrier", "Protoss Interceptor", "Protoss Dark Templar (Hero)", "Zeratul (Dark Templar)", "Tassadar/Zeratul (Archon)", "Fenix (Zealot)", "Fenix (Dragoon)", "Tassadar (Templar)", "Mojo (Scout)", "Warbringer (Reaver)", "Gantrithor (Carrier)", "Protoss Reaver", "Protoss Observer", "Protoss Scarab", "Danimoth (Arbiter)", "Aldaris (Templar)", "Artanis (Scout)", "Rhynadon (Badlands Critter)", "Bengalaas (Jungle Critter)", "Cargo Ship (Unused)", "Mercenary Gunship (Unused)", "Scantid (Desert Critter)", "Kakaru (Twilight Critter)", "Ragnasaur (Ashworld Critter)", "Ursadon (Ice World Critter)", "Lurker Egg", "Raszagal (Corsair)", "Samir Duran (Ghost)", "Alexei Stukov (Ghost)", "Map Revealer", "Gerard DuGalle (BattleCruiser)", "Zerg Lurker", "Infested Duran (Infested Terran)", "Disruption Web", "Terran Command Center", "Terran Comsat Station", "Terran Nuclear Silo", "Terran Supply Depot", "Terran Refinery", "Terran Barracks", "Terran Academy", "Terran Factory", "Terran Starport", "Terran Control Tower", "Terran Science Facility", "Terran Covert Ops", "Terran Physics Lab", "Starbase (Unused)", "Terran Machine Shop", "Repair Bay (Unused)", "Terran Engineering Bay", "Terran Armory", "Terran Missile Turret", "Terran Bunker", "Norad II (Crashed)", "Ion Cannon", "Uraj Crystal", "Khalis Crystal", "Infested Command Center", "Zerg Hatchery", "Zerg Lair", "Zerg Hive", "Zerg Nydus Canal", "Zerg Hydralisk Den", "Zerg Defiler Mound", "Zerg Greater Spire", "Zerg Queen's Nest", "Zerg Evolution Chamber", "Zerg Ultralisk Cavern", "Zerg Spire", "Zerg Spawning Pool", "Zerg Creep Colony", "Zerg Spore Colony", "Unused Zerg Building1", "Zerg Sunken Colony", "Zerg Overmind (With Shell)", "Zerg Overmind", "Zerg Extractor", "Mature Chrysalis", "Zerg Cerebrate", "Zerg Cerebrate Daggoth", "Unused Zerg Building2", "Protoss Nexus", "Protoss Robotics Facility", "Protoss Pylon", "Protoss Assimilator", "Unused Protoss Building1", "Protoss Observatory", "Protoss Gateway", "Unused Protoss Building2", "Protoss Photon Cannon", "Protoss Citadel of Adun", "Protoss Cybernetics Core", "Protoss Templar Archives", "Protoss Forge", "Protoss Stargate", "Stasis Cell/Prison", "Protoss Fleet Beacon", "Protoss Arbiter Tribunal", "Protoss Robotics Support Bay", "Protoss Shield Battery", "Khaydarin Crystal Formation", "Protoss Temple", "Xel'Naga Temple", "Mineral Field (Type 1)", "Mineral Field (Type 2)", "Mineral Field (Type 3)", "Cave (Unused)", "Cave-in (Unused)", "Cantina (Unused)", "Mining Platform (Unused)", "Independent Command Center (Unused)", "Independent Starport (Unused)", "Independent Jump Gate (Unused)", "Ruins (Unused)", "Khaydarin Crystal Formation (Unused)", "Vespene Geyser", "Warp Gate", "Psi Disrupter", "Zerg Marker", "Terran Marker", "Protoss Marker", "Zerg Beacon", "Terran Beacon", "Protoss Beacon", "Zerg Flag Beacon", "Terran Flag Beacon", "Protoss Flag Beacon", "Power Generator", "Overmind Cocoon", "Dark Swarm", "Floor Missile Trap", "Floor Hatch (Unused)", "Left Upper Level Door", "Right Upper Level Door", "Left Pit Door", "Right Pit Door", "Floor Gun Trap", "Left Wall Missile Trap", "Left Wall Flame Trap", "Right Wall Missile Trap", "Right Wall Flame Trap", "Start Location", "Flag", "Young Chrysalis", "Psi Emitter", "Data Disc", "Khaydarin Crystal", "Mineral Cluster Type 1", "Mineral Cluster Type 2", "Protoss Vespene Gas Orb Type 1", "Protoss Vespene Gas Orb Type 2", "Zerg Vespene Gas Sac Type 1", "Zerg Vespene Gas Sac Type 2", "Terran Vespene Gas Tank Type 1", "Terran Vespene Gas Tank Type 2" };

        public int StringIndex { get { return settings.lastKnownMapping[storageIndex]; } }

        public String OriginalContents { get { return escape(str); } }

        public String description { get { return escape(getDescription()); } }

        public String getDescription() {
            List<String> lst = new List<String>();
            if (unitIndexs.Length > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append(RelLanguage.LblUnitNames + " ");
                for (int i = 0; i < unitIndexs.Length; i++) {
                    int index = unitIndexs[i];
                    sb.Append("\"" + (index < unitNames.Length ? unitNames[index] : String.Format(RelLanguage.LblInvalidIndex, index)) + "\"");
                    if (i + 1 < unitIndexs.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
            }
            if (switchIndexes.Length > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append(RelLanguage.LblSwitchNames + " ");
                for (int i = 0; i < switchIndexes.Length; i++) {
                    int index = switchIndexes[i];
                    sb.Append(String.Format(RelLanguage.LblSwitchName, (index + 1)));
                    if (i + 1 < switchIndexes.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
            }
            if (locationIndexes.Length > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append(RelLanguage.LblLocationNames + " ");
                for (int i = 0; i < locationIndexes.Length; i++) {
                    int index = locationIndexes[i];
                    sb.Append(index == 63 ? RelLanguage.LblAnywhere : String.Format(RelLanguage.LblLocation, (index + 1)));
                    if (i + 1 < locationIndexes.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
            }
            if (triggerActionIndexes.Length > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append(RelLanguage.LblTriggerTexts + " ");
                for (int i = 0; i < triggerActionIndexes.Length; i++) {
                    int index = triggerActionIndexes[i];
                    sb.Append(String.Format(RelLanguage.LblTrigger, (index + 1)));
                    if (i + 1 < triggerActionIndexes.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
            }

            if (triggerCommentIndexes.Length > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append(RelLanguage.LblTriggerComments + " ");
                for (int i = 0; i < triggerCommentIndexes.Length; i++) {
                    int index = triggerCommentIndexes[i];
                    sb.Append(String.Format(RelLanguage.LblTriggerComment, (index + 1)));
                    if (i + 1 < triggerCommentIndexes.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
            }

            if (briefingActionIndexes.Length > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append(RelLanguage.LblBriefings + " ");
                for (int i = 0; i < briefingActionIndexes.Length; i++) {
                    int index = briefingActionIndexes[i];
                    sb.Append(String.Format(RelLanguage.LblBriefingTrigger, (index + 1)));
                    if (i + 1 < briefingActionIndexes.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
            }
            if (briefingCommentsIndexes.Length > 0) {
                /*
                StringBuilder sb = new StringBuilder();
                sb.Append("Briefing Comments: ");
                for (int i = 0; i < briefingCommentsIndexes.Length; i++) {
                    int index = briefingCommentsIndexes[i];
                    sb.Append("Briefing trigger comment " + (index + 1));
                    if (i + 1 < briefingCommentsIndexes.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
                */
            }

            if (forceNamesIndexes.Length > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append(RelLanguage.LblForceNames + " ");
                for (int i = 0; i < forceNamesIndexes.Length; i++) {
                    int index = forceNamesIndexes[i];
                    sb.Append(String.Format(RelLanguage.LblForceName, (index + 1)));
                    if (i + 1 < forceNamesIndexes.Length) {
                        sb.Append(", ");
                    }
                }
                lst.Add(sb.ToString());
            }
            if (isMapName) {
                lst.Add(RelLanguage.LblMapName);
            }
            if (isMapDescription) {
                lst.Add(RelLanguage.LblMapDescription);
            }

            StringBuilder tsb = new StringBuilder();
            for (int i = 0; i < lst.Count; i++) {
                tsb.Append(lst[i]);
                if (i + 1 < lst.Count) {
                    tsb.Append("\r\n");
                }
            }
            return tsb.ToString();
        }

        public StringList translations { get; set; }

        public class StringList {

            private TranslateString str;

            public StringList(TranslateString str) {
                this.str = str;
            }

            public String this[int index] {
                get {
                    return TranslateString.escape(str.settings.strings[index][str.storageIndex]);
                }

                set {
                    str.settings.strings[index][str.storageIndex] = TranslateString.unescape(value);
                }
            }

            public int Length {
                get { return str.settings.langauges.Length; }
            }
        }
    }

}

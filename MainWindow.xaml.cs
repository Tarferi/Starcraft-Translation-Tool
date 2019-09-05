﻿using Microsoft.Win32;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Globalization;
using QChkUI;
using System.ComponentModel;
using TranslatorUI;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls.Primitives;
#if DEBUG_STR_REMAP
using System.Diagnostics;
#endif

namespace WpfApplication1 {

    enum AppState {
        START,
        PICK_MAP,
        LOADING_MAP,
        READY,
        PROCESSING
    }


    public partial class MainWindow : Window {

        public static readonly string Version = Assembly.GetEntryAssembly().GetName().Version.Major.ToString() + "." + Assembly.GetEntryAssembly().GetName().Version.Minor.ToString() + "." + Assembly.GetEntryAssembly().GetName().Version.Build.ToString() + "." + Assembly.GetEntryAssembly().GetName().Version.Revision.ToString() + " BETA";

        public RelLanguage RelLanguage { get; set; }
        public bool isEnglish { get { return RelLanguage.languageName == "en"; } set { } }
        public bool isKorean { get { return RelLanguage.languageName == "ko"; } set { } }

        public MainWindow() {
            InitializeComponent();
            setLanguage("en");
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
        }

        public void asyncFoundUpdate() {
            new UpdateWindow(RelLanguage).ShowDialog();
        }

        private void setLanguage(String lng) {
            RelLanguage = RelLanguage.fromResource(lng, lng);
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
        private Settings settings;
        private AsyncWorker async = new AsyncWorker();
        private SupportType _support = SupportType.Unsupported;
        private SupportType support {get { return _support; } set {
                _support = value;
                btnCheckCond.Visibility = value == SupportType.EUDEditor ? Visibility.Visible : Visibility.Collapsed;
        } }

        private void setState(AppState state) {
            this.state = state;
            disableEverything();
            if (state == AppState.START) {
                brwsBtnSaveSettings.IsEnabled = true;
                brwsBtnSettings.IsEnabled = true;
                txtSet.IsEnabled = true;
                brwsBtnSaveSettings.Content = RelLanguage.BtnOpen;

                clearTable();
            } else if (state == AppState.PICK_MAP) {
                brwsBtnSaveSettings.Content = RelLanguage.BtnClose;
                brwsBtnSaveSettings.IsEnabled = true;
                brwsBtnInMap.IsEnabled = true;
                txtInMap.IsEnabled = true;
                btnOpenMap.IsEnabled = true;

                clearTable();
            } else if (state == AppState.LOADING_MAP) {
                clearTable();

                progress.IsEnabled = true;
                progress.Visibility = Visibility.Visible;
            } else if(state == AppState.PROCESSING) {
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

        private void setSettings(Settings settings) {

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
            for(int i = 0; i < settings.langauges.Length; i++) {
                comboRunLanguage.Items.Add(settings.langauges[i]);
            }
            if(prevSelected != null) {
                comboRunLanguage.SelectedItem = prevSelected;
            } else if(settings.langauges.Length > 0) {
                comboRunLanguage.SelectedIndex = 0;
            }
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

            
            TranslateString[] strings = getCurrentStrings();
            /*
             
            int[] lastValues = new int[strings.Length];
             for(int i =0; i < strings.Length; i++) {
                lastValues[i] = strings[i].StringIndex;
            }
            settings.lastKnownMapping = lastValues;
            */
            /*
            // Get languages
            if (tblTrans.Columns.Count >= 3) {
                int languages = tblTrans.Columns.Count - 3;
                settings.langauges = new String[languages];
                settings.strings = new String[languages][];
                for (int i = 0; i < languages; i++) {
                    String languageName = tblTrans.Columns[3 + i].Header.ToString();
                    settings.langauges[i] = languageName;
                    settings.strings[i] = new String[strings.Length];
                    for (int o = 0; o < strings.Length; o++) {
                        String str = TranslateString.unescape(strings[o].translations[i]);
                        if (str == null) {
                            str = "";
                        }
                        settings.strings[i][o] = str;
                    }
                }
            } else {
                settings.langauges = new String[0]; // Some default values
                settings.strings = new String[0][];
            }
            */

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
                            if(remI == remMax) { // Nothing to remove
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

            IDisposable d = Dispatcher.DisableProcessing();
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
                textEditStyle.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
                c.EditingElementStyle = textEditStyle;

                tblTrans.Columns.Add(c);
                
            }

            for(int i = 0; i < settings.ts.Length; i++) {
                TranslateString ms = settings.ts[i];
                Values.Add(ms);
            }

            if(settings.useCondition < 256) { // Switch
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


            d.Dispose();

            applyFilters();
            fireTableUpdate();
        }

        private void applyFilters() {
            IDisposable d = Dispatcher.DisableProcessing();


            ICollectionView Itemlist = CollectionViewSource.GetDefaultView(tblTrans);
            Predicate<object> predicate;
            bool showMap = checkMapDetails == null ? true : (bool)checkMapDetails.IsChecked;
            bool showMessages = checkDisplayMessages == null ? true : (bool)checkDisplayMessages.IsChecked;
            bool showComments = checkComments == null ? true : (bool)checkComments.IsChecked;
            bool showLocations = checkLocations == null ? true : (bool)checkLocations.IsChecked;
            bool showSwitches = checkSwitches == null ? true : (bool)checkSwitches.IsChecked;
            bool showUnits = checkUnitNames == null ? true : (bool)checkUnitNames.IsChecked;


            predicate = new Predicate<object>(item => {
                TranslateString ms = (TranslateString)item;
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
            d.Dispose();
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
                MapString[] us = TheLib.getStrings(set.inpuPath);
                if (us == null) {
                    if (errorOnFail) {
                        showErrorMessageBox(RelLanguage.WinMapErr, RelLanguage.ErrUnsupportedMap);
                        setState(stateOnError);
                    }
                    return false;
                }
                if(!checkSessionStrings(set, us)) {
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
                    set = Settings.getBlank(fileName);
                    if (!set.saveToFile(fileName)) {
                        showErrorMessageBox(RelLanguage.WinSaveSettings, RelLanguage.ErrFailedToCreateSettings);
                    } else {
                        this.setSettings(set);
                        setState(AppState.PICK_MAP);
                    }
                }
            } else {
                setSettings(set);
                Func<object, object> asyncLoader = (object o) => {
                    return tryLoadingGivenMap(set, false, AppState.PICK_MAP);
                };
                Action<object> syncLoaderEnd = (object o) => { bool b = (bool)o; if (!b) { setState(AppState.PICK_MAP); } };
                setState(AppState.PROCESSING);
                //async.addJob(asyncLoader, null, syncLoaderEnd);
                syncLoaderEnd(asyncLoader(null));
            }
        }

        private void brwsBtnSaveSettings_Click(object sender, RoutedEventArgs e) { // Open settings clicked
            if (state == AppState.START) { // First load, open
                _syncTryLoadBegin();
            } else if (state == AppState.PICK_MAP) { // Picking map, close
                setState(AppState.START);
            } else if (state == AppState.READY) { // Awaiting input, close
                setState(AppState.START);
            }
        }

        private static readonly Regex _regex = new Regex("^-?[0-9]+$"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text) {
            return !_regex.IsMatch(text);
        }

        private void NumericInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !_regex.IsMatch(e.Text);
        }

        private void btnOpenMap_Click(object sender, RoutedEventArgs e) { // Load map click button (can also be close map)
            if (state == AppState.PICK_MAP) { // Load
                String map = getComboBox(txtInMap);
                if (map.Length > 0) {
                    tryLoadingGivenMap(getSettings(), true, AppState.PICK_MAP);
                }
            } else if (state == AppState.READY) { // Unload
                setState(AppState.PICK_MAP);
            }
        }

        private String getSettingsFile() {
            return getFile(RelLanguage.WinOpenSettingsDialogTitle, new String[] { "smt" }, new String[] { RelLanguage.FormatName });
        }

        private String getMapFile() {
            return getFile(RelLanguage.WinOpenMapDialogTitle, new String[] { "scx", "scm" }, new String[] {RelLanguage.BwMapName, RelLanguage.ScMapName });
        }

        private static String getFile(String title, String[] extension, String[] extensionDescriptions) {
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
                set.saveToFile(set.settingsPath);
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
            String f = getMapFile();
            if (f != null) {
                setComboText(txtInMap, f);
                btnOpenMap_Click(sender, null);
            }
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

        private void createNewLangaugeCB(String languageName, String copyOf) {
            Settings settings = getSettings();

            // Check langauge unuqie
            for (int i = 0; i < settings.langauges.Length; i++) {
                String existingLanguage = settings.langauges[i];
                if(existingLanguage == languageName) {
                    showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrFailedToCreateTranslationLanguageExists);
                    return;
                }
            }

            // Find copy of langauges
            String[] translationData = null;
            for (int i = 0; i < settings.langauges.Length; i++) {
                String existingLanguage = settings.langauges[i];
                if(existingLanguage == copyOf && copyOf != null) {
                    String[] languageData = settings.strings[i];
                    translationData = new String[languageData.Length];
                    for (int a = 0; a < translationData.Length; a++) {
                        translationData[a] = languageData[a];
                    }
                    break;
                }
            }
            if(translationData == null && copyOf == null) { // Use defaults

                TranslateString[] strings = getCurrentStrings();

                translationData = new String[strings.Length];
                for (int a = 0; a < translationData.Length; a++) {
                    translationData[a] = strings[a].str;
                }
            } else if(translationData == null) {
                showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrFailedToCreateTranslation);
                return;
            }

            // Have translation data, store in settings and reload
            // Update languages array
            String[] newLanguages = new String[settings.langauges.Length + 1];
            for (int i = 0; i < settings.langauges.Length; i++) {
                newLanguages[i] = settings.langauges[i];
            }
            newLanguages[newLanguages.Length - 1] = languageName;
            settings.langauges = newLanguages;

            // Update string array
            String[][] newStrings = new String[settings.strings.Length + 1][];
            for (int i = 0; i < settings.strings.Length; i++) {
                newStrings[i] = settings.strings[i];
            }
            newStrings[newStrings.Length - 1] = translationData;
            settings.strings = newStrings;

            setSettings(settings);
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            Settings settings = getSettings();

            String[] translations = new String[settings.langauges.Length];
            for(int i = 0; i < settings.langauges.Length; i++) {
                translations[i] = settings.langauges[i];
            }

            AddTranslationDialog dialog = new AddTranslationDialog(RelLanguage, translations, createNewLangaugeCB);
            dialog.ShowDialog();
        }

        private void deleteLangaugeCB(String languageName) {
            if(languageName != null) {
                Settings settings = getSettings();

                // Check langauge unuqie
                for (int i = 0; i < settings.langauges.Length; i++) {
                    String existingLanguage = settings.langauges[i];
                    if (existingLanguage == languageName) { // Found index, update

                        // First update language array
                        String[] newLanguages = new String[settings.langauges.Length - 1];
                        for (int o = 0; o < i; o++) {
                            newLanguages[o] = settings.langauges[o];
                        }
                        for (int o = i + 1; o < settings.langauges.Length; o++) {
                            newLanguages[o - 1] = settings.langauges[o];
                        }
                        settings.langauges = newLanguages;

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
            for (int i = 0; i < settings.langauges.Length; i++) {
                translations[i] = settings.langauges[i];
            }

            DeleteTranslationDialog dialog = new DeleteTranslationDialog(RelLanguage, translations, deleteLangaugeCB);
            dialog.ShowDialog();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e) {
            int index = comboRunLanguage.SelectedIndex;
            Settings settings = getSettings();
            if(settings.outputPath.Length == 0) {
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
            Settings set = getSettings();
            if (!TheLib.checkUsedCondition(set)) {
                showErrorMessageBox(RelLanguage.WinTranslation, RelLanguage.ErrGuardCannotBeUsed);
            } else {
                MessageBox.Show(RelLanguage.LblGuardCanBeUsed, RelLanguage.WinTranslation, MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        private void MenuItem_Click_4(object sender, RoutedEventArgs e) {
            setLanguage("en");
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e) {
            setLanguage("ko");
        }
    }

    public class InvertedBooleanToVisibilityConverter : IValueConverter {

        private BooleanToVisibilityConverter def = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Visibility vis = (Visibility) def.Convert(value, targetType, parameter, culture);
            if(vis == Visibility.Visible) {
                return Visibility.Collapsed;
            } else {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            Visibility vis = (Visibility)def.ConvertBack(value, targetType, parameter, culture);
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

        RelLanguage RelLanguage;

        public TranslateString(RelLanguage RelLanguage, Settings settings, MapString ms, int index) {
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

        private static String escape(String input) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                char c = input[i];
                int ci = (int)c;
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
                    return 10;
                case 'A':
                    return 10;
                case 'b':
                    return 11;
                case 'B':
                    return 11;
                case 'c':
                    return 12;
                case 'C':
                    return 12;
                case 'd':
                    return 13;
                case 'D':
                    return 13;
                case 'e':
                    return 14;
                case 'E':
                    return 14;
                case 'f':
                    return 15;
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
                        sb.Append((char)toHexDigit(input[i + 1]));
                        i += 2;
                        continue;
                    }
                }
                if (i + 3 < input.Length) {
                    if (input[i] == '<' && isHexDigit(input[i + 1], input[i + 2]) && input[i + 3] == '>') {
                        sb.Append((char)toHexDigit(input[i + 1], input[i + 2]));
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

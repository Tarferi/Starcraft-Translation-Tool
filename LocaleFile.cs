using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TranslatorUI {

    public class RelLanguage {

        private Dictionary<String, String> languageData;

        private String get(String key) {
            if (languageData.ContainsKey(key)) {
                return languageData[key];
            } else {
                return key;
            }
        }

        private RelLanguage(String languageName, Dictionary<String, String> data) {
            __internal = get;
            this.languageData = data;
            this.languageName = languageName;
        }

        public String languageName;

        public static RelLanguage fromString(String languageName, String contents) {
            Dictionary<String, String> languageData = new Dictionary<string, string>();
            String[] data = contents.Split('\n');
            foreach (String str in data) {
                String s = str.Trim();
                if (s.Length > 0) {
                    if (s[0] != '#') {
                        String[] n = s.Split('=');
                        if (n.Length > 1) {
                            String key = n[0];
                            String value = s.Substring(key.Length + 1);
                            languageData.Add(key, value);
                        }
                    }
                }
            }
            return new RelLanguage(languageName, languageData);
        }

        public static RelLanguage fromResource(String languageName, String file) {
            try {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("lng_" + file + ".txt"));

                using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        String result = reader.ReadToEnd();
                        return RelLanguage.fromString(languageName, result);
                    }
                }
            } catch(Exception) { // Fallback to defaults
            }
            return new RelLanguage(languageName, new Dictionary<string, string>());
        }

        private Func<String, String> __internal;

        public String WinTitle { get { return __internal("WinTitle"); } }
        public String WinControl { get { return __internal("WinControl"); } }
        public String WinTranslation { get { return __internal("WinTranslation"); } }
        public String WinFilters { get { return __internal("WinFilters"); } }
        public String WinProcess { get { return __internal("WinProcess"); } }
        public String WinAddTranslation { get { return __internal("WinAddTranslation"); } }
        public String WinConfirmDeletion { get { return __internal("WinConfirmDeletion"); } }

        public String BtnBrowse { get { return __internal("BtnBrowse"); } }
        public String BtnLoad { get { return __internal("BtnLoad"); } }
        public String BtnUnload { get { return __internal("BtnUnload"); } }
        public String BtnSave { get { return __internal("BtnSave"); } }
        public String BtnClose { get { return __internal("BtnClose"); } }
        public String BtnAddTranslation { get { return __internal("BtnAddTranslation"); } }
        public String BtnRemoveTranslation { get { return __internal("BtnRemoveTranslation"); } }
        public String BtnRun { get { return __internal("BtnRun"); } }
        public String BtnCheck { get { return __internal("BtnCheck"); } }
        public String BtnOpen { get { return __internal("BtnOpen"); } }
        public String BtnCreate { get { return __internal("BtnCreate"); } }
        public String BtnDelete { get { return __internal("BtnDelete"); } }

        public String FilterShowMapDetails { get { return __internal("FilterShowMapDetails"); } }
        public String FilterShowTriggerText { get { return __internal("FilterShowTriggerText"); } }
        public String FilterShowTriggerComments { get { return __internal("FilterShowTriggerComments"); } }
        public String FilterShowLocationNames { get { return __internal("FilterShowLocationNames"); } }
        public String FilterShowSwitchNames { get { return __internal("FilterShowSwitchNames"); } }
        public String FilterShowUnitNames { get { return __internal("FilterShowUnitNames"); } }

        public String LblLanguage { get { return __internal("LblLanguage"); } }
        public String LblLoadedSettings { get { return __internal("LblLoadedSettings"); } }
        public String LblLoadedMap { get { return __internal("LblLoadedMap"); } }
        public String LblOutputMap { get { return __internal("LblOutputMap"); } }
        public String LblGuard { get { return __internal("LblGuard"); } }
        public String LblGuardSwitch { get { return __internal("LblGuardSwitch"); } }
        public String LblGuardDeaths { get { return __internal("LblGuardDeaths"); } }
        public String LblDefaultValues { get { return __internal("LblDefaultValues"); } }
        public String LblDefault { get { return __internal("LblDefault"); } }
        public String LblConfirmDeletion { get { return __internal("LblConfirmDeletion"); } }

        public String TableUsage { get { return __internal("TableUsage"); } }
        public String TableOriginal { get { return __internal("TableOriginal"); } }

        public String ErrMissingStrings { get { return __internal("ErrMissingStrings"); } }
        public String ErrMissingMapStrings { get { return __internal("ErrMissingMapStrings"); } }
        public String ErrRemapNeeded { get { return __internal("ErrRemapNeeded"); } }
        public String ErrConfirmTranslationUpdate { get { return __internal("ErrConfirmTranslationUpdate"); } }
        public String WinMapErr { get { return __internal("WinMapErr"); } }
        public String ErrUnsupportedMap { get { return __internal("ErrUnsupportedMap"); } }
        public String ErrMapFailedToLoad { get { return __internal("ErrMapFailedToLoad"); } }
        public String WinLoadSettings { get { return __internal("WinLoadSettings"); } }
        public String LblNewSettingsCreate { get { return __internal("LblNewSettingsCreate"); } }
        public String WinSaveSettings { get { return __internal("WinSaveSettings"); } }
        public String ErrFailedToCreateSettings { get { return __internal("ErrFailedToCreateSettings"); } }
        public String LblSettingsSaved { get { return __internal("LblSettingsSaved"); } }
        public String ErrFailedToCreateTranslation { get { return __internal("ErrFailedToCreateTranslation"); } }
        public String ErrFailedToCreateTranslationLanguageExists { get { return __internal("ErrFailedToCreateTranslationLanguageExists"); } }

        public String ErrTranslationFailed { get { return __internal("ErrTranslationFailed"); } }
        public String ErrInvalidTargetLanguage { get { return __internal("ErrInvalidTargetLanguage"); } }
        public String LblTranslationDone { get { return __internal("LblTranslationDone"); } }
        public String ErrGuardCannotBeUsed { get { return __internal("ErrGuardCannotBeUsed"); } }
        public String LblGuardCanBeUsed { get { return __internal("LblGuardCanBeUsed"); } }

        public String LblUnitNames { get { return __internal("LblUnitNames"); } }
        public String LblSwitchNames { get { return __internal("LblSwitchNames"); } }
        public String LblLocationNames { get { return __internal("LblLocationNames"); } }
        public String LblInvalidIndex { get { return __internal("LblInvalidIndex"); } }
        public String LblTriggerTexts { get { return __internal("LblTriggerTexts"); } }
        public String LblTriggerComments { get { return __internal("LblTriggerComments"); } }
        public String LblBriefings { get { return __internal("LblBriefings"); } }
        public String LblBriefingComments { get { return __internal("LblBriefingComments"); } }
        public String LblForceNames { get { return __internal("LblForceNames"); } }
        public String LblUnitName { get { return __internal("LblUnitName"); } }
        public String LblSwitchName { get { return __internal("LblSwitchName"); } }
        public String LblTrigger { get { return __internal("LblTrigger"); } }
        public String LblLocation { get { return __internal("LblLocation"); } }
        public String LblTriggerComment { get { return __internal("LblTriggerComment"); } }
        public String LblBriefingTrigger { get { return __internal("LblBriefingTrigger"); } }
        public String LblForceName { get { return __internal("LblForceName"); } }
        public String LblMapName { get { return __internal("LblMapName"); } }
        public String LblMapDescription { get { return __internal("LblMapDescription"); } }
        public String LblAnywhere { get { return __internal("LblAnywhere"); } }
        public String LblPlayer { get { return __internal("LblPlayer"); } }
        public String WinOpenSettingsDialogTitle { get { return __internal("WinOpenSettingsDialogTitle"); } }
        public String FormatName { get { return __internal("FormatName"); } }

        public String WinOpenMapDialogTitle { get { return __internal("WinOpenMapDialogTitle"); } }
        public String WinSaveMapDialogTitle { get { return __internal("WinSaveMapDialogTitle"); } }
        public String BwMapName { get { return __internal("BwMapName"); } }
        public String ScMapName { get { return __internal("ScMapName"); } }
        public String ScMapFile { get { return __internal("ScMapFile"); } }


        public String menuMainMenu { get { return __internal("menuMainMenu"); } }
        public String menuExit { get { return __internal("menuExit"); } }
        public String menuLanguage { get { return __internal("menuLanguage"); } }
        public String menuEnglish { get { return __internal("menuEnglish"); } }
        public String menuKorean { get { return __internal("menuKorean"); } }
        public String menuHelp { get { return __internal("menuHelp"); } }
        public String menuForum { get { return __internal("menuForum"); } }
        public String menuUpdate { get { return __internal("menuUpdate"); } }
        public String menuAbout { get { return __internal("menuAbout"); } }

        public String WinUpdateTitle { get { return __internal("WinUpdateTitle"); } }
        public String BtnUpdate { get { return __internal("BtnUpdate"); } }
        public String LblCurrentVersion { get { return __internal("LblCurrentVersion"); } }
        public String LblRemoteVersion { get { return __internal("LblRemoteVersion"); } }
        public String LblUpdateProgress { get { return __internal("LblUpdateProgress"); } }
        public String BtnRestart { get { return __internal("BtnRestart"); } }
        public String ErrorUpdateGetRemoteVersion { get { return __internal("ErrorUpdateGetRemoteVersion"); } }
        public String ErrorFailedToDownloadUpdate { get { return __internal("ErrorFailedToDownloadUpdate"); } }
        public String ErrorUpdateInvalidDownload { get { return __internal("ErrorUpdateInvalidDownload"); } }
        public String ErrorFailedToRunUpdator { get { return __internal("ErrorFailedToRunUpdator"); } }

        public String WinAbout { get { return __internal("WinAbout"); } }
        public String LblCreator { get { return __internal("LblCreator"); } }
        public String LblVersion { get { return __internal("LblVersion"); } }
        public String LblProduct { get { return __internal("LblProduct"); } }
        public String LblAutoUpdate { get { return __internal("LblAutoUpdate"); } }
        public String WinDeleteTranslation { get { return __internal("WinDeleteTranslation"); } }
        public String ErrNoOutputMap { get { return __internal("ErrNoOutputMap"); } }

        public String BtnExportTranslation { get { return __internal("BtnExportTranslation"); } }
        public String BtnImportTranslation { get { return __internal("BtnImportTranslation"); } }

        public String WinExport { get { return __internal("WinExport"); } }
        public String BtnExport { get { return __internal("BtnExport"); } }
        public String ExcelFile { get { return __internal("ExcelFile"); } }
        public String ErrorExportFailed { get { return __internal("ErrorExportFailed"); } }
        public String LblExportFinish { get { return __internal("LblExportFinish"); } }

        public String BtnImport { get { return __internal("BtnImport"); } }
        public String LblConfirmOverWrite { get { return __internal("LblConfirmOverWrite"); } }
        public String WinImport { get { return __internal("WinImport"); } }

        public String ErrorImportFailed { get { return __internal("ErrorImportFailed"); } }
        public String LblImportFinished { get { return __internal("LblImportFinished"); } }
        public String ErrorInvalidImportData { get { return __internal("ErrorInvalidImportData"); } }

        public String WinError { get { return __internal("WinError"); } }
        public String LblErrorUncaughtException { get { return __internal("LblErrorUncaughtException"); } }
        public String LblErrorCrash { get { return __internal("LblErrorCrash"); } }
        public String BtnExit { get { return __internal("BtnExit"); } }
        public String LblErrorCallstackDetails { get { return __internal("LblErrorCallstackDetails"); } }


    }

}

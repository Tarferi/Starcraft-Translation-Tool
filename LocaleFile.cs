using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TranslatorUI {

    public class LanguageManager {

        public readonly RelLanguage[] Languages;

        public readonly RelLanguage DefaultLanguage = null;

        public bool Valid { get { return DefaultLanguage != null; } }

        public LanguageManager() {
            String[] resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            List<RelLanguage> languages = new List<RelLanguage>();
            foreach(String resource in resources) {
                if(resource.StartsWith("TranslatorUI.lng_") && resource.EndsWith(".txt")) {
                    try { 
                    RelLanguage rl = RelLanguage.fromResource(resource);
                        if (rl.__Valid) {
                            languages.Add(rl);
                            if (rl.__LanguageName == "English") {
                                DefaultLanguage = rl;
                            }
                        }
                    } catch (Exception) { }
                }
            }
            Languages = new RelLanguage[languages.Count];
            for(int i = 0; i < languages.Count; i++) {
                Languages[i] = languages[i];
            }
        }

    }

    public class RelLanguage : DynamicObject {

        private Dictionary<String, object> languageData;

        private RelLanguage(Dictionary<String, object> data) {
            this.languageData = data;
        }

        public String __LanguageName { get { return __Valid ? this.languageData["CurrentLanguage"].ToString() : "Invalid Language"; } }

        public bool __Valid { get { return this.languageData.ContainsKey("CurrentLanguage"); } }

        public static RelLanguage fromString(String contents) {
            Dictionary<String, object> languageData = new Dictionary<string, object>();
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
            return new RelLanguage(languageData);
        }

        public static RelLanguage fromResource(String file) {
            try {
                var assembly = Assembly.GetExecutingAssembly();
                //string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("lng_" + file + ".txt"));
                string resourceName = file;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        String result = reader.ReadToEnd();
                        return RelLanguage.fromString(result);
                    }
                }
            } catch (Exception) { // Fallback to defaults
            }
            return new RelLanguage(new Dictionary<string, object>());
        }



        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (languageData.ContainsKey(binder.Name)) {
                result = languageData[binder.Name];
                return true;
            }
            result = binder.Name;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            return false;
        }
    }
}

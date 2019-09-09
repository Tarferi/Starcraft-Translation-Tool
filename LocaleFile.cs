using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TranslatorUI {

    public class RelLanguage : DynamicObject {

        private Dictionary<String, object> languageData;

        private RelLanguage(String languageName, Dictionary<String, object> data) {
            this.languageData = data;
            this.languageName = languageName;
        }

        public String languageName;

        public static RelLanguage fromString(String languageName, String contents) {
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
            } catch (Exception) { // Fallback to defaults
            }
            return new RelLanguage(languageName, new Dictionary<string, object>());
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

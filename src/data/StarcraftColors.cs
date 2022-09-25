using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace TranslatorData {
    class StarcraftColors {

        public class StarcraftColor {
            public readonly int key;
            public readonly int color;
            public readonly String description;
            public readonly Color Color;
            public readonly bool Valid;

            public StarcraftColor(int key, int hexColor, String description) {
                this.key = key;
                this.color = hexColor;
                this.description = description;
                Color = Color.FromArgb(hexColor);
                Valid = key >= 0;
            }
        }

        public readonly StarcraftColor[] TitleColors = new StarcraftColor[0xff];
        public readonly StarcraftColor[] BriefingColors = new StarcraftColor[0xff];
        public readonly StarcraftColor[] GameColors = new StarcraftColor[0xff];
        public readonly StarcraftColor DefaultColor;

        private readonly Dictionary<int, StarcraftColor> titleColors = new Dictionary<int, StarcraftColor>();
        private readonly Dictionary<int, StarcraftColor> briefingColors = new Dictionary<int, StarcraftColor>();
        private readonly Dictionary<int, StarcraftColor> gameColors = new Dictionary<int, StarcraftColor>();


        public StarcraftColor getBriefingColor(Color c) {
            int argb = c.ToArgb() & 0xffffff;
            if (briefingColors.ContainsKey(argb)) {
                return briefingColors[argb];
            }
            if(argb == DefaultColor.color) {
                return DefaultColor;
            }
            return null;
        }

        public StarcraftColor getTitleColor(Color c) {
            int argb = c.ToArgb() & 0xffffff;
            if (titleColors.ContainsKey(argb)) {
                return titleColors[argb];
            }
            if (argb == DefaultColor.color) {
                return DefaultColor;
            }
            return null;
        }

        public StarcraftColor getGameColor(Color c) {
            int argb = c.ToArgb() & 0xffffff;
            if (gameColors.ContainsKey(argb)) {
                return gameColors[argb];
            }
            if (argb == DefaultColor.color) {
                return DefaultColor;
            }
            return null;
        }

        private bool isGood;

        private StarcraftColors() {
            isGood = false;
            try {
                String[] data = TranslatorUI.Properties.Resources.colors.Split('\n');
                Dictionary<String, List<StarcraftColor>> items = new Dictionary<string, List<StarcraftColor>>();
                String lastKey = "";
                foreach(String _line in data) {
                    String line = _line.Trim();
                    if (line.EndsWith(":")) {
                        lastKey = line.Split(':')[0];
                        continue;
                    }
                    String[] l = line.Split('=');
                    if (l.Length > 1) {
                        String key = l[0];
                        l = l[1].Split(':');
                        String color = l[0];
                        String description = l[1];
                        int clr;
                        int k;
                        if (color.StartsWith("0x")) {
                            color = color.Substring(2);
                        }
                        if (key.StartsWith("0x")) {
                            key = key.Substring(2);
                        }
                        if (!Int32.TryParse(color, NumberStyles.HexNumber, null, out clr) || !Int32.TryParse(key, NumberStyles.HexNumber, null, out k)) {
                            return;
                        }
                        if (!items.ContainsKey(lastKey)) {
                            items.Add(lastKey, new List<StarcraftColor>());
                        }
                        items[lastKey].Add(new StarcraftColor(k, clr, description));
                    }
                }

                if(!items.ContainsKey("title") || !items.ContainsKey("briefing") || !items.ContainsKey("game")) {
                    return;
                }
                DefaultColor = new StarcraftColor(-1, 0, "Invalid Color Code");
                for (int i = 0; i < TitleColors.Length; i++) {
                    TitleColors[i] = DefaultColor;
                }
                for (int i = 0; i < BriefingColors.Length; i++) {
                    BriefingColors[i] = DefaultColor;
                }
                for (int i = 0; i < GameColors.Length; i++) {
                    GameColors[i] = DefaultColor;
                }
                foreach (StarcraftColor color in items["title"]) {
                    TitleColors[color.key] = color;
                    titleColors[color.color] = color;
                }
                foreach (StarcraftColor color in items["briefing"]) {
                    BriefingColors[color.key] = color;
                    briefingColors[color.color] = color;
                }
                foreach (StarcraftColor color in items["game"]) {
                    GameColors[color.key] = color;
                    gameColors[color.color] = color;
                }
                isGood = true;
            } catch (Exception) { // Fallback to defaults
            }
            
        }

        public static StarcraftColors load() {
            StarcraftColors c = new StarcraftColors();
            if (c.isGood) {
                return c;
            }
            return null;
        }
    }
}

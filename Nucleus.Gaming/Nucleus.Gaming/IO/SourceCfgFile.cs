﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nucleus.Gaming.IO {
    /// <summary>
    /// Reads and modifies parameters in a Source Engine configuration
    /// file (*.cfg)
    /// </summary>
    public class SourceCfgFile {
        protected string path;
        protected string rawData;
        protected string backupData;
        private Dictionary<string, List<SaveInfo>> sections;

        public string RawData {
            get { return rawData; }
        }

        public SourceCfgFile(string filePath) {
            path = filePath;
            sections = new Dictionary<string, List<SaveInfo>>();

            if (File.Exists(path)) {
                rawData = File.ReadAllText(path);
                backupData = string.Copy(rawData);

                Parse(backupData);
            } else {
                rawData = "";
                backupData = "";
            }
        }

        private void Parse(string data) {
            List<SaveInfo> currentSection = null;
            string currentSectionName = null;

            int currentIndex = 0;
            int nextBlockEnd;
            for (; ; )
            {
                int nextQuotes = data.IndexOf('"', currentIndex);
                if (nextQuotes == -1) {
                    break;
                }

                int delta = nextQuotes - currentIndex;

                if (currentSection == null) {
                    if (delta > 1) {
                        currentSectionName = data.Substring(currentIndex, nextQuotes - currentIndex);
                        currentSection = new List<SaveInfo>();
                        sections.Add(currentSectionName, currentSection);

                        nextBlockEnd = data.IndexOf('}', nextQuotes);
                    }
                } else {
                    if (delta > 1) {
                        string propertyName = data.Substring(currentIndex, nextQuotes - currentIndex);
                        if (!string.IsNullOrWhiteSpace(propertyName) && !propertyName.Contains("{")) {
                            // read to the right
                            int start = data.IndexOf('"', nextQuotes + 1);
                            int end = data.IndexOf('"', start + 1);

                            string propertyValue = data.Substring(start + 1, end - start - 1);
                            SaveInfo info = new SaveInfo(currentSectionName, propertyName, propertyValue);
                            currentSection.Add(info);

                            nextQuotes = end;
                        } else if (propertyName.Contains("}")) {
                            break;
                        }
                    }
                }

                currentIndex = nextQuotes + 1;
            }
        }

        /// <summary>
        /// Flushes all changes to disk
        /// </summary>
        public void Save(string newPath = "") {
            if (string.IsNullOrEmpty(newPath)) {
                newPath = path;
            }

            if (File.Exists(newPath)) {
                File.Delete(newPath);
            }

            using (Stream str = File.OpenWrite(newPath)) {
                using (StreamWriter writer = new StreamWriter(str)) {
                    foreach (var pair in sections) {
                        writer.WriteLine($"\"{pair.Key}\"");
                        writer.WriteLine("{");
                        var list = pair.Value;

                        for (int i = 0; i < list.Count; i++) {
                            SaveInfo info = list[i];
                            writer.WriteLine($"\r\"{info["Key"]}\"  \"{info["Value"]}\"");
                        }

                        writer.WriteLine("}");
                    }


                    writer.Flush();
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Reverts the data to the initially read data
        /// </summary>
        public void RevertToBackup() {
            rawData = string.Copy(backupData);
        }

        public void ChangeProperty(SaveInfo source) {
            string section = source["Section"];
            string key = source["Key"];
            string value = source["Value"];

            List<SaveInfo> infos;
            if (!sections.TryGetValue(section, out infos)) {
                infos = new List<SaveInfo>();
                sections.Add(section, infos);
            }

            SaveInfo info = infos.FirstOrDefault(c => c["Key"] == key);
            if (info == null) {
                infos.Add(new SaveInfo(section, key, value));
            } else {
                info["Value"] = value;
            }
            //int start;
            //int end;
            //if (GetPosition(rawData, propertyName, out start, out end))
            //{
            //    rawData = rawData.Remove(start, end - start);
            //    rawData = rawData.Insert(start, value);

            //    return true;
            //}
            //else
            //{
            //    // search for the section name
            //    if (GetPosition(rawData, section, out start, out end))
            //    {
            //        // write the property

            //    }
            //    else
            //    {
            //        // write section
            //        rawData += section + "{ }";

            //        // now write the property
            //        ChangeProperty(section, propertyName, value);
            //    }
            //}
            //return false;
        }

        private bool GetPosition(string text, string word, out int start, out int end) {
            start = -1;
            end = 0;

            int def = text.IndexOf(word);
            if (def == -1) {
                return false;
            }

            int aspas = 0;
            bool firstNumber = true;
            for (int i = def; i < text.Length; i++) {
                char c = text[i];
                if (start == -1) {
                    if (c == '"') {
                        aspas++;
                        if (aspas == 2) {
                            start = i;
                        }
                    }
                } else {
                    if (StringUtil.IsNumber(c) && firstNumber) {
                        firstNumber = false;
                        start = i;
                    }
                    if (!StringUtil.IsNumber(c)) {
                        end = i;
                        break;
                    }
                }
            }

            return true;
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Nucleus {
    public static class StringUtil {
        public static string LeftRepeat(string str, string toRepeat, int nCharMax) {
            string result = "";
            for (int i = 0; i < nCharMax - str.Length; i++) {
                result = result + toRepeat;
            }

            return result + str;
        }

        public static string Capitalize(string str) {
            return str[0].ToString().ToUpperInvariant() + str.Remove(0, 1);
        }

        public static string Num2(int number) {
            string strNumber = number.ToString();
            if (strNumber.Length == 1) {
                return "0" + strNumber;
            }
            return strNumber;
        }

        public static string[] Split(string str, char separator, int maxDepth) {
            List<string> splitted = new List<string>();

            int index = 0;
            int start = 0;
            for (int i = 0; i < maxDepth; i++) {
                if (start >= str.Length) {
                    if (str.Length - index > 0) {
                        splitted.Add(str.Remove(0, index + 1));
                    }
                    break;
                }

                int nextSeparator = str.IndexOf(separator, start);
                if (nextSeparator == -1) {
                    if (str.Length - index > 0) {
                        splitted.Add(str.Remove(0, index + 1));
                    }
                    break;
                }

                string firstToNext = str.Substring(0, nextSeparator);
                index = nextSeparator;
                start = nextSeparator + firstToNext.Length;
                splitted.Add(firstToNext);
            }

            return splitted.ToArray();
        }

        public static string ClearTextString(string strValue) {
            while (strValue.Contains("\t")) {
                strValue = strValue.Replace("\t", "");
            }
            return strValue;
        }

        public static string ClearStartSpaces(string strValue) {
            while (strValue[0] == ' ') {
                strValue = strValue.Remove(0, 1);
            }
            return strValue;
        }

        public static string GetCollisionStringStart(string aStr, string bStr) {
            string col = "";

            for (int i = 0; i < aStr.Length; i++) {
                if (bStr.Length <= i) {
                    break;
                }

                char a = aStr[i];
                char b = bStr[i];
                if (a == b) {
                    col += a;
                } else {
                    break;
                }
            }

            return col;
        }

        public static readonly char[] Numbers = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static bool IsNumber(char c) {
            // We COULD use Int.TryParse, but this looks way cleaner
            return Numbers.Contains(c);
        }

        public static string RepeatCharacter(char c, int times) {
            string s = "";
            for (int i = 0; i < times; i++) {
                s += c;
            }
            return s;
        }

        public static void MakeSameSize(ref string a, ref string b) {
            if (a.Length < b.Length) {
                int dif = b.Length - a.Length;
                for (int k = 0; k < dif; k++) {
                    a = " " + a;
                }
            } else if (a.Length > b.Length) {
                int dif = a.Length - b.Length;
                for (int k = 0; k < dif; k++) {
                    b = " " + b;
                }
            }
        }

        public static string ReplaceCaseInsensitive(string str, string toFind, string toReplace) {
            string lowerOriginal = str.ToLower();
            string lowerFind = toFind.ToLower();
            string lowerRep = toReplace.ToLower();

            int start = lowerOriginal.IndexOf(lowerFind);
            if (start == -1) {
                return str;
            }

            string end = str.Remove(start, toFind.Length);
            end = end.Insert(start, toReplace);

            return end;
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int ComputeLevenshteinDistance(string s, string t) {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) {
                return m;
            }

            if (m == 0) {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) {
            }

            for (int j = 0; j <= m; d[0, j] = j++) {
            }

            // Step 3
            for (int i = 1; i <= n; i++) {
                //Step 4
                for (int j = 1; j <= m; j++) {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static string ArrayToString(Array array) {
            string str = "";
            for (int i = 0; i < array.Length; i++) {
                object ob = ((IList)array)[i];
                str += ob;
                if (i != array.Length - 1) {
                    str += ", ";
                }
            }
            return str;
        }

        /// <summary>
        /// For strings like ====== TEST ======
        /// </summary>
        /// <param name="baseStr"></param>
        /// <param name="toFill"></param>
        /// <returns></returns>
        public static string FillWithLeftAndRight(string baseStr, char toFill, int maxWidth) {
            int toAdd = maxWidth - baseStr.Length - 2;
            if (toAdd < 0) {
                return baseStr;
            }

            baseStr = ' ' + baseStr + ' ';

            toAdd /= 2;
            // add at the end
            for (int i = 0; i < toAdd; i++) {
                baseStr = baseStr + toFill;
            }
            for (int i = 0; i < toAdd; i++) {
                baseStr = toFill + baseStr;
            }

            return baseStr;
        }

        public static string MirrorChars(char c, int width, char inBetween = ' ') {
            string s = c.ToString();

            for (int i = 0; i < width - 2; i++) {
                s = s + inBetween;
            }

            s = s + c;

            return s;
        }

        public static string FillToRight(string baseStr, char c, int maxWidth) {
            int toAdd = maxWidth - baseStr.Length;

            for (int i = 0; i < toAdd; i++) {
                baseStr = baseStr + c;
            }

            return baseStr;
        }

        public static string AddCharacterEachCount(string baseStr, string toAdd, int each, int minimum) {
            if (baseStr.Length < minimum) {
                return baseStr;
            }

            string newStr = baseStr;
            for (int i = newStr.Length - each; i > 0; i -= each) {
                newStr = newStr.Insert(i, toAdd);
            }
            return newStr;
        }
        
        public static string GetOrdinal(int placement) {
            if (placement == 1) {
                return placement + "st";
            } else if (placement == 2) {
                return placement + "nd";
            }
            else {
                return placement + "th";
            }
        }

#if WINFORMS
        /// <summary>
        /// This method can be made better
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="str"></param>
        /// <param name="graphics"></param>
        /// <param name="font"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string WrapString(float maxWidth, string str, Graphics graphics, Font font, out SizeF size) {
            size = graphics.MeasureString(str, font);

            string[] sep = str.Split(' ');
            if (sep.Length == 0) {
                return str;
            }

            float spaceSize = graphics.MeasureString(" ", font).Width;

            string result = sep[0];

            float currentWidth = graphics.MeasureString(result, font).Width;
            float maxUsedWidth = 0;
            int lines = 1;
            string currentLine = result;

            for (int i = 1; i < sep.Length; i++) {
                string word = sep[i];
                string spaceWord = " " + word;
                SizeF wordSize = graphics.MeasureString(spaceWord, font);
                currentWidth += wordSize.Width;

                if (currentWidth > maxWidth) {
                    currentWidth = wordSize.Width;
                    maxUsedWidth = Math.Max(maxUsedWidth, graphics.MeasureString(currentLine, font).Width);
                    result += "\n" + word;
                    currentLine = "";
                    lines++;
                } else {
                    result += spaceWord;
                    currentLine += spaceWord;
                }
            }

            if (maxUsedWidth == 0) {
                maxUsedWidth = Math.Max(maxUsedWidth, graphics.MeasureString(currentLine, font).Width);
            }
            size = new SizeF(maxUsedWidth, lines * font.GetHeight(graphics));

            return result;
        }
#endif
    }
}

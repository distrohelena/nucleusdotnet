using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus {
    public static class DateUtils {
        private static char[] Numbers = new char[] {
            '0','1','2','3','4','5','6','7','8','9'
        };

        public static int ClearHour(string str) {
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                if (Array.IndexOf(Numbers, c) == -1) {
                    // remove
                    str = str.Remove(i, 1);
                    i--;
                }
            }
            return int.Parse(str);
        }


        public static string ToShortString(DateTime dt) {
            return $"{StringUtil.Num2(dt.Month)}/{StringUtil.Num2(dt.Day)}/{dt.Year}";
        }
    }
}

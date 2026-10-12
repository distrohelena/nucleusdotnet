using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus {
    public static class EnumUtils {
        public static T Parse<T>(string parameter) {
            return (T)Enum.Parse(typeof(T), parameter);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus {
    public static class ProcessUtil {
        public static Process[] GetAllProcesses(string name) {
            Process[] allProcs = Process.GetProcesses();
            return allProcs.Where(c => c.ProcessName.Contains(name)).ToArray();
        }
    }
}

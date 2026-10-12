using System.Runtime.InteropServices;

namespace Nucleus.Platform.Windows.Interop {
    /// <summary>
    /// Interop functionality for Windows 8.1+
    /// </summary>
    public static class ShcoreInterop {
        [DllImport("shcore.dll")]
        public static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);
    }
}

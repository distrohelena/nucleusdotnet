using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
#if WINFORMS
using System.Windows.Forms;
#endif

namespace Nucleus {
    public static class ApplicationUtil {
        static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = false
        };

        /// <summary>
        /// TODO: bring my binary serializer back into Nucleus.Gaming
        /// Converts an object to JSON, then a Base64 string that can be passed to a program as start parameters
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetObjectAsArgument(object data) {
            string json = JsonSerializer.Serialize(data, SerializerOptions);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static void PopulateObjectWithArgument(object target, string base64Str) {
            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }

            string base64 = Encoding.UTF8.GetString(Convert.FromBase64String(base64Str));
            object? updated = JsonSerializer.Deserialize(base64, target.GetType(), SerializerOptions);
            if (updated == null) {
                return;
            }

            CopyProperties(updated, target);
        }

        public static bool OnlyOneInstance() {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) {
#if WINFORMS
                MessageBox.Show("Nucleus Coop is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
#endif
                return true;
            }
            return false;
        }

        public static bool IsGameTasksApp() {
            string entryApp = Assembly.GetEntryAssembly().Location;
            return entryApp.ToLower().Contains("startgame");
        }

        public static string GetAppDataPath() {
#if ALPHA
            string entryApp = Assembly.GetEntryAssembly().Location;
            string local = Path.GetDirectoryName(entryApp);

            if (IsGameTasksApp()) {
                // game tasks application, move to correct folder
                return Path.Combine(local, "..", "data");
            } else {
                return Path.Combine(local, "data");
            }
#else
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Nucleus Coop");
#endif
        }

        static void CopyProperties(object source, object destination) {
            Type targetType = destination.GetType();

            foreach (PropertyInfo property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if (!property.CanWrite || !property.CanRead) {
                    continue;
                }

                object? value = property.GetValue(source);
                property.SetValue(destination, value);
            }
        }
    }
}

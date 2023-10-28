using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SyncIMEStatus.Helpers;

namespace SyncIMEStatus
{
    public class SettingJson
    {
        public List<HookKeyCode> KeyCodeList { get; set; } = new List<HookKeyCode>();
        private const string defaultFile = "default.json";
        private const string defaultFolder = "Default";

        public SettingJson() { }

        /// <summary>
        /// オブジェクトをJson形式で保存します
        /// </summary>
        /// <param name="destinationPath">保存先</param>
        public void SerializeToFile(string saveName = defaultFile)
        {
            StringBuilder sb = new StringBuilder();

            if (Directory.Exists(Environment.CurrentDirectory))
            {
                sb.Append(Path.Combine(Environment.CurrentDirectory, saveName));
            }
            else
            {
                sb.Append(Path.Combine(Assembly.GetEntryAssembly().GetDirectory(), saveName));
            }

            using (var writer = new StreamWriter(sb.ToString()))
            {
                writer.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        /// <summary>
        /// Jsonファイルからオブジェクトを復元します
        /// </summary>
        /// <param name="directory">Jsonファイルが存在するディレクトリ</param>
        /// <returns></returns>
        public Task DeserializeFromFileAsync(string fileName = defaultFile)
        {
            string loadPath;
            if (File.Exists(loadPath = Path.Combine(Environment.CurrentDirectory, fileName)))
            {
            }
            else if(File.Exists(loadPath = Path.Combine(Assembly.GetEntryAssembly().GetDirectory(), fileName)))
            {
            }
            else if (File.Exists(loadPath = Path.Combine(Assembly.GetEntryAssembly().GetDirectory(), defaultFolder, fileName)))
            {
            }
            else
            {
                return Task.CompletedTask;
            }

            Debug.WriteLine($"loading {loadPath}");

            return Task.Run(() =>
            {
                try
                {
                    using (var stream = new FileStream(loadPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(stream))
                    {
                        var obj = JsonConvert.DeserializeObject<SettingJson>(sr.ReadToEnd());
                        if (obj != null)
                        {
                            if (obj.KeyCodeList != null)
                                this.KeyCodeList = obj.KeyCodeList;

                            Debug.WriteLine($"{loadPath} was loaded.");
                        }
                    }
                }
                catch (FileNotFoundException) { }
                catch (UnauthorizedAccessException) { }
            });
        }
    }
}

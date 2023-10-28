using SyncIMEStatus.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SyncIMEStatus.Helpers
{
    public class ResourceHelper : INotifyPropertyChanged
    {
        public static ResourceHelper Instance { get; } = new ResourceHelper();

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public readonly ResourceManager Manager = new ResourceManager(typeof(Resources));
        private readonly Resources _resources = new Resources();
        public Resources Resources
        {
            get { return this._resources; }
        }

        public ObservableCollection<CultureInfo> CultureCollection { get; } = new ObservableCollection<CultureInfo>();

        private CultureInfo _uiCulture;
        public CultureInfo UICulture
        {
            get { return _uiCulture; }
            set
            {
                _uiCulture = value;
                try
                {
                    CultureInfo.CurrentUICulture = value;
                    CultureInfo.CurrentCulture = value;
                    ChangeCulture(value);
                }
                catch { }
                NotifyPropertyChanged();
            }
        }

        private ResourceHelper()
        {
            Resources.Culture = CultureInfo.CurrentUICulture;
            _uiCulture = CultureInfo.CurrentUICulture;
            FindLocalizedResourceFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        /// <summary>
        /// 対応しているリソースファイルの言語リストを取得して
        /// CultureCollectionに格納する
        /// </summary>
        public void FindLocalizedResourceFiles(string location)
        {
            var cts = new CancellationTokenSource(3000);
            try
            {
                FindLocalizedResourceFilesCore(location, cts);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private Task FindLocalizedResourceFilesCore(string location, CancellationTokenSource cts)
        {
            return Task.Run(() =>
            {
                CultureCollection.Add(new CultureInfo("en-US", false));

                var dirInfo = new DirectoryInfo(location);
                foreach (DirectoryInfo di in dirInfo.GetDirectories())
                {
                    try
                    {
                        var ci = new CultureInfo(di.Name, false);
                        if (ci == null) { continue; }
                        CultureCollection.Add(ci);
                    }
                    catch { }
                }
                foreach (CultureInfo ci in CultureCollection)
                {
                    if (ci.Equals(CultureInfo.CurrentUICulture))
                    {
                        UICulture = ci;
                    }
                }
            }, cts.Token).ContinueWith(t => cts.Dispose());
        }

        public static string GetString(string key)
        {
            return Instance?.Manager.GetString(key) ?? key;
        }

        /// <summary>
        /// リソースのカルチャを変更する
        /// </summary>
        public void ChangeCulture(CultureInfo newCulture)
        {
            //var oldCultureName = Resources.Culture.Name;
            Resources.Culture = newCulture;
            NotifyPropertyChanged("Resources");

            //NotifyUpdate?.Invoke(this, new EventArgs());
            //var oldCulture = new CultureInfo(oldCultureName);
        }
    }
}

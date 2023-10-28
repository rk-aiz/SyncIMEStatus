using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SyncIMEStatus.Helpers
{
    public static class AssemblyExtensions
    {
        public static string GetDirectory(this Assembly assembly)
        {
            return System.IO.Path.GetDirectoryName(assembly.Location);
        }
    }
}

using System.Text;

namespace SyncIMEStatus.Helpers
{
    public static class StringBuilderExtensions
    {
        public static void AppendPath(this StringBuilder sb, string value)
        {
            if (sb[sb.Length - 1] == System.IO.Path.DirectorySeparatorChar)
            {
                sb.Append(value);
            }
            else
            {
                sb.Append(System.IO.Path.DirectorySeparatorChar + value);
            }
        }
    }
}

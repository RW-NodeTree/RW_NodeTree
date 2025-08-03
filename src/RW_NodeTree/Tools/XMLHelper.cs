using System.Text.RegularExpressions;
using Verse;

namespace RW_NodeTree.Tools
{
    public static class XMLHelper
    {
        private static readonly Regex KeyFormateMatcher = new Regex(@"^[A-Za-z_][\w.-]*");
        public static bool IsVaildityKeyFormat(this string? key)
        {
            if (!key.NullOrEmpty())
            {
                MatchCollection matchCollection = KeyFormateMatcher.Matches(key);
                return matchCollection.Count == 1 && matchCollection[0].Value.Length == key!.Length;
            }
            return false;
        }
    }
}

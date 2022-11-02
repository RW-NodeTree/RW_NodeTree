using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.Tools
{
    public static class XMLHelper
    {
        public static bool IsVaildityKeyFormat(this string key)
        {
            if(!key.NullOrEmpty())
            {
                MatchCollection matchCollection = Regex.Matches(key, @"^[A-Za-z_][\w.-]*");
                return matchCollection.Count == 1 && matchCollection[0].Value.Length == key.Length;
            }
            return false;
        }
    }
}

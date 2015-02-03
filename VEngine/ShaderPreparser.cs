using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VDGTech
{
    public class ShaderPreparser
    {
        public static string Preparse(string source)
        {
            Regex includeMatcher = new Regex("\\#include (.+)\n");
            Match match = includeMatcher.Match(source);
            while(match.Success)
            {
                string includeFile = Preparse(Media.ReadAllText(match.Groups[1].Value.Trim()));
                source = source.Remove(match.Index, match.Length);
                source = source.Insert(match.Index, includeFile + "\r\n");
                match = includeMatcher.Match(source);
            }
            return source;
        }
    }
}

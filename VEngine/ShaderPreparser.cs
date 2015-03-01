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
        public static string Preparse(string filename, string source)
        {
            Regex includeMatcher = new Regex("\\#include (.+)\n");
            Match match = includeMatcher.Match(source);
            while(match.Success)
            {
                string includeFile = Preparse(match.Groups[1].Value.Trim(), Media.ReadAllText(match.Groups[1].Value.Trim()));
                source = source.Remove(match.Index, match.Length);
                source = source.Insert(match.Index, includeFile + "\r\n");
                match = includeMatcher.Match(source);
            }
            return PrependWithInfo(filename, source);
        }
        public static string Preparse(string source)
        {
            Regex includeMatcher = new Regex("\\#include (.+)\n");
            Match match = includeMatcher.Match(source);
            while(match.Success)
            {
                string includeFile = Preparse(match.Groups[1].Value.Trim(), Media.ReadAllText(match.Groups[1].Value.Trim()));
                source = source.Remove(match.Index, match.Length);
                source = source.Insert(match.Index, includeFile + "\r\n");
                match = includeMatcher.Match(source);
            }
            return PrependWithInfo("Main file", source);
        }

        public static string PrependWithInfo(string srcFile, string content)
        {
            content = content.Replace("\r\n", "\n");
            string[] split = content.Split('\n');
            for(int i = 0; i < split.Length; i++)
            {
                split[i] = split[i] +" //" + srcFile + ":" + (i + 1).ToString();
            }
            return string.Join("\r\n", split);
        }
    }
}

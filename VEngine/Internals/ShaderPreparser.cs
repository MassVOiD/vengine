using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VEngine
{
    public class ShaderPreparser
    {
        public static string Preparse(string filename, string source, Dictionary<string, string> exportedConsts)
        {
            Regex includeMatcher = new Regex("\\#include (.+)\n");
            Match match = includeMatcher.Match(source);
            while(match.Success)
            {
                string includeFile = Preparse(match.Groups[1].Value.Trim(), Media.ReadAllText(match.Groups[1].Value.Trim()), exportedConsts);
                source = source.Remove(match.Index, match.Length);
                source = source.Insert(match.Index, includeFile + "\r\n");
                match = includeMatcher.Match(source);
            }

            Regex exportedConstMatcher = new Regex("\\#pragma const ([A-z0-9_-]+) (.+)\n");
            match = exportedConstMatcher.Match(source);
            while(match.Success)
            {
                string constName = match.Groups[1].Value.Trim();
                string constValue = match.Groups[2].Value.Trim();
                source = source.Remove(match.Index, match.Length);
                source = source.Insert(match.Index, "#define " + constName + " " + constValue + "\r\n");
                match = exportedConstMatcher.Match(source);
                exportedConsts[constName] = constValue;
            }

            List<string[]> regexes = new List<string[]>();
            Regex regexReplaceMatcher = new Regex("\\#pragma regex replace (.+?) with (.+)\n");
            match = regexReplaceMatcher.Match(source);
            while(match.Success)
            {
                string from = match.Groups[1].Value.Trim();
                string to= match.Groups[2].Value.Trim();
                source = source.Remove(match.Index, match.Length);
                match = regexReplaceMatcher.Match(source);
                regexes.Add(new string[] { from, to });
            }

            Regex includeOnceMatcher = new Regex("\\#include_once (.+)\n");
            var included = new List<string>();
            match = includeOnceMatcher.Match(source);
            while(match.Success)
            {
                string file = match.Groups[1].Value.Trim();
                if(included.Contains(file))
                {
                    source = source.Remove(match.Index, match.Length);
                }
                else
                {
                    included.Add(file);
                    string includeFile = Preparse(match.Groups[1].Value.Trim(), Media.ReadAllText(match.Groups[1].Value.Trim()), exportedConsts);
                    source = source.Remove(match.Index, match.Length);
                    source = source.Insert(match.Index, includeFile + "\r\n");
                    match = includeMatcher.Match(source);
                }
            }
            foreach(var r in regexes)
            {
                source = Regex.Replace(source, r[0], r[1]);
            }
            return PrependWithInfo(filename, source);
        }

        public static string Preparse(string source, Dictionary<string, string> exportedConsts)
        {
            return Preparse("Main file", source, exportedConsts);
        }

        public static string PrependWithInfo(string srcFile, string content)
        {
            content = content.Replace("\r\n", "\n");
            string[] split = content.Split('\n');
            for(int i = 0; i < split.Length; i++)
            {
                string info = string.Format("//L[{0}]F[{1}]", i+1, srcFile);
                split[i] = split[i] + info;
            }
            return string.Join("\r\n", split);
        }
    }
}
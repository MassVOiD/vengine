using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

namespace VDGTech
{
    public class Media
    {
        public static string SearchPath;
        private static Dictionary<string, string> Map;

        public static string Get(string name)
        {
            if(Map == null)
                LoadFileMap();
            if(!Map.ContainsKey(name.ToLower()))
                throw new KeyNotFoundException(name);
            return Map[name.ToLower()];
        }
        public static List<string> QueryRegex(string query)
        {
            if(Map == null)
                LoadFileMap();
            List<string> outFiles = new List<string>();
            Regex regex = new Regex(query);
            foreach(var e in Map)
            {
                if(regex.IsMatch(e.Key))
                {
                    outFiles.Add(e.Value);
                }
            }
            if(outFiles.Count == 0)
            {
                throw new KeyNotFoundException(query);
            }
            return outFiles;
        }

        public static string ReadAllText(string name)
        {
            if(Map == null)
                LoadFileMap();
            if(!Map.ContainsKey(name.ToLower()))
                throw new KeyNotFoundException(name);
            return File.ReadAllText(Map[name.ToLower()]);
        }

        private static void LoadFileMap(string path = null)
        {
            path = path == null ? SearchPath : path;
            if(Map == null)
                Map = new Dictionary<string, string>();
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach(string file in files) if(!Map.ContainsKey(Path.GetFileName(file).ToLower()))
                    Map.Add(Path.GetFileName(file).ToLower(), Path.GetFullPath(file));
            foreach(string dir in dirs)
                LoadFileMap(dir);
        }
    }
}
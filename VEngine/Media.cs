using System.Collections.Generic;
using System.IO;

namespace VDGTech
{
    public class Media
    {
        private static Dictionary<string, string> Map;
        public static string SearchPath;

        public static string Get(string name)
        {
            if (Map == null) LoadFileMap();
            if (!Map.ContainsKey(name)) throw new KeyNotFoundException(name);
            return Map[name];
        }

        public static string ReadAllText(string name)
        {
            if (Map == null) LoadFileMap();
            if (!Map.ContainsKey(name)) throw new KeyNotFoundException(name);
            return File.ReadAllText(Map[name]);
        }

        private static void LoadFileMap(string path = null)
        {
            path = path == null ? SearchPath : path;
            if (Map == null) Map = new Dictionary<string, string>();
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string file in files) if(!Map.ContainsKey(Path.GetFileName(file))) Map.Add(Path.GetFileName(file), Path.GetFullPath(file));
            foreach (string dir in dirs) LoadFileMap(dir);
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace VEngine
{
    public class Media
    {
        public static string SearchPath;

        public static string Get(string name)
        {
            if(!CompletedLoading)
                LoadFileMap();
            if(!Map.ContainsKey(name.ToLower()))
                throw new KeyNotFoundException(name);
            return Map[name.ToLower()];
        }

        public static void LoadFileMap()
        {
            if(LoadingThread != null)
                LoadingThread.Join();
            else
            {
                LoadingThread = new Thread(new ThreadStart(() =>
                {
                    LoadFileMapImpl();
                }));
                LoadingThread.Start();
                LoadingThread.Join();
            }
        }

        public static List<string> QueryRegex(string query)
        {
            if(!CompletedLoading)
                LoadFileMap();
            lock (locker)
            {
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
        }

        public static string ReadAllText(string name)
        {
            if(!CompletedLoading)
                LoadFileMap();
            lock (locker)
            {
                if(!Map.ContainsKey(name.ToLower()))
                    throw new KeyNotFoundException(name);
                return File.ReadAllText(Map[name.ToLower()]);
            }
        }

        private static bool AlreadyLoading = false;
        private static bool CompletedLoading = false;
        private static Thread LoadingThread = null;
        private static object locker = new object();
        private static Dictionary<string, string> Map;

        private static void LoadFileMapImpl(string path = null)
        {
            path = path == null ? SearchPath : path;
            if(Map == null)
                Map = new Dictionary<string, string>();
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach(string file in files)
                if(!Map.ContainsKey(Path.GetFileName(file).ToLower()))
                    Map.Add(Path.GetFileName(file).ToLower(), Path.GetFullPath(file));
            foreach(string dir in dirs)
                LoadFileMapImpl(dir);
            if(path == null)
            {
                CompletedLoading = true;
                AlreadyLoading = false;
            }
        }
    }
}
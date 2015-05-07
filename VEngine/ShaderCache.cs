using System.Collections.Generic;

namespace VEngine
{
    internal class ShaderCache
    {
        private static Dictionary<string, ShaderProgram> Cache = new Dictionary<string, ShaderProgram>();

        public static void CacheShaderProgram(string allnames, ShaderProgram compiled)
        {
            Cache.Add(allnames, compiled);
        }

        public static ShaderProgram GetShaderProgramOrNull(string allnames)
        {
            return Cache.ContainsKey(allnames) ? Cache[allnames] : null;
        }
    }
}
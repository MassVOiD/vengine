using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGTech
{
    class ShaderCache
    {
        private static Dictionary<int, ShaderProgram> Cache = new Dictionary<int,ShaderProgram>();

        public static void CacheShaderProgram(string allnames, ShaderProgram compiled)
        {
            Cache.Add(allnames.GetHashCode(), compiled);
        }

        public static ShaderProgram GetShaderProgramOrNull(string allnames){
            return Cache.ContainsKey(allnames.GetHashCode()) ? Cache[allnames.GetHashCode()] : null;
        }
    }
}

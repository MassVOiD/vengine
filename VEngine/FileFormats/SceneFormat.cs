using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenTK;

namespace VEngine.FileFormats
{
    class GameScene
    {
        public List<ILight> Lights;
        public List<GenericMaterial> Materials;
        public List<Mesh3d> Meshes;
        public List<Camera> Cameras;

        public GameScene(string filekey)
        {
            string file = Media.Get(filekey);
            Load(file);
        }

        private void Load(string file)
        {
            var regx = new Regex("(.+?)[ ]+(.+)");
            string[] lines = System.IO.File.ReadAllLines(file);
            GenericMaterial currentMaterial = new GenericMaterial(Vector4.One);

            foreach(var l in lines)
            {

            }

        }
    }
}

using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ManualShaderMaterial : IMaterial
    {
        public ManualShaderMaterial(string vertex, string fragment, string geometry = null, string tesscontrol = null, string tesseval = null)
        {
            Program = ShaderProgram.Compile(vertex, fragment, geometry, tesscontrol, tesseval);
        }
        public static ManualShaderMaterial FromMedia(string vertex, string fragment, string geometry = null, string tesscontrol = null, string tesseval = null)
        {
            return new ManualShaderMaterial(
                vertex,
                fragment,
                geometry != null ? geometry : null,
                tesscontrol != null ? tesscontrol : null,
                tesseval != null ? tesseval : null);
        }

        protected ShaderProgram Program;

        public static ManualShaderMaterial FromName(string name)
        {
            return new ManualShaderMaterial(name + ".vertex.glsl", name + ".fragment.glsl");
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }


        public virtual bool Use()
        {
            bool res = Program.Use();
            return res;
        }
    }
}
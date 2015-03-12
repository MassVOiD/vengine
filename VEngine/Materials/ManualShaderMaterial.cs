using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech
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
                Media.ReadAllText(vertex),
                Media.ReadAllText(fragment),
                geometry != null ? Media.ReadAllText(geometry) : null,
                tesscontrol != null ? Media.ReadAllText(tesscontrol) : null,
                tesseval != null ? Media.ReadAllText(tesseval) : null);
        }

        protected ShaderProgram Program;

        public static ManualShaderMaterial FromName(string name)
        {
            return new ManualShaderMaterial(Media.ReadAllText(name + ".vertex.glsl"), Media.ReadAllText(name + ".fragment.glsl"));
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
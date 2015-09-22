using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using VEngine.Particles;

namespace VEngine
{
    public class VEngineWindowAdapter : AbsDisplayAdapter
    {
        public VEngineWindowAdapter(string title, int width, int height, GameWindowFlags flags)
            : base(title, width, height, flags)
        {

        }


    }
}
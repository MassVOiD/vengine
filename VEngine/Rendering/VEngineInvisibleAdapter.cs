using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class VEngineInvisibleAdapter : AbsDisplayAdapter
    {
        public VEngineInvisibleAdapter()
            : base("", 1, 1, GameWindowFlags.Default)
        {

        }
    }
}
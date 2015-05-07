using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace VEngine
{
    class CubeMapFramebuffer : ITransformable
    {
        public TransformationManager Transformation;
        public int Resolution;

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}

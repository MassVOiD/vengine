using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VDGTech;
using System.Drawing;

namespace Tester
{
    class InfiniteGround : IRenderable
    {
        Mesh3d Ground;
        Object3dInfo Ground3dInfo;
        bool BuffersGenerated = false;

        const float Size = 2000.0f;

        float[] Vertices = {
                -Size, 0.0f, -Size, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                Size, 0.0f, -Size, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -Size, 0.0f, Size, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                Size, 0.0f, Size, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };
        uint[] Indices = {
                2, 1, 0, 1, 2, 3
            };

        public InfiniteGround()
        {
            Ground3dInfo = new Object3dInfo(Vertices.ToList(), Indices.ToList());
            Ground = new Mesh3d(Ground3dInfo, new ManualShaderMaterial(Media.ReadAllText("InfiniteGround.vertex.glsl"), Media.ReadAllText("InfiniteGround.fragment.glsl")));
        }

        public void Draw()
        {
            if (!BuffersGenerated)
            {
                BuffersGenerated = true;
            }
            Ground.Draw();
        }
    }
}

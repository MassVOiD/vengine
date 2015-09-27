using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;

namespace VEngine
{
    public class SimplePointLight: ITransformable, ILight
    {
        public TransformationManager Transformation;
        public Vector4 Color;
        public float Attenuation = 1.0f;
        public SimplePointLight(Vector3 position, Vector4 color)
        {
            Transformation = new TransformationManager(position);
            Color = color;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public Vector4 GetColor()
        {
            return Color;
        }

        public Vector3 GetPosition()
        {
            return Transformation.Position;
        }

        public LightMixMode GetMixMode()
        {
            return LightMixMode.Additive;
        }

        public MixRange GetMixRange()
        {
            return new MixRange();
        }
    }
}

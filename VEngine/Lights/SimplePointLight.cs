﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;

namespace VDGTech
{
    public class SimplePointLight: ITransformable
    {
        public TransformationManager Transformation;
        public Color Color;
        public SimplePointLight(Vector3 position, Color color)
        {
            Transformation = new TransformationManager(position);
            Color = color;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}

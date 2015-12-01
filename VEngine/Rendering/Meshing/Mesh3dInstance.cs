using System;
using System.Collections.Generic;
using System.Text;

namespace VEngine
{
    public class Mesh3dInstance : ITransformable
    {
        public uint Id;
        public string Name;
        public TransformationManager Transformation;

        public Mesh3dInstance(TransformationManager trans, string name)
        {
            Transformation = trans;
            Name = name;
            Id = ObjectIDGenerator.GetNext();
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}
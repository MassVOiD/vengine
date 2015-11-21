using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEngine
{
    public class Mesh3dInstance : ITransformable
    {
        public TransformationManager Transformation;
        public string Name;
        public uint Id;

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

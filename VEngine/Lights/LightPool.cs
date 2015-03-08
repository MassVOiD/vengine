using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace VDGTech
{
    public static class LightPool
    {
        private static byte InternalCounter = 0;
        private static List<ILight> Lights = new List<ILight>();

        public static void Add(ILight light)
        {
            if(!Lights.Contains(light)) Lights.Add(light);
        }

        public static Vector4[] GetColors()
        {
            return Lights.Select<ILight, Vector4>(a => a.GetColor()).ToArray();
        }

        public static float[] GetFarPlanes()
        {
            return Lights.Select<ILight, float>(a => a.GetFarPlane()).ToArray();
        }

        public static Matrix4[] GetPMatrices()
        {
            return Lights.Select<ILight, Matrix4>(a => a.GetPMatrix()).ToArray();
        }

        public static Vector3[] GetPositions()
        {
            return Lights.Select<ILight, Vector3>(a => a.GetPosition()).ToArray();
        }

        public static Matrix4[] GetVMatrices()
        {
            return Lights.Select<ILight, Matrix4>(a => a.GetVMatrix()).ToArray();
        }

        public static void MapAll()
        {
            //for(int i =0;i<Lights.Count;i++){
            //    Lights[i].Map();
            // }
            if(Lights.Count > InternalCounter)
                Lights[InternalCounter].Map();
            InternalCounter++;
            if(InternalCounter >= Lights.Count)
                InternalCounter = 0;
        }

        public static void Remove(ILight light)
        {
            if(Lights.Contains(light))
                Lights.Remove(light);
        }

        public static void UseTextures(int index)
        {
            int i = index;
            foreach(var light in Lights)
            {
                light.UseTexture(i);
                i++;
            }
        }
    }
}
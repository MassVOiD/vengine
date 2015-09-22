using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine
{
    sealed public class Scene : IRenderable, ITransformable
    {
        private List<ILight> Lights = new List<ILight>();
        private List<IRenderable> Renderables = new List<IRenderable>();
        private static ShaderStorageBuffer SSBO = new ShaderStorageBuffer();
        public TransformationManager Transformation;

        public Scene()
        {
            Transformation = new TransformationManager(Vector3.Zero, Quaternion.Identity, 1.0f);
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public void Add(ILight e)
        {
            Lights.Add(e);
        }
        public void Add(IRenderable e)
        {
            Renderables.Add(e);
        }

        public void Remove(ILight e)
        {
            Lights.Remove(e);
        }
        public void Remove(IRenderable e)
        {
            Renderables.Remove(e);
        }
        public void Draw(Matrix4 parentTransformation)
        {
            var cpos = Camera.MainDisplayCamera.GetPosition();
            Renderables.Sort((a, b) => {
                if(a is ITransformable && b is ITransformable)
                {
                    var e1 = (a as ITransformable).GetPosition();
                    var e2 = (b as ITransformable).GetPosition();
                    var x = (e1 - cpos).Length;
                    var y = (e2 - cpos).Length;
                    return x < y ? -1 : 1;
                } else return 0;
            });
            for(int i=0;i<Renderables.Count;i++)
                Renderables[i].Draw(parentTransformation * Transformation.GetWorldTransform());
        }
        
        public void MapLights(Matrix4 parentTransformation)
        {
            foreach(var e in Lights)
                if(e is IShadowMapableLight)
                {
                    (e as IShadowMapableLight).Map(parentTransformation * Transformation.GetWorldTransform());
                }

            foreach(var e in Renderables)
                if(e is Scene)
                    (e as Scene).MapLights(parentTransformation * Transformation.GetWorldTransform());
        }

        private void MapLightsSSBOToShader(ShaderProgram sp)
        {

        }

        private static List<byte> Bytes(Vector4 vec)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec.X));
            b.AddRange(BitConverter.GetBytes(vec.Y));
            b.AddRange(BitConverter.GetBytes(vec.Z));
            b.AddRange(BitConverter.GetBytes(vec.W));
            return b;
        }
        private static List<byte> Bytes(Vector3 vec, float additional = 0)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec.X));
            b.AddRange(BitConverter.GetBytes(vec.Y));
            b.AddRange(BitConverter.GetBytes(vec.Z));
            b.AddRange(BitConverter.GetBytes(additional));
            return b;
        }
        private static List<byte> Bytes(float vec)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec));
            return b;
        }

        private static Vector3 mul(Matrix4 mat, Vector3 vec)
        {
            return Vector4.Transform(new Vector4(vec, 1.0f), mat).Xyz;
        }

        private static List<byte> Bytes(int vec)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec));
            return b;
        }
        private static List<Matrix4> pmats = new List<Matrix4>();
        private static List<Matrix4> vmats = new List<Matrix4>();
        private static List<Vector3> poss = new List<Vector3>();
        private static List<float> fplanes = new List<float>();
        private static List<Vector4> colors = new List<Vector4>();
        private static List<int> mmodes = new List<int>();
        public void SetLightingUniforms(ShaderProgram shader, Matrix4 initialTransformation, bool initial = true)
        {
            Matrix4 newmat = initialTransformation * Transformation.GetWorldTransform();
            if(initial) {
                pmats = new List<Matrix4>();
                vmats = new List<Matrix4>();
                poss = new List<Vector3>();
                fplanes = new List<float>();
                colors = new List<Vector4>();
                mmodes = new List<int>();
                ipointer = 0;
            }
            foreach(var e in Lights)
                if(e is IShadowMapableLight)
                {
                    var l = e as IShadowMapableLight;
                    var p = e as ILight;
                    pmats.Add(l.GetPMatrix());
                    vmats.Add(newmat * l.GetVMatrix());
                    poss.Add(mul(newmat, p.GetPosition()));
                    fplanes.Add(l.GetFarPlane());
                    colors.Add(p.GetColor());
                    mmodes.Add((int)p.GetMixMode());
                    l.UseTexture(ipointer * 2 + 2);
                    ipointer++;
                }

            foreach(var e in Renderables)
                if(e is Scene)
                    (e as Scene).SetLightingUniforms(shader, newmat, false);
            if(initial)
            {
                shader.SetUniformArray("LightsPs", pmats.ToArray());
                shader.SetUniformArray("LightsVs", vmats.ToArray());
                shader.SetUniformArray("LightsPos", poss.ToArray());
                shader.SetUniformArray("LightsFarPlane", fplanes.ToArray());
                shader.SetUniformArray("LightsColors", colors.ToArray());
                shader.SetUniformArray("LightsMixModes", mmodes.ToArray());
                shader.SetUniform("LightsCount", pmats.Count);
                pmats = new List<Matrix4>();
                vmats = new List<Matrix4>();
                poss = new List<Vector3>();
                fplanes = new List<float>();
                colors = new List<Vector4>();
                mmodes = new List<int>();
            }
        }

        private static int ipointer = 0;
        private static List<byte> Buffer;
        private static List<float> ShadowMapsFarPlanes;
        private static int LastBufferElements = 0;
        private static int SimpleLightsCount = 0;

        public List<IRenderable> GetFlatRenderableList()
        {
            var o = new List<IRenderable>();
            Renderables.ForEach((a) =>
            {
                if(a is Scene)
                    o.AddRange((a as Scene).GetFlatRenderableList());
                else
                    o.Add(a);
            });
            return o;
        }

        public void RecreateSimpleLightsSSBO()
        {
            foreach(var e in Lights)
            {
                if(e is SimplePointLight)
                {

                }
                Buffer.AddRange(Bytes(e.GetPosition(), e is IShadowMapableLight ? 1 : 0));
                Buffer.AddRange(Bytes(e.GetColor()));

                SimpleLightsCount++;
            }
            foreach(var e in Renderables)
                if(e is Scene)
                    (e as Scene).RecreateSimpleLightsSSBO();

            SSBO.MapData(Buffer.ToArray());
        }

    }
}

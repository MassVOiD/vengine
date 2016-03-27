using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace VEngine
{
    sealed public class Scene
    {
        private List<byte> Buffer;

        private List<Vector4> colors = new List<Vector4>();

        private List<float> fplanes = new List<float>();

        private int ipointer = 0;

        private int LastBufferElements = 0;

        private List<int> mmodes = new List<int>();

        private List<Matrix4> pmats = new List<Matrix4>();
        private List<float> blurfactors = new List<float>();
        private List<int> exclgroups = new List<int>();
        private List<int> shadowmaplayers = new List<int>();

        private List<Vector3> poss = new List<Vector3>();

        private List<float> ShadowMapsFarPlanes;

        private int SimpleLightsCount = 0;

        private ShaderStorageBuffer SSBO = new ShaderStorageBuffer();

        private List<Matrix4> vmats = new List<Matrix4>();

        private List<Light> Lights = new List<Light>();

        private List<IRenderable> Renderables = new List<IRenderable>();

        public Scene()
        {
            SSBO.Type = OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw;
        }

        public void Add(Scene e)
        {
            Lights.AddRange(e.Lights);
            Renderables.AddRange(e.Renderables);
        }

        public void Add(Light e)
        {
            Lights.Add(e);
        }
        public List<Light> GetLights()
        {
            return Lights;
        }

        public void Add(IRenderable e)
        {
            Renderables.Add(e);
        }

        public int LastDrawnObjectsCount = 0;

        public void Draw()
        {
            // var cpos = Camera.MainDisplayCamera.GetPosition();
            /*Renderables.Sort((a, b) =>
            {
                if(a is ITransformable && b is ITransformable)
                {
                    var e1 = (a as ITransformable).GetPosition();
                    var e2 = (b as ITransformable).GetPosition();
                    var x = (e1 - cpos).Length;
                    var y = (e2 - cpos).Length;
                    return x < y ? -1 : 1;
                }
                else
                    return 0;
            });*/
            LastDrawnObjectsCount = 0;
            for(int i = 0; i < Renderables.Count; i++)
                if(Renderables[i].Draw())
                    LastDrawnObjectsCount++;
        }

        public void RunOcclusionQueries()
        {
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.ChooseShaderNoFragment();
            Game.World.SetUniforms(Game.DisplayAdapter.MainRenderer);

            for(int i = 0; i < Renderables.Count; i++)
                if(Renderables[i] is Mesh3d)
                    (Renderables[i] as Mesh3d).RunOcclusionQuery();

            GenericMaterial.OverrideShaderPack = null;
        }

        public List<IRenderable> GetFlatRenderableList()
        {
            var o = new List<IRenderable>();
            Renderables.ForEach((a) =>
            {
                o.Add(a);
            });
            return o;
        }

        public void Remove(Light e)
        {
            Lights.Remove(e);
        }

        public void Remove(IRenderable e)
        {
            Renderables.Remove(e);
        }
        
    }
}
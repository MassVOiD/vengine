using System;
using System.Collections.Generic;
using BulletSharp;
using OpenTK;
using VEngine.UI;

namespace VEngine
{
    public class World
    {
        public static World Root;

        public volatile bool Disposed;

        public Scene RootScene;

        public UIRenderer UI;

        public World()
        {
            RootScene = new Scene();
            UI = new UIRenderer();
            if(Root == null)
                Root = this;
        }

        public delegate void MeshCollideDelegate(Mesh3d meshA, Mesh3d meshB, Vector3 collisionPoint, Vector3 normalA);

        public event MeshCollideDelegate MeshCollide;
        
        public void Draw(bool ignoreMeshWithDisabledDepthTest = false, bool ignoreDisableDepthWriteFlag = false)
        {
            RootScene.Draw(Matrix4.Identity);
        }
        
    }
}
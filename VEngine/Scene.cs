using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGTech
{
    public class Scene
    {
        public List<ILight> Lights = new List<ILight>();
        public List<IRenderable> Meshes = new List<IRenderable>();
        public List<Line2d> Lines2d = new List<Line2d>();

        public void Add(ILight e)
        {
            Lights.Add(e);
        }
        public void Add(IRenderable e)
        {
            Meshes.Add(e);
        }
        public void Add(Line2d e)
        {
            Lines2d.Add(e);
        }

        public void Remove(ILight e)
        {
            Lights.Remove(e);
        }
        public void Remove(IRenderable e)
        {
            Meshes.Remove(e);
        }
        public void Remove(Line2d e)
        {
            Lines2d.Remove(e);
        }

        public virtual void Create(){
            foreach(var e in Meshes) World.Root.Add(e);
            foreach(var e in Lines2d)if(!World.Root.LinesPool.Contains(e))World.Root.LinesPool.Add(e);
            foreach(var e in Lights) LightPool.Add(e);
        }

        public virtual void Destroy()
        {
            foreach(var e in Meshes) World.Root.Remove(e);
            foreach(var e in Lines2d)if(World.Root.LinesPool.Contains(e))World.Root.LinesPool.Remove(e);
            foreach(var e in Lights) LightPool.Remove(e);
        }
    }
}

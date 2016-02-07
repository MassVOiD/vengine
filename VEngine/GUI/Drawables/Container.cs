using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine.GUI.Drawables
{
    class Container : GUIDrawable
    {
        private List<GUIDrawable> Drawables = new List<GUIDrawable>();

        //public Container(

        public override void Draw(Vector2 parent)
        {
            throw new NotImplementedException();
        }
    }
}

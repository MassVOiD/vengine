using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine.GUI
{
    abstract class GUIDrawable
    {
        enum LayoutType
        {
            Relative,
            Absolute
        };

        public Vector2 Margin;
        public Vector2 Padding;
        public Vector3 BackgroundColor;

        public abstract void Draw(Vector2 parent);
    }
}

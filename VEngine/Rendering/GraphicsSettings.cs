using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEngine
{
    public class GraphicsSettings
    {


        public GraphicsSettings()
        {
          
        }

        public bool UseFog = false;
        public bool UseLightPoints = false;
        public bool UseDepth = false;
        public bool UseBloom = false;
        public bool UseDeferred = false;
        public bool UseRSM = false;
        public bool UseSSReflections = false;
        public bool UseVDAO = false;
        public bool UseHBAO = false;

        public void SetUniforms(ShaderProgram program)
        {

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEngine
{
    internal class InternalRenderingState
    {
        public enum State
        {
            DistancePass,
            EarlyZPass,
            ForwardOpaquePass,
            ForwardTransparentPass,
            ShadowMapPass,
            Idle
        }
        public static State PassState = State.Idle;
    }
}

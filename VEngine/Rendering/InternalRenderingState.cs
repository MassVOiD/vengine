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
            ForwardOpaquePass,
            ForwardTransparentBlendingAdditivePass,
            ForwardTransparentBlendingAlphaPass,
            ShadowMapPass,

            Idle
        }
        public static State PassState = State.Idle;
    }
}

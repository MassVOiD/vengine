using System;
using System.Collections.Generic;
using System.Text;

namespace VEngine
{
    internal static class ObjectIDGenerator
    {
        public static uint GetNext()
        {
            return Counter++;
        }

        private static uint Counter = 0;
    }
}
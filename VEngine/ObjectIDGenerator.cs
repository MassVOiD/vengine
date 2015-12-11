using System;
using System.Collections.Generic;
using System.Text;

namespace VEngine
{
    internal static class ObjectIDGenerator
    {
        private static uint Counter = 0;

        public static uint GetNext()
        {
            return Counter++;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEngine
{
    static class ObjectIDGenerator
    {
        static uint Counter = 0;
        public static uint GetNext()
        {
            return Counter++;
        }
    }
}

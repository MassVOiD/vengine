using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.IO;

namespace VEngine
{
    public class AMCParsedCapture
    {
        public List<Dictionary<string, Quaternion>> Frames;
        public AMCParsedCapture(string file)
        {
            Frames = new List<Dictionary<string,Quaternion>>();
            var current = new Dictionary<string, Quaternion>();
            var lines = File.ReadAllLines(file);
            foreach(var line in lines)
            {
                if(line.StartsWith("#") || line.StartsWith(":"))
                    continue;
                if(IsNumeric(line))
                {
                    if(current.Count > 0)
                    {
                        Frames.Add(current);
                    }
                    current = new Dictionary<string, Quaternion>();
                }
                else
                {
                    var split = line.Split(' ');
                    string bname = split[0];
                    var rot = CreateRotation(
                        split.Length > 1 ? float.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture) : 0,
                        split.Length > 2 ? float.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture) : 0,
                        split.Length > 3 ? float.Parse(split[3], System.Globalization.CultureInfo.InvariantCulture) : 0);
                    current.Add(bname, rot);
                }
            }
            Frames.Add(current);
        }
        public static bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        private Quaternion CreateRotation(float x, float y, float z)
        {
            Quaternion res = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(x));
            res = Quaternion.Multiply(Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(y)), res);
            res = Quaternion.Multiply(Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(z)), res);
            return res;
        }
    }
}

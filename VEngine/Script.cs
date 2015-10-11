using System.Reflection;
using Jint;

namespace VEngine
{
    public class Script
    {
        private Engine engine;

        public Script()
        {
            engine = new Engine(cfg =>
                cfg.Culture(System.Globalization.CultureInfo.InvariantCulture)
                .AllowClr(new Assembly[] { typeof(VEngine.GLThread).Assembly, typeof(OpenTK.Vector3).Assembly })
                .Strict(false));
        }

        public object Execute(string script)
        {
            return engine.Execute(script).GetCompletionValue();
        }

        public object GetValue(string name)
        {
            return engine.GetValue(name);
        }

        public void SetValue(string name, object data)
        {
            engine.SetValue(name, data);
        }
    }
}
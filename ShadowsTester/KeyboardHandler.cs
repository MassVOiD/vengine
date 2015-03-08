namespace ShadowsTester
{
    internal class KeyboardHandler
    {
        static public void Process()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
        }
    }
}
using System;
using OpenTK.Input;

namespace VEngine.Game
{
    public class KeyboardController
    {
        public KeyboardController()
        {
            GLThread.OnKeyDown += OnKeyDown;
            GLThread.OnKeyUp += OnKeyUp;
            GLThread.OnKeyPress += OnKeyPress;
            GLThread.OnUpdate += OnUpdate;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnKeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace VEngine
{
    public class KeyboardController
    {
        private List<Bind> Binds = new List<Bind>();

        private class Bind
        {
            public Action Action;
            public Key Key;
            public BindType Type;
        }

        public KeyboardController()
        {
            Game.OnKeyDown += OnKeyDown;
            Game.OnKeyUp += OnKeyUp;
            // Game.OnUpdate += OnUpdate;
        }

        public enum BindType
        {
            Down,
            Up,
            ContinousDown,
            ContinousUp
        };

        public void RegisterKey(BindType type, Key key, Action action)
        {
            Binds.Add(new Bind()
            {
                Type = type,
                Key = key,
                Action = action
            });
        }

        public void UnregisterKey(BindType type, Key key)
        {
            Binds.RemoveAll((a) => a.Key == key && a.Type == type);
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            Binds.FindAll((a) => a.Type == BindType.Down).ForEach((a) => a.Action.Invoke());
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            Binds.FindAll((a) => a.Type == BindType.Up).ForEach((a) => a.Action.Invoke());
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
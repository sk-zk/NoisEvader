using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class InputHelper
    {
        private KeyboardState oldKbState;
        private KeyboardState newKbState;
        public KeyboardState OldKeyboardState => oldKbState;
        public KeyboardState KeyboardState => newKbState;

        private MouseState oldMouseState;
        private MouseState newMouseState;
        public MouseState OldMouseState => newMouseState;
        public MouseState MouseState => newMouseState;

        public Vector2 MouseVelocity =>
            new Vector2(newMouseState.X - oldMouseState.X,
                newMouseState.Y - oldMouseState.Y);

        public InputHelper()
        {
            oldKbState = default;
            newKbState = default;

            oldMouseState = default;
            newMouseState = default;
        }

        public void Update()
        {
            oldKbState = newKbState;
            newKbState = Keyboard.GetState();

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();
        }

        public bool KeyPressed(Keys key) =>
            oldKbState.IsKeyUp(key) && newKbState.IsKeyDown(key);

        public bool KeyComboPressed(Keys key1, Keys key2)
        {
            return (KeyPressed(key1) && newKbState.IsKeyDown(key2))
                || (KeyPressed(key2) && newKbState.IsKeyDown(key1));
        }

        public bool MouseLeftPressed() =>
            oldMouseState.LeftButton == ButtonState.Released
            && newMouseState.LeftButton == ButtonState.Pressed;

        public bool MouseLeftReleased() =>
            oldMouseState.LeftButton == ButtonState.Pressed
            && newMouseState.LeftButton == ButtonState.Released;

        public bool MousePositionChanged() =>
            oldMouseState.X != newMouseState.X ||
            oldMouseState.Y != newMouseState.Y;
    }
}

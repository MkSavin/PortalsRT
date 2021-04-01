using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PortalsRT.Input
{
    public class ControlsHelper
    {
        KeyboardKeyEventArgs? eventArgs = null;
        KeyboardState keyboardState = null;

        public ControlsHelper(KeyboardKeyEventArgs _eventArgs)
        {
            eventArgs = _eventArgs;
        }

        public ControlsHelper(KeyboardState _keyboardState)
        {
            keyboardState = _keyboardState;
        }

        public bool IsKeyPressed(Keys key) => eventArgs != null ? eventArgs.Value.Key == key : (keyboardState != null ? keyboardState.IsKeyDown(key) : false);

        public ControlsHelper KeyPress(Keys key, Action keyPressCallback)
        {
            if (IsKeyPressed(key))
            {
                keyPressCallback?.Invoke();
            }

            return this;
        }
    }
}

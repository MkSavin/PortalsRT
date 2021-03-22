using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PortalsRT.Input
{
    public class Controls
    {
        KeyboardState keyboard;
        Vector2 mouseDelta;

        // TODO: move to settings class
        public Vector2 MouseSensetivity = new Vector2(0.6f, 0.6f);

        public Controls(KeyboardState _keyboard, Vector2 _mouseDelta)
        {
            keyboard = _keyboard;
            mouseDelta = _mouseDelta;
        }

        public bool OneOfKeysDown(params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (keyboard.IsKeyDown(key))
                {
                    return true; 
                }
            }

            return false;
        }

        public int OppositeInput(bool straight, bool inverse)
        {
            return straight == inverse ? 0 : (straight ? 1 : -1); 
        }

        public Vector3 GetMoveRelativeInputDirection()
        {
            return
                -Vector3.UnitZ * OppositeInput(OneOfKeysDown(Keys.W, Keys.Up), OneOfKeysDown(Keys.S, Keys.Down)) +
                Vector3.UnitX * OppositeInput(OneOfKeysDown(Keys.D, Keys.Right), OneOfKeysDown(Keys.A, Keys.Left));
        }

        public Vector3 GetMoveAbsoluteInputDirection()
        {
            return
                Vector3.UnitY * OppositeInput(OneOfKeysDown(Keys.Space), OneOfKeysDown(Keys.LeftShift, Keys.RightShift));
        } 

        public Vector3 GetLookUpInputDirection()
        {
            Vector2 rawInput = mouseDelta * MouseSensetivity;

            return new Vector3(rawInput.Y, rawInput.X, 0);
        }
    }
}

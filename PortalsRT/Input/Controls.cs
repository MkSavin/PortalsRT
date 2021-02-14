using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortalsRT.Input
{
    public static class Controls
    {
        public static float MouseSensitivityX = 1;
        public static float MouseSensitivityY = 1;

        public static KeyboardState KeyboardState { get; set; }
        public static MouseState MouseState { get; set; }

        private static MouseState mousePrevState;

        public static (float X, float Y) MouseRawInput()
        {
            if (mousePrevState == null)
            {
                mousePrevState = MouseState;
            }

            // var result = (MouseState.X - mousePrevState.X, MouseState.Y - mousePrevState.Y);
            var result = (MouseState.Delta.X, MouseState.Delta.Y);

            Console.WriteLine(result);

            mousePrevState = MouseState;

            return result;
        }

        public static (float X, float Y) MouseInput()
        {
            var result = MouseRawInput();

            return (result.X * MouseSensitivityX / 500, result.Y * MouseSensitivityY / 500);
        }

        public static bool IsPressed(params Keys[] Keyss)
        {
            foreach (var Keys in Keyss)
            {
                if (KeyboardState.IsKeyDown(Keys))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsWindowState()
        {
            return IsPressed(Keys.F11);
        }

        static bool windowStatePressedPreviously;

        public static bool IsWindowStateChanged()
        {
            var windowState = IsWindowState();
            var result = windowState && !windowStatePressedPreviously;
            windowStatePressedPreviously = windowState;

            return result;
        }

        public static bool IsForward()
        {
            return IsPressed(Keys.W, Keys.Up);
        }

        public static bool IsBackward()
        {
            return IsPressed(Keys.S, Keys.Down);
        }

        public static bool IsLeft()
        {
            return IsPressed(Keys.A, Keys.Left);
        }

        public static bool IsRight()
        {
            return IsPressed(Keys.D, Keys.Right);
        }

        public static bool IsTop()
        {
            return IsPressed(Keys.Space);
        }

        public static bool IsBottom()
        {
            return IsPressed(Keys.LeftShift, Keys.RightShift);
        }
        public static float OppositeInput(bool positive, bool negative)
        {
            var value = 0;

            // Solution with +/- solves the rollover problem
            if (positive)
            {
                value += 1;
            }

            if (negative)
            {
                value -= 1;
            }

            return value;
        }

        public static float ForwardInput()
        {
            return OppositeInput(IsForward(), IsBackward());
        }

        public static float LeftInput()
        {
            return OppositeInput(IsLeft(), IsRight());
        }

        public static float TopInput()
        {
            return OppositeInput(IsTop(), IsBottom());
        }

        public static float JumpInput()
        {
            return IsTop() ? 1 : 0;
        }

    }
}

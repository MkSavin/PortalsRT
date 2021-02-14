using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;
using PortalsRT.Input;
using PortalsRT.Logic;
using PortalsRT.Math;

namespace PortalsRT.Scene
{
    class Camera
    {
        public static Camera Instance { get; } = new Camera();

        public Transform transform { get; set; } = new Transform();

        public Vector3 velocity = Vector3.Zero;

        public float FOV = 60;

        public float speed = 13F;
        public float speedResistance = 7F;
        public float jumpSpeed = 0.05F;
        public float rotationSpeed = 130F;

        public bool falling = false;

        public void ProcessControls()
        {
            AddForwardInput(Controls.ForwardInput() * (!Game.GameMode.NoClip && falling ? 0.5f : 1));
            AddLeftInput(Controls.LeftInput() * (!Game.GameMode.NoClip && falling ? 0.5f : 1));

            if (Game.GameMode.NoClip)
            {
                AddTopRelativeInput(Controls.TopInput());
            }
            else
            {
                AddJumpInput(Controls.JumpInput());
            }
        }

        #region Position Input
        public void AddDirectionInput(Vector3 direction, float value, bool ticked = false)
        {
            velocity += direction * value * speed * (ticked ? 1 : (float) Game.DeltaTime);
        }

        public void AddForwardInput(float value)
        {
            AddDirectionInput(-Vector3.UnitZ, value);
        }

        public void AddLeftInput(float value)
        {
            AddDirectionInput(-Vector3.UnitX, value);
        }

        public void AddTopRelativeInput(float value)
        {
            AddDirectionInput(Vector3.UnitY, -value);
        }

        public void AddJumpInput(float value)
        {
            if (!falling)
            {
                AddDirectionInput(Vector3.UnitY, -value * jumpSpeed, true);

                if (value > 0)
                {
                    falling = true;
                }
            }
        }

        public void AddTopAbsoluteInput(float value)
        {
            transform.position.Y += -value * speed * (float) Game.DeltaTime;
        }
        #endregion

        #region Rotation Input
        public void AddPitchInput(float value)
        {
            transform.rotation.X += value * rotationSpeed / (float)Trigonometry.DegToRad * (float)Game.DeltaTime;
            transform.rotation.X = Helpers.Clamp(transform.rotation.X, -90, 90);
        }

        public void AddYawInput(float value)
        {
            transform.rotation.Y += value * rotationSpeed / (float)Trigonometry.DegToRad * (float)Game.DeltaTime;
        }

        public void AddRollInput(float value)
        {
            transform.rotation.Z += value * rotationSpeed / (float)Trigonometry.DegToRad * (float)Game.DeltaTime;
        }
        #endregion

        #region Physics
        protected void ProcessGravity()
        {
            if (!Game.GameMode.NoClip && falling)
            {
                velocity.Y -= -Game.GameMode.Gravity * (float)Game.DeltaTime;
                velocity.Y = Helpers.Clamp(velocity.Y, -2, 2);
            }
        }

        protected void ProcessVelocity()
        {
            transform.position += velocity;

            velocity.X /= 1.3f;
            velocity.Z /= 1.3f;
        }

        protected void ProcessLimitations()
        {
            if (transform.position.Y < 0 && !Game.GameMode.NoClip)
            {
                falling = false;
                transform.position.Y = 0;
                velocity.Y = 0;
            }

            if (velocity.LengthFast > speed / 10)
            {
                velocity = velocity.Normalized() * speed / 10;
            }
        }

        protected void ProcessPhysics()
        {
            ProcessGravity();
            ProcessVelocity();
            ProcessLimitations();
        }

        public void ProcessGameMode()
        {
            ProcessPhysics();
        }
        #endregion
    }
}

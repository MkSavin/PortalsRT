using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using PortalsRT.Input;
using PortalsRT.Logic;
using PortalsRT.Mathematics;
using PortalsRT.Mathematics.Vector;
using PortalsRT.Shaders;

namespace PortalsRT.Scene
{
    class Camera : SceneObject
    {
        public static Camera Instance { get; } = new Camera(new Transform(new Vector3(0, 1, 3)));

        public Vector3 relativeVelocity = Vector3.Zero;

        public float speed = 0.3F;

        public bool CameraMoved { get; private set; } = false;

        public bool IsPerspective { get; set; } = true;

        public Camera(Transform _transform = null)
        {
            transform = _transform ?? Transform.Zero;
        }

        public void UploadTransformToShader(ShaderProgram shader)
        {
            shader.SetVector3("camera_position", transform.position);
            shader.SetVector3("camera_rotation", transform.rotation);
            shader.SetInt("camera_moved", CameraMoved ? 1 : 0);
        }

        public Matrix3 AbsoluteToRelativeRotationMatrix()
        {
            return
                Matrix3.CreateRotationY(transform.rotation.Y) *
                Matrix3.CreateRotationX(transform.rotation.X) *
                Matrix3.CreateRotationZ(transform.rotation.Z);
        }

        public void ProcessInput(KeyboardState keyboard, Vector2 mouseDelta)
        {
            Controls controls = new Controls(keyboard, mouseDelta);

            relativeVelocity += controls.GetMoveRelativeInputDirection() * speed * (float) Game.DeltaTime;
            relativeVelocity += controls.GetMoveAbsoluteInputDirection() * speed * (float) Game.DeltaTime * AbsoluteToRelativeRotationMatrix();

            transform.rotation += controls.GetLookUpInputDirection() * (float)Game.DeltaTime;

            transform.rotation.X = Helpers.Clamp(transform.rotation.X, (float) -Math.PI / 2, (float) Math.PI / 2);

            CameraMoved = controls.IsInputActive() || relativeVelocity.LengthFast > 1e-5;
        }

        public void ProcessPhysics() 
        {
            transform.position += relativeVelocity * AbsoluteToRelativeRotationMatrix().Inverted();

            relativeVelocity /= 1.1F;
        }
    }
}

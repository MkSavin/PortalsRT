using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using PortalsRT.Input;
using PortalsRT.Logic;
using PortalsRT.Mathematics;
using PortalsRT.Mathematics.Vector;
using PortalsRT.Scene.Objects;
using PortalsRT.Shaders;

namespace PortalsRT.Scene
{
    class Camera : SceneObject
    {
        public static Camera Instance { get; } = new Camera(new Transform(new Vector3(0, 1, 3)));

        public Vector3 relativeVelocity = Vector3.Zero;

        public Vector3 AbsoluteVelocity { 
            get
            {
                return relativeVelocity * AbsoluteToRelativeRotationMatrix().Inverted();
            }
        }

        public float boundsSphereRadius = 0.1f;
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

            transform.rotation.X = Helpers.Clamp(transform.rotation.X, (float)-Math.PI / 2, (float)Math.PI / 2);

            CameraMoved = controls.IsInputActive() || relativeVelocity.LengthFast > 1e-5;
        }

        public void ProcessPhysics(List<SceneObject> sceneObjects) 
        {
            transform.position += AbsoluteVelocity;

            relativeVelocity /= 1.1F;

            if (relativeVelocity.Length < 1e-3)
            {
                relativeVelocity = Vector3.Zero;
            }

            List<Portal> portals = new List<Portal>();

            foreach (var sceneObject in sceneObjects)
            {
                if (sceneObject is Portal)
                {
                    portals.Add(sceneObject as Portal);
                }
            }

            // Console.SetCursorPosition(0, 0);
            // Console.WriteLine("                                               ");
            // Console.WriteLine("                                               ");
            // Console.WriteLine("                                               ");

            foreach (var portal in portals)
            {
                // Determine that camera is colliding portal and velocity vector is opposite of portal normal
                // Camera distance vector and rotation to portalspace (portal localspace)
                Matrix3 portalspaceRotationTransform = portal.transform.TransformRotationMatrix();
                Vector3 distanceVector = portal.transform.position - transform.position;
                Vector3 portalNormalizedSize = portal.NormalizedSize();

                distanceVector *= portalspaceRotationTransform;

                distanceVector += portalNormalizedSize / 2;

                if (portal.IsPointInBounds(distanceVector))
                {
                    Vector3 portalspaceVelocity = AbsoluteVelocity * portalspaceRotationTransform.Inverted();

                    // Vector3.UnitY is normal of portal in portalspace
                    if (Vector3.Dot(portalspaceVelocity, Vector3.UnitY) < 0 && Vector3.Dot(distanceVector, Vector3.UnitY) > 0 /*|| Vector3.Dot(portalspaceVelocity, Vector3.UnitY) > 0 && Vector3.Dot(distanceVector, Vector3.UnitY) < 0*/)
                    {
                        Vector3 positionRelativeToPortalSize = new Vector3(distanceVector.X / portalNormalizedSize.X, 0, distanceVector.Z / portalNormalizedSize.Z);

                        var invertDirection = Vector3.UnitX * portalspaceRotationTransform;
                        invertDirection.Z = invertDirection.Y;
                        invertDirection.Y = 0;

                        var invertedRelativePosition = Vector3.One - positionRelativeToPortalSize;
                        positionRelativeToPortalSize += (invertedRelativePosition - positionRelativeToPortalSize) * invertDirection;

                        var targetPortal = portal.targetPortal;

                        portalNormalizedSize = targetPortal.NormalizedSize();

                        positionRelativeToPortalSize *= portalNormalizedSize;

                        distanceVector = positionRelativeToPortalSize - portalNormalizedSize / 2;

                        Vector3 portalspaceCameraRotation = transform.rotation * portalspaceRotationTransform;

                        portalspaceRotationTransform = targetPortal.transform.TransformRotationMatrix();

                        distanceVector *= portalspaceRotationTransform.Inverted();

                        var epsilonOffset = portalspaceRotationTransform.Inverted() * Vector3.UnitY * 1e-3f;

                        transform.position = targetPortal.transform.position - distanceVector + epsilonOffset;
                        transform.rotation = portalspaceCameraRotation * portalspaceRotationTransform.Inverted() + Vector3.UnitY * (float)Math.PI;
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace PortalsRT.Mathematics.Vector
{
    public class Transform
    {
        public static Transform Zero { get; } = new Transform();

        public Transform(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.scale;
        }

        public Transform(Vector3? _position = null, Vector3? _rotation = null, Vector3? _scale = null)
        {
            position = _position ?? Vector3.Zero;
            rotation = _rotation ?? Vector3.Zero;
            scale = _scale ?? Vector3.One;
        }

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Transform ApplyTransformMatrix(Matrix4 transformMatrix)
        {
            scale = Vector3.TransformVector(scale, transformMatrix);
            rotation = Vector3.TransformVector(rotation, transformMatrix);
            position = Vector3.TransformVector(position, transformMatrix);

            return this;
        }

        public Matrix3 TransformScaleMatrix() => Matrix3.CreateScale(scale);

        public Matrix3 TransformRotationMatrix(string[] rotationOrder = null)
        {
            if (rotationOrder == null)
            {
                rotationOrder = new[] { "x", "y", "z" };
            }

            Matrix3 transformMatrix = Matrix3.Identity;

            foreach (var orderItem in rotationOrder)
            {
                switch (orderItem)
                {
                    case "x":
                        transformMatrix = Matrix3.CreateRotationX(rotation.X);
                        break;
                    case "y":
                        transformMatrix = Matrix3.CreateRotationY(rotation.Y);
                        break;
                    case "z":
                        transformMatrix = Matrix3.CreateRotationZ(rotation.Z);
                        break;
                }
            }

            return transformMatrix;
        }

        public Matrix4 TransformTranslateMatrix() => Matrix4.CreateTranslation(position);

        public Matrix4 TransformMatrix(string[] rotationOrder = null)
        {
            return
                new Matrix4(TransformScaleMatrix())
                * new Matrix4(TransformRotationMatrix(rotationOrder))
                * TransformTranslateMatrix();
        }

        public Transform ToLocalCoordinates(Transform transform, string[] rotationOrder = null)
        {
            return new Transform(transform)
                .ApplyTransformMatrix(
                    TransformMatrix(rotationOrder)
                    .Inverted()
                );
        }

        public override string ToString()
        {
            return "P: [" + position + "], R: [" + rotation + "], S: [" + scale + "]";
        }
    }
}

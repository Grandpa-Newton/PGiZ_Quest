﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;

namespace QuestGame
{
    class Camera : Game3DObject
    {
        private float _fovY;
        public float FOVY { get => _fovY; set => _fovY = value; }

        private float _aspect;
        public float Aspect { get => _aspect; set => _aspect = value; }

        public float Width;

        public float Height;

        public Camera(Vector4 position, float yaw = 0.0f, float pitch = 0.0f, float roll = 0.0f, float fovY = MathUtil.PiOverTwo, float aspect = 1.0f)
            : base(position, yaw, pitch, roll)
        {
            _fovY = fovY;
            _aspect = aspect;
        }

        public Matrix GetProjectionMatrix()
        {
            //return Matrix.OrthoLH(_fovY, _aspect, 0.1f, 50.0f);
            return Matrix.OrthoLH(0.003f * Width, 0.003f * Height, 0.1f, 50.0f);
        }

        public Matrix GetViewMatrix()
        {
            Matrix rotation = Matrix.RotationYawPitchRoll(_yaw, _pitch, _roll);
            Vector3 viewTo = (Vector3)Vector4.Transform(Vector4.UnitZ, rotation);
            Vector3 viewUp = (Vector3)Vector4.Transform(Vector4.UnitY, rotation);
            return Matrix.LookAtLH((Vector3)_position, (Vector3)_position + viewTo, viewUp);
        }
    }
}
using QuestGame;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab01
{
    internal class Sprite
    {

        private DirectX3DGraphics _directX3DGraphics;

        Bitmap _bitmap;

        Vector2 _centerPosition;

        public Vector2 CenterPosition
        {
            get => _centerPosition;
            set => _centerPosition = value;
        }

        private float _angle;

        public float Angle
        {
            get => _angle;
            set => _angle = value;
        }

        private Vector2 _defaultSize;

        public Vector2 DefaultSize
        {
            get => _defaultSize;
            set => _defaultSize = value;
        }

        private Vector2 _translation;

        public Vector2 Translation
        {
            get => _translation;
            set => _translation = value;
        }
        
        private float _defaultScale;

        public float DefaultScale
        {
            get => _defaultScale;
            set => _defaultScale = value;
        }

        private static readonly float _pu = 1.0f;

        public Sprite(DirectX3DGraphics directX3DGraphics, Bitmap bitmap, Vector2 centerPosition, float angle, Vector2 defaultSize, float defaultScale)
        {
            _directX3DGraphics = directX3DGraphics;
            _bitmap = bitmap;
            _centerPosition = centerPosition;
            _angle = angle;
            _defaultSize = defaultSize;
            _defaultScale = defaultScale;
        }

        public void Draw(float opacity)
        {

            Vector2 center;

            center.X = _bitmap.Size.Width / 2f;
            center.Y = _bitmap.Size.Height / 2f;

            float width = _directX3DGraphics.D2DRenderTarget.Size.Width / _defaultSize.X;
            float height = _directX3DGraphics.D2DRenderTarget.Size.Height / _defaultSize.Y;

            _translation.X = (-center.X * _pu * _defaultScale + _centerPosition.X) * width;
            _translation.Y = _directX3DGraphics.D2DRenderTarget.Size.Height - (center.Y * _pu * _defaultScale + _centerPosition.Y) * height;

            var transform = _directX3DGraphics.D2DRenderTarget.Transform;
            //new Vector2(, );

            //_directX3DGraphics.D2DRenderTarget.Transform = Matrix3x2.Translation(_renderForm.Width / 2f, _renderForm.Height/2f);
            _directX3DGraphics.D2DRenderTarget.Transform = Matrix3x2.Rotation(-_angle, center) *
                Matrix3x2.Scaling(width * _pu * _defaultScale, width * _pu * _defaultScale, Vector2.Zero) *
                Matrix3x2.Translation(_translation);
            _directX3DGraphics.D2DRenderTarget.DrawBitmap(_bitmap, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            _directX3DGraphics.D2DRenderTarget.Transform = transform;

        }
    }
}

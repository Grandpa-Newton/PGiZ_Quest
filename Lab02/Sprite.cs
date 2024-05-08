using SharpDX;
using SharpDX.Direct2D1;
using QuestGame.Infrastructure;

namespace QuestGame.Graphics
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

        public Sprite(DirectX3DGraphics directX3DGraphics, Bitmap bitmap, Vector2 centerPosition, float angle,
            Vector2 defaultSize, float defaultScale)
        {
            _directX3DGraphics = directX3DGraphics;
            _bitmap = bitmap;
            _centerPosition = centerPosition;
            _angle = angle;
            _defaultSize = defaultSize;
            _defaultScale = defaultScale;
        }

        public void Draw(float opacity = 1.0f)
        {
            Vector2 center;

            center.X = _bitmap.Size.Width / 2f;
            center.Y = _bitmap.Size.Height / 2f;

            float width = _directX3DGraphics.D2DRenderTarget.Size.Width / _defaultSize.X;
            float height = _directX3DGraphics.D2DRenderTarget.Size.Height / _defaultSize.Y;

            _translation.X = (-center.X * _defaultScale + _centerPosition.X) * width;
            _translation.Y = _directX3DGraphics.D2DRenderTarget.Size.Height -
                             (center.Y * _defaultScale + _centerPosition.Y) * height;

            var transform = _directX3DGraphics.D2DRenderTarget.Transform;

            _directX3DGraphics.D2DRenderTarget.Transform = Matrix3x2.Rotation(-_angle, center) *
                                                           Matrix3x2.Scaling(width * _defaultScale,
                                                               width * _defaultScale, Vector2.Zero) *
                                                           Matrix3x2.Translation(_translation);
            _directX3DGraphics.D2DRenderTarget.DrawBitmap(_bitmap, opacity, BitmapInterpolationMode.Linear);
            _directX3DGraphics.D2DRenderTarget.Transform = transform;
        }

        public Sprite Clone()
        {
            return new Sprite(_directX3DGraphics, _bitmap, _centerPosition, _angle, _defaultSize, _defaultScale);
        }
    }
}
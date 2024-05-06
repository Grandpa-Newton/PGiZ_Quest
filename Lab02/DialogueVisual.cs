using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;
using Factory = SharpDX.DirectWrite.Factory;

namespace Lab01
{
    internal class DialogueVisual
    {

        private RenderTarget _d2dRenderTarget;
        private readonly SolidColorBrush _blackBrush;

        public DialogueVisual(RenderTarget d2dRenderTarget)
        {
            _d2dRenderTarget = d2dRenderTarget;
            _blackBrush = new SolidColorBrush(d2dRenderTarget, Color.Black);
        }

        public void Draw(RenderTarget d2dRenderTarget, Factory factory, Sprite sprite, string name, string text, RenderForm renderForm)
        {
            var blackColor = Color.Black;
            blackColor.A = 170;
            var blackBrush = new SolidColorBrush(d2dRenderTarget, blackColor);
            var whiteBrush = new SolidColorBrush(d2dRenderTarget, Color.WhiteSmoke);
            float sideOffset = renderForm.Width * 250f / 1920f;
            float bottomOffset = renderForm.Height * 75f / 1080f;
            float topOffset = renderForm.Height * 250f / 1080f;
            d2dRenderTarget.FillRoundedRectangle(new RoundedRectangle()
            {
                Rect = new RawRectangleF(sideOffset, renderForm.Height - topOffset, renderForm.Width-sideOffset, renderForm.Height - bottomOffset),
                RadiusX = 5f,
                RadiusY = 5f
            }, blackBrush);
            sprite.Draw();
            var mainTextFormat = new TextFormat(factory, "Calibri", 40 * (renderForm.Width/1920f))
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center,
            };
            var mainTextLayout = new TextLayout(factory, name + ": " + text, mainTextFormat, renderForm.Width,
                renderForm.Height);
            float textOffset = renderForm.Height / 2f - renderForm.Height * 165f / 1080f;
            d2dRenderTarget.DrawTextLayout(new RawVector2(0f, textOffset), mainTextLayout, whiteBrush, DrawTextOptions.None);
            
            
        }
    }
}
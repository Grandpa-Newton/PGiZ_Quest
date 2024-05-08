using System;
using SharpDX;
using SharpDX.Direct3D11;

namespace QuestGame.Graphics
{
    class Texture : IDisposable
    {
        private Texture2D _textureObject;

        private ShaderResourceView _shaderResourceView;

        public ShaderResourceView ShaderResourceView
        {
            get => _shaderResourceView;
        }

        private int _width;

        private int _height;

        private SamplerState _samplerState;

        public SamplerState SamplerState
        {
            get => _samplerState;
        }

        public Texture(Texture2D textureObject, ShaderResourceView shaderResourceView,
            int width, int height, SamplerState samplerState)
        {
            _textureObject = textureObject;
            _shaderResourceView = shaderResourceView;
            _width = width;
            _height = height;
            _samplerState = samplerState;
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _shaderResourceView);
            Utilities.Dispose(ref _textureObject);
        }
    }
}
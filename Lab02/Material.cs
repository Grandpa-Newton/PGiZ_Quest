using SharpDX;
using System.Runtime.InteropServices;

namespace QuestGame.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public Vector4 Emmisive;
        public Vector4 Ambient;
        public Vector4 Diffuse;
        public Vector4 Specular;
        public float SpecularPower;
        public int UseTexture;
        public Vector2 padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialProperties
    {
        public Material Material;
    }
}
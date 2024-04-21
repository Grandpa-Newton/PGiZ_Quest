using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuestGame
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public Vector4 Emmisive;
        public Vector4 Ambient;
        public Vector4 Diffuse;
        public Vector4 Specular;
        public float SpecularPower;
        public int UseTexture; // ??? (int)
        public Vector2 padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialProperties
    {
        public Material Material;
    }
}

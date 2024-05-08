#define MAX_LIGHTS 4

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1

Texture2D Texture : register(t0);
sampler Sampler : register(s0);

struct _Material
{
    float4  Emissive;
    float4  Ambient;
    float4  Diffuse;
    float4  Specular;
    float   SpecularPower;
    bool    UseTexture;
    float2  Padding;
};

cbuffer MaterialProperties : register(b0)
{
    _Material Material;
};

struct Light
{
    float4      Position;
    float4      Direction;
    float4      Color;
    float       SpotAngle;              
    float       ConstantAttenuation;
    float       LinearAttenuation;
    float       QuadraticAttenuation;
    int         LightType; 
    bool        Enabled;
    int2        Padding;
};

cbuffer LightProperties : register(b1)
{
    float4 EyePosition;
    float4 GlobalAmbient;
    Light Lights[MAX_LIGHTS];
};

float4 DoDiffuse(Light light, float3 L, float3 N)
{
    float NdotL = max(0, dot(N, L));
    return light.Color * NdotL;
}

float4 DoSpecular(Light light, float3 V, float3 L, float3 N, float k, float3 T)
{
    float3 R = normalize(reflect(-L, N));
    float RdotV = max(0, dot(R, V));

    float3 H = normalize(L + V);
    float NdotH = max(0, dot(N, H));
    float NdotH2 = NdotH * NdotH;

    float HdotT = max(0, dot(H, T));

    float ward = exp(-((k * (1.0 - NdotH2) / NdotH2) * ((k * (1.0 - NdotH2) / NdotH2))));

    return light.Color * pow(ward, Material.SpecularPower);
}

float DoAttenuation(Light light, float d)
{
    return 1.0f / (light.ConstantAttenuation + light.LinearAttenuation * d + light.QuadraticAttenuation * d * d);
}

struct LightingResult
{
    float4 Diffuse;
    float4 Specular;
};

LightingResult DoPointLight(Light light, float3 V, float4 P, float3 N, float k, float3 T)
{
    LightingResult result;

    float3 L = (light.Position - P).xyz;
    float distance = length(L);
    L = L / distance;

    float attenuation = DoAttenuation(light, distance);

    result.Diffuse = DoDiffuse(light, L, N) * attenuation;
    result.Specular = DoSpecular(light, V, L, N, k, T) * attenuation;

    return result;
}

LightingResult DoDirectionalLight(Light light, float3 V, float4 P, float3 N, float k, float3 T)
{
    LightingResult result;

    float3 L = -light.Direction.xyz;

    result.Diffuse = DoDiffuse(light, L, N);
    result.Specular = DoSpecular(light, V, L, N, k, T);

    return result;
}

LightingResult ComputeLighting(float4 P, float3 N, float k, float3 T)
{
    float3 V = normalize(EyePosition - P).xyz;      // v 

    LightingResult totalResult = { {0, 0, 0, 0}, {0, 0, 0, 0} };

    [unroll]
    for (int i = 0; i < MAX_LIGHTS; ++i)
    {
        LightingResult result = { {0, 0, 0, 0}, {0, 0, 0, 0} };

        if (!Lights[i].Enabled) continue;

        switch (Lights[i].LightType)
        {
        case DIRECTIONAL_LIGHT:
        {
            result = DoDirectionalLight(Lights[i], V, P, N, k, T);
        }
        break;
        case POINT_LIGHT:
        {
            result = DoPointLight(Lights[i], V, P, N, k, T);
        }
        break;
        }
        totalResult.Diffuse += result.Diffuse;
        totalResult.Specular += result.Specular;
    }

    totalResult.Diffuse = saturate(totalResult.Diffuse);
    totalResult.Specular = saturate(totalResult.Specular);

    return totalResult;
}

struct PixelShaderInput
{
    float4 PositionWS   : TEXCOORD1;
    float4 NormalWS     : TEXCOORD2;
    float2 TexCoord     : TEXCOORD0;                 
    float3 TMatrix      : TEXCOORD3;
};

float4 pixelShader(PixelShaderInput IN) : SV_TARGET
{
    /*const float k = 10.0;

    LightingResult lit = ComputeLighting(IN.PositionWS, normalize(IN.NormalWS), k, IN.TMatrix);

    float4 emissive = Material.Emissive;
    float4 ambient = Material.Ambient * GlobalAmbient;
    float4 diffuse = Material.Diffuse * lit.Diffuse;
    float4 specular = Material.Specular * lit.Specular;*/

    float4 texColor = Texture.Sample(Sampler, IN.TexCoord);

    return texColor;

    /*float4 color = { 1, 0, 0, 1 };

    if (Material.UseTexture)
    {
        
        return texColor;
    }

    float4 finalColor = (emissive + ambient + diffuse + specular) * texColor;


    return finalColor;*/

}

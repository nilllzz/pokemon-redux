matrix World;
matrix WorldViewProj;
float Alpha;
float3 CameraPosition;

float3 SunLightDirection;
float4 SunLightColor;
float SunLightIntensity;

#define MAXLIGHT 32

float3 PointLightPosition[MAXLIGHT];
float4 PointLightColor[MAXLIGHT];
float PointLightIntensity[MAXLIGHT];
float PointLightRadius[MAXLIGHT];

int MaxLightsRendered = 0;

Texture2D DiffuseTexture;

SamplerState textureSampler
{
    MinFilter = linear;
    MagFilter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput Output;

    Output.Position = mul(input.Position, WorldViewProj);
    Output.Normal = mul(float4(input.Normal, 0), World).xyz;
    Output.TexCoord = input.TexCoord;
    Output.WorldPos = mul(input.Position, World).xyz;

    return Output;
}

float4 CalcDiffuseLight(float3 normal, float3 lightDirection, float4 lightColor, float lightIntensity)
{
    return saturate(dot(normal, -lightDirection)) * lightIntensity * lightColor;
}

float4 CalcSpecularLight(float3 normal, float3 lightDirection, float3 cameraDirection, float4 lightColor, float lightIntensity)
{
    float3 halfVector = normalize(-lightDirection + -cameraDirection);
    float specular = saturate(dot(halfVector, normal));
	
    float specularPower = 20;

    return lightIntensity * lightColor * pow(abs(specular), specularPower);
}

float lengthSquared(float3 v1)
{
    return v1.x * v1.x + v1.y * v1.y + v1.z * v1.z;
}

float4 calcColor(float4 baseColor, VertexShaderOutput input)
{
    float4 diffuseLight = float4(0, 0, 0, 1);
    float4 specularLight = float4(0, 0, 0, 0);

    float3 cameraDirection = normalize(input.WorldPos - CameraPosition);

    // global light source
    diffuseLight += CalcDiffuseLight(input.Normal, SunLightDirection, SunLightColor, SunLightIntensity);
    specularLight += CalcSpecularLight(input.Normal, SunLightDirection, cameraDirection, SunLightColor, SunLightIntensity);

    // points lights
    [loop]
    for (int i = 0; i < MaxLightsRendered; i++)
    {
        float3 PointLightDirection = input.WorldPos - PointLightPosition[i];
                           
        float DistanceSq = lengthSquared(PointLightDirection);

        float radius = PointLightRadius[i];
             
        [branch]
        if (DistanceSq < abs(radius * radius))
        {
            float Distance = sqrt(DistanceSq);

            //normalize
            PointLightDirection /= Distance;

            float du = Distance / (1 - DistanceSq / (radius * radius - 1));
            float denom = du / abs(radius) + 1;
            float attenuation = 1 / (denom * denom);

            diffuseLight += CalcDiffuseLight(input.Normal, PointLightDirection, PointLightColor[i], PointLightIntensity[i]) * attenuation;
            
            specularLight += CalcSpecularLight(input.Normal, PointLightDirection, cameraDirection, PointLightColor[i], PointLightIntensity[i]) * attenuation;
        }
    }

    return (baseColor * (diffuseLight + specularLight)) + baseColor * SunLightColor * SunLightIntensity;
}

float4 MainPS(VertexShaderOutput input) : SV_Target
{
    float4 baseColor = DiffuseTexture.Sample(textureSampler, input.TexCoord);
    
    // for full transparency, don't do any calculations and return directly.
    if (baseColor.x == 0 && baseColor.y == 0 && baseColor.z == 0 && baseColor.w == 0)
    {
        return baseColor;
    }
    
    float4 color = calcColor(baseColor, input);
    
    color.x = color.x * Alpha;
    color.y = color.y * Alpha;
    color.z = color.z * Alpha;
    color.w = Alpha;
    
    return color;
}

technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 MainVS();
        PixelShader = compile ps_4_0 MainPS();
    }
}

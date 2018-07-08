sampler s0; // contains texture to sample from

float offset; // effect offset value
float startY; // screen pos to start effect at
float endY; // screen pos to end effect at

float4 PS_MAIN(float4 position : SV_Position, float4 col : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    if (position.y >= startY &&
        position.y <= endY)
    {
        uv.y += sin(offset + uv.y * 200) * 0.03;
    }

    float4 color = tex2D(s0, uv);
    return color;
}

technique Confusion
{
    pass Pass1
    {
        PixelShader = compile ps_4_0 PS_MAIN();
    }
}

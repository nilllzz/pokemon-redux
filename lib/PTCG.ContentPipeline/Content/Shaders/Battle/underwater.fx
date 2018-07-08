sampler s0; // contains texture to sample from

float offset; // effect offset value
float startY; // screen pos to start effect at
float endY; // screen pos to end effect at

float4 PS_MAIN(float4 position : SV_Position, float4 col : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    if (position.y >= startY &&
        position.y <= endY &&
        (position.y + offset) % 10 < 5)
    {
        uv.y += 0.003;
    }

    float4 color = tex2D(s0, uv);
    return color;
}

technique Underwater
{
    pass Pass1
    {
        PixelShader = compile ps_4_0 PS_MAIN();
    }
}

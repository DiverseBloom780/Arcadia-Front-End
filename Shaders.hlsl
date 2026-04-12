cbuffer PerObject : register(b0) {
    float4x4 WorldViewProj;
    float Opacity; // Passed per-item for smooth fading
    float IsSelected; // 1.0 for the center item
    float4 ColorTint; // RGBA Tint
};

struct VS_INPUT {
    float4 Pos : POSITION;
    float2 Tex : TEXCOORD;
};

struct PS_INPUT {
    float4 Pos : SV_POSITION;
    float2 Tex : TEXCOORD;
};

Texture2D ObjTexture : register(t0);
SamplerState ObjSamplerState : register(s0);

PS_INPUT VS(VS_INPUT input) {
    PS_INPUT output;
    output.Pos = mul(input.Pos, WorldViewProj);
    output.Tex = input.Tex;
    return output;
}

float4 PS(PS_INPUT input) : SV_Target {
    float4 color = ObjTexture.Sample(ObjSamplerState, input.Tex);
    
    // Apply custom tint
    color *= ColorTint;
    
    // Add a subtle glow/brightness to the selected item
    if (IsSelected > 0.5) {
        color.rgb *= 1.2;  // Contrast
    }

    color.a *= Opacity; // Apply the calculated wheel fade
    return color;
}
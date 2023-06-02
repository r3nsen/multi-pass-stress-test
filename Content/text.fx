#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

Texture2D tex;
sampler2D texture_sampler = sampler_state
{
	Texture = <tex>;
};

struct VertexShaderInput
{
	float4 pos : POSITION0;
	float4 col : COLOR0;
	float2 tex : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 pos : SV_POSITION;
	float4 col : COLOR0;
	float2 tex : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.pos = mul(input.pos, WorldViewProjection);
	output.col = input.col;
	output.tex = input.tex;
	return output;
}

// Computation of the median value using minima and maxima
float median(float a, float b, float c)
{
	return max(min(a, b), min(max(a, b), c));
}
float smin(float a, float b, float k)
{
	float h = max(k - abs(a - b), 0.0) / k;
	return min(a, b) - h * h * k * (1.0 / 4.0);
}
float4 textRenderPS(VertexShaderOutput input) : COLOR
{
		float2 pos = input.tex;
;
		float4 _tex = tex2D(texture_sampler, pos);
		float3 s = _tex.rgb;
		float d = median(s.r, s.g, s.b) - .5;
		float w = saturate(d / fwidth(d) + .5);
		
		w = -(_tex.a - 1);
		
		w = smoothstep(.7, .1, w); // .7, .3
		
		float4 col = 1;// w;
		col.rgb = input.col * w;
		col.rgba;
		return col;
}

technique BasicColorDrawing
{
	pass P0 // Ring of circles
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL textRenderPS();
	}
}
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

Texture2D tex, sky;
sampler2D texture_sampler = sampler_state
{
	Texture = <tex>;
};
sampler2D sky_sampler = sampler_state
{
	Texture = <sky>;
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
float2 mod(float2 a, float2 b)
{
	return a - b * floor(a / b);
}
float2 mirror(float2 pos)
{
	return mod(pos, 1.) * (mod((floor(pos) - 1.), 2.) * 2. - 1.) + mod(floor(pos), 2.);
}
float4 MainPS(VertexShaderOutput input) : COLOR
{
	// wave,  speed	
	// speed[x, y] += ( wave[ x +/- 1, y +/- 1 ] - wave[ x, y ]) * a
	// wave[x, y] -= speed[x, y] * b

	// mirror equation:
	 //- (x % 1) * ((floor(x) - 1) % 2) * 2 - 1 + floor(x) % 2

	float2 size = 1. / float2(800., 480.);
	float speed_force = .65;
	float damp = .99;

	float4 col = tex2D(texture_sampler, input.tex);

	float med = 0.;
	for (float i = -1.; i <= 1.; i++)
		for (float j = -1; j <= 1.; j++)
			med += (col.x - tex2D(texture_sampler, mirror(input.tex + float2(i, j) * size)).x) * speed_force;

	//med -= col.y;
	//med /= 64.;
	//med *= speed_force;

	float speed = (col.y + med); // wave = old_speed + wave_diff
	float wave = col.x - speed * .3; // wave = old_wave - old_speed
	col.x = wave;
	col.y = speed * damp;
	return col;// *input.col;
}
float4 CopyPS(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(texture_sampler, input.tex);
	col.rgb -= .5;
	col.g = 0.;
	return col;
}
float4 ShowPS(VertexShaderOutput input) : COLOR
{
	//float4 col = tex2D(texture_sampler, input.tex);
	////col.xy = abs(col.xy);
	////col.x *= .1;
	////col.x = col.x * col.x * col.x * .25;// *.1;
	////col.y *= .0;
	////col.z =  -col.x;
	//col.rgb += .5;
	//col.x = col.x * col.x + .2;// *.1;

	//col.g = 0;
	//	col.b =col.b *.2 + .2;
	////col.b = 1. - col.r;
	float2 size = 1. / float2(800., 480.);
	float4 scol = tex2D(texture_sampler, input.tex);
	float4 scolx = tex2D(texture_sampler, input.tex - float2(1., 0.) * size);
	float4 scoly = tex2D(texture_sampler, input.tex - float2(0., 1.) * size);

	float2 dd = float2(scolx.x - scol.x, scoly.x - scol.x) / size;//float2(ddx(scol).x, ddy(scol).y);
	float4 col = tex2D(sky_sampler, input.tex + dd * size);
	col.rgb += saturate(dot(dd, .5)) *.08;
	//col.xy = dd;
	//col.z *= .01;

	return col;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL CopyPS();
	}
	pass P1
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
	pass P2
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ShowPS();
	}
};
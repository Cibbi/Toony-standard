#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"

struct VertexData 
{
	float4 vertex     : POSITION;
	float3 normal     : NORMAL;
    float4 color      : COLOR;
    float2 uv         : TEXCOORD0;
};

struct FragmentData 
{
	float4 pos        : SV_POSITION;
	float2 uv         : TEXCOORD0;
};

float _OutlineWidth;
float _OutlineOffsetX;
float _OutlineOffsetY;
float4 _OutlineColor;
float _IsOutlineEmissive;

sampler2D _OutlineWidthMap;
sampler2D _OutlineTexture;

float4 _MainTex_ST;

#if defined(_ALPHATEST_ON)
    float4 _Color;
    float _Cutoff;
    UNITY_DECLARE_TEX2D(_MainTex);
#endif

FragmentData VertexOutlineFunction(VertexData v) 
{
    FragmentData o;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);


	float camDist = distance(UnityObjectToWorldDir(v.vertex), _WorldSpaceCameraPos);
    float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M,normalize(v.normal )));
    o.pos = UnityObjectToClipPos(v.vertex);
    float outlineWidth = tex2Dlod(_OutlineWidthMap, float4(o.uv,0,0)).r * _OutlineWidth;
    float2 width = normalize(clipNormal.xy) / _ScreenParams.xy * min(3,o.pos.w) * 2 * outlineWidth;
    float2 offset = float2(_OutlineOffsetX, _OutlineOffsetY) / _ScreenParams.xy* min(3,o.pos.w) * 2;
    float2 finalPosOffset = width + offset;
    o.pos.xy += finalPosOffset;
	
	return o;
}

float4 FragmentOutlineFunction(FragmentData i) : COLOR
{
    float indirectDiffuse = unity_SHAr.w + unity_SHAg.w + unity_SHAb.w;
    
    #if defined(_ALPHATEST_ON)
        float4 albedo;
        albedo = UNITY_SAMPLE_TEX2D (_MainTex, i.uv) * _Color;
		clip(albedo.a - _Cutoff);
	#endif

    float4 outlineColor = tex2D(_OutlineTexture, i.uv) * _OutlineColor;

	return lerp(max(_LightColor0.a, indirectDiffuse) * outlineColor, outlineColor, _IsOutlineEmissive);
}

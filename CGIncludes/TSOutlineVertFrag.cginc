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
	//float3 normal     : NORMAL;
	//float4 tangentDir : TANGENT;
	//float2 uv         : TEXCOORD0;
	//float3 worldPos   : TEXCOORD1;
    //UNITY_FOG_COORDS(2)
};

float _OutlineWidth;
float _OutlineOffsetX;
float _OutlineOffsetY;
float4 _OutlineColor;
float _IsOutlineEmissive;

FragmentData VertexOutlineFunction(VertexData v) 
{
    FragmentData o;
	float camDist = distance(UnityObjectToWorldDir(v.vertex), _WorldSpaceCameraPos);
    float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M,normalize(v.normal )));
    o.pos = UnityObjectToClipPos(v.vertex);
    float2 width = normalize(clipNormal.xy) / _ScreenParams.xy * min(3,o.pos.w) * 2 * _OutlineWidth;
    float2 offset = float2(_OutlineOffsetX, _OutlineOffsetY) / _ScreenParams.xy* min(3,o.pos.w) * 2;
    float2 finalPosOffset = width + offset;
    o.pos.xy += finalPosOffset;
	
	return o;
}

float4 FragmentOutlineFunction(FragmentData i) : COLOR
{
    float indirectDiffuse = unity_SHAr.w + unity_SHAg.w + unity_SHAb.w;

	return lerp(max(_LightColor0.a,indirectDiffuse)*_OutlineColor,_OutlineColor,_IsOutlineEmissive);
}

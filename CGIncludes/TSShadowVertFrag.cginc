#include "UnityCG.cginc"

float4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
float _Cutoff;
sampler3D _DitherMaskLOD;

struct VertexData 
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct InterpolatorsVertex  
{
	float4 position : SV_POSITION;
	#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_DITHER_ON)
		float2 uv : TEXCOORD0;
	#endif
	#if defined(SHADOWS_CUBE)
		float3 lightVec : TEXCOORD1;
	#endif
};

struct Interpolators 
{
	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_DITHER_ON)
		UNITY_VPOS_TYPE vpos : VPOS;
	#else
		float4 position : SV_POSITION;
	#endif
	
	#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_DITHER_ON)
		float2 uv : TEXCOORD0;
	#endif
	#if defined(SHADOWS_CUBE)
		float3 lightVec : TEXCOORD1;
	#endif
};

InterpolatorsVertex  ShadowVertexFunction (VertexData v) 
{
	InterpolatorsVertex  i;
	#if defined(SHADOWS_CUBE)
		i.position = UnityObjectToClipPos(v.position);
		i.lightVec =
		mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
	#else
		i.position = UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
		i.position = UnityApplyLinearShadowBias(i.position);
	#endif
	#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_DITHER_ON)
		i.uv = TRANSFORM_TEX(v.uv, _MainTex);
	#endif
	return i;
}

float4 ShadowFragmentFunction (Interpolators i) : SV_TARGET 
{

	#if defined(_ALPHATEST_ON) 
		float alpha = tex2D(_MainTex, i.uv.xy).a*_Color.a;
		clip(alpha - _Cutoff);
	#endif

	#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON) || defined(_DITHER_ON)
		float alpha = tex2D(_MainTex, i.uv.xy).a*_Color.a;
		float dither = tex3D(_DitherMaskLOD, float3(i.vpos.xy * .25, alpha * 0.9375)).a;
		clip(dither - 0.01);
	#endif

	#if defined(SHADOWS_CUBE)
		float depth = length(i.lightVec) + unity_LightShadowBias.x;
		depth *= _LightPositionRange.w;
		return UnityEncodeCubeShadowDepth(depth);
	#else
		return 0;
	#endif
}
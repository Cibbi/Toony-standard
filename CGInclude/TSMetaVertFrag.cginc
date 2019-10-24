#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityMetaPass.cginc"

UNITY_DECLARE_TEX2D(_MainTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailTexture);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_GlossinessMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap);

float4 _Color;
float4 _MainTex_ST, _DetailTexture_ST;
float _Metallic;
float _Glossiness;

float4 _EmissionColor;

struct VertexData 
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
    float2 uv2 : TEXCOORD2;
};

struct Interpolators 
{
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
};

float3 GetAlbedo (Interpolators i) 
{
    float4 albedo;
	albedo = UNITY_SAMPLE_TEX2D (_MainTex, i.uv.xy) * _Color;
	//float3 normalMap = UnpackScaleNormal (UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, i.uv),_BumpScale);
	#if defined (_DETAIL_MAP)
		float4 detailMask=UNITY_SAMPLE_TEX2D_SAMPLER (_DetailMask, _MainTex, i.uv.xy);
		float4 detailTexture=UNITY_SAMPLE_TEX2D_SAMPLER (_DetailTexture, _MainTex, i.uv.zw)*_DetailColor;
		albedo=lerp(albedo, albedo * detailTexture, detailMask.r * _DetailIntensity); 
		
		//float3 detailNormals =UnpackScaleNormal (UNITY_SAMPLE_TEX2D_SAMPLER(_DetailBumpMap, _MainTex, i.detailUv),_DetailBumpScale);
		//float3 finalNormals = normalize(float3(normalMap.xy + detailNormals.xy, normalMap.z*detailNormals.z));
		//normalMap = lerp(normalMap,finalNormals,detailMask.r*_DetailIntensity);
	#endif
    return albedo;
}
float3 GetEmission (Interpolators i) 
{
    float3 emission = 0;
    #if defined (_EMISSION)
		emission = (UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, i.uv.xy) * _EmissionColor).rgb;
	#endif
    return emission;
}

float GetMetallic(Interpolators i)
{
    return (UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicMap, _MainTex, i.uv) * _Metallic).r;
}

float3 GetSpecular(Interpolators i)
{
    return UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicMap, _MainTex, i.uv.xy).rgb;
}

float GetRoughness(Interpolators i)
{
    return  1-(UNITY_SAMPLE_TEX2D_SAMPLER(_GlossinessMap, _MainTex, i.uv.xy) * _Glossiness).r;
}

void GetMetaSurfaceData (Interpolators i, inout UnityMetaInput data) 
{
    float3 albedo = GetAlbedo(i);
    float3 emission = GetEmission(i);
    float3 specular = 0;
    float roughness = 0;
    #if defined (_ENABLE_SPECULAR)
    roughness = GetRoughness(i);
    float oneMinusReflectivity;
        #if defined(_SPECULAR_WORKFLOW)
            specular = GetSpecular(i);
            albedo = EnergyConservationBetweenDiffuseAndSpecular(albedo, specular, /*out*/ oneMinusReflectivity);
        #else
            albedo = DiffuseAndSpecularFromMetallic (albedo, GetMetallic(i), /*out*/ specular, /*out*/ oneMinusReflectivity);
        #endif
    #endif
    data.Emission = emission;
	data.Albedo = albedo + specular * roughness * roughness;
	data.SpecularColor = specular;
}

Interpolators MetaVertexFunction (VertexData v) 
{
	Interpolators i;
    i.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);

	i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTexture);
	return i;
}

float4 MetaFragmentFunction (Interpolators i) : SV_TARGET 
{
	UnityMetaInput surfaceData;
    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, surfaceData);
	GetMetaSurfaceData(i, surfaceData);
    //surfaceData.Emission = float3(1,0,0);
	//surfaceData.Albedo = .5;
	//surfaceData.SpecularColor = float3(1,0,0);
	return UnityMetaFragment(surfaceData);
}

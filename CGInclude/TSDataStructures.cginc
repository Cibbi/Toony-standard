struct VertexData 
{
	float4 vertex     : POSITION;
	float2 uv         : TEXCOORD0;
    float2 uv1        : TEXCOORD1;
    float2 uv2        : TEXCOORD2;
	float3 normal     : NORMAL;
	float4 tangentDir : TANGENT;
};

struct FragmentData 
{
	float4 pos        : SV_POSITION;
	float3 normal     : NORMAL;
	float4 tangentDir : TANGENT;
	float2 uv         : TEXCOORD0;
	float2 detailUv   : TEXCOORD1;
	float2 HPuv       : TEXCOORD2;
	float3 worldPos   : TEXCOORD3;
    
    UNITY_SHADOW_COORDS(4)
    UNITY_FOG_COORDS(5)

    #if defined(LIGHTMAP_ON)
        float2 lightmapUV : TEXCOORD6;
    #endif
    #if defined(DYNAMICLIGHTMAP_ON)
		float2 dynamicLightmapUV : TEXCOORD7;
	#endif
};

struct DirectionData
{
    float3 light;
    float3 view;
    float3 tangent;
    #if defined(_ANISOTROPIC_SPECULAR) && defined (_ENABLE_SPECULAR)
        float3 tangentMap;
    #endif
    float3 bitangent;
    float3 halfD;
    float3 reflect;
};

struct BaseDots
{
    float NdotL;
    float NdotV; 
    float NdotH;
    float LdotH;
};

struct RampData
{
    sampler2D ramp;
    float offset;
    float4 color;
};

//struct that is passed to the BRDF
struct BRDFData 
{
    float attenuation;
    DirectionData dir;
    float3 worldPos;

    float3 albedo;
    float alpha;
    float3 normal;
    float occlusion;
    float occlusionOffsetIntensity;

    RampData mainRamp;
    float3 mainRampMin;
	float3 mainRampMax;
    #if defined (_ENABLE_SPECULAR)
        float metallic;
        #if defined(_SPECULAR_WORKFLOW)
            float3 specular;
        #endif
        float roughness;
        #if defined(_ANISOTROPIC_SPECULAR)
            float anisotropy;
        #endif
        #if defined(_FAKE_SPECULAR)
            float3 fakeHighlights;
        #endif
        float indirectSpecular;
        float3 customIndirect;

        float toonyHighlights;    
        RampData highlightRamp;
        float highlightPattern;
    #endif

    #if defined(LIGHTMAP_ON)
        float3 lightmap;
        #if defined(DIRLIGHTMAP_COMBINED)
            float4 lightmapDirection;
        #endif
    #endif

    #if defined(DYNAMICLIGHTMAP_ON)
        float3 dynamicLightmap;
        #if defined(DIRLIGHTMAP_COMBINED)
            float4 dynamicLightmapDirection;
        #endif
    #endif


};
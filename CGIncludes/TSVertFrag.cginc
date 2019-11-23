UNITY_DECLARE_TEX2D(_MainTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailBumpMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MSOD);
//UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicMap);
//UNITY_DECLARE_TEX2D_NOSAMPLER(_GlossinessMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_AnisotropyMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_TangentMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailTexture);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ThicknessMap);

float4 _MainTex_ST, _DetailTexture_ST, _HighlightPattern_ST;
float4 _Color, _RampColor, _HighlightRampColor, _IndirectColor, _DetailColor, _RimColor, _EmissionColor, _SSColor;

float _Cutoff, _Occlusion, _RampOffset, _ShadowIntensity, _OcclusionOffsetIntensity,
	  _RimIntensity, _RimStrength, _RimSharpness, _Metallic, _Glossiness, _Anisotropy,
	  _FakeHighlightIntensity, _HighlightRampOffset, _HighlightIntensity, _DetailIntensity,
	  _SSDistortion, _SSPower, _SSScale;
float _BumpScale, _DetailBumpScale;
float _RampOn, _RimLightOn, _SSSOn, _EmissiveRim, _IndirectSpecular, _ToonyHighlights;
float4 _MainRampMin, _MainRampMax;

sampler2D _Ramp, _HighlightRamp;
sampler2D _Matcap;
samplerCUBE _Cubemap;
sampler2D _HighlightPattern;
sampler2D _FakeHighlights;

#include "TSBRDF.cginc"

FragmentData VertexFunction (VertexData v)
{
	FragmentData i;
	UNITY_INITIALIZE_OUTPUT(FragmentData, i);
	i.pos        = UnityObjectToClipPos(v.vertex);
	i.normal     = UnityObjectToWorldNormal(v.normal);
	i.worldPos   = mul(unity_ObjectToWorld, v.vertex);			
	i.tangentDir = v.tangentDir;
	i.uv         = TRANSFORM_TEX(v.uv, _MainTex);
	i.detailUv   = TRANSFORM_TEX(v.uv, _DetailTexture);
	i.HPuv       = TRANSFORM_TEX(v.uv, _HighlightPattern);

	UNITY_TRANSFER_SHADOW(i, v.uv);
	UNITY_TRANSFER_FOG(i, i.pos);

	#if defined(LIGHTMAP_ON)
		i.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif

	#if defined(DYNAMICLIGHTMAP_ON)
		i.dynamicLightmapUV = v.uv2 * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif

	return i;
}

float4 FragmentFunction (FragmentData i) : SV_TARGET
{	
	//early albedo sampling in order to clip as soon as possible
	float4 albedo;
	albedo = UNITY_SAMPLE_TEX2D (_MainTex, i.uv) * _Color;
	#if defined (_DETAIL_MULX2)
		float4 detailMask=UNITY_SAMPLE_TEX2D_SAMPLER (_DetailMask, _MainTex, i.uv);
		float4 detailTexture=UNITY_SAMPLE_TEX2D_SAMPLER (_DetailTexture, _MainTex, i.detailUv)*_DetailColor;
		albedo=lerp(albedo, albedo * detailTexture, detailMask.r * _DetailIntensity);
	#endif
	//clipping discarted fragments
	#if defined(_ALPHATEST_ON)
		clip(albedo.a - _Cutoff);
	#endif
	
	float3 normalMap = UnpackScaleNormal (UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, i.uv),_BumpScale);
	#if defined (_DETAIL_MULX2)
		float3 detailNormals =UnpackScaleNormal (UNITY_SAMPLE_TEX2D_SAMPLER(_DetailBumpMap, _MainTex, i.detailUv),_DetailBumpScale);
		float3 finalNormals = normalize(float3(normalMap.xy + detailNormals.xy, normalMap.z*detailNormals.z));
		normalMap = lerp(normalMap,finalNormals,detailMask.r*_DetailIntensity);
	#endif

	//calculationg direction vectors
	float3 NormalDirection = normalize(i.normal);
	float3 WorldTangent    = normalize(UnityObjectToWorldDir(i.tangentDir.xyz));
	float3 WorldBinormal   = normalize(cross(NormalDirection,WorldTangent) * i.tangentDir.w * unity_WorldTransformParams.w);
	CalculateNormals(NormalDirection, WorldTangent, WorldBinormal, normalMap);			
	float3 LightDirection  = normalize(UnityWorldSpaceLightDir(i.worldPos));
    float3 ViewDirection   = normalize(UnityWorldSpaceViewDir(i.worldPos));
	float3 worldRefl       = reflect(-ViewDirection, NormalDirection);
	
	//sampling MSOD map
	float4 msod = UNITY_SAMPLE_TEX2D_SAMPLER(_MSOD, _MainTex, i.uv);

	//sampling occlusion
	float occlusion = lerp(1,msod.b,_Occlusion).r;

	//sampling specular related textures
	#if !defined (_SPECULARHIGHLIGHTS_OFF)
		#if defined(_SPECGLOSSMAP)
			float3 specular = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicMap, _MainTex, i.uv).rgb;
		#else
			float metallic = msod.r * _Metallic;
		#endif
		float roughness = 1-(msod.g * _Glossiness);

		#if defined(_ANISOTROPIC_SPECULAR)
			float3 tangentTS = UNITY_SAMPLE_TEX2D_SAMPLER(_TangentMap, _MainTex, i.uv);
			
			float anisotropy = (UNITY_SAMPLE_TEX2D_SAMPLER(_AnisotropyMap, _MainTex, i.uv) * _Anisotropy).r;
			float3 tangentMap = GetModifiedTangent(tangentTS, WorldTangent);
		#endif

		#if defined(_FAKE_SPECULAR)
			float3 fakeHighlights = tex2D(_FakeHighlights , remap(worldRefl.xy,-1,1,0.1,0.9)).rgb*(sin((1-roughness)*UNITY_PI))*_FakeHighlightIntensity;
		#endif

		#if defined(_ANISOTROPIC_SPECULAR)
			float3  anisotropyDirection = anisotropy >= 0.0 ? WorldBinormal : WorldTangent;
			float3  anisotropicTangent  = cross(anisotropyDirection, ViewDirection);
			float3  anisotropicNormal   = cross(anisotropicTangent, anisotropyDirection);
			float   bendFactor          = abs(anisotropy) * saturate(1-(Pow5(1-roughness)));
			float3  bentNormal          = normalize(lerp(NormalDirection, anisotropicNormal, bendFactor));
			worldRefl = reflect(-ViewDirection, bentNormal);
		#endif

		float3 customIndirect = 0;
		if(_IndirectSpecular == 1)
		{
			float3 matcap =lerp(tex2D(_Matcap,float2(.5,.5)),tex2Dlod(_Matcap , half4( remap(worldRefl.xy,-1,1,0.1,0.9),0, remap(roughness, 1, 0, 5, 0))).rgb,1-roughness);
			customIndirect=matcap*occlusion; 
		}

		else if(_IndirectSpecular == 2)
		{
			float3 cubemap = texCUBElod(_Cubemap, half4(worldRefl.xyz, remap(roughness, 1, 0, 5, 0))).rgb;
			customIndirect=cubemap*occlusion;
		}
		else if (_IndirectSpecular == 3)
		{
			customIndirect=_IndirectColor.rgb*occlusion;
		}
	#endif

    float SSSthickness = 0;

	if(_SSSOn > 0)
	{
		SSSthickness = UNITY_SAMPLE_TEX2D_SAMPLER(_ThicknessMap, _MainTex, i.uv).r;
	}
	
	//passing the required data to the brdf 
	BRDFData s;

	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
	attenuation = FadeShadows(i, attenuation);


	s.attenuation   = attenuation;
	s.dir.light     = LightDirection;
	s.dir.view      = ViewDirection;
	s.dir.tangent   = WorldTangent;
	s.dir.bitangent = WorldBinormal;
	s.dir.reflect   = worldRefl;
    s.worldPos      = i.worldPos;

	s.albedo 	= albedo.rgb;
	s.alpha  	= albedo.a;
	s.normal 	= NormalDirection;
	s.occlusion = occlusion;
	s.occlusionOffsetIntensity = _OcclusionOffsetIntensity;

	s.mainRamp.ramp   = _Ramp;
	s.mainRamp.color  = float4(_RampColor.rgb, _ShadowIntensity);
	s.mainRamp.offset = _RampOffset;
	s.mainRampMin = _MainRampMin.rgb;
	s.mainRampMax = _MainRampMax.rgb;

	#if !defined (_SPECULARHIGHLIGHTS_OFF)
		#if defined(_SPECGLOSSMAP)
			s.metallic = 1;
			s.specular = specular;
		#else
			s.metallic = metallic;
		#endif
		s.roughness = roughness;

		#if defined(_ANISOTROPIC_SPECULAR)
			s.anisotropy 	 = anisotropy;
			s.dir.tangentMap = tangentMap;
		#endif
		#if defined(_FAKE_SPECULAR)
			s.fakeHighlights = fakeHighlights;
		#endif

		s.indirectSpecular = _IndirectSpecular;
		s.customIndirect   = customIndirect;

		s.toonyHighlights      = _ToonyHighlights;
		s.highlightRamp.ramp   = _HighlightRamp;
		s.highlightRamp.color  = _HighlightRampColor*_HighlightIntensity;
		s.highlightRamp.offset = _HighlightRampOffset;
		s.highlightPattern     = tex2D(_HighlightPattern, i.HPuv).r;
	#endif	

	s.sss.color = _SSColor.rgb * min(1, _SSSOn);
    s.sss.thickness = SSSthickness;
    s.sss.distortion = _SSDistortion;
    s.sss.power = _SSPower;
    s.sss.scale = _SSScale;			

	//lightmap sampling
	#if defined(LIGHTMAP_ON)
		s.lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV));
	
		//directional map sampling
		#if defined(DIRLIGHTMAP_COMBINED)
			s.lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, i.lightmapUV);
		#endif
	#endif
	//dynamic Lightmap sampling
	#if defined(DYNAMICLIGHTMAP_ON)
		s.dynamicLightmap = DecodeRealtimeLightmap( UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, i.dynamicLightmapUV));
		
		#if defined(DIRLIGHTMAP_COMBINED)
			s.dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, i.dynamicLightmapUV);
		#endif
	#endif

	//BRDF
	float4 BRDFResult = TS_BRDF(s);
	#if defined (UNITY_PASS_FORWARDBASE)
		//adding rim light
		float NdotV = max(dot(s.normal, s.dir.view),0);
		if(_RimLightOn!=0 && _EmissiveRim==0)
		{
			half3 rim=pow((1-NdotV),max((1-_RimStrength)*10,0.001));
			_RimSharpness/=2;
			rim=1+((smoothstep(_RimSharpness,1-_RimSharpness,rim)*_RimIntensity)*_RimColor);
			BRDFResult.rgb*=rim;
		}
		else if(_RimLightOn!=0 && _EmissiveRim==1)
		{
			half3 rim=pow((1-NdotV),max((1-_RimStrength)*10,0.001));
			_RimSharpness/=2;
			rim=(smoothstep(_RimSharpness,1-_RimSharpness,rim)*_RimIntensity)*_RimColor;
			BRDFResult.rgb+=rim;
		}

		//adding emission (only in the base pass)
		#if defined (_EMISSION)
			BRDFResult.rgb += (UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, i.uv) * _EmissionColor).rgb;
		#endif
	#endif
	//apply fog
	UNITY_APPLY_FOG(i.fogCoord, BRDFResult);

	return BRDFResult;//*0.0001 + attenuation;
}
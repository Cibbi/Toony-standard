
half _ToonyHighlights;
half _FakeLight;
half _IndirectSpecular;
half _RimLightOn;
half _EmissiveRim;

half _OcclusionOffsetIntensity;



// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

#ifndef UNITY_PBS_LIGHTING_INCLUDED
#define UNITY_PBS_LIGHTING_INCLUDED

#ifdef UNITY_PASS_SHADOWCASTER
	#undef INTERNAL_DATA
	#undef WorldReflectionVector
	#undef WorldNormalVector
	#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
	#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
	#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
#endif

#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"
#include "UnityGBuffer.cginc" 
#include "UnityGlobalIllumination.cginc"
#include "UnityStandardBRDF.cginc"

inline float remap(float value, float oldMin, float oldMax, float newMin, float newMax) {
	return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
}
inline float2 remap(float2 value, float2 oldMin, float2 oldMax, float2 newMin, float2 newMax) {
	return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
}
inline float3 remap(float3 value, float3 oldMin, float3 oldMax, float3 newMin, float3 newMax) {
	return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
}

sampler2D RampSampler;
half RampOffset = 0;
half4 RampColor = half4(1,1,1,1);

half3 CustomIndirect = half3(0,0,0);

half HighlightPattern;

void setRampParameters(sampler2D ramp, half offset, half4 color)
{
	RampSampler = ramp;
	RampOffset = offset;
	RampColor = color;
}

sampler2D HighlightRampSampler;
half HighlightRampOffset = 0;
half3 HighlightRampColor = half3(1,1,1);

void setHighlightRampParameters(sampler2D ramp, half offset, half3 color, half intensity)
{
	HighlightRampSampler = ramp;
	HighlightRampOffset = offset;
	HighlightRampColor = color * 10 * intensity;
}

half3 FakeLightColor = half3(0.5, 0.5, 0.5);
half3 FakeLightDir = half3(0.7, 1, 0);
void setFakeLightParameters(half3 color, half3 dir)
{
	FakeLightColor = color;
	FakeLightDir = dir;
}

half RimStrength=0;
half RimSharpness=0;
half RimIntensity=0;
half3 RimColor=0;
void setRimLightParameters(half3 color, half strength, half sharpness, half intensity)
{
	RimColor=color;
	RimStrength=strength;
	RimSharpness=sharpness;
	RimIntensity=intensity;
}

void setCustomIndirect(half3 customIndirect)
{
	CustomIndirect = customIndirect;
}

half3 fakeHighlights=0;
void setFakeHighlights(half3 fh)
{
	fakeHighlights=fh;
}

half Anisotropy=0;
float2x3 worldVectors=0;
void setAnisotropy(float3 tangentTS, float3 tangentDir, float3 bitangentDir)
{
	float3x3 worldToTangent;
	worldToTangent[0] = float3(1, 0, 0);
	worldToTangent[1] = float3(0, 1, 0);
	worldToTangent[2] = float3(0, 0, 1); 

	//float3 tangentTS = tex2D(_TangentMap, IN.uv_MainTex);
	float3 tangentTWS = mul(tangentTS, worldToTangent);
	float3 fTangent;
	if (tangentTS.z < 1)
		fTangent = tangentTWS;
	else
		fTangent = tangentDir;
	worldVectors = float2x3(fTangent, bitangentDir/*, IN.normalDir*/);
}
// Anisotropic GGX
// From HDRenderPipeline
float D_GGXAnisotropic(float TdotH, float BdotH, float NdotH, float roughnessT, float roughnessB)
{
	float f = TdotH * TdotH / (roughnessT * roughnessT) + BdotH * BdotH / (roughnessB * roughnessB) + NdotH * NdotH;
	return 1.0 / (roughnessT * roughnessB * f * f);
}

// Smith Joint GGX Anisotropic Visibility
// Taken from https://cedec.cesa.or.jp/2015/session/ENG/14698.html
float SmithJointGGXAnisotropic(float TdotV, float BdotV, float NdotV, float TdotL, float BdotL, float NdotL, float roughnessT, float roughnessB)
{
	float aT = roughnessT;
	float aT2 = aT * aT;
	float aB = roughnessB;
	float aB2 = aB * aB;

	float lambdaV = NdotL * sqrt(aT2 * TdotV * TdotV + aB2 * BdotV * BdotV + NdotV * NdotV);
	float lambdaL = NdotV * sqrt(aT2 * TdotL * TdotL + aB2 * BdotL * BdotL + NdotL * NdotL);

	return 0.5 / (lambdaV + lambdaL);
}

// Convert Anistropy to roughness
void ConvertAnisotropyToRoughness(float roughness, float anisotropy, out float roughnessT, out float roughnessB)
{
	// (0 <= anisotropy <= 1), therefore (0 <= anisoAspect <= 1)
	// The 0.9 factor limits the aspect ratio to 10:1.
	float anisoAspect = sqrt(1.0 - 0.9 * anisotropy);
	roughnessT = roughness / anisoAspect; // Distort along tangent (rougher)
	roughnessB = roughness * anisoAspect; // Straighten along bitangent (smoother)
}

/*float3 GetAnisotropicModifiedNormal(float3 grainDir, float3 N, float3 V, float anisotropy)
{
	float3 grainNormal = ComputeGrainNormal(grainDir, V);
	// TODO: test whether normalizing 'grainNormal' is worth it.
	return normalize(lerp(N, grainNormal, anisotropy));
}*/

float ClampRoughnessForAnalyticalLights(float roughness)
{
	return max(roughness, 0.000001);
}




half4 BRDF_Unity_PBS(half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness, half multiplier, half metallic, half3 ramp,
	float3 normal, float3 viewDir,
	UnityLight light, UnityIndirect gi)
{

	float perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
	float3 halfDir = Unity_SafeNormalize(float3(light.dir) + viewDir);

	// NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
	// In this case normal should be modified to become valid (i.e facing camera) and not cause weird artifacts.
	// but this operation adds few ALU and users may not want it. Alternative is to simply take the abs of NdotV (less correct but works too).
	// Following define allow to control this. Set it to 0 if ALU is critical on your platform.
	// This correction is interesting for GGX with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
	// Edit: Disable this code by default for now as it is not compatible with two sided lighting used in SpeedTree.
#define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0

#if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
	// The amount we shift the normal toward the view vector is defined by the dot product.
	half shiftAmount = dot(normal, viewDir);
	normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
	// A re-normalization should be applied here but as the shift is small we don't do it to save ALU.
	//normal = normalize(normal);

	half nv = saturate(dot(normal, viewDir)); // TODO: this saturate should no be necessary here
#else
	half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact
#endif
	half nl = saturate(dot(normal, light.dir));
	float nh = saturate(dot(normal, halfDir));
	half lv = saturate(dot(light.dir, viewDir));
	half lh = saturate(dot(light.dir, halfDir));

	// Diffuse term
	//half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness);

	//half diffuseTerm = nl;
#if defined(_ENABLE_SPECULAR)
	// Specular term
	// HACK: theoretically we should divide diffuseTerm by Pi and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi too in cases when they are injected into ambient SH
	float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

	#if defined(_ANISOTROPIC_SPECULAR)

		float3 tangent = worldVectors[0];
		float3 bitangent = worldVectors[1];
		float3 H = Unity_SafeNormalize(light.dir + viewDir);


		//Tangent vectors
		float TdotH = dot(tangent, H);
		float TdotL = dot(tangent, light.dir);
		float BdotH = dot(bitangent, H);
		float BdotL = dot(bitangent, light.dir);
		float TdotV = dot(viewDir, tangent);
		float BdotV = dot(viewDir, bitangent);

		float roughnessT;
		float roughnessB;
		roughness = max(roughness, 0.0002);
		ConvertAnisotropyToRoughness(roughness, Anisotropy, roughnessT, roughnessB);
		//Clamp roughness
		roughnessT = ClampRoughnessForAnalyticalLights(roughnessT);
		roughnessB = ClampRoughnessForAnalyticalLights(roughnessB);
		//Visibility & Distribution terms
		float V = SmithJointGGXAnisotropic(TdotV, BdotV, nv, TdotL, BdotL, nl, roughnessT, roughnessB);
		float D = D_GGXAnisotropic(TdotH, BdotH, nh, roughnessT, roughnessB);

	#else 
		#if defined(_FAKE_SPECULAR)

			half V = 1;
			float D = fakeHighlights*50*(1-metallic+(0.1*metallic));
		#else

			#if UNITY_BRDF_GGX
				// GGX with roughtness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughtness remapping.
				roughness = max(roughness, 0.002);
				half V = SmithJointGGXVisibilityTerm(nl, nv, roughness);
				float D = GGXTerm(nh, roughness);
			#else
			// Legacy
				half V = SmithBeckmannVisibilityTerm(nl, nv, roughness);
				half D = NDFBlinnPhongNormalizedTerm(nh, PerceptualRoughnessToSpecPower(perceptualRoughness));
			#endif
		#endif

	#endif




	//half3 D2=D;
	//toony highlights instead of the pbr ones
		half newMin = max(HighlightRampOffset, 0);
		half newMax = max(HighlightRampOffset + 1, 0);
		half Duv=remap(clamp(D,0,2), 0, 2, newMin, newMax);	
	half3 D2 =lerp(D, (tex2D(HighlightRampSampler, float2(Duv, Duv)).rgb*HighlightRampColor*(1-metallic+(0.1*metallic))),_ToonyHighlights);
	//specular pattern
	D2*=HighlightPattern;
	#if defined(_ANISOTROPIC_SPECULAR)
	float3 specularTerm = V * D2;
	#else
	half3 specularTerm = V * D2 * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later
	#endif

	#ifdef UNITY_COLORSPACE_GAMMA
		specularTerm = sqrt(max(1e-4h, specularTerm));
	#endif

	//specular value, added to add compatibility to msra maps (or similar) coming from unreal engine, has some influence in the end result only if the material is not metallic
	multiplier = lerp(multiplier*2, 1, metallic);

	

	#if defined(_FAKE_SPECULAR)
		specularTerm = max(0, specularTerm * ramp * ramp);
	#else
		// specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
		specularTerm = max(0, specularTerm * nl);
	#endif

	/*#if defined(_SPECULARHIGHLIGHTS_OFF)
		specularTerm = 0.0;
	#endif*/

	// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
	half surfaceReduction;
	#ifdef UNITY_COLORSPACE_GAMMA
		surfaceReduction = 1.0 - 0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
	#else
		surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
	#endif

	// To provide true Lambert lighting, we need to be able to kill specular completely.
	specularTerm *= any(specColor) ? 1.0 : 0.0;
	half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));

	if (_IndirectSpecular>0)
	{
		gi.specular = CustomIndirect;
	}

	

#endif	

	

	float3 directLighting = (ShadeSH9(half4(0.0, 1.0, 0.0, 1.0)) + light.color);

	half3 DiffuseColor = diffColor * (/*indirectLighting +*/ directLighting * ramp);		 //diffuseTerm
	half3 DirectSpecular = 0;
	half3 IndirectSpecular = 0;

	half lightColGrey = (directLighting.r + directLighting.g + directLighting.b) / 3;
		if (any(_WorldSpaceLightPos0.xyz) == 0&&_FakeLight==1)
		{
			light.color = FakeLightColor * lightColGrey;
			DiffuseColor*=FakeLightColor;
		}

	#if defined(_ENABLE_SPECULAR)
		DirectSpecular = specularTerm * ramp * light.color * FresnelTerm(specColor, lh);		
		IndirectSpecular = surfaceReduction * gi.specular * lightColGrey * multiplier * ramp * FresnelLerp(specColor, grazingTerm, nv);

		
	#endif
	half3 color = DiffuseColor + DirectSpecular + IndirectSpecular;

	//rim light
	if(_RimLightOn!=0 && _EmissiveRim==0)
	{
		half3 rim=pow((1-nv),max((1-RimStrength)*10,0.001));
		RimSharpness/=2;
		rim=1+((smoothstep(RimSharpness,1-RimSharpness,rim)*RimIntensity)*RimColor);
		color*=rim;
	}
	else if(_RimLightOn!=0 && _EmissiveRim==1)
	{
		half3 rim=pow((1-nv),max((1-RimStrength)*10,0.001));
		RimSharpness/=2;
		rim=(smoothstep(RimSharpness,1-RimSharpness,rim)*RimIntensity)*RimColor;
		color+=rim;
	}

	return half4(color, 1);
}


#define UNITY_BRDF_PBS BRDF_Unity_PBS



// Surface shader output structure to be used with physically
// based shading model.

//-------------------------------------------------------------------------------------
// Metallic workflow

struct SurfaceOutputToonyStandard
{
    fixed3 Albedo;      // base (diffuse or specular) color
    float3 Normal;      // tangent space normal, if written
    half3 Emission;		// emission value
    half3 Metallic;      // 0=non-metal, 1=metal
						// Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
						// Everywhere in the code you meet smoothness it is perceptual smoothness
    half Smoothness;    // 0=rough, 1=smooth
	half Specular;		//specular component from unreal engine materials
    half Occlusion;     // occlusion (default 1)
	half Anisotropy;
	half HighlightPattern;
    fixed Alpha;        // alpha for transparencies
};

inline half4 LightingToonyStandard (SurfaceOutputToonyStandard s, float3 viewDir, UnityGI gi)
{
    s.Normal = normalize(s.Normal);
	viewDir = normalize(viewDir);
	float3 lightDir = gi.light.dir;
	float3 attenRGB = gi.light.color / ((_LightColor0.rgb) + 0.000001);
	float atten = max(max(attenRGB.r, attenRGB.g), attenRGB.b);
	#if DIRECTIONAL
	atten=round(atten);
	gi.light.color=_LightColor0.rgb;
	#else
	gi.light.color=_LightColor0.rgb*atten;
	#endif
	
	if (any(_WorldSpaceLightPos0.xyz)==0&&_FakeLight==1)
	{
		lightDir = FakeLightDir;
		gi.light.dir = FakeLightDir;
		//gi.light.color = float3(0.5, 0.5, 0.5);
		atten = 1;
	}


    half oneMinusReflectivity;
    half3 specColor;

	#if !defined(_ENABLE_SPECULAR)
	s.Metallic = 0;
	#endif

	#if defined(_SPECULAR_WORKFLOW)
		s.Albedo = EnergyConservationBetweenDiffuseAndSpecular(s.Albedo, s.Metallic, /*out*/ oneMinusReflectivity);
	#else
		s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic.r, /*out*/ specColor, /*out*/ oneMinusReflectivity);
	#endif

	

	Anisotropy=s.Anisotropy;
	HighlightPattern=s.HighlightPattern;
	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

	half NdotL = dot(s.Normal, lightDir);
	NdotL = min(NdotL,atten);
	
	RampOffset=RampOffset+(s.Occlusion*_OcclusionOffsetIntensity)-_OcclusionOffsetIntensity;
	half newMin = max(RampOffset, 0);
	half newMax = max(RampOffset + 1, 0);
	half rampUv=remap(NdotL, -1, 1, newMin, newMax);
	half3 ramp = tex2D(RampSampler,float2(rampUv, rampUv)).rgb;
	ramp *= RampColor.rgb;
	ramp = remap(ramp, float3(0, 0, 0), float3(1, 1, 1),1-RampColor.aaa, float3(1, 1, 1));

	#if defined(_SPECULAR_WORKFLOW)
		half4 c = BRDF_Unity_PBS(s.Albedo, s.Metallic, oneMinusReflectivity, s.Smoothness, s.Specular, 0, ramp, s.Normal, viewDir, gi.light, gi.indirect);
	#else
		half4 c = BRDF_Unity_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Specular, s.Metallic.r, ramp, s.Normal, viewDir, gi.light, gi.indirect);
	#endif

	

    c.a = outputAlpha;
    return c;
}

inline void LightingToonyStandard_GI (
    SurfaceOutputToonyStandard s,
    UnityGIInput data,
    inout UnityGI gi)
{
#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
    gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
#else
    Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
    gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
#endif
	
}

//-------------------------------------------------------------------------------------
// Specular workflow
/*
struct SurfaceOutputStandardSpecular
{
    fixed3 Albedo;      // diffuse color
    fixed3 Specular;    // specular color
    float3 Normal;      // tangent space normal, if written
    half3 Emission;
    half Smoothness;    // 0=rough, 1=smooth
    half Occlusion;     // occlusion (default 1)
	half3 Ramp;		//used for toon diffuse
    fixed Alpha;        // alpha for transparencies
};

inline half4 LightingStandardSpecular (SurfaceOutputStandardSpecular s, float3 viewDir, UnityGI gi)
{
    s.Normal = normalize(s.Normal);

    // energy conservation
    half oneMinusReflectivity;
    s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out oneMinusReflectivity);

    // shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    // this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
    half outputAlpha;
    s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out outputAlpha);

    half4 c = BRDF_Unity_PBS(s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, 1, 1, 0, s.Normal, viewDir, gi.light, gi.indirect);
    c.a = outputAlpha;
    return c;
}

inline half4 LightingStandardSpecular_Deferred (SurfaceOutputStandardSpecular s, float3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
{
    // energy conservation
    half oneMinusReflectivity;
    s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out oneMinusReflectivity);

    half4 c = BRDF_Unity_PBS(s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, 1, 1, 0, s.Normal, viewDir, gi.light, gi.indirect);

    UnityStandardData data;
    data.diffuseColor   = s.Albedo;
    data.occlusion      = s.Occlusion;
    data.specularColor  = s.Specular;
    data.smoothness     = s.Smoothness;
    data.normalWorld    = s.Normal;

    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

    half4 emission = half4(s.Emission + c.rgb, 1);
    return emission;
}

inline void LightingStandardSpecular_GI (
    SurfaceOutputStandardSpecular s,
    UnityGIInput data,
    inout UnityGI gi)
{
#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
    gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
#else
    Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, s.Specular);
    gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
#endif
}
*/
#endif // UNITY_PBS_LIGHTING_INCLUDED

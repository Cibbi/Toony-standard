Shader "Hidden/Cibbis shaders/toony standard/Cutout"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Normal]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Scale", Float) = 1.0
		_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
		_OcclusionMap("Occlusion Map", 2D) = "white" {}
		_Occlusion("Occlusion", Range(0,1)) = 1.0

		_Ramp("Ramp Texture", 2D) = "white" {}
		_RampColor("Ramp Color", Color) = (1,1,1,1)
		_ShadowIntensity("Shadow Intensity", Range(0,1)) = 0.4
		_RampOffset("Ramp Offset", Range(-1,1)) = 0.0
		_OcclusionOffsetIntensity("Occlusion Offset", Range(0,1)) = 0.0

		_RimStrength("Rim Strength", Range(0,1)) = 0.0
		_RimSharpness("Rim Sharpness", Range(0,1)) = 0.0
		_RimIntensity("Rim Intensity", Range(-1,1)) = 0.0
		[HDR]_RimColor("Rim Color", Color) = (1,1,1,1)

		_Metallic("Metallic", Range(0,1)) = 0.0
		_MetallicMap("Metallic Map", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.0
		_GlossinessMap("Smoothness Map", 2D) = "white" {}
		_TangentMap("Tangent Map", 2D) = "white" {}
		_Anisotropy ("Anisotropy", Range(-1,1)) = 0.0
		_AnisotropyMap("Anisotropy Map", 2D) = "white" {}
		_FakeHighlights("Fake Highlights", 2D) = "black" {}
		_Matcap("Matcap", 2D) = "white" {}
		_Cubemap("Cubemap", CUBE) = "" {}
		[HDR]_IndirectColor("Color", Color) = (.7,.7,.7,1)
		_HighlightPattern("Highlight Pattern", 2D) = "white" {}

		_HighlightRamp("Highlight Ramp Texture", 2D) = "white" {}
		[HDR]_HighlightRampColor("Highlight Ramp Color", Color) = (1,1,1,1)
		_HighlightRampOffset("Highlight Ramp Offset", Range(-1,1)) = 0.0
		_HighlightIntensity("Highlight Intensity", Range(0,1)) = 1.0
		_FakeHighlightIntensity("Fake Highlight Intensity", Range(0,1)) = 0.5		

		_DetailMask ("Detail Mask", 2D) = "white" {}
		_DetailIntensity("Detail Intensity", Range(0,1)) = 1.0
		_DetailTexture ("Albedo (RGB)", 2D) = "white" {}
		_DetailColor ("Color", Color) = (1,1,1,1)
		[Normal]_DetailBumpMap("Detail Normal Map", 2D) = "bump" {}
		_DetailBumpScale("Detail Normal Scale", Float) = 1.0


		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Int) = 2

		// Blending state
		[HideInInspector] _Mode("__Mode", Float) = 0.0
		[HideInInspector] _SpMode("__SpMode", Float) = 0.0
		[HideInInspector] _IndirectSpecular("__IndirectSpecular", Float) = 0.0
		[HideInInspector] _Workflow("__Workflow", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _MainRampMin("__MainRampMin", Color) = (0.001,0.001,0.001,0.001)
		[HideInInspector] _MainRampMax("__MainRampMax", Color) = (1,1,1,1)
 
		[HideInInspector] _ToonyHighlights("__ToonyHighlights", Float) = 0.0
		[HideInInspector] _OcclusionOffset("__OcclusionOffset", Float) = 0.0
		[HideInInspector] _EmissiveRim("__EmissiveRim", Float) = 0.0

		[HideInInspector] _RampOn("__RampOn", Float) = 1.0
		[HideInInspector] _RimLightOn("__RimLight", Float) = 0.0
		[HideInInspector] _SpecularOn("__EnableSpecular", Float) = 0.0
		[HideInInspector] _DetailMapOn("__DetailMap", Float) = 0.0

		[HideInInspector] _ToonRampBox("__ToonRampBox", Float) = 1.0
		[HideInInspector] _RimLightBox("__RimLightBox", Float) = 0.0
		[HideInInspector] _SpecularBox("__SpecularBox", Float) = 0.0
		[HideInInspector] _DetailBox("__DetailBox", Float) = 0.0

		[HideInInspector] _NeedsFix("__NeedsFix", Float) = 0.5
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "TransparentCutout" 
			"Queue" = "AlphaTest"
		}
		Blend One Zero
		ZWrite On
		Cull [_Cull]

		Pass 
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VertexFunction
			#pragma fragment FragmentFunction
			#pragma multi_compile_fwdbase	
			#pragma multi_compile_fog
			//#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile VERTEXLIGHT_ON
			#define UNITY_PASS_FORWARDBASE

			#define _ALPHATEST_ON
			#pragma shader_feature _SPECULAR_WORKFLOW
			#pragma shader_feature _ _ANISOTROPIC_SPECULAR _FAKE_SPECULAR
			#pragma shader_feature _ENABLE_SPECULAR
			#pragma shader_feature _DETAIL_MAP
			#pragma shader_feature _EMISSION
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"

			//#include "UnityStandardConfig.cginc"

			#include "CGInclude/TSDataStructures.cginc"
			#include "CGInclude/TSFunctions.cginc"
			#include "CGInclude/TSVertFrag.cginc"
			
			ENDCG
		}

		Pass 
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			Blend One One

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex VertexFunction
			#pragma fragment FragmentFunction
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog

			#define _ALPHATEST_ON
			#pragma shader_feature _SPECULAR_WORKFLOW
			#pragma shader_feature _ _ANISOTROPIC_SPECULAR _FAKE_SPECULAR
			#pragma shader_feature _ENABLE_SPECULAR
			#pragma shader_feature _DETAIL_MAP
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"

			//#include "UnityStandardConfig.cginc"
			#include "CGInclude/TSDataStructures.cginc"
			#include "CGInclude/TSFunctions.cginc"
			#include "CGInclude/TSVertFrag.cginc"
			
			ENDCG
		}
		Pass 
		{
			Tags 
			{
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2

			#pragma vertex ShadowVertexFunction
			#pragma fragment ShadowFragmentFunction

			#define _ALPHATEST_ON

			#include "CGInclude/TSShadowVertFrag.cginc"

			ENDCG
		}	
		Pass 
		{
			Tags 
			{
				"LightMode" = "Meta"
			}

			Cull Off

			CGPROGRAM

			#pragma vertex MetaVertexFunction
			#pragma fragment MetaFragmentFunction

			#pragma shader_feature _SPECULAR_WORKFLOW
			#pragma shader_feature _ _ANISOTROPIC_SPECULAR _FAKE_SPECULAR
			#pragma shader_feature _ENABLE_SPECULAR
			#pragma shader_feature _DETAIL_MAP
			#pragma shader_feature _EMISSION

			#include "CGInclude/TSMetaVertFrag.cginc"

			ENDCG
		}
	}
	CustomEditor "Cibbi.ToonyStandard.ToonyStandardGUI"
}

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

UNITY_DECLARE_TEX2D(_MainTex);
UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap);
sampler2D _Ramp;
sampler2D _HighlightRamp;
UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_GlossinessMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_TangentMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_AnisotropyMap);
sampler2D _Matcap;
sampler2D _HighlightPattern;
samplerCUBE _Cubemap;
sampler2D _FakeHighlights;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailMask);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailTexture);
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailBumpMap);

UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);

struct Input {
	float2 uv_MainTex;
	float2 uv_HighlightPattern;
	float2 uv_DetailTexture;
	float3 worldPos;
	float3 worldRefl; INTERNAL_DATA
	float3 normal;
	float3 viewDir;
	float3 normalDir;
	float3 tangentDir;
	float3 bitangentDir;
};
half _BumpScale;
half4 _EmissionColor;
half _Occlusion;
half _ShadowIntensity;
half _RampOffset;
half _HighlightRampOffset;
half _HighlightIntensity;
half _FakeHighlightIntensity;
half _Glossiness;
half _Anisotropy;
half _Metallic;
half4 _RampColor;
half3 _HighlightRampColor;
fixed4 _Color;
half4 _FakeLightColor;
half _FakeLightX;
half _FakeLightY;
half _FakeLightZ;

half _RimStrength;
half _RimSharpness;
half _RimIntensity;
half3 _RimColor;

half3 _IndirectColor;

half _DetailIntensity;
half4 _DetailColor;
half _DetailBumpScale;

half _Cutoff;

//helper parameters



//Vertex shader
void vert(inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT(Input, o);
	//Normal 2 World
	o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
	//Tangent 2 World
	float3 tangentMul = normalize(mul(unity_ObjectToWorld, v.tangent.xyz));
	o.tangentDir = float4(tangentMul, v.tangent.w);
	// Bitangent
	o.bitangentDir = cross(o.normalDir, o.tangentDir);
}

// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
// #pragma instancing_options assumeuniformscaling
UNITY_INSTANCING_BUFFER_START(Props)
	// put more per-instance properties here
UNITY_INSTANCING_BUFFER_END(Props)

void surf (Input IN, inout SurfaceOutputToonyStandard o) {

	

	// Albedo comes from a texture tinted by color
	fixed4 c = UNITY_SAMPLE_TEX2D (_MainTex, IN.uv_MainTex) * _Color;
	#if defined(_ALPHATEST_ON)
		clip(c.a - _Cutoff);
	#endif
	setRampParameters(_Ramp, _RampOffset,half4(_RampColor.rgb, _ShadowIntensity));

	setFakeLightParameters(_FakeLightColor.rgb, half3(_FakeLightX,_FakeLightY,_FakeLightZ));

	setRimLightParameters(_RimColor, _RimStrength, _RimSharpness, _RimIntensity);

	#if defined (_DETAIL_MAP)
		//blending of the texture
		half4 detailMask = UNITY_SAMPLE_TEX2D_SAMPLER (_DetailMask, _MainTex, IN.uv_MainTex);
		half4 detailTexture = UNITY_SAMPLE_TEX2D_SAMPLER (_DetailTexture, _MainTex, IN.uv_DetailTexture)*_DetailColor;
		half4 finalAlbedo = lerp(c,c*detailTexture,detailMask.r*_DetailIntensity);
		o.Albedo=finalAlbedo.rgb;
		o.Alpha=finalAlbedo.a;

		//blending normal maps with the whiteout method
		half3 mainNormals = UnpackScaleNormal (UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, IN.uv_MainTex),_BumpScale);
		half3 detailNormals =UnpackScaleNormal (UNITY_SAMPLE_TEX2D_SAMPLER(_DetailBumpMap, _MainTex, IN.uv_DetailTexture),_DetailBumpScale);
		float3 finalNormals = normalize(float3(mainNormals.xy + detailNormals.xy, mainNormals.z*detailNormals.z));
		o.Normal = lerp(mainNormals,finalNormals,detailMask.r*_DetailIntensity);

	#else
	
		o.Albedo = c.rgb;
		o.Alpha = c.a;
		o.Normal= UnpackScaleNormal (UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, IN.uv_MainTex),_BumpScale);

	#endif

	#if defined(_EMISSION)
		o.Emission = (UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, IN.uv_MainTex) * _EmissionColor).rgb;
		
	#endif
	// Metallic and smoothness come from slider variables
	#if defined(_SPECULAR_WORKFLOW)
		o.Metallic = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicMap, _MainTex, IN.uv_MainTex);
	#else
		o.Metallic = (UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicMap, _MainTex, IN.uv_MainTex) * _Metallic);
	#endif
	o.Smoothness = (UNITY_SAMPLE_TEX2D_SAMPLER(_GlossinessMap, _MainTex, IN.uv_MainTex) * _Glossiness).r;
	o.Specular = 0.5;
	#if defined(_ANISOTROPIC_SPECULAR)
	float3 tangentTS = UNITY_SAMPLE_TEX2D_SAMPLER(_TangentMap, _MainTex, IN.uv_MainTex);
	setAnisotropy(tangentTS, IN.tangentDir, IN.bitangentDir);
	o.Anisotropy = (UNITY_SAMPLE_TEX2D_SAMPLER(_AnisotropyMap, _MainTex, IN.uv_MainTex) * _Anisotropy).r;
	#endif
	o.HighlightPattern=tex2D(_HighlightPattern, IN.uv_HighlightPattern).r;

	o.Occlusion=lerp(1,UNITY_SAMPLE_TEX2D_SAMPLER(_OcclusionMap, _MainTex, IN.uv_MainTex),_Occlusion).r;	

		if(_IndirectSpecular==1)
		{
			half3 matcap =lerp(tex2D(_Matcap,float2(.5,.5)),tex2Dlod(_Matcap , half4( remap(WorldReflectionVector(IN, o.Normal).xy,-1,1,0.1,0.9),0, remap(o.Smoothness, 0, 1, 5, 0))).rgb,o.Smoothness);
 			setCustomIndirect(matcap*o.Occlusion); 
		}

		else if(_IndirectSpecular==2)
		{
			half3 cubemap = texCUBElod(_Cubemap, half4(WorldReflectionVector(IN, o.Normal).xyz, remap(o.Smoothness, 0, 1, 5, 0))).rgb;
			setCustomIndirect(cubemap*o.Occlusion);
		}
		else if (_IndirectSpecular==3)
		{
			setCustomIndirect(_IndirectColor*o.Occlusion);
		}
	#if defined(_FAKE_SPECULAR)
		half3 fh = tex2D(_FakeHighlights , remap(WorldReflectionVector(IN, o.Normal).xy,-1,1,0.1,0.9)).rgb*(sin(o.Smoothness*UNITY_PI))*_FakeHighlightIntensity;
		//fh=pow(fh,o.Smoothness*10);
	
		setFakeHighlights(fh);

	#endif

	setHighlightRampParameters(_HighlightRamp, _HighlightRampOffset,_HighlightRampColor, _HighlightIntensity);
		
}
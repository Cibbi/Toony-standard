//Base functions
//
inline float remap(float value, float oldMin, float oldMax, float newMin, float newMax) {
    return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
}
inline float2 remap(float2 value, float2 oldMin, float2 oldMax, float2 newMin, float2 newMax) {
    return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
}
inline float3 remap(float3 value, float3 oldMin, float3 oldMax, float3 newMin, float3 newMax) {
    return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
}
inline float4 remap(float4 value, float4 oldMin, float4 oldMax, float4 newMin, float4 newMax) {
    return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
}

inline half Pow5 (half x)
{
    return x*x * x*x * x;
}

inline half3 FresnelTerm (half3 F0, half cosA)
{
    half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
    return F0 + (1-F0) * t;
}

inline half3 FresnelLerp (half3 F0, half3 F90, half cosA)
{
    half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
    return lerp (F0, F90, t);
}

float ClampRoughness(float roughness)
{
    #if defined(_ANISOTROPIC_SPECULAR)
        return roughness;
    #else
        return max(roughness, 0.002);
    #endif
}

float sqr(float x) { return x*x; }
//
//end base functions

//Probe sampling functions
//
struct Unity_GlossyEnvironmentData
{
    // - Deferred case have one cubemap
    // - Forward case can have two blended cubemap (unusual should be deprecated).

    // Surface properties use for cubemap integration
    half    roughness; // CAUTION: This is perceptualRoughness but because of compatibility this name can't be change :(
    half3   reflUVW;
};

half perceptualRoughnessToMipmapLevel(half perceptualRoughness)
{
    return perceptualRoughness * UNITY_SPECCUBE_LOD_STEPS;
}

half4 Unity_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn)
{
    half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;

    // TODO: CAUTION: remap from Morten may work only with offline convolution, see impact with runtime convolution!
    // For now disabled
    #if 0
        float m = PerceptualRoughnessToRoughness(perceptualRoughness); // m is the real roughness parameter
        const float fEps = 1.192092896e-07F;        // smallest such that 1.0+FLT_EPSILON != 1.0  (+1e-4h is NOT good here. is visibly very wrong)
        float n =  (2.0/max(fEps, m*m))-2.0;        // remap to spec power. See eq. 21 in --> https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf

        n /= 4;                                     // remap from n_dot_h formulatino to n_dot_r. See section "Pre-convolved Cube Maps vs Path Tracers" --> https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html

        perceptualRoughness = pow( 2/(n+2), 0.25);      // remap back to square root of real roughness (0.25 include both the sqrt root of the conversion and sqrt for going from roughness to perceptualRoughness)
    #else
        // MM: came up with a surprisingly close approximation to what the #if 0'ed out code above does.
        perceptualRoughness = perceptualRoughness*(1.7 - 0.7*perceptualRoughness);
    #endif


    half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
    half3 R = glossIn.reflUVW;
    half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, R, mip);

    return float4(DecodeHDR(rgbm, hdr),rgbm.a);
}
//
//end probe sampling functions

//unity's base diffuse based on disney implementation
float DisneyDiffuse(half NdotV, half NdotL, half LdotH, half perceptualRoughness)
{
    float fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
    // Two schlick fresnel term
    float lightScatter   = (1 + (fd90 - 1) * Pow5(1 - NdotL));
    float viewScatter    = (1 + (fd90 - 1) * Pow5(1 - NdotV));

    return lightScatter * viewScatter;
}

//calculation for normal maps based on xiexe's one
void CalculateNormals(inout float3 normal, inout float3 tangent, inout float3 bitangent, float3 normalmap)
{
    float3 tspace0 = float3(tangent.x, bitangent.x, normal.x);
    float3 tspace1 = float3(tangent.y, bitangent.y, normal.y);
    float3 tspace2 = float3(tangent.z, bitangent.z, normal.z);

    float3 calcedNormal;
    calcedNormal.x = dot(tspace0, normalmap);
    calcedNormal.y = dot(tspace1, normalmap);
    calcedNormal.z = dot(tspace2, normalmap);
    
    calcedNormal = normalize(calcedNormal);
    float3 bumpedTangent = (cross(bitangent, calcedNormal));
    float3 bumpedBitangent = (cross(calcedNormal, bumpedTangent));

    normal = calcedNormal;
    tangent = bumpedTangent;
    bitangent = bumpedBitangent;
}

float FadeShadows (FragmentData i, float attenuation) 
{
    #if HANDLE_SHADOWS_BLENDING_IN_GI && !defined (SHADOWS_SHADOWMASK)
        // UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
        float viewZ =dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance =UnityComputeShadowFadeDistance(i.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        attenuation = saturate(attenuation + shadowFade);
    #endif
    #if defined(LIGHTMAP_ON) && defined (SHADOWS_SHADOWMASK)
        // UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
        float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        float bakedAttenuation = UnitySampleBakedOcclusion(i.lightmapUV, i.worldPos);
        attenuation = UnityMixRealtimeAndBakedShadows(attenuation, bakedAttenuation, shadowFade);
        //attenuation = saturate(attenuation + shadowFade);
        //attenuation = bakedAttenuation;

    #endif
    return attenuation;
}

float3 GetModifiedTangent(float3 tangentTS, float3 tangentDir)
{
    float3x3 worldToTangent;
    worldToTangent[0] = float3(1, 0, 0);
    worldToTangent[1] = float3(0, 1, 0);
    worldToTangent[2] = float3(0, 0, 1); 

    float3 tangentTWS = mul(tangentTS, worldToTangent);
    float3 fTangent;
    if (tangentTS.z < 1)
    tangentDir = tangentTS;
    else
    tangentDir = tangentDir;
    
    return tangentDir;
}

float GTR2(float NdotH, float a)
{
    float a2 = a*a;
    float t = 1 + (a2-1)*NdotH*NdotH;
    return a2 / (UNITY_PI * t*t + 1e-7f);
}

float GTR2_aniso(float NdotH, float HdotX, float HdotY, float ax, float ay)
{
    return 1 / (UNITY_PI * ax*ay * sqr( sqr(HdotX/ax) + sqr(HdotY/ay) + NdotH*NdotH ));
}

float smithG_GGX(float NdotV, float alphaG)
{
    float a = alphaG*alphaG;
    float b = NdotV*NdotV;
    return 1 / (NdotV + sqrt(a + b - a*b));
}

float smithG_GGX_aniso(float NdotV, float VdotX, float VdotY, float ax, float ay)
{
    return 1 / (NdotV + sqrt( sqr(VdotX*ax) + sqr(VdotY*ay) + sqr(NdotV) ));
}

half3 Unity_SafeNormalize(half3 inVec)
{
    half dp3 = max(0.001f, dot(inVec, inVec));
    return inVec * rsqrt(dp3);
}

//modified version of Shade4PointLights
float3 Shade4PointLights(float3 normal, float3 worldPos)
{
    float4 toLightX = unity_4LightPosX0 - worldPos.x;
    float4 toLightY = unity_4LightPosY0 - worldPos.y;
    float4 toLightZ = unity_4LightPosZ0 - worldPos.z;
    float4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;
    float4 NdotL = 0;
    NdotL += toLightX * normal.x;
    NdotL += toLightY * normal.y;
    NdotL += toLightZ * normal.z;
    // correct NdotL
    float4 corr = rsqrt(lengthSq);
    NdotL =  NdotL * corr;
    //attenuation
    float4 atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);

    float4 diff = max(NdotL,0) * atten;
    // final color
    float3 col = 0;
    col += unity_LightColor[0] * diff.x;
    col += unity_LightColor[1] * diff.y;
    col += unity_LightColor[2] * diff.z;
    col += unity_LightColor[3] * diff.w;
    return col;
}

float3 RampDotLVertLight(float3 normal, float3 worldPos, RampData rampData, float3 rampMin, float3 rampMax, float occlusion, float occlusionOffsetIntensity)
{
    //from Shade4PointLights function to get NdotL + attenuation
    float4 toLightX = unity_4LightPosX0 - worldPos.x;
    float4 toLightY = unity_4LightPosY0 - worldPos.y;
    float4 toLightZ = unity_4LightPosZ0 - worldPos.z;
    float4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;
    float4 NdotL = 0;
    NdotL += toLightX * normal.x;
    NdotL += toLightY * normal.y;
    NdotL += toLightZ * normal.z;
    // correct NdotL
    float4 corr = rsqrt(lengthSq);
    NdotL =  NdotL * corr;
    //attenuation
    float4 atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
    //ramp calculation for all 4 vertex lights
    float offset = rampData.offset+(occlusion*occlusionOffsetIntensity)-occlusionOffsetIntensity;
    //Calculating ramp uvs based on offset
    float newMin = max(offset, 0);
    float newMax = max(offset + 1, 0);
    float4 rampUv = remap(min(NdotL,remap(atten,0,1,-1,1)), float4(-1,-1,-1,-1), float4(1,1,1,1), float4(newMin,newMin,newMin,newMin), float4(newMax,newMax,newMax,newMax));
    float3 ramp = remap(remap(tex2D(rampData.ramp, float2(rampUv.x, rampUv.x)).rgb * rampData.color.rgb,float3(0, 0, 0), float3(1, 1, 1),1-rampData.color.aaa, float3(1, 1, 1)),rampMin,rampMax,0,1).rgb * unity_LightColor[0].rgb;
    ramp +=       remap(remap(tex2D(rampData.ramp, float2(rampUv.y, rampUv.y)).rgb * rampData.color.rgb,float3(0, 0, 0), float3(1, 1, 1),1-rampData.color.aaa, float3(1, 1, 1)),rampMin,rampMax,0,1).rgb * unity_LightColor[1].rgb;
    ramp +=       remap(remap(tex2D(rampData.ramp, float2(rampUv.z, rampUv.z)).rgb * rampData.color.rgb,float3(0, 0, 0), float3(1, 1, 1),1-rampData.color.aaa, float3(1, 1, 1)),rampMin,rampMax,0,1).rgb * unity_LightColor[2].rgb;
    ramp +=       remap(remap(tex2D(rampData.ramp, float2(rampUv.w, rampUv.w)).rgb * rampData.color.rgb,float3(0, 0, 0), float3(1, 1, 1),1-rampData.color.aaa, float3(1, 1, 1)),rampMin,rampMax,0,1).rgb * unity_LightColor[3].rgb;
    
    return ramp;
}
//TODO: remove the fucking lightColor parameter or fucking use it
float4 RampDotL(float NdotL, RampData rampData, float rampMin, float rampMax, float occlusion, float occlusionOffsetIntensity)
{
    //Adding the occlusion into the offset of the ramp
    float offset=rampData.offset+(occlusion*occlusionOffsetIntensity)-occlusionOffsetIntensity;
    //Calculating ramp uvs based on offset
    float newMin = max(offset, 0);
    float newMax = max(offset + 1, 0);
    float rampUv = remap(NdotL, -1, 1, newMin, newMax);
    float3 ramp = tex2D(rampData.ramp, float2(rampUv, rampUv)).rgb;
    //Adding the color and remapping it based on the shadow intensity stored into the alpha channel of the ramp color
    ramp *= rampData.color.rgb;
    ramp = remap(ramp, float3(0, 0, 0), float3(1, 1, 1),1-rampData.color.aaa, float3(1, 1, 1));

    //getting the modified ramp for highlights and all lights that are not directional
    float3 rampA = remap(ramp, rampMin, rampMax,0,1);
    float rampGrey = max(max(rampA.r, rampA.g), rampA.b);
    #if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE) 
        return float4(ramp,rampGrey); 
    #else
        return float4(rampA,rampGrey);
    #endif
    
}

float3 RampDotLSimple(float NdotL, RampData rampData, float occlusion, float occlusionOffsetIntensity)
{
    //Adding the occlusion into the offset of the ramp
    float offset = rampData.offset+(occlusion*occlusionOffsetIntensity)-occlusionOffsetIntensity;
    //Calculating ramp uvs based on offset
    float newMin = max(offset, 0);
    float newMax = max(offset + 1, 0);
    float rampUv = remap(NdotL, -1, 1, newMin, newMax);
    float3 ramp = tex2D(rampData.ramp, float2(rampUv, rampUv)).rgb;
    //Adding the color and remapping it based on the shadow intensity stored into the alpha channel of the ramp color
    ramp *= rampData.color.rgb;
    ramp = remap(ramp, float3(0, 0, 0), float3(1, 1, 1),1-rampData.color.aaa, float3(1, 1, 1));
    return ramp; 
}

float3 StylizedLightmap(float3 lightmap, RampData rampData, float occlusion, float occlusionOffsetIntensity)
{
    //Adding the occlusion into the offset of the ramp
    float offset = rampData.offset+(occlusion*occlusionOffsetIntensity)-occlusionOffsetIntensity;
    //Calculating ramp uvs based on offset
    float newMin = max(offset, 0);
    float newMax = max(offset + 1, 0);
    float rampUv = remap((lightmap.r + lightmap.g + lightmap.b)/3, 0, 1, newMin, newMax);
    float3 ramp = tex2D(rampData.ramp, float2(rampUv, rampUv)).rgb;
    ramp = remap(ramp, float3(0, 0, 0), float3(1, 1, 1),1-rampData.color.aaa, float3(1, 1, 1))*lightmap;

    return ramp;
}

//edited DecodeDirectionalLightmap from UnityCG
inline half3 DecodeDirectionalToonLightmap (
half3 color, fixed4 dirTex, half3 normalWorld, RampData rampData, float occlusion, float occlusionOffsetIntensity) 
{

    half halfLambert = dot(normalWorld, dirTex.xyz);
    return color * RampDotLSimple(halfLambert, rampData, occlusion, occlusionOffsetIntensity) / max(1e-4h, dirTex.w);
    //return color * halfLambert / max(1e-4h, dirTex.w);
}

float3 PbrToToonHighlights(float toonyHighlights, RampData highlightRamp, float D, float metallic)
{
    UNITY_BRANCH
    if(toonyHighlights>0)
    {
        //Calculating highlight ramp uvs based on offset
        half newMin = max(highlightRamp.offset, 0);
        half newMax = max(highlightRamp.offset + 1, 0);
        half Duv=remap(clamp(D,0,2), 0, 2, newMin, newMax);	
        //have to recheck the metallic thing in here in case of a specular workflow where there's no metallic value
        return (tex2D(highlightRamp.ramp, float2(Duv, Duv)).rgb*highlightRamp.color.rgb*10*(1-metallic+(0.2*metallic)));
    }
    else
    {
        return D;
    }
}

// Standard specular calculation
float3 DirectSpecular(BaseDots dots, float toonyHighlights, RampData highlightRamp, float metallic, float highlightPattern, float4 ramp, float3 specColor, float roughness)
{
    float3 D = GTR2(dots.NdotH, roughness);
    float V = smithG_GGX(max(dots.NdotL,lerp(0.3,0,roughness)), roughness) * smithG_GGX(dots.NdotV, roughness);

    D = PbrToToonHighlights(toonyHighlights, highlightRamp, D, metallic);
    //masking with the highlight pattern
    D *= highlightPattern;
    float3 specularTerm = V * D * UNITY_PI;
    #ifdef UNITY_COLORSPACE_GAMMA
        specularTerm = sqrt(max(1e-4h, specularTerm));
    #endif   

    specularTerm = max(0, specularTerm * ramp.a);
    specularTerm *= any(specColor) ? 1.0 : 0.0;
    specularTerm *= FresnelTerm(specColor, dots.LdotH);
    return specularTerm;

}

// Fake specular calculation
float3 DirectFakeSpecular(float3 fakeHighlights,float LdotH, float toonyHighlights, RampData highlightRamp, float metallic, float highlightPattern, float4 ramp, float3 specColor, float roughness)
{
    half V = 1;
    float3 D = fakeHighlights*50*(1-metallic+(0.1*metallic));

    D = PbrToToonHighlights(toonyHighlights, highlightRamp, D, metallic);
    //masking with the highlight pattern
    D *= highlightPattern;
    float3 specularTerm = V * D;
    #ifdef UNITY_COLORSPACE_GAMMA
        specularTerm = sqrt(max(1e-4h, specularTerm));
    #endif    
    specularTerm = max(0, specularTerm * ramp.a);
    specularTerm *= any(specColor) ? 1.0 : 0.0;
    specularTerm *= FresnelTerm(specColor, LdotH);
    return specularTerm;
}

// Anisotropic specular calculation
float3 DirectAnisotropicSpecular(DirectionData dir, BaseDots dots, float anisotropy, float toonyHighlights, RampData highlightRamp, float metallic, float highlightPattern, float4 ramp, float3 specColor, float roughness)
{
    #if defined(_ANISOTROPIC_SPECULAR) && !defined (_SPECULARHIGHLIGHTS_OFF)
        //Anisotropic specific dot products
        float TdotH = dot(dir.tangentMap, dir.halfD);
        float TdotL = dot(dir.tangentMap, dir.light);
        float BdotH = dot(dir.bitangent, dir.halfD);
        float BdotL = dot(dir.bitangent, dir.light);
        float TdotV = dot(dir.view, dir.tangentMap);
        float BdotV = dot(dir.view, dir.bitangent);

        //float aspect = sqrt(1-anisotropy*.9);
        //float ax = max(.005, roughness / aspect);
        //float ay = max(.005, roughness * aspect);
        float ax = max(roughness * (1.0 + anisotropy), 0.005);
        float ay = max(roughness * (1.0 - anisotropy), 0.005);


        float3 D = GTR2_aniso(dots.NdotH, TdotH, BdotH, ax, ay);
        
        float V  = smithG_GGX_aniso(dots.NdotL, TdotL, BdotL, ax, ay);
        V *= smithG_GGX_aniso(dots.NdotV, TdotV, BdotV, ax, ay);

        D = PbrToToonHighlights(toonyHighlights, highlightRamp, D, metallic);
        //masking with the highlight pattern
        D *= highlightPattern;

        float3 specularTerm = D * V* UNITY_PI;
        #ifdef UNITY_COLORSPACE_GAMMA
            specularTerm = sqrt(max(1e-4h, specularTerm));
        #endif    
        specularTerm = max(0, specularTerm * ramp.a * dots.NdotL);
        specularTerm *= any(specColor) ? 1.0 : 0.0;
        specularTerm *= FresnelTerm(specColor, dots.LdotH);
        return specularTerm;
    #else
        return 0;
    #endif
}

//Subsurface Scattering - Based on a 2011 GDC Conference from by Colin Barre-Bresebois & Marc Bouchard and modified by Xiexe
float3 calcSubsurfaceScattering(SSSData sss, BaseDots dots, DirectionData dir, float atten, float3 normal, float4 lightCol, float3 indirectDiffuse, float3 albedo)
{
    UNITY_BRANCH
    if(any(sss.color.rgb)) // Skip all the SSS stuff if the color is 0.
    {
        float attenuation = saturate(atten * (dots.NdotL * 0.5 + 0.5));
        float3 H = normalize(dir.light + normal * sss.distortion);
        float VdotH = pow(saturate(dot(dir.view, -H)), sss.power);
        float3 I = sss.color * (VdotH + indirectDiffuse) * attenuation * sss.thickness * sss.scale;
        float3 SSS = lightCol.rgb * I * albedo.rgb;
        SSS = max(0, SSS); // Make sure it doesn't go NaN
        return SSS;
    }
    else
    {
        return 0;
    }
}
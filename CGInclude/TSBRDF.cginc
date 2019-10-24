float4 TS_BRDF(BRDFData i)
{
    float4 lightCol = float4(_LightColor0.rgb, i.attenuation);
    float3 indirectDiffuse=0;
    #if defined(UNITY_PASS_FORWARDBASE)
        float3 probeLightDir = 0;
        
        #if defined(LIGHTMAP_ON)
            indirectDiffuse = i.lightmap;
            //indirectDiffuse = StylizedLightmap(i.lightmap, i.mainRamp, i.occlusion, i.occlusionOffsetIntensity);
            #if defined(DIRLIGHTMAP_COMBINED)
                //indirectDiffuse = DecodeDirectionalToonLightmap(indirectDiffuse, i.lightmapDirection, i.normal, i.mainRamp, i.occlusion, i.occlusionOffsetIntensity);
                indirectDiffuse = DecodeDirectionalLightmap(indirectDiffuse, i.lightmapDirection, i.normal);
                //probeLightDir = i.lightmapDirection;
            #endif
        #endif

        #if defined(DYNAMICLIGHTMAP_ON)
            
			#if defined(DIRLIGHTMAP_COMBINED)
                //indirectDiffuse += DecodeDirectionalToonLightmap(i.dynamicLightmap, i.dynamicLightmapDirection, i.normal, i.mainRamp, i.occlusion, i.occlusionOffsetIntensity);
                indirectDiffuse += DecodeDirectionalLightmap(i.dynamicLightmap, i.dynamicLightmapDirection, i.normal);
			#else
                //indirectDiffuse += i.dynamicLightmap;
                indirectDiffuse += StylizedLightmap(i.dynamicLightmap, i.mainRamp, i.occlusion, i.occlusionOffsetIntensity);
			#endif
		#endif

        #if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
            //if there's no direct light, we get the probe light direction to use as direct light direction and
            //we consider the indirect light color as it was the direct light color.
            indirectDiffuse = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
            if(any(_WorldSpaceLightPos0.xyz)==0)
            {
                probeLightDir = unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz;
                lightCol.rgb = indirectDiffuse; 
                indirectDiffuse=0;
            }     
            i.dir.light = normalize(UnityWorldSpaceLightDir(i.worldPos) + probeLightDir);
        #endif
        

    #endif

    i.dir.halfD = Unity_SafeNormalize(i.dir.light + i.dir.view);
    //basic dot products
    BaseDots dots;
    dots.NdotL = dot(i.normal, i.dir.light);
    dots.NdotV = abs(dot(i.normal, i.dir.view));
    dots.NdotH = max(dot(i.normal, i.dir.halfD),0);
    dots.LdotH = max(dot(i.dir.light, i.dir.halfD),0);
    
    dots.NdotL = min(dots.NdotL,lightCol.a);
    
    //toon version of the NdotL for the direct light
	float4 ramp = RampDotL(dots.NdotL, lightCol, i.mainRamp, i.mainRampMin, i.mainRampMax, i.occlusion, i.occlusionOffsetIntensity);
    

    //The max operation is done after cause we needed the -1 to 0 values for correctly sampling the ramp
    dots.NdotL=max(dots.NdotL,0);

    //setup the albedo based on workflow and premultiply alpha
    float oneMinusReflectivity;
    float3 specColor;
    #if defined (_ENABLE_SPECULAR)
        #if defined(_SPECULAR_WORKFLOW)
            i.albedo = EnergyConservationBetweenDiffuseAndSpecular(i.albedo, i.specular, /*out*/ oneMinusReflectivity);
            specColor = i.specular;
        #else
            i.albedo = DiffuseAndSpecularFromMetallic (i.albedo, i.metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);
        #endif
        float outputAlpha;
        i.albedo = PreMultiplyAlpha(i.albedo, i.alpha, oneMinusReflectivity, /*out*/ outputAlpha);
        //by not using the output alpha as the final alpha you can get "transparent metals", looks fun so i'll keep it like this for now
        //i.alpha = outputAlpha;
    #else
        #if defined(_ALPHAPREMULTIPLY_ON)
            i.albedo *= i.alpha;
        #endif
    #endif
    //diffuse color
    float3 DiffuseColor = i.albedo * ramp * (lightCol.rgb + indirectDiffuse);
    float3 vertexDiffuse=0;
    #if defined(VERTEXLIGHT_ON)
        vertexDiffuse = RampDotLVertLight(i.normal, i.worldPos, i.mainRamp, i.mainRampMin, i.mainRampMax, i.occlusion, i.occlusionOffsetIntensity);
        vertexDiffuse*=i.albedo;
    #endif

    float3 specularTerm=0;
    float3 indirectSpecular=0;
    #if defined (_ENABLE_SPECULAR)
    //the original roughness value is saved cause it is needed on the indirect specular for sampling the specular probe
    float3 baseRoughness=i.roughness;
    //Direct specular calculation
    //
        i.roughness *= i.roughness;
        i.roughness = ClampRoughness(i.roughness);
        #if defined(_ANISOTROPIC_SPECULAR)
            specularTerm = DirectAnisotropicSpecular(i.dir, dots, i.anisotropy, i.toonyHighlights, i.highlightRamp, i.metallic, i.highlightPattern, ramp, specColor, i.roughness);  
        #else
            #if defined(_FAKE_SPECULAR)
                specularTerm = DirectFakeSpecular(i.fakeHighlights, dots.LdotH, i.toonyHighlights, i.highlightRamp, i.metallic, i.highlightPattern, ramp, specColor,  i.roughness);
            #else
                specularTerm = DirectSpecular(dots, i.toonyHighlights, i.highlightRamp, i.metallic, i.highlightPattern, ramp, specColor,  i.roughness);
            #endif
        #endif
    //
    //End direct specular calculation

    //Indirect specular calculation
    //
        //indirect specular is added only on the base pass
        #if defined(UNITY_PASS_FORWARDBASE)
        UNITY_BRANCH
        if (i.indirectSpecular>0)
        {
            //using the fake specular probe toned down based on the average light, it's not phisically accurate
            //but having a probe that reflects arbitrary stuff isn't accurate to begin with
            half lightColGrey = max((lightCol.r + lightCol.g + lightCol.b) / 3, (indirectDiffuse.r + indirectDiffuse.g + indirectDiffuse.b) / 3);
            indirectSpecular=i.customIndirect*min(lightColGrey,1);
        }
        else
        {
            //Sampling the probe
            Unity_GlossyEnvironmentData envData;
		    envData.roughness = baseRoughness;
		    envData.reflUVW = BoxProjectedCubemapDirection(i.dir.reflect, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
		    indirectSpecular = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
        }
        float grazingTerm = saturate(1-i.roughness + (1 - oneMinusReflectivity));
        indirectSpecular*=FresnelLerp(specColor, grazingTerm, dots.NdotV);
        #endif
    //
    //End indirect specular calculation
    
    #endif
    //Final color calculation = Diffuse color (that contains also rhe indirect component) 
    //                        + the vertex lights contribution 
    //                        + direct specular + indirect specular
    float4 finalColor = float4(DiffuseColor + vertexDiffuse +  specularTerm * lightCol.rgb * lightCol.a + indirectSpecular*i.occlusion,1);
    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
        finalColor.a = i.alpha;
    #endif
    return finalColor;

}
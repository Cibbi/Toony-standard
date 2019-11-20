using System;
using UnityEditor;
using UnityEngine;

namespace Cibbi.ToonyStandard
{
    public class BasicMainSection 
    {
        private static class Styles
        {
            public static GUIContent cullMode = new GUIContent("Cull mode", "Controls which face of the mesh is rendered \n\nOff: Double sided \n\nFront: Single sided (internal parts showing) \n\nBack: Single sided");
            public static GUIContent blendMode = new GUIContent("Blend mode", "Blend state \n\nOpaque: Opaque object \n\nCutout: Opaque object with cutout parts decided by the alpha channel of the main texture" +
                                                                " \n\nFade: Transparent object that does completely fade out at 0 opacity \n\nTransparent: Transparent object that is still visible at 0 opacity due to the fresnel effect, more realistic than fade");
            public static GUIContent mainTex = new GUIContent("Main texture", "Main texture (RGB channels) and transparency (A channel)");
            public static GUIContent cutOff = new GUIContent("Alpha cutoff", "Transparency threshold to cut out");
            public static GUIContent normal = new GUIContent("Normal", "Normal Map");
            public static GUIContent emission = new GUIContent("Color", "Emission map and Color");
            public static GUIContent ramp = new GUIContent("Toon ramp", "Toon ramp texture");
            public static GUIContent rampOffset = new GUIContent("Ramp offset", "Applies an offset that shifts the ramp texture, usefull to avoid to make different toon ramps that are really similar");
            public static GUIContent shadowIntensity = new GUIContent("Shadow intensity", "Defines how intense the toon ramp is");
        }

         MaterialProperty _blendMode;
        MaterialProperty _Cull;

        MaterialProperty _MainTex;
        MaterialProperty _Color;
        MaterialProperty _Cutoff;
        MaterialProperty _BumpMap;
        MaterialProperty _BumpScale;
        MaterialProperty _Emission;
        MaterialProperty _EmissionColor;
        MaterialProperty _Ramp;
        MaterialProperty _RampColor;
        MaterialProperty _RampOffset;
        MaterialProperty _ShadowIntensity;

        public BasicMainSection(MaterialProperty[] properties)
        {
            FindProperties(properties);
            InitializeInspectorLevel(properties);
        }

        private void InitializeInspectorLevel(MaterialProperty[] properties)
        {
            FindProperty("_Occlusion", properties).floatValue=0;
            FindProperty("_FakeLight", properties).floatValue=0;
            FindProperty("_RimLightOn", properties).floatValue=0;
            FindProperty("_RampOn", properties).floatValue=1;

            FindProperty("_MetallicMap", properties).textureValue=null;  
            FindProperty("_GlossinessMap", properties).textureValue=null;    
            FindProperty("_GlossinessMap", properties).textureValue=null;  
            FindProperty("_HighlightRamp", properties).textureValue=TSConstants.DefaultRamp; 
            FindProperty("_HighlightRampOffset", properties).floatValue=0;  
            FindProperty("_DetailMapOn", properties).floatValue=0;
            // _workflow.floatValue=(float)Workflow.Metallic;
            //_SpMode.floatValue=(float)SpMode.Standard;
            //_indirectSpecular.floatValue=(float)IndirectSpecular.Probe;
        }

        public void FindProperties(MaterialProperty[] properties)
        {
            _blendMode = FindProperty("_Mode", properties);
            _Cull = FindProperty("_Cull", properties);

            _MainTex = FindProperty("_MainTex", properties);
            _Color = FindProperty("_Color", properties);
            _Cutoff = FindProperty("_Cutoff", properties);
            _BumpMap = FindProperty("_BumpMap", properties);
            _BumpScale = FindProperty("_BumpScale", properties);
            _Emission = FindProperty("_EmissionMap", properties);
            _EmissionColor = FindProperty("_EmissionColor", properties);
            _Ramp = FindProperty("_Ramp", properties);
            _RampColor = FindProperty("_RampColor", properties);
            _RampOffset = FindProperty("_RampOffset", properties);
            _ShadowIntensity = FindProperty("_ShadowIntensity", properties);
        }

        public void DrawSection(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
                TSFunctions.DrawSelector(Enum.GetNames(typeof(ToonyStandardGUI.BlendMode)), _blendMode, Styles.blendMode, materialEditor);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material mat in _blendMode.targets)
                {
                    ToonyStandardGUI.SetupMaterialWithBlendMode(mat, (ToonyStandardGUI.BlendMode)_blendMode.floatValue, mat.GetFloat("_OutlineOn")>0);
                }
            }

            // Draw cull mode
            materialEditor.ShaderProperty(_Cull, Styles.cullMode);
            EditorGUILayout.Space();

            // Draw main properties
            materialEditor.TexturePropertySingleLine(Styles.mainTex, _MainTex, _Color);
            if ((ToonyStandardGUI.BlendMode)_blendMode.floatValue == ToonyStandardGUI.BlendMode.Cutout)
            {
                EditorGUI.indentLevel += MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
                materialEditor.ShaderProperty(_Cutoff, Styles.cutOff);
                EditorGUI.indentLevel -= MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
            }
            materialEditor.TexturePropertySingleLine(Styles.normal, _BumpMap, _BumpScale);

            materialEditor.TexturePropertySingleLine(Styles.ramp, _Ramp, _RampColor);
            materialEditor.ShaderProperty(_RampOffset, Styles.rampOffset);
            materialEditor.ShaderProperty(_ShadowIntensity, Styles.shadowIntensity);

            // Emission
            EditorGUI.BeginChangeCheck();
                if (materialEditor.EmissionEnabledProperty())
                {
                    materialEditor.TexturePropertySingleLine(Styles.emission, _Emission, _EmissionColor);
                    materialEditor.LightmapEmissionProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel);
                }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material mat in _Emission.targets)
                {
                    MaterialEditor.FixupEmissiveFlag(mat);
                    bool shouldEmissionBeEnabled = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
                    TSFunctions.SetKeyword(mat, "_EMISSION", shouldEmissionBeEnabled);
                    if (shouldEmissionBeEnabled)
                    {
                        mat.SetOverrideTag("IsEmissive", "true");
                    }
                    else
                    {
                        mat.SetOverrideTag("IsEmissive", "false");
                    }
                }
            }
            
            materialEditor.TextureScaleOffsetProperty(_MainTex);

            EditorGUILayout.Space();
        }

        private static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties)
        {
            return FindProperty(propertyName, properties, true);
        }

        private static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory)
        {
            for (var i = 0; i < properties.Length; i++)
                if (properties[i] != null && properties[i].name == propertyName)
                    return properties[i];

            // We assume all required properties can be found, otherwise something is broken
            if (propertyIsMandatory)
                throw new ArgumentException("Could not find MaterialProperty: '" + propertyName + "', Num properties: " + properties.Length);
            return null;
        }

    }
}
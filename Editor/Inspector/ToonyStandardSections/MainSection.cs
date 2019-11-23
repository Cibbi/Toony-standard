using System;
using UnityEditor;
using UnityEngine;

namespace Cibbi.ToonyStandard
{
    public class MainSection 
    {

        private static class Styles
        {
            public static GUIContent cullMode = new GUIContent("Cull mode", "Controls which face of the mesh is rendered \n\nOff: Double sided \n\nFront: Single sided (internal parts showing) \n\nBack: Single sided");
            public static GUIContent blendMode = new GUIContent("Blend mode", "Blend state \n\nOpaque: Opaque object \n\nCutout: Opaque object with cutout parts decided by the alpha channel of the main texture" +
                                                                " \n\nFade: Transparent object that does completely fade out at 0 opacity \n\nTransparent: Transparent object that is still visible at 0 opacity due"+
                                                                " to the fresnel effect, more realistic than fade \n\nDither: uses a dithering pattern to simulate transparent objects");
            public static GUIContent mainTex = new GUIContent("Main texture", "Main texture (RGB channels) and transparency (A channel)");
            public static GUIContent cutOff = new GUIContent("Alpha cutoff", "Transparency threshold to cut out");
            public static GUIContent normal = new GUIContent("Normal", "Normal Map");
            public static GUIContent emission = new GUIContent("Color", "Emission map and Color");
            public static GUIContent occlusion = new GUIContent("Occlusion", "Occlusion map and intensity");
            public static GUIContent MSOT = new GUIContent("MSOT Map", "Multiple maps in a single texture \n\nR: metallic\nG: smoothness\nB: Occlusion\nA: SSS Thickness map");
            public static GUIContent TexturePackerButton = new GUIContent("Open texture packer", "Open the texture packer for generating the MSOT texture");
            public static void ToggleTexturePackerContent(bool isOpen)
            {
                if(isOpen)
                {
                    Styles.TexturePackerButton.text = "Close texture packer";
                    Styles.TexturePackerButton.tooltip = "Close the texture packer for generating the MSOT texture";
                }
                else
                {
                    Styles.TexturePackerButton.text = "Open texture packer";
                    Styles.TexturePackerButton.tooltip = "Open the texture packer for generating the MSOT texture";
                }
            }
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
        MaterialProperty _OcclusionMap;
        MaterialProperty _Occlusion;
        MaterialProperty _MSOT;

        InspectorLevel level;
        bool isTexturePackerOpen;
        TexturePacker packer;
        ToonyStandardGUI gui;

        public MainSection(MaterialProperty[] properties,InspectorLevel level, TexturePacker packer, ToonyStandardGUI gui)
        {
            FindProperties(properties);
            this.level=level;
            isTexturePackerOpen = false;
            Styles.ToggleTexturePackerContent(isTexturePackerOpen);
            this.packer = packer;
            this.gui=gui;
        }

        private void InitializeInspectorLevel()
        {

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
            _OcclusionMap = FindProperty("_OcclusionMap", properties);
            _Occlusion = FindProperty("_Occlusion", properties);
            _MSOT = FindProperty("_MSOT", properties);
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

            //draw cull mode
            materialEditor.ShaderProperty(_Cull, Styles.cullMode);
            EditorGUILayout.Space();

            //draw main properties
            materialEditor.TexturePropertySingleLine(Styles.mainTex, _MainTex, _Color);
            if ((ToonyStandardGUI.BlendMode)_blendMode.floatValue == ToonyStandardGUI.BlendMode.Cutout)
            {
                EditorGUI.indentLevel += MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
                materialEditor.ShaderProperty(_Cutoff, Styles.cutOff);
                EditorGUI.indentLevel -= MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
            }
            materialEditor.TexturePropertySingleLine(Styles.normal, _BumpMap, _BumpScale);
            
            if(level==InspectorLevel.Normal)
            {   
                Rect r = TSFunctions.GetControlRectForSingleLine(); 
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertyMiniThumbnail(r,_OcclusionMap, Styles.occlusion.text,Styles.occlusion.tooltip);
                if(EditorGUI.EndChangeCheck())
                {
                    gui.RegenerateMSOT();
                }
                TSFunctions.ProperSlider(MaterialEditor.GetRectAfterLabelWidth(r), ref _Occlusion);

            }
            else
            {
                materialEditor.ShaderProperty(_Occlusion, Styles.occlusion);
            }

            //emission
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
            //if in expert mode show the MSOT map and a button for the texture packer
            if(level==InspectorLevel.Expert)
            {   
                EditorGUILayout.BeginHorizontal();
                    materialEditor.TexturePropertySingleLine(Styles.MSOT, _MSOT);
                if(GUILayout.Button(Styles.TexturePackerButton))
                {
                    EditorGUILayout.EndHorizontal();
                    isTexturePackerOpen=!isTexturePackerOpen;
                    Styles.ToggleTexturePackerContent(isTexturePackerOpen);
                }
                else
                {
                    EditorGUILayout.EndHorizontal();
                }

                if(isTexturePackerOpen)
                {
                    EditorGUILayout.BeginVertical("box");
                    packer.DrawGUI();
                    EditorGUILayout.EndVertical();
                    if(_MSOT.textureValue != (Texture)packer.resultTex && packer.resultTex != null)
                    {   
                        _MSOT.textureValue = packer.resultTex;
                        packer.resultTex = null;
                    }
                }
            }
            EditorGUILayout.Space();
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
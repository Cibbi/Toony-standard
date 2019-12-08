using UnityEditor;
using UnityEngine;

namespace Cibbi.ToonyStandard
{
    public class RampSection : OrderedSection
    {
        private static class Styles
        {
            public static GUIContent title = new GUIContent("Toon Ramp Options", "Various options for the toon ramp");

            public static GUIContent ramp = new GUIContent("Toon ramp", "Toon ramp texture");
            public static GUIContent rampOffset = new GUIContent("Ramp offset", "Applies an offset that shifts the ramp texture, usefull to avoid to make different toon ramps that are really similar");
            public static GUIContent occlusionOffset = new GUIContent("Occlusion ramp offset", "Uses the occlusion texture to apply an additional offset to the toon ramp on specific zones");
            public static GUIContent occlusionOffsetIntensity = new GUIContent("Occlusion offset intensity", "intensity of the occlusion driven ramp offset");
            public static GUIContent shadowIntensity = new GUIContent("Shadow intensity", "Defines how intense the toon ramp is");
            public static GUIContent GradientEditorButton = new GUIContent("Open gradient editor", "Open the gradient editor for creating a custom toon ramp");
            public static void ToggleGradientEditorToggle(bool isOpen)
            {
                if (isOpen)
                {
                    Styles.GradientEditorButton.text = "Close gradient editor";
                    Styles.GradientEditorButton.tooltip = "Close the gradient editor";
                }
                else
                {
                    Styles.GradientEditorButton.text = "Open gradient editor";
                    Styles.GradientEditorButton.tooltip = "Open the gradient editor for creating a custom toon ramp";
                }
            }
        }

        MaterialProperty _Ramp;
        MaterialProperty _RampColor;
        MaterialProperty _RampOffset;
        MaterialProperty _ShadowIntensity;
        MaterialProperty _OcclusionOffsetIntensity;

        MaterialProperty _OcclusionOffset;

        MaterialProperty _ToonRampBox;
        MaterialProperty _RampOn;

        ToonyStandardGUI inspector;

        GradientEditor gradientEditor;

        bool isGradientEditorOpen;
        bool needToStorePreviousRamp;

        Texture PreviousRamp;

        public RampSection(ToonyStandardGUI inspector, MaterialProperty[] properties, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);
            this.inspector = inspector;
            gradientEditor = new GradientEditor();
            isGradientEditorOpen = false;
            needToStorePreviousRamp = true;
            Selection.selectionChanged += ResetRampTexture;
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _Ramp = FindProperty("_Ramp", properties);
            _RampColor = FindProperty("_RampColor", properties);
            _RampOffset = FindProperty("_RampOffset", properties);
            _ShadowIntensity = FindProperty("_ShadowIntensity", properties);
            _OcclusionOffsetIntensity = FindProperty("_OcclusionOffsetIntensity", properties);

            _OcclusionOffset = FindProperty("_OcclusionOffset", properties);

            _ToonRampBox = FindProperty("_ToonRampBox", properties);
            _RampOn = FindProperty("_RampOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            bool isOcclusionOffsetEnabled;
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            materialEditor.TexturePropertySingleLine(Styles.ramp, _Ramp, _RampColor);

            if (GUILayout.Button(Styles.GradientEditorButton))
            {
                EditorGUILayout.EndHorizontal();
                isGradientEditorOpen = !isGradientEditorOpen;
                Styles.ToggleGradientEditorToggle(isGradientEditorOpen);
            }
            else
            {
                EditorGUILayout.EndHorizontal();
            }
            if (isGradientEditorOpen)
            {
                EditorGUILayout.BeginVertical("box");
                if (needToStorePreviousRamp)
                {
                    needToStorePreviousRamp = false;
                    PreviousRamp = _Ramp.textureValue;
                }
                if (_Ramp.textureValue != (Texture)gradientEditor.GetGradientTexture())
                {
                    _Ramp.textureValue = (Texture)gradientEditor.GetGradientTexture();
                }
                if (gradientEditor.DrawGUI())
                {
                    materialEditor.Repaint();
                }
                if (GUILayout.Button("Save and apply"))
                {
                    string path = inspector.GetTextureDestinationPath((Material)_Ramp.targets[0], "_ramp.png");
                    _Ramp.textureValue = (Texture)gradientEditor.SaveGradient(path);
                    needToStorePreviousRamp = true;
                    PreviousRamp = null;
                    isGradientEditorOpen = false;
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (PreviousRamp != null)
                {
                    _Ramp.textureValue = PreviousRamp;
                    PreviousRamp = null;
                    needToStorePreviousRamp = true;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                inspector.GenerateRampMinMax(properties);
            }

            EditorGUILayout.Space();

            materialEditor.ShaderProperty(_RampOffset, Styles.rampOffset);
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(_ShadowIntensity, Styles.shadowIntensity);
            if (EditorGUI.EndChangeCheck())
            {
                inspector.GenerateRampMinMax(properties);
            }

            isOcclusionOffsetEnabled = TSFunctions.ProperToggle(ref _OcclusionOffset, Styles.occlusionOffsetIntensity);
            if (!isOcclusionOffsetEnabled && !_OcclusionOffset.hasMixedValue)
            {
                _OcclusionOffsetIntensity.floatValue = 0;
            }
            if (isOcclusionOffsetEnabled)
            {
                materialEditor.ShaderProperty(_OcclusionOffsetIntensity, Styles.occlusionOffsetIntensity);
            }


            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _ToonRampBox.floatValue = TSFunctions.floatBoolean(isOpen);
            if (!isEnabled)
            {
                if (!_RampOn.hasMixedValue)
                {
                    _RampOn.floatValue = 0;
                }
            }
        }

        protected override MaterialProperty GetIndex()
        {
            return _RampOn;
        }



        protected override MaterialProperty GetBox()
        {
            return _ToonRampBox;
        }

        public void ResetRampTexture()
        {
            if (PreviousRamp != null)
            {
                _Ramp.textureValue = PreviousRamp;
                PreviousRamp = null;
                needToStorePreviousRamp = true;
            }
            Selection.selectionChanged -= ResetRampTexture;
        }
    }
}
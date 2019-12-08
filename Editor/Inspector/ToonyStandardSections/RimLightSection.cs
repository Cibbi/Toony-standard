using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class RimLightSection : OrderedSection
    {
        private static class Styles
        {
            public static GUIContent title = new GUIContent("Rim Light Options", "Various options for rim light, can be disabled");

            public static GUIContent rimColor = new GUIContent("Rim color", "Color of the rim light");
            public static GUIContent rimStrength = new GUIContent("Rim strength", "Defines how far the rim light extends");
            public static GUIContent rimSharpness = new GUIContent("Rim sharpness", "Defines how sharp the rim is");
            public static GUIContent rimIntensity = new GUIContent("Rim intensity", "Defines the intensity of the rim, below 0 will make a rim darker than the base");
            public static GUIContent emissiveRim = new GUIContent("Emissive rim", "If turned on, the rim light will be emissive");
        }

        MaterialProperty _RimColor;
        MaterialProperty _RimStrength;
        MaterialProperty _RimSharpness;
        MaterialProperty _RimIntensity;

        MaterialProperty _EmissiveRim;

        MaterialProperty _RimLightBox;
        MaterialProperty _RimLightOn;

        public RimLightSection(MaterialProperty[] properties, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _RimColor = FindProperty("_RimColor", properties);
            _RimStrength = FindProperty("_RimStrength", properties);
            _RimSharpness = FindProperty("_RimSharpness", properties);
            _RimIntensity = FindProperty("_RimIntensity", properties);

            _EmissiveRim = FindProperty("_EmissiveRim", properties);

            _RimLightBox = FindProperty("_RimLightBox", properties);
            _RimLightOn = FindProperty("_RimLightOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            bool isEmissiveRimEnabled;

            EditorGUILayout.Space();
            TSFunctions.ProperColorBox(ref _RimColor, Styles.rimColor);
            materialEditor.ShaderProperty(_RimIntensity, Styles.rimIntensity);
            materialEditor.ShaderProperty(_RimStrength, Styles.rimStrength);
            materialEditor.ShaderProperty(_RimSharpness, Styles.rimSharpness);
            EditorGUI.BeginChangeCheck();
            isEmissiveRimEnabled = TSFunctions.ProperToggle(ref _EmissiveRim, Styles.emissiveRim);
            if (EditorGUI.EndChangeCheck())
            {
                _EmissiveRim.floatValue = TSFunctions.floatBoolean(isEmissiveRimEnabled);
            }

            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _RimLightBox.floatValue = TSFunctions.floatBoolean(isOpen);
            if (!isEnabled)
            {
                if (!_RimLightOn.hasMixedValue)
                {
                    _RimLightOn.floatValue = 0;
                }
            }
        }

        protected override MaterialProperty GetIndex()
        {
            return _RimLightOn;
        }

        protected override MaterialProperty GetBox()
        {
            return _RimLightBox;
        }
    }
}
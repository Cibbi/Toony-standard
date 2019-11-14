using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class BasicSpecularSection : OrderedSection
    {
        public enum IndirectSpecular
        {
            Probe,
            Matcap,
            Cubemap,
            Color
        }
        public enum Workflow
        {
            Metallic,
            Specular
        }
        public enum SpMode
        {
            Standard,
            Anisotropic,
            Fake
        }

        private static class Styles
        {
            public static GUIContent title = new GUIContent("Specular Options", "Various options for specular calculations, can be disabled");

            public static GUIContent smoothness = new GUIContent("Smoothness", "Smoothness map and intensity, usually the slider is set to 1 when using a smoothness texture");
            public static GUIContent metallic = new GUIContent("Metallic", "Metallic map and intensity, usually the slider is set to 1 when using a metallic texture");
            public static GUIContent toonyHighlight = new GUIContent("Toony highlights", "Make the the current highlights toony style");
            public static GUIContent highlightIntensity = new GUIContent("Highlight intensity", "Defines how intense the highlight ramp is");
        }

        MaterialProperty _Glossiness;
        MaterialProperty _Metallic;
        MaterialProperty _HighlightIntensity;

        MaterialProperty _ToonyHighlights;

        MaterialProperty _SpecularBox;
        MaterialProperty _SpecularOn;

        public BasicSpecularSection(MaterialProperty[] properties, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);

            foreach (Material mat in _SpecularOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", !(mat.GetFloat(_SpecularOn.name) != 0));
            }
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _Glossiness = FindProperty("_Glossiness", properties);
            _Metallic = FindProperty("_Metallic", properties);
            _HighlightIntensity = FindProperty("_HighlightIntensity", properties);

            _ToonyHighlights = FindProperty("_ToonyHighlights", properties);

            _SpecularBox = FindProperty("_SpecularBox", properties);
            _SpecularOn = FindProperty("_SpecularOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            bool isToonyHighlightsEnabled;
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(_Metallic, Styles.metallic);
            materialEditor.ShaderProperty(_Glossiness, Styles.smoothness);

            isToonyHighlightsEnabled=TSFunctions.ProperToggle(ref _ToonyHighlights,Styles.toonyHighlight);

            if (isToonyHighlightsEnabled)
            {
                materialEditor.ShaderProperty(_HighlightIntensity, Styles.highlightIntensity);
            }

            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _SpecularBox.floatValue = TSFunctions.floatBoolean(isOpen);
            foreach (Material mat in _SpecularOn.targets)
            {
                if (!isEnabled)
                {   
                    if(!_SpecularOn.hasMixedValue)
                    {
                        TSFunctions.SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", !isEnabled);
                        _SpecularOn.floatValue = TSFunctions.floatBoolean(!mat.IsKeywordEnabled("_SPECULARHIGHLIGHTS_OFF"));
                    }
                }
            }
        }

        public override void OnAdd()
        {
            foreach (Material mat in _SpecularOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", !(mat.GetFloat(_SpecularOn.name) != 0));
            }
        }

        protected override MaterialProperty GetIndex()
        {
            return _SpecularOn;
        }

        protected override MaterialProperty GetBox()
        {
            return _SpecularBox;
        }
    }
}
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

        public RampSection(ToonyStandardGUI inspector, MaterialProperty[] properties, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);
            this.inspector=inspector;
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
            materialEditor.TexturePropertySingleLine(Styles.ramp, _Ramp, _RampColor);
            if(EditorGUI.EndChangeCheck())
            {
                inspector.GenerateRampMinMax(properties);
            }
            materialEditor.ShaderProperty(_RampOffset, Styles.rampOffset);
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(_ShadowIntensity, Styles.shadowIntensity);
            if(EditorGUI.EndChangeCheck())
            {
                inspector.GenerateRampMinMax(properties);
            }

            isOcclusionOffsetEnabled=TSFunctions.ProperToggle(ref _OcclusionOffset,Styles.occlusionOffsetIntensity);
            if(!isOcclusionOffsetEnabled&&!_OcclusionOffset.hasMixedValue)
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
                if(!_RampOn.hasMixedValue)
                {
                    _RampOn.floatValue = 0;
                }
                _Ramp.textureValue=TSConstants.DefaultRamp;
                _RampOffset.floatValue=0f;
                _ShadowIntensity.floatValue=0.4f;
                _OcclusionOffset.floatValue=0f;
                _OcclusionOffsetIntensity.floatValue=0f;
            }
            if(!_RampOn.hasMixedValue && _RampOn.floatValue==0f)
            {
                _Ramp.textureValue=TSConstants.DefaultRamp;
                _RampOffset.floatValue=0f;
                _ShadowIntensity.floatValue=0.4f;
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
    }
}
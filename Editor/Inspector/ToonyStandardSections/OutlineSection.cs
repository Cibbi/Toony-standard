using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class OutlineSection : OrderedSection
    {
        private static class Styles
        {
            public static GUIContent title = new GUIContent("Outline Options", "Options for outlines");

            public static GUIContent outlineWidth = new GUIContent("Outline width (px)", "Width of the outline, the value is in pixel");
            public static GUIContent outlineOffsetX = new GUIContent("Horizzontal offset (px)", "Horizzontal offset of the outline, the value is in pixel");
            public static GUIContent outlineOffsetY = new GUIContent("Vertical offset (px)", "Vertical offset of the outline, the value is in pixel");
            public static GUIContent outlineColor = new GUIContent("Outline Color", "Color of the outline");
            public static GUIContent isOutlineEmissive = new GUIContent("Emissive outline", "When enabled the outline will ignore the average light");
        }

        MaterialProperty _OutlineWidth;
        MaterialProperty _OutlineOffsetX;
        MaterialProperty _OutlineOffsetY;
        MaterialProperty _OutlineColor;
        MaterialProperty _IsOutlineEmissive;

        MaterialProperty _OutlineBox;
        MaterialProperty _OutlineOn;

        public OutlineSection(MaterialProperty[] properties, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _OutlineWidth = FindProperty("_OutlineWidth", properties);
            _OutlineOffsetX = FindProperty("_OutlineOffsetX", properties);
            _OutlineOffsetY = FindProperty("_OutlineOffsetY", properties);
            _OutlineColor = FindProperty("_OutlineColor", properties);
            _IsOutlineEmissive = FindProperty("_IsOutlineEmissive", properties);

            _OutlineBox = FindProperty("_OutlineBox", properties);
            _OutlineOn = FindProperty("_OutlineOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            EditorGUILayout.Space();
            materialEditor.ShaderProperty(_OutlineWidth, Styles.outlineWidth);
            materialEditor.ShaderProperty(_OutlineOffsetX, Styles.outlineOffsetX);
            materialEditor.ShaderProperty(_OutlineOffsetY, Styles.outlineOffsetY);
            TSFunctions.ProperColorBox(ref _OutlineColor, Styles.outlineColor);
            TSFunctions.ProperToggle(ref _IsOutlineEmissive, Styles.isOutlineEmissive);

            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _OutlineBox.floatValue = TSFunctions.floatBoolean(isOpen);
            if (!isEnabled)
            {
                if(!_OutlineOn.hasMixedValue)
                {
                    _OutlineOn.floatValue = 0;
                    foreach (Material mat in _OutlineOn.targets)
                    {
                        ToonyStandardGUI.SetupMaterialWithBlendMode(mat, (ToonyStandardGUI.BlendMode)mat.GetFloat("_Mode"), mat.GetFloat("_OutlineOn")>0);
                    }
                }
            }
        }

        public override void OnAdd()
        {
            foreach (Material mat in _OutlineOn.targets)
            {
                ToonyStandardGUI.SetupMaterialWithBlendMode(mat, (ToonyStandardGUI.BlendMode)mat.GetFloat("_Mode"), mat.GetFloat("_OutlineOn")>0);
            }
        }

        protected override MaterialProperty GetIndex()
        {
            return _OutlineOn;
        }

        protected override MaterialProperty GetBox()
        {
            return _OutlineBox;
        }
    }
}
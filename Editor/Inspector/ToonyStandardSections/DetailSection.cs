using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class DetailSection : OrderedSection
    {
        private static class Styles
        {
            public static GUIContent title = new GUIContent("Detail Options", "Various options for detail textures, can be disabled");

            public static GUIContent detailMask = new GUIContent("Detail mask", "Detail mask used to decide where the detail map should be visible or not");
            public static GUIContent detailIntensity = new GUIContent("Detail intensity", "used to decide the intensity of the detail map");
            public static GUIContent detailPattern = new GUIContent("Detail pattern", "Detail texture and color");
            public static GUIContent detailNormal = new GUIContent("Detail normal", "Detail Normal Map with scale property");
        }

        MaterialProperty _DetailMask;
        MaterialProperty _DetailIntensity;
        MaterialProperty _DetailTexture;
        MaterialProperty _DetailColor;
        MaterialProperty _DetailBumpMap;
        MaterialProperty _DetailBumpScale;
        MaterialProperty _DetailTileAndOffset;

        MaterialProperty _DetailBox;
        MaterialProperty _DetailMapOn;

        public DetailSection(MaterialProperty[] properties, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);

            foreach (Material mat in _DetailMapOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_DETAIL_MULX2", mat.GetFloat(_DetailMapOn.name) != 0);
            }
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _DetailMask = FindProperty("_DetailMask", properties);
            _DetailIntensity = FindProperty("_DetailIntensity", properties);
            _DetailTexture = FindProperty("_DetailTexture", properties);
            _DetailColor = FindProperty("_DetailColor", properties);
            _DetailBumpMap = FindProperty("_DetailBumpMap", properties);
            _DetailBumpScale = FindProperty("_DetailBumpScale", properties);

            _DetailBox = FindProperty("_DetailBox", properties);
            _DetailMapOn = FindProperty("_DetailMapOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            EditorGUILayout.Space();
            materialEditor.TexturePropertySingleLine(Styles.detailMask, _DetailMask);
            materialEditor.ShaderProperty(_DetailIntensity, Styles.detailIntensity);
            materialEditor.TexturePropertySingleLine(Styles.detailPattern, _DetailTexture, _DetailColor);
            materialEditor.TexturePropertySingleLine(Styles.detailNormal, _DetailBumpMap, _DetailBumpScale);
            materialEditor.TextureScaleOffsetProperty(_DetailTexture);
            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _DetailBox.floatValue = TSFunctions.floatBoolean(isOpen);
            foreach (Material mat in _DetailMapOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_DETAIL_MULX2", isEnabled);
            }
            if (!isEnabled)
            {
                if(!_DetailMapOn.hasMixedValue)
                {
                    _DetailMapOn.floatValue = 0;
                }
            }
        }

        public override void OnAdd()
        {
            foreach (Material mat in _DetailMapOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_DETAIL_MULX2", mat.GetFloat(_DetailMapOn.name) != 0);
            }
        }

        protected override MaterialProperty GetIndex()
        {
            return _DetailMapOn;
        }

        protected override MaterialProperty GetBox()
        {
            return _DetailBox;
        }
    }
}
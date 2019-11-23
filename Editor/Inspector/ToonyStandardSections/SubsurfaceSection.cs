using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class SubsurfaceSection : OrderedSection
    {
        private static class Styles
        {
            public static GUIContent title = new GUIContent("Subsurface Scattering Options", "Subsurface scattering options, can be disabled");

            public static GUIContent thickness = new GUIContent("Thickness map", "Map used to determine the thickness of an object");
            public static GUIContent color = new GUIContent("Color", "Color of the scattering light");
            public static GUIContent distortion = new GUIContent("Distortion", "Distortion of the normals");
            public static GUIContent power = new GUIContent("Power", "Defines how the subsurface extends");
            public static GUIContent scale = new GUIContent("Scale", "Scale of the subsurface scattering");
        }

        MaterialProperty _ThicknessMap;
        MaterialProperty _SSColor;
        MaterialProperty _SSDistortion;
        MaterialProperty _SSPower;
        MaterialProperty _SSScale;

        MaterialProperty _SSSBox;
        MaterialProperty _SSSOn;

        ToonyStandardGUI inspector;
        InspectorLevel level;

        public SubsurfaceSection(MaterialProperty[] properties, InspectorLevel level, ToonyStandardGUI gui, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            this.inspector = gui;
            this.level = level;
            FindProperties(properties);
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _ThicknessMap = FindProperty("_ThicknessMap", properties);
            _SSColor = FindProperty("_SSColor", properties);
            _SSDistortion = FindProperty("_SSDistortion", properties);
            _SSPower = FindProperty("_SSPower", properties);
            _SSScale = FindProperty("_SSScale", properties);

            _SSSBox = FindProperty("_SSSBox", properties);
            _SSSOn = FindProperty("_SSSOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            EditorGUILayout.Space();
            if(level==InspectorLevel.Normal)
            {   
                //Rect r = TSFunctions.GetControlRectForSingleLine(); 
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertySingleLine(Styles.thickness, _ThicknessMap);
                if(EditorGUI.EndChangeCheck())
                {
                    inspector.RegenerateMSOT();
                }
            }  
            TSFunctions.ProperColorBox(ref _SSColor, Styles.color);
            //materialEditor.ShaderProperty(_SSColor, Styles.color);
            materialEditor.ShaderProperty(_SSDistortion, Styles.distortion);
            materialEditor.ShaderProperty(_SSPower, Styles.power);
            materialEditor.ShaderProperty(_SSScale, Styles.scale);
            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _SSSBox.floatValue = TSFunctions.floatBoolean(isOpen);
            if (!isEnabled)
            {
                if(!_SSSOn.hasMixedValue)
                {
                    _SSSOn.floatValue = 0;
                }
            }
        }

        public override void OnAdd()
        {
        }

        protected override MaterialProperty GetIndex()
        {
            return _SSSOn;
        }

        protected override MaterialProperty GetBox()
        {
            return _SSSBox;
        }
    }
}
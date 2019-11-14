using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class StencilSection : OrderedSection
    {
        private static class Styles
        {
            public static GUIContent title = new GUIContent("Stencil Options", "Options for stenciling");

            public static GUIContent stencilID = new GUIContent("Stencil ID (0-255)", "ID of the stencil, used for comparison");
            public static GUIContent stencilComp = new GUIContent("Stencil comparison", "Type of comparison done");
            public static GUIContent stencilOperation = new GUIContent("Stencil operation", "Operation to do when the stencil comparison is positive");
        }

        MaterialProperty _StencilID;
        MaterialProperty _StencilComp;
        MaterialProperty _StencilOp;

        MaterialProperty _StencilBox;
        MaterialProperty _StencilOn;

        public StencilSection(MaterialProperty[] properties, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _StencilID = FindProperty("_StencilID", properties);
            _StencilComp = FindProperty("_StencilComp", properties);
            _StencilOp = FindProperty("_StencilOp", properties);

            _StencilBox = FindProperty("_StencilBox", properties);
            _StencilOn = FindProperty("_StencilOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            EditorGUILayout.Space();
            materialEditor.ShaderProperty(_StencilID, Styles.stencilID);
            materialEditor.ShaderProperty(_StencilComp, Styles.stencilComp);
            materialEditor.ShaderProperty(_StencilOp, Styles.stencilOperation);

            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _StencilBox.floatValue = TSFunctions.floatBoolean(isOpen);
            if (!isEnabled)
            {
                if(!_StencilOn.hasMixedValue)
                {
                    _StencilOn.floatValue = 0;
                    _StencilID.floatValue = 0;
                    _StencilComp.floatValue = 0;
                    _StencilOp.floatValue = 0;
                }
            }
        }

        protected override MaterialProperty GetIndex()
        {
            return _StencilOn;
        }

        protected override MaterialProperty GetBox()
        {
            return _StencilBox;
        }
    }
}
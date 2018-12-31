namespace Cibbi.SimpleInspectors
{
    using UnityEditor;
    using UnityEngine;

    using System;
    public class StoredShaderProperty : StoredProperty  {

        GUIContent label;
        MaterialProperty property;

        /// <summary>
        /// Object that stores a shader property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the shader property</param>
        /// <param name="property">The shader property</param>
        public StoredShaderProperty(GUIContent label, MaterialProperty property){

            this.label=label;
            this.property=property;
        }

        /// <summary>
        /// Object that stores a shader property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the shader property</param>
        /// <param name="property">The shader property</param>
        public StoredShaderProperty(string label,MaterialProperty property): this(new GUIContent(label), property){}

        /// <summary>
        /// Object that stores a shader property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="property">The shader property</param>
        public StoredShaderProperty(MaterialProperty property): this(new GUIContent(property.displayName,property.displayName), property){}
        
        /// <summary>
        /// Draws the property stored inside this object
        /// </summary>
        /// <param name="materialEditor">Material editor to draw the property in</param>
        public override void DrawProperty(MaterialEditor materialEditor){
            
            materialEditor.ShaderProperty(property, label);
        }

        /// <summary>
        /// Get the stored property
        /// </summary>
        public MaterialProperty GetStoredProperty()
        {
            return property;
        }
    }
}
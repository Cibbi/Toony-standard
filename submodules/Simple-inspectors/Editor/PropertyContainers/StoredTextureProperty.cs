namespace Cibbi.SimpleInspectors
{
    using UnityEditor;
    using UnityEngine;

    using System;

    public class StoredTextureProperty : StoredProperty  {

        GUIContent label;
        MaterialProperty textureProperty;
        MaterialProperty extraProperty1;
        MaterialProperty extraProperty2;
        bool showTextureScaleAndOffset;

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        /// <param name="extraProperty1">First optional property inlined after the texture property</param>
        /// <param name="extraProperty2">Second optional property inlined after the texture property</param>
        public StoredTextureProperty(GUIContent label, MaterialProperty textureProperty, MaterialProperty extraProperty1, MaterialProperty extraProperty2){

            this.label=label;
            this.textureProperty=textureProperty;
            this.extraProperty1=extraProperty1;
            this.extraProperty2=extraProperty2;

            showTextureScaleAndOffset=false;
        }

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        /// <param name="extraProperty1">First optional property inlined after the texture property</param>
        /// <param name="extraProperty2">Second optional property inlined after the texture property</param>
        public StoredTextureProperty(string label, MaterialProperty textureProperty, MaterialProperty extraProperty1, MaterialProperty extraProperty2) : this(new GUIContent(label,label),textureProperty,extraProperty1,extraProperty2){}

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        /// <param name="extraProperty1">First optional property inlined after the texture property</param>
        /// <param name="extraProperty2">Second optional property inlined after the texture property</param>
        public StoredTextureProperty( MaterialProperty textureProperty, MaterialProperty extraProperty1, MaterialProperty extraProperty2) : this(new GUIContent(textureProperty.displayName,textureProperty.displayName),textureProperty,extraProperty1,extraProperty2){}

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        /// <param name="extraProperty1">First optional property inlined after the texture property</param>
        public StoredTextureProperty(GUIContent label, MaterialProperty textureProperty, MaterialProperty extraProperty1): this(label,textureProperty,extraProperty1,null){}

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        /// <param name="extraProperty1">First optional property inlined after the texture property</param>
        public StoredTextureProperty(string label, MaterialProperty textureProperty, MaterialProperty extraProperty1): this(new GUIContent(label,label),textureProperty,extraProperty1,null){}

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        /// <param name="extraProperty1">First optional property inlined after the texture property</param>
        public StoredTextureProperty(MaterialProperty textureProperty, MaterialProperty extraProperty1): this(new GUIContent(textureProperty.displayName,textureProperty.displayName),textureProperty,extraProperty1,null){}
        
        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        public StoredTextureProperty(GUIContent label, MaterialProperty textureProperty): this(label,textureProperty,null,null){}

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        public StoredTextureProperty(string label, MaterialProperty textureProperty): this(new GUIContent(label,label),textureProperty,null,null){}

        /// <summary>
        /// Object that stores a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="label">The label used for the texture property</param>
        /// <param name="textureProperty">The texture property</param>
        public StoredTextureProperty(MaterialProperty textureProperty): this(new GUIContent(textureProperty.displayName,textureProperty.displayName),textureProperty,null,null){}
        
        /// <summary>
        /// Draws the property stored inside this object
        /// </summary>
        /// <param name="materialEditor">Material editor to draw the property in</param>
        public override void DrawProperty(MaterialEditor materialEditor){

            if(extraProperty1 != null){
                if(extraProperty2 != null){
                    materialEditor.TexturePropertySingleLine(label, textureProperty, extraProperty1, extraProperty2);
                }
                else{
                    materialEditor.TexturePropertySingleLine(label, textureProperty, extraProperty1);
                }
            }
            else{
                materialEditor.TexturePropertySingleLine(label, textureProperty);
            }
            if(showTextureScaleAndOffset&&textureProperty.textureValue)
            {
                EditorGUI.indentLevel++;
                materialEditor.TextureScaleOffsetProperty(textureProperty);
                EditorGUI.indentLevel--;
            }

        }

        /// <summary>
        /// Get the stored texture property
        /// </summary>
        public MaterialProperty GetGetStoredTextureProperty()
        {
            return textureProperty;
        }

        /// <summary>
        /// Get the first extra stored property
        /// </summary>
        public MaterialProperty GetGetExtraProperty1()
        {
            return extraProperty1;
        }

        /// <summary>
        /// Get the second extra stored property
        /// </summary>
        public MaterialProperty GetGetExtraProperty2()
        {
            return extraProperty2;
        }

        /// <summary>
        /// Toggle the showing of the texture scale and offset field
        /// </summary>
        /// <param name="showTextureScaleAndOffset">true: show texture scale and offset false: don't show texture scale and offset</param>
        public void ShowTextureScaleAndOffset(bool showTextureScaleAndOffset)
        {
            this.showTextureScaleAndOffset=showTextureScaleAndOffset;
        }
    }
}
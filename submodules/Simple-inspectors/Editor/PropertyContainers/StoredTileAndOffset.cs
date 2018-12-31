namespace Cibbi.SimpleInspectors
{
    using UnityEditor;
    using UnityEngine;

    using System;
    public class StoredTileAndOffset : StoredProperty  {

        MaterialProperty property;

        /// <summary>
        /// Object that stores the texture scale and offset field of a texture property to be drawn later or passed to other objects
        /// </summary>
        /// <param name="property">The texture property</param>
        public StoredTileAndOffset(MaterialProperty property){

            this.property=property;
        }    
        /// <summary>
        /// Draws the texture scale and offset property stored inside this object
        /// </summary>
        /// <param name="materialEditor">Material editor to draw the texture tile and offset in</param>
        public override void DrawProperty(MaterialEditor materialEditor){
            
             materialEditor.TextureScaleOffsetProperty(property);
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
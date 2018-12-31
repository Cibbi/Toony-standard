namespace Cibbi.SimpleInspectors
{
	using UnityEditor;
    using UnityEngine;

	public class StoredSelectorProperty : StoredProperty {

        MaterialProperty selectedOption;
        GUIContent[] options;
        GUIContent label;

        /// <summary>
        /// Stores a property where you can select between multiple options
        /// </summary>
        /// <param name="label">Label of the property</param>
        /// <param name="options">Array of strings containing the options</param>
        /// <param name="selectedOption">Property that contains the value indicating the currently selected option</param>
        public StoredSelectorProperty(GUIContent label, string[] options, MaterialProperty selectedOption)
        {
            this.selectedOption=selectedOption;
            this.options= new GUIContent[options.Length];
            int i=0;
            foreach(string option in options)
            {   
                this.options[i]=new GUIContent(option, option);
                i++;
            }
            this.label=label;
        }

        /// <summary>
        /// Stores a property where you can select between multiple options
        /// </summary>
        /// <param name="label">Label of the property</param>
        /// <param name="options">Array of strings containing the options</param>
        /// <param name="selectedOption">Property that contains the value indicating the currently selected option</param>
        public StoredSelectorProperty(string label, string[] options, MaterialProperty selectedOption) : this(new GUIContent(label, label), options, selectedOption){}

        /// <summary>
        /// Get the currently selected option
        /// </summary>
        /// <returns>Integer representing the currently selected option</returns>
        public int getSelectedOption()
        {
            return (int)selectedOption.floatValue;
        }

        /// <summary>
        /// Draws the property Stored inside this object
        /// </summary>
        /// <param name="materialEditor">Material editor to draw the property in</param>
		public override void DrawProperty(MaterialEditor materialEditor)
        {
            int bMode = (int)selectedOption.floatValue;
            EditorGUI.BeginChangeCheck();
            bMode = EditorGUILayout.Popup(label, (int)bMode, options);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(label.text);
                selectedOption.floatValue = (float)bMode;
            }

        }
	}

}
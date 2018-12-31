namespace Cibbi.SimpleInspectors
{
	using UnityEditor;
    using UnityEngine;

	public class StoredToggle : StoredProperty  {

		GUIContent label;
		bool enabled;

		/// <summary>
		/// Object that stores a toggle that is not dependent to any shader property, can be usefull for toggling on and off stuff in the editor or keywords
		/// </summary>
		/// <param name="label">label of he toggle</param>
		/// <param name="enabled">toggle enabled or disabled on creation</param>
		public StoredToggle(GUIContent label, bool enabled)
		{
			this.label=label;
			this.enabled=enabled;
		}

		/// <summary>
		/// Object that stores a toggle that is not dependent to any shader property, can be usefull for toggling on and off stuff in the editor or keywords
		/// </summary>
		/// <param name="label">label of he toggle</param>
		/// <param name="enabled">toggle enabled or disabled on creation</param>
		public StoredToggle(string label,bool enabled) : this(new GUIContent(label),enabled){}

        public bool IsEnabled()
		{
			return enabled;
		}

		/// <summary>
		/// Draws the property Stored inside this object
		/// </summary>
		/// <param name="materialEditor">Material editor to draw the property in</param>
		public override void DrawProperty(MaterialEditor materialEditor)
		{
			enabled=EditorGUILayout.Toggle(label, enabled);
		}
	}

}
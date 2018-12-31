namespace Cibbi.SimpleInspectors
{
	using UnityEditor;
	using UnityEngine;
	using System.Collections;

	public class PropertiesBox  {
		
		ArrayList properties=new ArrayList();
		bool canBeDisabled;
		bool isEnabled;
		bool isOpen;
		GUIContent label;

		/// <summary>
		/// Construct a box containing properties that can be collapsed and disabled
		/// </summary>
		/// <param name="properties">Properties array that will be contained by the box</param>
		/// <param name="label">Label of the box</param>
		/// <param name="isOpen">Says if the box is open or collapsed</param>
		/// <param name="canBeDisabled">Used to decide what style of box to use based on the ability to disable it's properties </param>
		/// <param name="isEnabled">Are the properties inside enabled? (does nothing if "canBeDisabled" is false)</param>
		public PropertiesBox(StoredProperty[] properties, GUIContent label,  bool isOpen, bool canBeDisabled, bool isEnabled)
		{
			foreach(StoredProperty property in properties)
			{
				this.properties.Add(property);
			}
			this.label=label; 
			this.isOpen=isOpen;
			this.canBeDisabled=canBeDisabled;
			this.isEnabled=isEnabled;
		}

		/// <summary>
		/// Construct an empty box that can be collapsed and disabled
		/// </summary>
		/// <param name="label">Label of the box</param>
		/// <param name="isOpen">Says if the box is open or collapsed</param>
		/// <param name="canBeDisabled">Used to decide what style of box to use based on the ability to disable it's properties </param>
		/// <param name="isEnabled">Are the properties inside enabled? (does nothing if "canBeDisabled" is false)</param>
		public PropertiesBox(GUIContent label,  bool isOpen, bool canBeDisabled, bool isEnabled) : this(new StoredProperty[0], label, isOpen, canBeDisabled, isEnabled){}

		/// <summary>
		/// Add a property to be shown inside the box
		/// </summary>
		/// <param name="property">Property to add</param>
		public void AddProperty(StoredProperty property)
		{
			properties.Add(property);
		}

		/// <summary>
		/// Check if the box is open or collapsed
		/// </summary>
		/// <returns>A boolean indicating if the box is open or not</returns>
		public bool IsOpen()
		{
			return isOpen;
		}

		/// <summary>
		/// Check if the box properties are enabled or not
		/// </summary>
		/// <returns>A boolean indicating if the properties are enabled or not</returns>
		public bool IsEnabled()
		{
			return isEnabled;
		}

		/// <summary>
		/// Draws the box into the given Material Editor
		/// </summary>
		/// <param name="materialEditor">Material editor to draw the box with it's properties</param>
		public void DrawBox(MaterialEditor materialEditor)
		{ 
			GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        	sectionStyle.alignment = TextAnchor.MiddleCenter;
			Color bCol=GUI.backgroundColor;
            GUI.backgroundColor = new Color (0.9f, 0.9f, 0.9f, 0.75f);
			EditorGUILayout.BeginVertical ("Button");
			GUI.backgroundColor = bCol;
			Rect r = EditorGUILayout.BeginHorizontal();
			//decide what type of box style use based on if the content of the box can be disabled or not
			if(canBeDisabled)
			{	
            	isEnabled = EditorGUILayout.Toggle(isEnabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
				isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
			}
			else
			{
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10.0f));
			}
            EditorGUILayout.LabelField(label, sectionStyle);
			isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
			EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
            EditorGUILayout.EndHorizontal();
            if (isOpen)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(!isEnabled&&canBeDisabled);
				//all the properties conserved into this object will be drawn inside the box
				foreach (StoredProperty property in properties)
				{
					property.DrawProperty(materialEditor);
				}
                EditorGUI.EndDisabledGroup();
            }
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
		}
	}

}
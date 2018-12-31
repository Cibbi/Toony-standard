namespace Cibbi.SimpleInspectors
{
	using UnityEditor;
	using System.Collections;

	public class StoredPropertyList : StoredProperty  {

		ArrayList properties=new ArrayList();

		bool enabled;
		bool idented;

		/// <summary>
		/// Creates a list of properties that can be enabled and disabled in bulk
		/// </summary>
		/// <param name="properties">Properties array to pass to the list</param>
		/// <param name="enabled">Are the properties visible?</param>
		/// <param name="idented">If true properties inside will be displayed with 1+ identation level</param>
		public StoredPropertyList(StoredProperty[] properties, bool enabled, bool idented)
		{
			foreach(StoredProperty property in properties)
			{
				this.properties.Add(property);
			}
			this.enabled=enabled;
			this.idented=idented;
		}

		/// <summary>
		/// Creates a list of properties that can be enabled and disabled in bulk
		/// </summary>
		/// <param name="enabled">Are the properties visible?</param>
		/// <param name="idented">If true properties inside will be displayed with 1+ identation level</param>
		public StoredPropertyList( bool enabled, bool idented) : this(new StoredProperty[0], enabled, idented){}


		/// <summary>
		/// Enable/Disable display of properties
		/// </summary>
		public void Enable(bool enabled)
		{
			this.enabled=enabled;
		}

		/// <summary>
		/// Add a property to be shown inside the box
		/// </summary>
		/// <param name="property">Property to add</param>
		public void AddProperty(StoredProperty property)
		{
			properties.Add(property);
		}

		/// <summary>
		/// Draws the properties Stored inside this object
		/// </summary>
		/// <param name="materialEditor">Material editor to draw the properties in</param>
		public override void DrawProperty(MaterialEditor materialEditor)
		{
			if(enabled)
			{
				if(idented)
				{
					EditorGUI.indentLevel++;
				}
				foreach (StoredProperty property in properties)
				{
					property.DrawProperty(materialEditor);
				}
				if(idented)
				{
					EditorGUI.indentLevel--;
				}
				
			}
		}
	}

}
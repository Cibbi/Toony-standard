namespace Cibbi.SimpleInspectors
{
	using UnityEditor;

	public abstract class StoredProperty  {

		/// <summary>
		/// Draws the property Stored inside this object
		/// </summary>
		/// <param name="materialEditor">Material editor to draw the property in</param>
		public abstract void DrawProperty(MaterialEditor materialEditor);
	}

}
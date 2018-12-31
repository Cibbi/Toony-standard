namespace Cibbi.SimpleInspectors
{
	using UnityEditor;
	using UnityEngine;

	using System;

	public abstract class SimpleInspector : ShaderGUI 
	{
		private bool isFirstCycle=true;
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    	{
			if(isFirstCycle)
			{
				Start(materialEditor, properties);
				isFirstCycle=false;
			}

			EditorGUI.BeginChangeCheck();

			Update(materialEditor, properties);

			if(EditorGUI.EndChangeCheck())
			{
				CheckChanges();
			}

		}

		public abstract void Start(MaterialEditor materialEditor, MaterialProperty[] properties);

		public abstract void Update(MaterialEditor materialEditor, MaterialProperty[] properties);
		
		public virtual void CheckChanges()
		{

		}
		
		protected void SetKeyword(Material m, string keyword, bool state)
		{
			if (state)
				m.EnableKeyword(keyword);
			else
				m.DisableKeyword(keyword);
		}

	}
}
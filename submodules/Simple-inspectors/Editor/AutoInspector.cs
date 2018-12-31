namespace Cibbi.SimpleInspectors
{
	using UnityEditor;
	using UnityEngine;

	using System;
	using System.Collections;

	public class AutoInspector : SimpleInspector 
	{
		ArrayList inspectorProperties=new ArrayList();

		MaterialProperty lastTextureProperty=null;
		MaterialProperty[] textureExtra=new MaterialProperty[2]{null,null};
		int extraPropertiesInserted=0;

		public override void Start(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			foreach(MaterialProperty property in properties)
			{
				//check if there is any texture property pending for extra properties checking and we're not exceeding the 2 extra properties
				if(extraPropertiesInserted < 2 && lastTextureProperty != null)
				{	
					//Debug.Log("Check1");
					//checks if the property we're iterating is an extra property to add to the texture one
					if(property.displayName.Contains("(Extra)"))
					{
						textureExtra[extraPropertiesInserted]=property;
						extraPropertiesInserted++;
						//Debug.Log("addedExtra");
						continue;
					}
					//if not we're assuming that there are no extra properties left to add, and setting the extraPropertiesInserted value so the next iteration we know we have to register the texture property
					else
					{
						//Debug.Log("check2");
						extraPropertiesInserted=2;
					}
				}
				//check if we have a texture property to register
				if(extraPropertiesInserted == 2 && lastTextureProperty != null)
				{
					finalizeTextureProperty();
					//Debug.Log("finalizedTexture");
				}

				if(property.displayName.Contains("(Texture)"))
				{	
					//check needed in the case 2 texture properties are present one after another, cause in that case the first texture property is not yet finalized and must be finalized before starting initializing the next one
					if(lastTextureProperty != null)
					{
						finalizeTextureProperty();
						//Debug.Log("finalizedTexture");
					}
					lastTextureProperty=property;
					extraPropertiesInserted=0;
					//Debug.Log("InitializedTexture");
					continue;
				}
				if(property.flags!=MaterialProperty.PropFlags.HideInInspector)
				{
					StoredProperty genericProperty= new StoredShaderProperty(property);
					inspectorProperties.Add(genericProperty);
					//Debug.Log("addedProperty");
				}


				
			}
			//checks if there are any pending texture properties left to be finalized, happens if the last property is a texture property
			if(lastTextureProperty != null)
			{
				finalizeTextureProperty();
			}
		}

		public override void Update(MaterialEditor materialEditor, MaterialProperty[] properties)
		{	
			EditorGUILayout.Space();
			foreach(StoredProperty property in inspectorProperties)
			{
				property.DrawProperty(materialEditor);
			}
			EditorGUILayout.Space();
		}
		
		public override void CheckChanges()
		{

		}


		private void finalizeTextureProperty()
		{
			StoredProperty texture = new StoredTextureProperty(lastTextureProperty.displayName.Replace("(Texture)",""),lastTextureProperty,textureExtra[0],textureExtra[1]);
			inspectorProperties.Add(texture);
			lastTextureProperty=null;
			textureExtra[0]=null;
			textureExtra[1]=null;
		}

	}
}
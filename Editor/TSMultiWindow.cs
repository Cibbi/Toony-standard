using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;

namespace Cibbi.ToonyStandard
{
	public enum SectionStyle
	{
		Bubbles,
		Box,
		Foldout
	}
	public class TSMultiWindow : EditorWindow
	{
		private static int windowWidth=500;
		private static int windowHeight=700;

		private enum MultiWindowSection
		{
			Settings,
			Updater,
			Credits
		}

		private enum InspectorLevel
		{
			Basic,
			Normal,
			Expert
		}

		

		private bool firstCycle=true;
		
		Vector2 MainAreaScrollPos;

		private MultiWindowSection section;
		private TSUpdater updater;

		InspectorLevel inspectorLevel;
		SectionStyle sectionStyle;
		Color sectionColor;
		Section exampleSection;
		bool isAutoUpdateDisabled;

		private static TSMultiWindow CreateBaseWindow()
		{
			TSMultiWindow window = EditorWindow.GetWindow<TSMultiWindow>();
			window.titleContent=new GUIContent("Toony Standard");
			window.minSize=new Vector2(windowWidth, windowHeight);
			window.maxSize=new Vector2(windowWidth, windowHeight);
			return window;
		}

		#region window initializzations
			[MenuItem("Window/Toony Standard/Settings")]
			static void InitSettingsWindow()
			{
				TSMultiWindow window = CreateBaseWindow();
				window.SetOpenSection(MultiWindowSection.Settings);
			}

			[MenuItem("Window/Toony Standard/Updater")]
			static void InitUpdaterWindow()
			{
				TSMultiWindow window = CreateBaseWindow();
				window.SetOpenSection(MultiWindowSection.Updater);
			}

			[MenuItem("Window/Toony Standard/Credits")]
			static void InitCreditsWindow()
			{
				TSMultiWindow window = CreateBaseWindow();
				window.SetOpenSection(MultiWindowSection.Credits);
			}

		#endregion

		void OnGUI()
		{
			if(firstCycle)
			{
				Start();
			}

			if(updater == null)
			{
				updater=new TSUpdater();
			}

			TSFunctions.DrawHeader(10);

			section=(MultiWindowSection)GUILayout.Toolbar((int)section,Enum.GetNames(typeof(MultiWindowSection)),EditorStyles.toolbarButton, GUI.ToolbarButtonSize.Fixed);

			MainAreaScrollPos=EditorGUILayout.BeginScrollView(MainAreaScrollPos);
			GUILayout.Space(10);
			switch (section)
			{
				case MultiWindowSection.Settings:
					DrawSettings();
					break;
				case MultiWindowSection.Updater:
					DrawUpdater();
					break;
				case MultiWindowSection.Credits:
					DrawCredits();
					break;
				default:
					EditorGUILayout.LabelField("Something went wrong, maybe you should contact the creator and report this");
					break;
			}
			EditorGUILayout.EndScrollView();

			TSFunctions.DrawFooter();
		}

		void Update()
		{
			if(updater!= null)
			{
				if(section==MultiWindowSection.Updater)
				{
					updater.Update();
				}
			}
		}

		public void Start()
		{
			firstCycle=false;

			if(EditorPrefs.HasKey("TSInspectorLevel"))
			{
				inspectorLevel=(InspectorLevel)EditorPrefs.GetInt("TSInspectorLevel");
			}
			else
			{
				inspectorLevel=InspectorLevel.Normal;
				EditorPrefs.SetInt("TSInspectorLevel",(int)InspectorLevel.Normal);
			}

			if(File.Exists(TSConstants.settingsJSONPath))
			{
				TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.settingsJSONPath));
				sectionStyle=(SectionStyle)settings.sectionStyle;
				sectionColor=settings.sectionColor;
				isAutoUpdateDisabled=settings.disableUpdates;
			}
			else
			{
				TSSettings settings=new TSSettings();
				settings.sectionStyle=(int)SectionStyle.Bubbles;
				settings.sectionColor=new Color(1,1,1,1);
				settings.disableUpdates=false;
				File.WriteAllText(TSConstants.settingsJSONPath,JsonUtility.ToJson(settings));
			}
			
		}

		private void SetOpenSection(MultiWindowSection section)
		{
			this.section=section;
		}

		private void DrawSettings()
		{
			EditorGUI.BeginChangeCheck();
			inspectorLevel = (InspectorLevel)EditorGUILayout.EnumPopup("Inspector level",inspectorLevel);
			if(EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetInt("TSInspectorLevel",(int)inspectorLevel);
			}

			EditorGUI.BeginChangeCheck();
			updater.updateStream = (UpdateStream)EditorGUILayout.EnumPopup("Update stream",updater.updateStream);
			if(EditorGUI.EndChangeCheck())
			{
				LocalVersionJSON local=JsonUtility.FromJson<LocalVersionJSON>(File.ReadAllText(TSConstants.localJSONPath));
				local.beta=updater.updateStream==UpdateStream.Beta;
				File.WriteAllText(TSConstants.localJSONPath,JsonUtility.ToJson(local));
				updater.Reset();
			}

			EditorGUI.BeginChangeCheck();
			sectionStyle=(SectionStyle)EditorGUILayout.EnumPopup("Section style",sectionStyle);
			sectionColor=EditorGUILayout.ColorField("Section color",sectionColor);
			isAutoUpdateDisabled=EditorGUILayout.Toggle("Disable auto update",isAutoUpdateDisabled);
			if(EditorGUI.EndChangeCheck())
			{
				TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.settingsJSONPath));
				settings.sectionStyle=(int)sectionStyle;
				settings.sectionColor=sectionColor;
				settings.disableUpdates=isAutoUpdateDisabled;
				File.WriteAllText(TSConstants.settingsJSONPath,JsonUtility.ToJson(settings));
				exampleSection=new Section(new GUIContent("Example Section"),true,delegate(MaterialEditor m){EditorGUILayout.LabelField("Example content");},delegate(bool a, bool b){});
			}


			GUILayout.Space(20); 
			if(exampleSection==null)
			{
				exampleSection=new Section(new GUIContent("Example Section"),true,delegate(MaterialEditor m){EditorGUILayout.LabelField("Example content");},delegate(bool a, bool b){});
			}
			exampleSection.DrawSection(null);
		}

		private void DrawUpdater()
		{
			updater.DrawGUI();
		}

		private void DrawCredits()
		{
			
		}

		public void OnInspectorUpdate()
		{
			if(updater != null)
			{
				if(section==MultiWindowSection.Updater)
				{
					Repaint();
				}
			}
		}
	}

	[Serializable]
	public class TSSettings
	{
		public int sectionStyle;
		public Color sectionColor;
		public bool disableUpdates;
	}
}

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;

namespace Cibbi.ToonyStandard
{	
	/// <summary>
	/// Enum for selectioon of the style for the inspector sections
	/// </summary>
	public enum SectionStyle
	{
		Bubbles,
		Box,
		Foldout
	}

	/// <summary>
	/// Inspector level enum for selecting which inspector show based on user skill level
	/// </summary>
	public enum InspectorLevel
	{
		Basic,
		Normal,
		Expert
	}

	/// <summary>
	/// Window with multiple sections
	/// </summary>
	public class TSMultiWindow : EditorWindow
	{
		private static int windowWidth=500;
		private static int windowHeight=700;

		/// <summary>
		/// Sections of the multiwindow
		/// </summary>
		private enum MultiWindowSection
		{
			Settings,
			Updater,
			Credits
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

		/// <summary>
		/// Creates the base window object
		/// </summary>
		/// <returns>The window object</returns>
		private static TSMultiWindow CreateBaseWindow()
		{
			TSMultiWindow window = EditorWindow.GetWindow<TSMultiWindow>();
			window.titleContent=new GUIContent("Toony Standard");
			window.minSize=new Vector2(windowWidth, windowHeight);
			window.maxSize=new Vector2(windowWidth, windowHeight);
			return window;
		}

		/// <summary>
		/// All possible windows initializzations, there's one for each section and that will open the window with the desired section already selected
		/// </summary>
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

		/// <summary>
		/// Main GUI update
		/// </summary>
		void OnGUI()
		{	
			// If this is the first iteration since the window is opened, do the needed initializzations
			if(firstCycle)
			{
				Start();
			}
			// Check if for some reason the updater is not initialized, this eventuality tends to happen if some code gets recompiled while the window is open
			if(updater == null)
			{
				updater=new TSUpdater();
			}

			TSFunctions.DrawHeader(position.width,10);

			section=(MultiWindowSection)GUILayout.Toolbar((int)section,Enum.GetNames(typeof(MultiWindowSection)),EditorStyles.toolbarButton, GUI.ToolbarButtonSize.Fixed);

			MainAreaScrollPos=EditorGUILayout.BeginScrollView(MainAreaScrollPos);
			GUILayout.Space(10);
			// Based on the section selected draw only the gui of said section
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

		/// <summary>
		/// Main update cycle, independent from OnGUI, used mostly to make the updater run in background when it's doing something
		/// </summary>
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

		/// <summary>
		/// Initializations needed when first opening the window
		/// </summary>
		public void Start()
		{
			firstCycle=false;

			// Gets the inspector level from editorPrefs, if for some reason is not present it just defaults to normal and writes it (should never happen if i remembered to do the first time window)
			if(EditorPrefs.HasKey(TSConstants.TSEPInspectorLevel))
			{
				inspectorLevel=(InspectorLevel)EditorPrefs.GetInt(TSConstants.TSEPInspectorLevel);
			}
			else
			{
				inspectorLevel=InspectorLevel.Normal;
				EditorPrefs.SetInt(TSConstants.TSEPInspectorLevel,(int)InspectorLevel.Normal);
			}

			// Loads the settings file if exists, creates a default one if not
			if(File.Exists(TSConstants.SettingsJSONPath))
			{
				TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.SettingsJSONPath));
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
				File.WriteAllText(TSConstants.SettingsJSONPath,JsonUtility.ToJson(settings));
			}
			
		}
		/// <summary>
		/// Set the section that should be open
		/// </summary>
		/// <param name="section">Section to open</param>
		private void SetOpenSection(MultiWindowSection section)
		{
			this.section=section;
		}

		/// <summary>
		/// Draws the settings GUI
		/// </summary>
		private void DrawSettings()
		{
			EditorGUI.BeginChangeCheck();
			inspectorLevel = (InspectorLevel)EditorGUILayout.EnumPopup(TSConstants.TSWindowLabels.InspectorLevel,inspectorLevel);
			if(EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetInt(TSConstants.TSEPInspectorLevel,(int)inspectorLevel);
			}

			EditorGUI.BeginChangeCheck();
			updater.updateStream = (UpdateStream)EditorGUILayout.EnumPopup(TSConstants.TSWindowLabels.UpdateStream,updater.updateStream);
			if(EditorGUI.EndChangeCheck())
			{
				LocalVersionJSON local=JsonUtility.FromJson<LocalVersionJSON>(File.ReadAllText(TSConstants.LocalJSONPath));
				local.beta=updater.updateStream==UpdateStream.Beta;
				File.WriteAllText(TSConstants.LocalJSONPath,JsonUtility.ToJson(local));
				updater.Reset();
			}

			EditorGUI.BeginChangeCheck();
			sectionStyle=(SectionStyle)EditorGUILayout.EnumPopup(TSConstants.TSWindowLabels.SectionStyle,sectionStyle);
			sectionColor=EditorGUILayout.ColorField(TSConstants.TSWindowLabels.Color,sectionColor);
			isAutoUpdateDisabled=EditorGUILayout.Toggle(TSConstants.TSWindowLabels.DisableAutoUpdates,isAutoUpdateDisabled);
			if(EditorGUI.EndChangeCheck())
			{
				TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.SettingsJSONPath));
				settings.sectionStyle=(int)sectionStyle;
				settings.sectionColor=sectionColor;
				settings.disableUpdates=isAutoUpdateDisabled;
				File.WriteAllText(TSConstants.SettingsJSONPath,JsonUtility.ToJson(settings));
				exampleSection=new Section(new GUIContent("Example Section"),true,delegate(MaterialEditor m){EditorGUILayout.LabelField("Example content");},delegate(bool a, bool b){});
			}


			GUILayout.Space(20); 
			if(exampleSection==null)
			{
				exampleSection=new Section(new GUIContent("Example Section"),true,delegate(MaterialEditor m){EditorGUILayout.LabelField("Example content");},delegate(bool a, bool b){});
			}
			exampleSection.DrawSection(null);
		}

		/// <summary>
		/// Draws the updater GUI
		/// </summary>
		private void DrawUpdater()
		{
			updater.DrawGUI();
		}

		/// <summary>
		/// Draws the credits GUI
		/// </summary>
		private void DrawCredits()
		{
			
			EditorGUILayout.LabelField("This shader is in its current state also thanks to these folks:",TSConstants.Styles.multilineLabel);
			GUILayout.Space(20);
			EditorGUILayout.LabelField("LambdaDelta",TSConstants.Styles.sectionTitleCenter);
			EditorGUILayout.LabelField("He made the official logo cause i suck at doing art stuff.",TSConstants.Styles.multilineLabelCenter);
			GUILayout.Space(10);
			EditorGUILayout.LabelField("AlphaSatanOmega",TSConstants.Styles.sectionTitleCenter);
			EditorGUILayout.LabelField("My personal slav... ehm, i mean... beta tester!",TSConstants.Styles.multilineLabelCenter);
			GUILayout.Space(10);
			EditorGUILayout.LabelField("RetroGEO",TSConstants.Styles.sectionTitleCenter);
			EditorGUILayout.LabelField("A lot of the initial features of this shader were direct ports from my old, never released shader, and on that one he helped a lot on providing feedback on what could have been a nice addiction, also he heavily promoted my shader on release.",TSConstants.Styles.multilineLabelCenter);
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Xiexe",TSConstants.Styles.sectionTitleCenter);
			EditorGUILayout.LabelField("I don't think he knows it but i peek at his github sometimes for stuff he does, so it's fair to have also his name here.",TSConstants.Styles.multilineLabelCenter);
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Senpai Army Community",TSConstants.Styles.sectionTitleCenter);
			EditorGUILayout.LabelField("The community that saw the birth of this shader before its official release, and helped with the initial testing.",TSConstants.Styles.multilineLabelCenter);
			GUILayout.Space(10);
			//"https://github.com/yallie/unzip"
			EditorGUILayout.LabelField("Yallie",TSConstants.Styles.sectionTitleCenter);
			EditorGUILayout.LabelField("I have no idea who he is, but he has a github with an unzip script that made the beta update stream possible, so props to him.",TSConstants.Styles.multilineLabelCenter);
			if (GUILayout.Button("https://github.com/yallie/unzip",TSConstants.Styles.multilineLabelCenter))
        	{
            	Application.OpenURL("https://github.com/yallie/unzip");
			}

			GUILayout.Space(20);
			EditorGUILayout.LabelField("And at the end, thanks to you, who are ultimately using my shader for doing cool stuff, you and everyone who's using it is the reason to why i continue to (kinda slowly) develop this shader.",TSConstants.Styles.multilineLabel);			
		}

		/// <summary>
		/// Used here to manually refresh the ui if in the updater section, needed to make sure that the progress bar actually updates properly
		/// </summary>
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

	/// <summary>
	/// Class used for the settings Json file for when using JsonUtility
	/// </summary>
	[Serializable]
	public class TSSettings
	{
		public int sectionStyle;
		public Color sectionColor;
		public bool disableUpdates;
	}
}

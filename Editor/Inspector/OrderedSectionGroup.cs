using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cibbi.ToonyStandard
{
    
    public class OrderedSectionGroup
    {
        private List<OrderedSection> sections;
        private Color sectionBgColor;
        SectionStyle sectionStyle;
        GUIStyle buttonStyle;

        /// <summary>
        /// Default constructor
        /// </summary>
        public OrderedSectionGroup()
        {
            TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.SettingsJSONPath));
            sectionStyle=(SectionStyle)settings.sectionStyle;
			switch(sectionStyle)
            {
                case SectionStyle.Bubbles:
                    buttonStyle="button";
                    break;
                case SectionStyle.Foldout:
                    buttonStyle=new GUIStyle("button");
                    break;
                case SectionStyle.Box:
                    buttonStyle=new GUIStyle("box");
                    buttonStyle.alignment=TextAnchor.MiddleCenter;
                    buttonStyle.stretchWidth=true;
                    buttonStyle.normal.textColor=Color.white;
                    buttonStyle.fontStyle=FontStyle.Bold;
                    break;
            }
            
			sectionBgColor=settings.sectionColor;
            sections=new List<OrderedSection>();
        }

        /// <summary>
        /// adds a new Section to the list
        /// </summary>
        /// <param name="section"></param>
        public void addSection(OrderedSection section)
        {
            sections.Add(section);
        }

        /// <summary>
        /// Reorders the section list
        /// </summary>
        public void ReorderSections()
        {
            sections.Sort(CompareSectionsOrder);
            int i=1;
            foreach (OrderedSection section in sections)
            {
                if(section.getIndexNumber()!=0  &&  !section.isIndexMixed())
                {
                    section.setIndexNumber(i);
                    i++;
                }
            }
        }

        /// <summary>
        /// Draws the list of sections
        /// </summary>
        /// <param name="materialEditor">Material editor provided by the material inspector window</param>
        public void DrawSectionsList(MaterialEditor materialEditor)
        {
            ReorderSections();

            foreach (OrderedSection section in sections)
            {
                if(!HasMixedIndexZero(section))
                {
                    section.DrawSection(materialEditor);
                }
            }

            if(sectionStyle==SectionStyle.Foldout)
            {
                TSFunctions.DrawLine(new Color(0.35f,0.35f,0.35f,1),1,0);
                GUILayout.Space(10);
            }
        }

        /// <summary>
        /// Draws the add button if there are still sections that can be enabled
        /// </summary>
        public void DrawAddButton()
        {
            if(ListHasMixedIndexZero(sections))
            {
                
                Color bCol = GUI.backgroundColor;
                GUI.backgroundColor = sectionBgColor;
                bool buttonPressed;
                if(sectionStyle==SectionStyle.Foldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    buttonPressed=GUILayout.Button("+",buttonStyle,GUILayout.MinWidth(200));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    buttonPressed=GUILayout.Button("+",buttonStyle);
                }
               
                
                if (buttonPressed)
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (OrderedSection section in sections)
                    {
                        if(HasMixedIndexZero(section))
                        {
                            menu.AddItem(section.getSectionTitle(), false, TurnOnSection, section);
                        }
                    }
                   menu.ShowAsContext();
                }
                GUI.backgroundColor = bCol;
            }
        }

        /// <summary>
        /// Turns on a section, setting it's index to the best number
        /// </summary>
        /// <param name="sectionVariable">The section to turn on</param>
        public void TurnOnSection(object sectionVariable)
        {
            OrderedSection section = (OrderedSection)sectionVariable;
            section.setIndexNumber(753);
        }
        
        /// <summary>
        /// Compares 2 ordered section to determine which one is the first one
        /// </summary>
        /// <param name="x">First section to compare</param>
        /// <param name="y">Second section to compare</param>
        /// <returns></returns>
        private static int CompareSectionsOrder(OrderedSection x, OrderedSection y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    //
                    if (x.getIndexNumber()>y.getIndexNumber())
                    {
                        return 1;
                    }
                    else if (x.getIndexNumber()<y.getIndexNumber())
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Checks a list of ordered sections to see if at least one section in any material has an index value of 0, meaning is not active
        /// </summary>
        /// <param name="sections">List of ordered sections to check</param>
        /// <returns>True if theres at least one section in any material that is not active, false otherwise</returns>
        private static bool ListHasMixedIndexZero(List<OrderedSection> sections)
        {
            bool zero = false;
            foreach(OrderedSection section in sections)
            {
                zero=HasMixedIndexZero(section);
                if(zero)
                {
                    break;
                }
            }
            return zero;
        }

        /// <summary>
        /// Checks if a section disabled on at least one of the selected materials
        /// </summary>
        /// <param name="section">Section to check</param>
        /// <returns>True if the section is disabled on at least one material</returns>
        private static bool HasMixedIndexZero(OrderedSection section)
        {
            bool zero = false;
            foreach (Material mat in section.getIndexTarget())
            {
                zero = mat.GetFloat(section.getIndexName())==0;
                if(zero)
                {
                    break;
                }
            }
            return zero;
        }
    }
}
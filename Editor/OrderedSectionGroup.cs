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

        public OrderedSectionGroup()
        {
            TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.settingsJSONPath));
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

        public void addSection(OrderedSection section)
        {
            sections.Add(section);
        }

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

        public void DrawSectionsList(MaterialEditor materialEditor)
        {
            foreach (OrderedSection section in sections)
            {
                if(!HasMixedIndexZero(section))
                {
                    section.DrawSection(materialEditor);
                }
            }
        }

        public void DrawAddButton()
        {
            if(ListHasMixedIndexZero(sections))
            {
                if(sectionStyle==SectionStyle.Foldout)
                {
                    TSFunctions.DrawLine(new Color(0.35f,0.35f,0.35f,1),1,0);
                    GUILayout.Space(10);
                }
                Color bCol = GUI.backgroundColor;
                GUI.backgroundColor = sectionBgColor;
                if (GUILayout.Button("+",buttonStyle))
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

        public void TurnOnSection(object sectionVariable)
        {
            OrderedSection section = (OrderedSection)sectionVariable;
            section.setIndexNumber(753);
        }

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
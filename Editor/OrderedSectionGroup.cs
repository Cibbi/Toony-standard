using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Cibbi.ToonyStandard
{
    
    public class OrderedSectionGroup
    {
        private List<OrderedSection> sections;

        public OrderedSectionGroup()
        {
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
            if (GUILayout.Button("+"))
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
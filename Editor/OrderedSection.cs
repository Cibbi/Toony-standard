using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public delegate BoxParameters IndexNumber();
    public struct BoxParameters
    {
        public MaterialProperty box;
        public MaterialProperty index;

        public BoxParameters(MaterialProperty box, MaterialProperty index)
        {
            this.box=box;
            this.index=index;
        }
    }
    public class OrderedSection : Section
    {
        private IndexNumber indexNumber;

        public OrderedSection(GUIContent sectionTitle,  SectionContent content, ChangesCheck changesCheck, IndexNumber indexNumber) : base(sectionTitle, indexNumber().box.floatValue!=0, content, changesCheck)
        {
            this.indexNumber = indexNumber;
        }

        public System.Object[] getIndexTarget()
        {
            return indexNumber().index.targets;
        }

        public string getIndexName()
        {
            return indexNumber().index.name;
        }

        public int getIndexNumber()
        {
            return (int)indexNumber().index.floatValue;
        }

        public void setIndexNumber(int index)
        {
            indexNumber().index.floatValue = index;
        }
        public bool isIndexMixed()
        {
            return indexNumber().index.hasMixedValue;
        }


        public bool isBoxMixed()
        {
            return indexNumber().box.hasMixedValue;
        }

    }
}
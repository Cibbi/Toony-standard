using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public delegate BoxParameters IndexNumber();
    /// <summary>
    /// Parameters needed by an ordered section packed into a single structure
    /// </summary>
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

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="sectionTitle">Title of the section</param>
        /// <param name="content">Delegate function for drawing the section content</param>
        /// <param name="changesCheck">Delegate fucntion for checks that need to be done knowing if the box is open or enabled at all</param>
        /// <param name="indexNumber">Delegate fucntion that returns a BoxParameters object that contains the index and box properties</param>
        /// <returns></returns>
        public OrderedSection(GUIContent sectionTitle,  SectionContent content, ChangesCheck changesCheck, IndexNumber indexNumber) : base(sectionTitle, indexNumber().box.floatValue!=0, content, changesCheck)
        {
            this.indexNumber = indexNumber;
        }

        /// <summary>
        /// Returns all materials targeted by the index property
        /// </summary>
        /// <returns>An array containing all materials targeted by the index property</returns>
        public System.Object[] getIndexTarget()
        {
            return indexNumber().index.targets;
        }

        /// <summary>
        /// Returns the index property name
        /// </summary>
        /// <returns>The index property name</returns>
        public string getIndexName()
        {
            return indexNumber().index.name;
        }

        /// <summary>
        /// Return the index float value
        /// </summary>
        /// <returns>The index float value</returns>
        public int getIndexNumber()
        {
            return (int)indexNumber().index.floatValue;
        }

        /// <summary>
        /// Sets the index value
        /// </summary>
        /// <param name="index">Value to set</param>
        public void setIndexNumber(int index)
        {
            indexNumber().index.floatValue = index;
        }
        /// <summary>
        /// Checks if the index value has mixed values
        /// </summary>
        /// <returns>True if it has mixed value, false otherwise</returns>
        public bool isIndexMixed()
        {
            return indexNumber().index.hasMixedValue;
        }

        /// <summary>
        /// Checks if the box value has mixed values
        /// </summary>
        /// <returns>True if it has mixed value, false otherwise</returns>
        public bool isBoxMixed()
        {
            return indexNumber().box.hasMixedValue;
        }

    }
}
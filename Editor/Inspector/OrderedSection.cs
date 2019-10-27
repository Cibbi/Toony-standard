using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
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
    public abstract class OrderedSection : Section
    {
        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="sectionTitle">Title of the section</param>
        /// <param name="open">Is the section expanded?</param>
        /// <param name="enabled">IS the section enabled?</param>
        /// <returns></returns>
        public OrderedSection(GUIContent sectionTitle, bool open, bool enabled) : base(sectionTitle, open)
        {
            this.isEnabled = enabled;
        }

        public virtual void OnAdd()
        {

        }

        protected abstract MaterialProperty GetIndex();

        protected abstract MaterialProperty GetBox();

        /// <summary>
        /// Returns all materials targeted by the index property
        /// </summary>
        /// <returns>An array containing all materials targeted by the index property</returns>
        public System.Object[] GetIndexTargets()
        {
            return GetIndex().targets;
        }

        /// <summary>
        /// Returns the index property name
        /// </summary>
        /// <returns>The index property name</returns>
        public string GetIndexName()
        {
            return GetIndex().name;
        }

        /// <summary>
        /// Return the index float value
        /// </summary>
        /// <returns>The index float value</returns>
        public int GetIndexNumber()
        {
            return (int)GetIndex().floatValue;
        }

        /// <summary>
        /// Sets the index value
        /// </summary>
        /// <param name="index">Value to set</param>
        public void SetIndexNumber(int index)
        {
           GetIndex().floatValue = index;
        }
        /// <summary>
        /// Checks if the index value has mixed values
        /// </summary>
        /// <returns>True if it has mixed value, false otherwise</returns>
        public bool IsIndexMixed()
        {
            return GetIndex().hasMixedValue;
        }

        /// <summary>
        /// Checks if the box value has mixed values
        /// </summary>
        /// <returns>True if it has mixed value, false otherwise</returns>
        public bool IsBoxMixed()
        {
            return GetBox().hasMixedValue;
        }
    }
}
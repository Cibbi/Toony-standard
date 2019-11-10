using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Cibbi.ToonyStandard
{
    public class GradientTexture {

        public enum BlendMode
        {
            Linear,
            Fixed
        }

        [SerializeField]
        public List<ColorKey> keys = new List<ColorKey>();

        private Texture2D texture;

        public BlendMode blendMode;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">with of the result gradient texture</param>
        public GradientTexture(int width)
        {
            blendMode = BlendMode.Linear;
            keys.Add(new ColorKey(Color.black,0));
            keys.Add(new ColorKey(Color.white,1));

            texture = new Texture2D(width, 1,TextureFormat.RGB24, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            UpdateTexture();
            
        }

        /// <summary>
        /// Returns a color at the specified time
        /// </summary>
        /// <param name="time">Time of the color to sample</param>
        /// <returns></returns>
        public Color Evaluate(float time)
        {
            ColorKey keyLeft = keys[0];
            ColorKey keyRight = keys[keys.Count - 1];

            for (int i = 0; i < keys.Count; i++)
            {
                if(keys[i].Time <= time)
                {
                    keyLeft = keys[i];
                    
                }
                if(keys[i].Time >= time)
                {
                    keyRight = keys[i];
                    break;
                }
            }
            
            if(blendMode == BlendMode.Linear)
            {
                float blendTime = Mathf.InverseLerp(keyLeft.Time, keyRight.Time, time);
                return Color.Lerp(keyLeft.Color, keyRight.Color, blendTime);
            }
            else
            {
                return  keyRight.Color;
            }
        }

        /// <summary>
        /// Adds a new key, and removes any key that is in the same time
        /// </summary>
        /// <param name="color">Color of the key</param>
        /// <param name="time">Time of the key</param>
        /// <returns>The key index</returns>
        public int AddKey(Color color, float time)
        {
            return AddKey(color, time, true);
        }
        //internal version that has an additional skippable check for deleting a key that is in the same time of the new one
        private int AddKey(Color color, float time, bool shouldDelete)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (time<keys[i].Time)
                {
                    keys.Insert(i, new ColorKey(color, time));
                    UpdateTexture();
                    return i;
                }
                else if (time == keys[i].Time && shouldDelete)
                {
                    keys[i] = new ColorKey(color, time);
                    UpdateTexture();
                    return -1;
                }
            }
            keys.Add(new ColorKey(color, time));
            UpdateTexture();
            return keys.Count-1;
        }

        /// <summary>
        /// Removes a key at the selected index
        /// </summary>
        /// <param name="index">Index of the key to remove</param>
        public void RemoveKey(int index)
        {
            RemoveKey(index, true);
        }

        //private version with an additional check to decide if removing it with only one key left, is just for updating key time
        //correctly when there's only one key in the list
        private void RemoveKey(int index, bool checkMin)
        {
            if(keys.Count > 1 && checkMin)
            {
                keys.RemoveAt(index);
            }
            else if(!checkMin)
            {
                keys.RemoveAt(index);
            }
            UpdateTexture();
        }

        /// <summary>
        /// Updates the key time position
        /// </summary>
        /// <param name="index">Index of the key to update</param>
        /// <param name="time">New time</param>
        /// <returns>The new index of the key</returns>
        public int UpdateKeyTime(int index, float time)
        {
            if(time < 0)
            {
                time = 0;
            }
            else if (time > 1)
            {
                time = 1;
            }

            if(index<0) index = 0;

            Color col = keys[index].Color;
            RemoveKey(index, false);
            return AddKey(col, time, false);

        }

        /// <summary>
        /// Updates the key color
        /// </summary>
        /// <param name="index">Index of the key</param>
        /// <param name="col">Color of the key</param>
        public void UpdateKeyColor(int index, Color col)
        {
            keys[index] = new ColorKey(col, keys[index].Time); 
            UpdateTexture();
        }

        /// <summary>
        /// Texture of the gradient
        /// </summary>
        /// <returns>Texture of the gradient</returns>
        public Texture2D GetTexture()
        {
            return texture;
        }

        public void UpdateTextureWidth(int width)
        {
            texture = new Texture2D(width, 1,TextureFormat.RGB24, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            UpdateTexture();
        }

        /// <summary>
        /// Updates the internal gradient Texture
        /// </summary>
        public void UpdateTexture()
        {
            Color [] colors = new Color[texture.width];
            for(int i = 0; i<texture.width; i++)
            {
                colors[i] = Evaluate((float)i / (texture.width-1));
            }
            texture.SetPixels(colors);
            //texture.Apply();
            texture.Apply(true);
        }

        //key struct
        [System.Serializable]
        public struct ColorKey
        {
            [SerializeField]
            public Color Color;
            [SerializeField]
            public float Time;

            public ColorKey(Color color, float time)
            {
                Color = color;
                Time = time;
            }
        }
    }
}
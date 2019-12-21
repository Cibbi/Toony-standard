using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Cibbi.ToonyStandard
{
    public class GradientEditor
    {

        /// <summary>
        /// Supported gradient widths
        /// </summary>
        public enum RampWidth
        {
            XS_128 = 128,
            S_256 = 256,
            M_512 = 512,
            L_1024 = 1024,
            XL_2048 = 2048,
            XXL_4096 = 4096
        }

        GradientTexture gradient;

        Rect gradientPreviewRect;
        Rect keySelectionAreaRect;
        Rect[] keyRects;
        bool mouseIsDownOverKey;
        int selectedKeyIndex;

        RampWidth rampWidth;
        GradientTexture.BlendMode blendMode;

        bool repaint;

        /// <summary>
        /// Constructor
        /// </summary>
        public GradientEditor()
        {
            gradient = new GradientTexture(1024);
            rampWidth = RampWidth.L_1024;
            blendMode = GradientTexture.BlendMode.Linear;
        }

        /// <summary>
        /// Draws the gui related
        /// </summary>
        /// <returns>True if there's a need to call the Repaint() method, false otherwise</returns>
        public bool DrawGUI()
        {
            repaint = false;
            Rect windowRect = GUILayoutUtility.GetRect(100, 1000, 60, 60);

            gradientPreviewRect = new Rect(windowRect.x + 10, windowRect.y + 10, windowRect.width - 20, 25);
            keySelectionAreaRect = new Rect(gradientPreviewRect.x - 10, gradientPreviewRect.yMax, gradientPreviewRect.width + 20, 25);
            GUI.DrawTexture(gradientPreviewRect, gradient.GetTexture());
            if (keyRects == null || keyRects.Length != gradient.keys.Count)
            {
                keyRects = new Rect[gradient.keys.Count];
            }
            Rect selectedRect = Rect.zero;
            for (int i = 0; i < gradient.keys.Count; i++)
            {
                Rect keyRect = new Rect(gradientPreviewRect.x + gradientPreviewRect.width * gradient.keys[i].Time - 10, gradientPreviewRect.yMax, 20, 25);
                keyRects[i] = keyRect;
                if (i == selectedKeyIndex)
                {
                    selectedRect = keyRect;
                }
                else
                {
                    GUI.DrawTexture(keyRect, TSConstants.UpColor);
                    GUI.DrawTexture(keyRect, TSConstants.UpColorInternal, ScaleMode.ScaleToFit, true, 0, gradient.keys[i].Color, 0, 0);
                }

            }
            if (selectedRect != Rect.zero)
            {
                GUI.DrawTexture(selectedRect, TSConstants.UpColorSelected);
                GUI.DrawTexture(selectedRect, TSConstants.UpColorInternal, ScaleMode.ScaleToFit, true, 0, gradient.keys[selectedKeyIndex].Color, 0, 0);
            }

            Color col = EditorGUILayout.ColorField("Color", gradient.keys[selectedKeyIndex].Color);
            if (!col.Equals(gradient.keys[selectedKeyIndex].Color))
            {
                gradient.UpdateKeyColor(selectedKeyIndex, col);
            }

            float time = EditorGUILayout.FloatField("Location", gradient.keys[selectedKeyIndex].Time);
            if (time != gradient.keys[selectedKeyIndex].Time)
            {
                gradient.UpdateKeyTime(selectedKeyIndex, time);
            }

            rampWidth = (RampWidth)EditorGUILayout.EnumPopup("Ramp size", rampWidth);
            if ((int)rampWidth != gradient.GetTexture().width)
            {
                gradient.UpdateTextureWidth((int)rampWidth);
            }

            blendMode = (GradientTexture.BlendMode)EditorGUILayout.EnumPopup("Blend mode", blendMode);
            if (blendMode != gradient.blendMode)
            {
                gradient.blendMode = blendMode;
                gradient.UpdateTexture();
            }

            HandleEvents();
            EditorGUILayout.Space();
            return repaint;
        }

        /// <summary>
        /// Handles mouse and keyboard events
        /// </summary>
        private void HandleEvents()
        {
            Event guiEvent = Event.current;
            // Check when left mouse down
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                // Check if selecting a keyframe
                for (int i = 0; i < keyRects.Length; i++)
                {
                    if (keyRects[i].Contains(guiEvent.mousePosition))
                    {
                        mouseIsDownOverKey = true;
                        selectedKeyIndex = i;
                        repaint = true;
                        GUI.FocusControl(null);
                        break;
                    }
                }
                // Creates a new keyframe if not selected one
                if (!mouseIsDownOverKey && keySelectionAreaRect.Contains(guiEvent.mousePosition))
                {
                    float keytime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                    selectedKeyIndex = gradient.AddKey(gradient.Evaluate(keytime), keytime);
                    mouseIsDownOverKey = true;
                    repaint = true;
                }
            }

            // Check left mouse up
            if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
            {
                mouseIsDownOverKey = false;
            }

            // Check if mouse is dragging
            if (mouseIsDownOverKey && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
            {
                float keytime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                selectedKeyIndex = gradient.UpdateKeyTime(selectedKeyIndex, keytime);
                repaint = true;
            }
            // Check if using the delete key
            if (guiEvent.keyCode == KeyCode.Delete && guiEvent.type == EventType.KeyDown)
            {
                gradient.RemoveKey(selectedKeyIndex);
                if (selectedKeyIndex >= gradient.keys.Count)
                {
                    selectedKeyIndex = gradient.keys.Count - 1;
                }
                repaint = true;
            }
        }

        /// <summary>
        /// Save the gradient at a specified path
        /// </summary>
        /// <param name="path">Destination path (including name and extention)</param>
        /// <returns>The saved texture</returns>
        public Texture2D SaveGradient(string path)
        {
            byte[] bytes;
            bytes = gradient.GetTexture().EncodeToPNG();

            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            path = path.Substring(path.LastIndexOf("Assets"));
            TextureImporter t = AssetImporter.GetAtPath(path) as TextureImporter;
            t.wrapMode = TextureWrapMode.Clamp;
            t.isReadable = true;
            AssetDatabase.ImportAsset(path);
            Texture2D res = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return res;
        }

        /// <summary>
        /// Get the currently runtime generated texture
        /// </summary>
        /// <returns>Current runtime generated texture</returns>
        public Texture2D GetGradientTexture()
        {
            return gradient.GetTexture();
        }

        public void TranslateTextureToGradient(Texture2D texture)
        {
#if UNITY_2018_1_OR_NEWER
            if (!texture.isReadable)
            {
                texture = TSFunctions.SetTextureImporterFormat(texture, true);
            }
#else
            texture = TSFunctions.SetTextureImporterFormat(texture, true);
#endif
            gradient = new GradientTexture((int)rampWidth);
            blendMode = GradientTexture.BlendMode.Linear;
            int sampleWidth = Mathf.Max(texture.width, texture.height);
            Color[] colors = new Color[sampleWidth];

            for (int i = 0; i < sampleWidth; i++)
            {
                colors[i] = texture.GetPixel(texture.width * i / sampleWidth, texture.height * i / sampleWidth);
                //Debug.Log(colors[i]);
            }
            Color[] delta = GetDelta(colors);
            int deltaVariance = 0;
            for (int i = 0; i < delta.Length; i++)
            {
                if (delta[i].r != 0 || delta[i].g != 0 || delta[i].b != 0)
                {
                    deltaVariance++;
                }
                //Debug.Log("color " + colors[i]);
                //Debug.Log("delta " + delta[i]);
            }
            //Debug.Log(deltaVariance);
            //Debug.Log(Mathf.Max(texture.width, texture.height) * 0.1);
            if (deltaVariance < Mathf.Max(texture.width, texture.height) * 0.1)
            {
                blendMode = GradientTexture.BlendMode.Fixed;
                gradient.blendMode = blendMode;
            }
            delta[0] = delta[1];

            Color[] deltaSquared = GetDelta(delta);
            List<GradientTexture.ColorKey> KeyChanged = new List<GradientTexture.ColorKey>();
            List<Color> deltaChanged = new List<Color>();
            KeyChanged.Add(new GradientTexture.ColorKey(colors[0], 0));
            deltaChanged.Add(deltaSquared[0]);
            for (int i = 1; i < sampleWidth - 1; i++)
            {
                //Debug.Log("color " + colors[i]);
                //Debug.Log("delta " + delta[i]);
                //Debug.Log("deltaSquared " + deltaSquared[i]);
                if (Mathf.Abs(deltaSquared[i].r) > 0.002 || Mathf.Abs(deltaSquared[i].g) > 0.002 || Mathf.Abs(deltaSquared[i].b) > 0.002)
                {
                    if (blendMode == GradientTexture.BlendMode.Fixed)
                    {
                        KeyChanged.Add(new GradientTexture.ColorKey(colors[i - 1], i / ((float)sampleWidth - 1)));
                        deltaChanged.Add(deltaSquared[i]);
                    }
                    else
                    {
                        KeyChanged.Add(new GradientTexture.ColorKey(colors[i], i / ((float)sampleWidth - 1)));
                        deltaChanged.Add(deltaSquared[i]);
                        //Debug.Log(colors[i] + "-" + (i / (float)sampleWidth));
                    }

                }
            }
            KeyChanged.Add(new GradientTexture.ColorKey(colors[(int)sampleWidth - 1], 1));
            deltaChanged.Add(deltaSquared[(int)sampleWidth - 1]);
            List<GradientTexture.ColorKey> finalKeys = new List<GradientTexture.ColorKey>();
            finalKeys.Add(KeyChanged[0]);
            Color lastDelta = new Color(0, 0, 0, 0);
            if (blendMode == GradientTexture.BlendMode.Fixed)
            {
                for (int i = 1; i < KeyChanged.Count; i++)
                {
                    //float deltaVarianceSum = Mathf.Abs(getDeltaVariance(deltaChanged[i]) - getDeltaVariance(lastDelta));
                    if ((KeyChanged[i].Time - finalKeys[finalKeys.Count - 1].Time) < 0.05)
                    {
                        /*if (getDeltaVariance(deltaChanged[i]) > getDeltaVariance(lastDelta))
                        {
                            finalKeys.RemoveAt(finalKeys.Count - 1);
                            finalKeys.Add(KeyChanged[i]);
                            lastDelta = deltaChanged[i];
                        }*/
                    }
                    else
                    {
                        finalKeys.Add(KeyChanged[i]);
                        lastDelta = deltaChanged[i];
                    }
                }
            }
            else
            {
                for (int i = 1; i < KeyChanged.Count; i++)
                {
                    float deltaVarianceSum = Mathf.Abs(getDeltaVariance(deltaChanged[i]) - getDeltaVariance(lastDelta));
                    if ((KeyChanged[i].Time - finalKeys[finalKeys.Count - 1].Time) + deltaVarianceSum > 0.1 && deltaVarianceSum > 0.002)
                    {
                        finalKeys.Add(KeyChanged[i]);
                        lastDelta = deltaChanged[i];

                    }
                }
                if (finalKeys[finalKeys.Count - 1].Time != 1)
                {
                    finalKeys.Add(KeyChanged[KeyChanged.Count - 1]);
                }
            }

            gradient.keys = finalKeys;
            gradient.UpdateTexture();
        }

        private Color[] GetDelta(Color[] colors)
        {
            Color[] delta = new Color[colors.Length];
            delta[0] = new Color(0, 0, 0);
            for (int i = 1; i < colors.Length; i++)
            {
                delta[i].r = colors[i - 1].r - colors[i].r;
                delta[i].g = colors[i - 1].g - colors[i].g;
                delta[i].b = colors[i - 1].b - colors[i].b;
                //Debug.Log(delta[i]);
            }

            return delta;
        }

        private float getDeltaVariance(Color delta)
        {
            return Mathf.Max(new float[] { Mathf.Abs(delta.r), Mathf.Abs(delta.g), Mathf.Abs(delta.b) });
        }
    }

}
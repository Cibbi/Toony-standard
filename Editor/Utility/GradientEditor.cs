using UnityEngine;
using UnityEditor;
namespace Cibbi.ToonyStandard
{
    public class GradientEditor {

        public enum RampWidth
        {
            XS_128   = 128,
            S_256    = 256,
            M_512    = 512,
            L_1024   = 1024,
            XL_2048  = 2048,
            XXL_4096 = 4096
        }

        GradientTexture gradient;

        Rect gradientPreviewRect;
        Rect keySelectionAreaRect;
        Rect [] keyRects;
        bool mouseIsDownOverKey;
        int selectedKeyIndex;

        RampWidth rampWidth;
        GradientTexture.BlendMode blendMode;

        bool repaint;

        public GradientEditor()
        {
            gradient = new GradientTexture(1024);
            rampWidth = RampWidth.L_1024;
            blendMode = GradientTexture.BlendMode.Linear;
        }

        public bool DrawGUI()
        {
            //EditorGUILayout.LabelField("Gradient");
            repaint = false;
            Rect windowRect = GUILayoutUtility.GetRect(100,1000,60,60);

            gradientPreviewRect = new Rect(windowRect.x + 10, windowRect.y + 10, windowRect.width - 20, 25);
            keySelectionAreaRect = new Rect(gradientPreviewRect.x-10, gradientPreviewRect.yMax, gradientPreviewRect.width+20,25);
           //EditorGUI.DrawRect(keySelectionAreaRect, Color.white);
            GUI.DrawTexture(gradientPreviewRect, gradient.GetTexture());
            if(keyRects == null || keyRects.Length != gradient.keys.Count)
            {
                keyRects = new Rect[gradient.keys.Count];
            }
            Rect selectedRect = Rect.zero;
            for (int i = 0; i < gradient.keys.Count; i++)
            {
                Rect keyRect = new Rect(gradientPreviewRect.x + gradientPreviewRect.width * gradient.keys[i].Time - 10, gradientPreviewRect.yMax, 20,25);
                keyRects[i] = keyRect;
                //EditorGUI.DrawRect(keyRect, gradient.keys[i].Color);
                if(i == selectedKeyIndex)
                {
                    selectedRect = keyRect;
                }
                else
                {
                    GUI.DrawTexture(keyRect,TSConstants.UpColor);
                    GUI.DrawTexture(keyRect,TSConstants.UpColorInternal,ScaleMode.ScaleToFit,true,0,gradient.keys[i].Color,0,0);
                }      
                
            }
            if(selectedRect != Rect.zero)
            {
                GUI.DrawTexture(selectedRect,TSConstants.UpColorSelected);
                GUI.DrawTexture(selectedRect,TSConstants.UpColorInternal,ScaleMode.ScaleToFit,true,0,gradient.keys[selectedKeyIndex].Color,0,0);
            } 

            //Rect settingsRect = new Rect(windowRect.x + 10, gradientPreviewRect.yMax + 25, windowRect.width - 20, 100);
            //EditorGUI.DrawRect(settingsRect, Color.white);
            //GUILayout.BeginArea(settingsRect);
                
                Color col = EditorGUILayout.ColorField("Color",gradient.keys[selectedKeyIndex].Color);
                if(!col.Equals(gradient.keys[selectedKeyIndex].Color))
                {
                    gradient.UpdateKeyColor(selectedKeyIndex, col);
                }

                float time = EditorGUILayout.FloatField("Location", gradient.keys[selectedKeyIndex].Time);
                if(time != gradient.keys[selectedKeyIndex].Time)
                {
                    gradient.UpdateKeyTime(selectedKeyIndex,time);
                }

                rampWidth = (RampWidth)EditorGUILayout.EnumPopup("Ramp size", rampWidth);
                if((int)rampWidth != gradient.GetTexture().width)
                {
                    gradient.UpdateTextureWidth((int)rampWidth);
                }

                blendMode = (GradientTexture.BlendMode)EditorGUILayout.EnumPopup("Blend mode", blendMode);
                if(blendMode != gradient.blendMode)
                {
                    gradient.blendMode = blendMode;
                    gradient.UpdateTexture();
                }

            //GUILayout.EndArea();

            HandleEvents();
            //GUILayout.EndArea();
            EditorGUILayout.Space();
            return repaint;
        }

        private void HandleEvents()
        {
            Event guiEvent = Event.current;
            if(guiEvent.type == EventType.MouseDown && guiEvent.button ==0)
            {
                for (int i = 0; i < keyRects.Length; i++)
                {
                    if(keyRects[i].Contains(guiEvent.mousePosition))
                    {
                        mouseIsDownOverKey = true;
                        selectedKeyIndex = i;
                        repaint = true;
                        GUI.FocusControl(null);
                        break;
                    }
                }

                if(!mouseIsDownOverKey && keySelectionAreaRect.Contains(guiEvent.mousePosition))
                {
                    float keytime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                    selectedKeyIndex = gradient.AddKey(gradient.Evaluate(keytime), keytime);
                    mouseIsDownOverKey = true;
                    repaint = true;
                }
            }

            if(guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
            {
                mouseIsDownOverKey = false;
            }

            if(mouseIsDownOverKey && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
            {
                float keytime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                selectedKeyIndex = gradient.UpdateKeyTime(selectedKeyIndex,keytime);
                repaint = true;
            }

            if(guiEvent.keyCode == KeyCode.Delete && guiEvent.type == EventType.KeyDown)
            {
                gradient.RemoveKey(selectedKeyIndex);
                if(selectedKeyIndex >= gradient.keys.Count)
                {
                    selectedKeyIndex = gradient.keys.Count-1;
                }
                repaint = true;
            }
        }

        public Texture2D SaveGradient(string path)
        {
            byte[] bytes;
            bytes = gradient.GetTexture().EncodeToPNG();
            
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            TextureImporter t = AssetImporter.GetAtPath(path) as TextureImporter;
            t.wrapMode = TextureWrapMode.Clamp;
            t.isReadable = true;
            AssetDatabase.ImportAsset(path);
            Texture2D res = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return res;
        }
        public Texture2D GetGradientTexture()
        {
            return gradient.GetTexture();
        }
    }

}
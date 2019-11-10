using UnityEngine;
using UnityEditor;

namespace Cibbi.ToonyStandard
{	
    public class TexturePacker
    {
        public enum DefaultTexture
        {
            DefaultWhite,
            DefaultBlack
        }
        public enum Resolution
        {
            XS_128x128    = 128,
            S_256x256     = 256,
            M_512x512     = 512,
            L_1024x1024   = 1024,
            XL_2048x2048  = 2048,
            XXL_4096x4096 = 4096
        }

        public ComputeShader compute;
        public RenderTexture result;
        public Resolution resolution;

        public Texture2D rTexture;
        public Texture2D gTexture;
        public Texture2D bTexture;
        public Texture2D aTexture;

        public DefaultTexture rDefault;
        public DefaultTexture gDefault;
        public DefaultTexture bDefault;
        public DefaultTexture aDefault;

        public int rChannel;
        public int gChannel;
        public int bChannel;
        public int aChannel;

        public bool rReverse;
        public bool gReverse;
        public bool bReverse;
        public bool aReverse;

        public float rNeedsGamma;
        public float gNeedsGamma;
        public float bNeedsGamma;
        public float aNeedsGamma;
        Texture2D tex;
        public Texture2D resultTex;

        private string defaultPath;

        int kernel;

        public TexturePacker(Resolution res, string defaultPath)
        {
            resolution = res;
            result = new RenderTexture((int)resolution, (int)resolution,32,RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            result.enableRandomWrite = true;
            result.Create();

            compute = TSConstants.PackerCompute;

            tex = new Texture2D(result.width, result.height, TextureFormat.RGBA32, false);
            this.defaultPath = defaultPath;
        }

        public int RiseResolutionByOneLevel()
        {
            switch (resolution)
            {
                case Resolution.XS_128x128:
                    resolution = Resolution.S_256x256;
                    return 1;
                case Resolution.S_256x256:
                    resolution = Resolution.M_512x512;
                    return 1;
                case Resolution.M_512x512:
                    resolution = Resolution.L_1024x1024;
                    return 1;
                case Resolution.L_1024x1024:
                    resolution = Resolution.XL_2048x2048;
                    return 1;
                case Resolution.XL_2048x2048:
                    resolution = Resolution.XXL_4096x4096;
                    return 1;
                case Resolution.XXL_4096x4096:
                    break;
            }
            return 0;
        }

        public void PackTexture(string path)
        {
            if(result.width!=(int)resolution||result.height!=(int)resolution)
            {
                result = new RenderTexture((int)resolution, (int)resolution,32,RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                result.enableRandomWrite = true;
                result.Create();
                tex = new Texture2D(result.width, result.height, TextureFormat.RGBA32, false);
            }

            rNeedsGamma = NeedsGamma(rTexture);
            gNeedsGamma = NeedsGamma(gTexture);
            bNeedsGamma = NeedsGamma(bTexture);
            aNeedsGamma = NeedsGamma(aTexture);

            kernel = compute.FindKernel("PackTexture");

            compute.SetTexture(kernel, "Result", result);

            SetTextureWithDefault("RChannel", "rWidth", "rHeight", rTexture, rDefault);
            compute.SetFloat("rSelectedChannel", rChannel);
            compute.SetFloat("rGamma", rNeedsGamma);
            compute.SetFloat("rReverse", rReverse?1:0);

            SetTextureWithDefault("GChannel", "gWidth", "gHeight", gTexture, gDefault);
            compute.SetFloat("gSelectedChannel", gChannel);
            compute.SetFloat("gGamma", gNeedsGamma);
            compute.SetFloat("gReverse", gReverse?1:0);

            SetTextureWithDefault("BChannel", "bWidth", "bHeight", bTexture, bDefault);
            compute.SetFloat("bSelectedChannel", bChannel);
            compute.SetFloat("bGamma", bNeedsGamma);
            compute.SetFloat("bReverse", bReverse?1:0);
            
            SetTextureWithDefault("AChannel", "aWidth", "aHeight", aTexture, aDefault);
            compute.SetFloat("aSelectedChannel", aChannel);
            compute.SetFloat("aGamma", aNeedsGamma);
            compute.SetFloat("aReverse", aReverse?1:0);

            compute.SetFloat("width", (float)resolution);
            compute.SetFloat("height", (float)resolution);

            compute.Dispatch(kernel, (int)resolution/16,(int)resolution/16,1);
                
            RenderTexture.active = result;
            tex.ReadPixels(new Rect(0, 0, result.width, result.height), 0, 0);
            byte[] bytes;
            bytes = tex.EncodeToPNG();
            
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            resultTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public void DrawGUI()
        {
            EditorGUILayout.Space();
            //compute = (ComputeShader) EditorGUILayout.ObjectField("compute",compute,typeof(ComputeShader),false);
            EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(60);
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(50,16),"Metallic");
                        EditorGUILayout.EndHorizontal();
                        rChannel = GUILayout.Toolbar(rChannel,new string[]{"R","G","B","A"},EditorStyles.toolbarButton);
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(44);
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(50,16),"Reverse");
                            rReverse = EditorGUI.Toggle(GUILayoutUtility.GetRect(16,16),rReverse);
                        EditorGUILayout.EndHorizontal();
                        rDefault = (DefaultTexture) EditorGUILayout.EnumPopup(rDefault,GUILayout.Width(110));
                    EditorGUILayout.EndVertical();
                    rTexture = (Texture2D) EditorGUI.ObjectField(GUILayoutUtility.GetRect(64,64),rTexture,typeof(Texture2D),false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                    gTexture = (Texture2D) EditorGUI.ObjectField(GUILayoutUtility.GetRect(64,64),gTexture,typeof(Texture2D),false);
                    EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(75,16),"Smoothness");
                            GUILayout.Space(35);
                        EditorGUILayout.EndHorizontal();
                        gChannel = GUILayout.Toolbar(gChannel,new string[]{"R","G","B","A"},EditorStyles.toolbarButton);
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(4);
                            gReverse = EditorGUI.Toggle(GUILayoutUtility.GetRect(16,16),gReverse);
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(50,16),"Reverse");
                            GUILayout.Space(40);
                        EditorGUILayout.EndHorizontal();
                        gDefault = (DefaultTexture) EditorGUILayout.EnumPopup(gDefault,GUILayout.Width(110));
                    EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal(); 
                GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(0);
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(110,16),"Ambient Occlusion");
                        EditorGUILayout.EndHorizontal();
                        bChannel = GUILayout.Toolbar(bChannel,new string[]{"R","G","B","A"},EditorStyles.toolbarButton);
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(44);
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(50,16),"Reverse");
                            bReverse = EditorGUI.Toggle(GUILayoutUtility.GetRect(16,16),bReverse);
                        EditorGUILayout.EndHorizontal();
                        bDefault = (DefaultTexture) EditorGUILayout.EnumPopup(bDefault,GUILayout.Width(110));
                    EditorGUILayout.EndVertical();
                    bTexture = (Texture2D) EditorGUI.ObjectField(GUILayoutUtility.GetRect(64,64),bTexture,typeof(Texture2D),false);
                EditorGUILayout.EndHorizontal();
                                
                EditorGUILayout.BeginHorizontal("box");
                    aTexture = (Texture2D) EditorGUI.ObjectField(GUILayoutUtility.GetRect(64,64),aTexture,typeof(Texture2D),false);
                    EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(110,16),"Detail mask");
                            GUILayout.Space(0);
                        EditorGUILayout.EndHorizontal();
                        aChannel = GUILayout.Toolbar(aChannel,new string[]{"R","G","B","A"},EditorStyles.toolbarButton);
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(4);
                            aReverse = EditorGUI.Toggle(GUILayoutUtility.GetRect(16,16),aReverse);
                            EditorGUI.LabelField(GUILayoutUtility.GetRect(50,16),"Reverse");
                            GUILayout.Space(40);
                        EditorGUILayout.EndHorizontal();
                        aDefault = (DefaultTexture) EditorGUILayout.EnumPopup(aDefault,GUILayout.Width(110));
                    EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal(); 
                GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            //result = (RenderTexture) EditorGUILayout.ObjectField("result",result,typeof(RenderTexture),false);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resolution",GUILayout.Width(65));
            resolution = (Resolution) EditorGUILayout.EnumPopup(resolution,GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Pack textures",GUILayout.Width(150)))
            {
                PackTexture(defaultPath);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        
        }

        private void SetTextureWithDefault(string channelName, string widthName, string heightName, Texture2D texture, DefaultTexture defaultTex)
        {
            if(texture == null)
            {
                if(defaultTex == DefaultTexture.DefaultWhite)
                {
                    compute.SetTexture(kernel, channelName, Texture2D.whiteTexture);
                    compute.SetFloat(widthName, Texture2D.whiteTexture.width);
                    compute.SetFloat(heightName, Texture2D.whiteTexture.height);
                }
                else
                {
                    compute.SetTexture(kernel, channelName, Texture2D.blackTexture);
                    compute.SetFloat(widthName, Texture2D.blackTexture.width);
                    compute.SetFloat(heightName, Texture2D.blackTexture.height);
                }
            }
            else
            {
                compute.SetTexture(kernel, channelName, texture);
                compute.SetFloat(widthName, texture.width);
                    compute.SetFloat(heightName, texture.height);
            }
        }

        private float NeedsGamma(Texture2D texture)
        {
            string assetPath = AssetDatabase.GetAssetPath( texture );
                var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
                if ( tImporter != null )
                {
                if(tImporter.sRGBTexture)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
                }
                return 0;
        }
    }
}
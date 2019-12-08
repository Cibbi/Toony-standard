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
            XS_128x128 = 128,
            S_256x256 = 256,
            M_512x512 = 512,
            L_1024x1024 = 1024,
            XL_2048x2048 = 2048,
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

        private string[] names;

        int kernel;

        public bool drawInternalConfirmButton;

        /// <summary>
        /// Constructor of the texture packer
        /// </summary>
        /// <param name="res">Default resolution of the output texture</param>
        /// <param name="textureNames">Names of the 4 textures</param>
        /// <param name="defaultPath">Default output path (with texture name and extention)</param>
        public TexturePacker(Resolution res, string[] textureNames, string defaultPath)
        {
            resolution = res;
            result = new RenderTexture((int)resolution, (int)resolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            result.enableRandomWrite = true;
            result.Create();

            compute = TSConstants.PackerCompute;

            tex = new Texture2D(result.width, result.height, TextureFormat.RGBA32, false);
            names = textureNames;
            if (names.Length > 4)
            {
                string[] oldNames = names;
                names = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    names[i] = oldNames[i];
                }
            }
            else if (names.Length < 4)
            {
                string[] oldNames = names;
                names = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    if (i < oldNames.Length)
                    {
                        names[i] = oldNames[i];
                    }
                    else
                    {
                        names[i] = "Texture";
                    }
                }
            }

            this.defaultPath = defaultPath;
            drawInternalConfirmButton = true;
        }

        /// <summary>
        /// Increases the resolution of the output texture by one level in the ladder of supported resolutions
        /// </summary>
        /// <returns>True if success, false already on max resolution</returns>
        public bool RiseResolutionByOneLevel()
        {
            switch (resolution)
            {
                case Resolution.XS_128x128:
                    resolution = Resolution.S_256x256;
                    return true;
                case Resolution.S_256x256:
                    resolution = Resolution.M_512x512;
                    return true;
                case Resolution.M_512x512:
                    resolution = Resolution.L_1024x1024;
                    return true;
                case Resolution.L_1024x1024:
                    resolution = Resolution.XL_2048x2048;
                    return true;
                case Resolution.XL_2048x2048:
                    resolution = Resolution.XXL_4096x4096;
                    return true;
                case Resolution.XXL_4096x4096:
                    break;
            }
            return false;
        }

        /// <summary>
        /// Generates the texture using the current inputs and saves it into the given path
        /// </summary>
        /// <param name="path">Path of the saved texture (name and extension included</param>
        public void PackTexture(string path)
        {
            if (result.width != (int)resolution || result.height != (int)resolution)
            {
                result = new RenderTexture((int)resolution, (int)resolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
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
            compute.SetFloat("rReverse", rReverse ? 1 : 0);

            SetTextureWithDefault("GChannel", "gWidth", "gHeight", gTexture, gDefault);
            compute.SetFloat("gSelectedChannel", gChannel);
            compute.SetFloat("gGamma", gNeedsGamma);
            compute.SetFloat("gReverse", gReverse ? 1 : 0);

            SetTextureWithDefault("BChannel", "bWidth", "bHeight", bTexture, bDefault);
            compute.SetFloat("bSelectedChannel", bChannel);
            compute.SetFloat("bGamma", bNeedsGamma);
            compute.SetFloat("bReverse", bReverse ? 1 : 0);

            SetTextureWithDefault("AChannel", "aWidth", "aHeight", aTexture, aDefault);
            compute.SetFloat("aSelectedChannel", aChannel);
            compute.SetFloat("aGamma", aNeedsGamma);
            compute.SetFloat("aReverse", aReverse ? 1 : 0);

            compute.SetFloat("width", (float)resolution);
            compute.SetFloat("height", (float)resolution);

            compute.Dispatch(kernel, (int)resolution / 16, (int)resolution / 16, 1);

            RenderTexture.active = result;
            tex.ReadPixels(new Rect(0, 0, result.width, result.height), 0, 0);
            byte[] bytes;
            bytes = tex.EncodeToPNG();

            System.IO.File.WriteAllBytes(path, bytes);
            path = path.Substring(path.LastIndexOf("Assets"));
            AssetDatabase.ImportAsset(path);
            resultTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>
        /// Draws a leftside texture input
        /// </summary>
        /// <param name="name">Name of the texture</param>
        /// <param name="channel">Channel used</param>
        /// <param name="reverse">Reversed texture</param>
        /// <param name="defaultTexture">Default used if there's no texture</param>
        /// <param name="texture">Currently selected texture</param>
        private void DrawLeft(string name, ref int channel, ref bool reverse, ref DefaultTexture defaultTexture, ref Texture2D texture)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(GUILayoutUtility.GetRect(110, 16), name, TSConstants.Styles.rightLabel);
            EditorGUILayout.EndHorizontal();
            channel = GUILayout.Toolbar(channel, new string[] { "R", "G", "B", "A" }, EditorStyles.toolbarButton);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(44);
            EditorGUI.LabelField(GUILayoutUtility.GetRect(50, 16), "Reverse");
            reverse = EditorGUI.Toggle(GUILayoutUtility.GetRect(16, 16), reverse);
            EditorGUILayout.EndHorizontal();
            defaultTexture = (DefaultTexture)EditorGUILayout.EnumPopup(defaultTexture, GUILayout.Width(110));
            EditorGUILayout.EndVertical();
            texture = (Texture2D)EditorGUI.ObjectField(GUILayoutUtility.GetRect(64, 64), texture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();
        }
        /// <summary>
        /// Draws a rightside texture input
        /// </summary>
        /// <param name="name">Name of the texture</param>
        /// <param name="channel">Channel used</param>
        /// <param name="reverse">Reversed texture</param>
        /// <param name="defaultTexture">Default used if there's no texture</param>
        /// <param name="texture">Currently selected texture</param>
        private void DrawRight(string name, ref int channel, ref bool reverse, ref DefaultTexture defaultTexture, ref Texture2D texture)
        {
            EditorGUILayout.BeginHorizontal("box");
            texture = (Texture2D)EditorGUI.ObjectField(GUILayoutUtility.GetRect(64, 64), texture, typeof(Texture2D), false);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(GUILayoutUtility.GetRect(110, 16), name);
            EditorGUILayout.EndHorizontal();
            channel = GUILayout.Toolbar(channel, new string[] { "R", "G", "B", "A" }, EditorStyles.toolbarButton);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4);
            reverse = EditorGUI.Toggle(GUILayoutUtility.GetRect(16, 16), reverse);
            EditorGUI.LabelField(GUILayoutUtility.GetRect(50, 16), "Reverse");
            GUILayout.Space(40);
            EditorGUILayout.EndHorizontal();
            defaultTexture = (DefaultTexture)EditorGUILayout.EnumPopup(defaultTexture, GUILayout.Width(110));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        public void DrawGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            DrawLeft(names[0], ref rChannel, ref rReverse, ref rDefault, ref rTexture);
            DrawRight(names[1], ref gChannel, ref gReverse, ref gDefault, ref gTexture);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawLeft(names[2], ref bChannel, ref bReverse, ref bDefault, ref bTexture);
            DrawRight(names[3], ref aChannel, ref aReverse, ref aDefault, ref aTexture);
            EditorGUILayout.EndHorizontal();
            //result = (RenderTexture) EditorGUILayout.ObjectField("result",result,typeof(RenderTexture),false);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resolution", GUILayout.Width(65));
            resolution = (Resolution)EditorGUILayout.EnumPopup(resolution, GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            if (drawInternalConfirmButton)
            {
                if (GUILayout.Button("Pack textures", GUILayout.Width(150)))
                {
                    PackTexture(defaultPath);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

        }

        private void SetTextureWithDefault(string channelName, string widthName, string heightName, Texture2D texture, DefaultTexture defaultTex)
        {
            if (texture == null)
            {
                if (defaultTex == DefaultTexture.DefaultWhite)
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
            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                if (tImporter.sRGBTexture)
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
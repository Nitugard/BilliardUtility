#if UNITY_EDITOR
using System.Linq;
using ibc.builder;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ibc.builder
{

    [RequireComponent(typeof(Camera))]
    public class EightPoolTextureGenerator : MonoBehaviour
    {

        public const string BallMaterialPath = "Assets/Art/Materials/";
        public const string BallTexturePath = "Assets/Art/Textures/";

        
        [SerializeField] private Color _backgroundColor = new Color(0.97f, 1f, 0.99f);
        [SerializeField] private Color[] _colors = new Color[]
        {
            new Color(0.99f, 0.78f, 0.3f),
            new Color(0.15f, 0.34f, 0.68f),
            new Color(0.98f, 0.3f, 0.26f),
            new Color(0.34f, 0.29f, 0.61f),
            new Color(0.99f, 0.55f, 0.14f),
            new Color(0.18f, 0.62f, 0.21f),
            new Color(0.77f, 0.19f, 0.18f),
            new Color(0.2f, 0.2f, 0.22f),
        };

        [SerializeField] private RenderTexture _renderTexture;
        
        [ContextMenu("Generate Materials")]
        public void UpdateMaterials()
        {
            for (int i = 1; i < 16; ++i)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>($"{BallMaterialPath}/Ball_{i}");
                if (mat == null)
                {
                    //create material
                    mat = new Material(Shader.Find("Standard"));
                    AssetDatabase.CreateAsset(mat, $"{BallTexturePath}/Ball_{i}.mat");
                }
                
                var tex = AssetDatabase.LoadAssetAtPath<Texture>($"{BallTexturePath}/Ball_{i}");
                mat.mainTexture = tex;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [ContextMenu("Generate Textures")]
        public void GenerateTextures()
        {
            Camera cam = GetComponent<Camera>();
            
            if (_renderTexture != null)
            {
                cam.targetTexture = _renderTexture;
                
                var labels = FindObjectsOfType<Text>().Where(t => t.name == "text");
                var stripe = FindObjectsOfType<Image>().First(t => t.name == "stripe");
                var circles = FindObjectsOfType<Image>().Where(t => t.name == "circle");
                foreach (var c in circles)
                    c.color = _backgroundColor;

                stripe.enabled = false;
                for (int i = 0; i < 8; ++i)
                {
                    cam.backgroundColor = _colors[i];
                    foreach (var l in labels)
                        l.text = $"{1 + i}";

                    cam.Render();
                    SaveTexture(_renderTexture, 1 + i);
                }

                stripe.enabled = true;
                for (int i = 0; i < 7; ++i)
                {
                    cam.backgroundColor = _backgroundColor;
                    foreach (var l in labels)
                        l.text = $"{8 + i + 1}";
                    stripe.color = _colors[i];
                    cam.Render();
                    SaveTexture(_renderTexture, 8 + i + 1);
                }
            }
            else
            {
                Debug.LogWarning("Could not find render texture");
            }

            cam.targetTexture = null;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void SaveTexture(RenderTexture rt, int i)
        {
            byte[] bytes = toTexture2D(rt).EncodeToPNG();
            System.IO.File.WriteAllBytes($"{Application.dataPath}/Art/Textures/Ball_{i}.png", bytes);
        }

        public static Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, true);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}
#endif
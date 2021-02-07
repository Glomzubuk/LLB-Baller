using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LLHandlers;

namespace Baller
{
    public class BundledAssetLoader
    {

        public static UnityEngine.Object GetAsset(string _bundleName, string _assetName)
        {
            var gameBundles = AssetBundle.GetAllLoadedAssetBundles().ToArray();
            foreach (AssetBundle ab in gameBundles) if (ab.name == _bundleName) return ab.LoadAsset(_assetName);

            Debug.Log("Could not find " + _assetName + " in bundle " + _bundleName);
            return null;
        }

        public static UnityEngine.Object[] GetAssetWithSubAssets(string _bundleName, string _assetName)
        {
            var gameBundles = AssetBundle.GetAllLoadedAssetBundles().ToArray();
            foreach (AssetBundle ab in gameBundles) if (ab.name == _bundleName) return ab.LoadAssetWithSubAssets(_assetName);

            Debug.Log("Could not find " + _assetName + " in bundle " + _bundleName);
            return null;
        }

        public static Shader GetShader(ShaderType _shaderType)
        {
            var gameBundles = AssetBundle.GetAllLoadedAssetBundles().ToArray();
            switch (_shaderType)
            {
                case ShaderType.Standard:
                    return Shader.Find("Standard");
                case ShaderType.Opaque:
                    foreach (AssetBundle ab in gameBundles) if (ab.name == "characters/boss") return ((Material)ab.LoadAsset("bossmat")).shader; break;
                case ShaderType.Transparent:
                    foreach (AssetBundle ab in gameBundles) if (ab.name == "characters/boss") return ((Material)ab.LoadAsset("bossomegaglassmat")).shader; break;
                case ShaderType.TransparentSwitch:
                    foreach (AssetBundle ab in gameBundles) if (ab.name == "characters/boss") return ((Material)ab.LoadAsset("bossOmegaGlassSwitchMat")).shader; break;
                case ShaderType.NoiseClone:
                    foreach (AssetBundle ab in gameBundles) if (ab.name == "characters/bag") return ((Material)ab.LoadAsset("ScreenSpaceNoiseOverlayMat")).shader; break;
                case ShaderType.Visualizer:
                    foreach (AssetBundle ab in gameBundles) if (ab.name == "characters/boss") return ((Material)ab.LoadAsset("bossvisualizereffectmat")).shader; break;
            }
            return null;
        }

        public static void LogAllAssetsInBundle(string _bundleName)
        {
            var gameBundles = AssetBundle.GetAllLoadedAssetBundles().ToArray();
            foreach (AssetBundle ab in gameBundles)
            {
                if (ab.name == _bundleName)
                {
                    var names = ab.GetAllAssetNames();
                    Debug.Log("Assets in [" + _bundleName + "] = " + String.Join("," + Environment.NewLine, names));
                }
            }
        }



        public static void LogAllBundles()
        {
            var gameBundles = AssetBundle.GetAllLoadedAssetBundles().ToArray();
            var s = "All loaded AssetBundles: ";
            foreach (AssetBundle ab in gameBundles)
            {
                s = s + ab.name + "," + Environment.NewLine;
            }
            Debug.Log(s);
        }
    }

    public enum ShaderType
    {
        Standard,
        Opaque,
        Transparent,
        TransparentSwitch,
        NoiseClone,
        Visualizer
    }
}

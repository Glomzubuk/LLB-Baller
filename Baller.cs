using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BepInEx;
using LLHandlers;
using GameplayEntities;
using LLBML.Utils;
using LLBML.Texture;

namespace Baller
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInProcess("LLBlaze.exe")]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    public class Baller : BaseUnityPlugin
    {
        public static Baller Instance;

        public static DirectoryInfo ResourceFolder;

        private Shader transparentShader = null;
        public Ball[] balls = new Ball[10];

        private void Awake()
        {
            Instance = this;
            ResourceFolder = ModdingFolder.GetModSubFolder(this.Info);
        }

        private void Start()
        {
            balls[0] = new Ball(BallType.REGULAR, "regular");
            balls[1] = new Ball(BallType.GRAVITY, "gravity");
            balls[2] = new Ball(BallType.BIG, "big");
            balls[3] = new Ball(BallType.BEACH, "beach");

            balls[4] = new Ball(BallType.REGULAR, "candy/regular");
            balls[5] = new Ball(BallType.REGULAR, "candy/strait");
            balls[6] = new Ball(BallType.REGULAR, "candy/saturn");

            balls[7] = new Ball(BallType.REGULAR, "nitro/regular");
            balls[8] = new Ball(BallType.REGULAR, "nitro/detective");
            balls[9] = new Ball(BallType.REGULAR, "nitro/lucha");

            foreach (Ball ball in balls)
            {
                FileInfo meshFile = ball.ballResourcesFolder
                    .GetFiles()
                    .Where(file => file.Extension.ToLower() == ".obj")
                    .FirstOrDefault();
                ball.mesh = meshFile != default(FileInfo) ? FastObjImporter.Instance.ImportFile(meshFile.FullName) : null;

                FileInfo texFile = ball.ballResourcesFolder
                    .GetFiles()
                    .Where(file => file.Extension.ToLower() == ".png" || file.Extension.ToLower() == ".dds")
                    .OrderBy(_ => UnityEngine.Random.value)
                    .FirstOrDefault();

                ball.tex = texFile != null ? TextureUtils.LoadTexture(texFile) : null;
            }
        }

        private void Update()
        {
            if (!transparentShader) transparentShader = BundledAssetLoader.GetShader(ShaderType.Transparent);

            if (LLBML.States.GameStates.IsInMatch() && BallHandler.instance?.GetBall(0) is BallEntity ballEntity)
            {
                SkinnedMeshRenderer[] smrs = ballEntity?.gameObject?.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer smr in smrs)
                {
                    foreach (Ball ball in balls)
                    {
                        string[] id = ball.identifier.Split('/');

                        switch (id[0])
                        {
                            case "regular":
                            case "gravity":
                            case "big":
                            case "beach":
                                ApplyNormal(smr, ball); break;
                            case "candy":
                                ApplyCandy(smr, ball, id[1]); break;
                            case "nitro":
                                ApplyNitro(smr, ball, id[1]); break;

                        }
                    }
                }
            }
        }

        public void ApplyNormal(SkinnedMeshRenderer smr, Ball ball)
        {
            if (ball.tex != null && (smr.name == "ballMesh_MainRenderer" || smr.name == "ballMesh002_MainRenderer"))
            {
                ApplyTexture(smr, ball.tex);
            }
            if (ball.mesh != null) {
                if (smr.name == "ballMesh_MainRenderer" || smr.name == "ballMesh002_MainRenderer" || smr.name.ToLower().Contains("outline"))
                {
                    smr.sharedMesh = ball.mesh;
                }
                else
                {
                    smr.sharedMesh = null;
                }
            }
        }

        public void ApplyCandy (SkinnedMeshRenderer smr, Ball ball, string type)
        {
            if (ball.tex != null && (!smr.name.Contains("Outline")))
            {
                smr.material.SetColor("_LitColor", Color.white);
            }

            switch (type)
            {
                case "strait":
                    if (smr.name.Contains("Strait"))
                    {
                        if (ball.tex != null && (!smr.name.Contains("Outline"))) ApplyTexture(smr, ball.tex);
                        if (ball.mesh != null) smr.sharedMesh = ball.mesh;
                    }
                    break;
                case "saturn":
                    if (smr.name.Contains("Saturn"))
                    {
                        if (ball.tex != null && (!smr.name.Contains("Outline"))) ApplyTexture(smr, ball.tex);
                        if (ball.mesh != null) smr.sharedMesh = ball.mesh;
                    }
                    break;
                case "regular":
                    if (smr.name.Contains("mesh001") && !smr.name.Contains("Strait") && !smr.name.Contains("Saturn"))
                    {
                        if (ball.tex != null && (!smr.name.Contains("Outline"))) ApplyTexture(smr, ball.tex);
                        if (ball.mesh != null) smr.sharedMesh = ball.mesh;
                    }
                    break;
            }
        }

        public void ApplyNitro (SkinnedMeshRenderer smr, Ball ball, string type)
        {
            switch (type)
            {
                case "detective":
                    if (smr.name.Contains("Detective"))
                    {
                        if (ball.tex != null && (!smr.name.Contains("Outline"))) ApplyTexture(smr, ball.tex);
                        if (ball.mesh != null) smr.sharedMesh = ball.mesh;
                    }
                    break;
                case "lucha":
                    if (smr.name.Contains("Lucha"))
                    {
                        if (ball.tex != null && (!smr.name.Contains("Outline"))) ApplyTexture(smr, ball.tex);
                        if (ball.mesh != null) smr.sharedMesh = ball.mesh;
                    }
                    break;
                case "regular":
                    if (smr.name.Contains("cuff") && !smr.name.Contains("Detective") && !smr.name.Contains("Luchador"))
                    {
                        if (ball.tex != null && (!smr.name.Contains("Outline"))) ApplyTexture(smr, ball.tex);
                        if (ball.mesh != null) smr.sharedMesh = ball.mesh;
                    }
                    break;
            }
        }

        public class Ball
        {
            public BallType type;
            public string identifier;
            public DirectoryInfo ballResourcesFolder;
            public Mesh mesh;
            public Texture2D tex;

            public Ball(BallType _type, string _identifier = "")
            {
                identifier = _identifier;
                ballResourcesFolder = Directory.CreateDirectory(Path.Combine(ResourceFolder.FullName, _identifier));
                type = _type;
            }
        }

        private void ApplyTexture(SkinnedMeshRenderer _smr, Texture2D _tex)
        {
            _smr.material.SetColor("_LitColor", Color.white);
            _smr.material.mainTexture = _tex;
            ApplyTransparencyToMat(_smr.material);
        }

        private void ApplyTransparencyToMat(Material _mat)
        {
            Texture2D tex = (Texture2D)_mat.mainTexture;
            Color pix = tex.GetPixel(0, 0);

            if (transparentShader)
            {
                _mat.shader = transparentShader;
                _mat.SetColor("_ShadowColor", new Color(0.5f, 0.5f, 0.5f, 1));
                _mat.SetFloat("_RefractionFresnelStrength", pix.r);
                _mat.SetFloat("_RefractionFresnelExponent", pix.g);
                _mat.SetFloat("_Transparency", 1 - pix.a);
            }
        }
    }
}

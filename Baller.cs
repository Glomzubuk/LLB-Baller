using UnityEngine;
using System.IO;
using LLHandlers;
using GameplayEntities;
using System;

namespace Baller
{
    public class Baller : MonoBehaviour
    {
        private static Baller instance = null;
        public static Baller Instance { get { return instance; } }
        public static void Initialize() { GameObject gameObject = new GameObject("Baller"); Baller modLoader = gameObject.AddComponent<Baller>(); DontDestroyOnLoad(gameObject); instance = modLoader; }

        private const string modVersion = "v1.0";
        private string resourceFolder = (Application.dataPath + "/Managed/BallerResources");

        public Ball[] balls = new Ball[9];


        private void Start()
        {
            balls[0] = new Ball(BallType.REGULAR);
            balls[1] = new Ball(BallType.GRAVITY);
            balls[2] = new Ball(BallType.BIG);
            balls[3] = new Ball(BallType.BEACH);

            balls[4] = new Ball(BallType.REGULAR, "candy/regular");
            balls[5] = new Ball(BallType.REGULAR, "candy/strait");
            balls[6] = new Ball(BallType.REGULAR, "candy/saturn");

            balls[7] = new Ball(BallType.REGULAR, "nitro/regular");
            balls[8] = new Ball(BallType.REGULAR, "nitro/detective");

            foreach (Ball ball in balls)
            {
                ball.mesh = FastObjImporter.Instance.ImportFile(resourceFolder + "/" + ball.identifier);
                ball.tex = TextureHelper.LoadTexture(resourceFolder + "/" + ball.identifier);
            }

        }

        private void Update()
        {
            if (BallHandler.instance != null)
            {
                BallEntity ballEntity = BallHandler.instance.GetBall(0);
                SkinnedMeshRenderer[] smrs = ballEntity.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (smrs.Length > 0)
                {
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        var i = "";
                        foreach (var smr in smrs)
                        {
                            i = i + smr.name + ",";
                        }
                        Debug.Log(i);
                    }

                    if (Input.GetKeyDown(KeyCode.B)) ballEntity.ballType = BallType.BIG;
                    foreach (SkinnedMeshRenderer smr in smrs)
                    {
                        foreach (Ball ball in balls)
                        {
                            if (ballEntity.ballType == ball.type && (ball.mesh != null || ball.tex != null) && !ball.identifier.Contains("candy") && !ball.identifier.Contains("nitro"))
                            {
                                if ((smr.name == "ballMesh_MainRenderer" || smr.name == "ballMesh002_MainRenderer") && ball.tex != null)
                                {
                                    smr.material.SetColor("_LitColor", Color.white);
                                    smr.material.mainTexture = ball.tex;
                                }

                                if (ball.mesh != null && smr.name.Contains("ballMesh")) smr.sharedMesh = ball.mesh;
                            }


                            if (ball.identifier.Contains("candy") && (ball.mesh != null || ball.tex != null))
                            {
                                if (ball.tex != null && (!smr.name.Contains("Outline")))
                                {
                                    smr.material.SetColor("_LitColor", Color.white);
                                    if (ball.tex != null && ball.identifier.Contains("strait") && smr.name.Contains("Strait")) smr.material.mainTexture = ball.tex;
                                    else if (ball.tex != null && ball.identifier.Contains("saturn") && smr.name.Contains("Saturn")) smr.material.mainTexture = ball.tex;
                                    else if (ball.tex != null && ball.identifier.Contains("regular") && (!smr.name.Contains("Strait") && !smr.name.Contains("Saturn") && smr.name.Contains("mesh001"))) smr.material.mainTexture = ball.tex;
                                }

                                if (ball.mesh != null && ball.identifier.Contains("strait") && smr.name.Contains("Strait")) smr.sharedMesh = ball.mesh;
                                else if (ball.mesh != null && ball.identifier.Contains("saturn") && smr.name.Contains("Saturn")) smr.sharedMesh = ball.mesh;
                                else if (ball.mesh != null && ball.identifier.Contains("regular") && (!smr.name.Contains("Strait") && !smr.name.Contains("Saturn") && smr.name.Contains("mesh001"))) smr.sharedMesh = ball.mesh;
                            }


                            if (ball.identifier.Contains("nitro") && (ball.mesh != null || ball.tex != null))
                            {
                                if (ball.tex != null && (!smr.name.Contains("Outline")))
                                {
                                    smr.material.SetColor("_LitColor", Color.white);
                                    if (ball.tex != null && ball.identifier.Contains("detective") && smr.name.Contains("Detective")) smr.material.mainTexture = ball.tex;
                                    else if (ball.tex != null && ball.identifier.Contains("regular") && (!smr.name.Contains("Detective") && smr.name.Contains("cuff"))) smr.material.mainTexture = ball.tex;
                                }

                                if (ball.mesh != null && ball.identifier.Contains("detective") && smr.name.Contains("Detective")) smr.sharedMesh = ball.mesh;
                                else if (ball.mesh != null && ball.identifier.Contains("regular") && (!smr.name.Contains("Detective") && smr.name.Contains("cuff"))) smr.sharedMesh = ball.mesh;
                            }
                        }
                    }
                }
            }
        }

        public class Ball
        {
            public BallType type;
            public string identifier;
            public Mesh mesh;
            public Texture2D tex;

            public Ball(BallType _type, string _identifier = "")
            {
                type = _type;
                switch (type)
                {
                    case BallType.REGULAR:
                        if (identifier != "") identifier = _identifier;
                        else identifier = "regular";
                        break;
                    case BallType.GRAVITY:
                        identifier = "gravity";
                        break;
                    case BallType.BIG:
                        identifier = "big";
                        break;
                    case BallType.BEACH:
                        identifier = "beach";
                        break;
                }
            }
        }
    }
}

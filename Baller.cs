using UnityEngine;
using LLHandlers;
using GameplayEntities;

namespace Baller
{
    public class Baller : MonoBehaviour
    {
        private static Baller instance = null;
        public static Baller Instance { get { return instance; } }
        public static void Initialize() { GameObject gameObject = new GameObject("Baller"); Baller modLoader = gameObject.AddComponent<Baller>(); DontDestroyOnLoad(gameObject); instance = modLoader; }

        private const string modVersion = "v1.2";
        private string resourceFolder = (Application.dataPath + "/Managed/BallerResources");

        private Shader transparentShader = null;
        public Ball[] balls = new Ball[9];


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

            foreach (Ball ball in balls)
            {
                ball.mesh = FastObjImporter.Instance.ImportFile(resourceFolder + "/" + ball.identifier);
                ball.tex = TextureHelper.LoadTexture(resourceFolder + "/" + ball.identifier);
            }

        }

        private void Update()
        {
            if (!transparentShader) transparentShader = BundledAssetLoader.GetShader(ShaderType.Transparent);

            if (BallHandler.instance != null)
            {
                BallEntity ballEntity = BallHandler.instance.GetBall(0);
                SkinnedMeshRenderer[] smrs = ballEntity.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (smrs.Length > 0)
                {
                    foreach (SkinnedMeshRenderer smr in smrs)
                    {
                        foreach (Ball ball in balls)
                        {
                            //Regular, Beach, Big and Gravity
                            if (ballEntity.ballType == ball.type && (ball.mesh != null || ball.tex != null) && !ball.identifier.Contains("candy") && !ball.identifier.Contains("nitro"))
                            {
                                if ((smr.name == "ballMesh_MainRenderer" || smr.name == "ballMesh002_MainRenderer") && ball.tex != null) ApplyTexture(smr, ball.tex);

                                if (ball.mesh != null && (smr.name == "ballMesh_MainRenderer" || smr.name == "ballMesh002_MainRenderer" || smr.name.ToLower().Contains("outline"))) smr.sharedMesh = ball.mesh;
                                else {
                                    if (ball.mesh != null) smr.sharedMesh = null;
                                }
                            }

                            //Candyballs
                            if (ball.identifier.Contains("candy") && (ball.mesh != null || ball.tex != null))
                            {
                                if (ball.tex != null && (!smr.name.Contains("Outline")))
                                {
                                    smr.material.SetColor("_LitColor", Color.white);
                                    if (ball.tex != null && ball.identifier.Contains("strait") && smr.name.Contains("Strait")) ApplyTexture(smr, ball.tex);
                                    else if (ball.tex != null && ball.identifier.Contains("saturn") && smr.name.Contains("Saturn")) ApplyTexture(smr, ball.tex);
                                    else if (ball.tex != null && ball.identifier.Contains("regular") && (!smr.name.Contains("Strait") && !smr.name.Contains("Saturn") && smr.name.Contains("mesh001"))) ApplyTexture(smr, ball.tex);
                                }

                                if (ball.mesh != null && ball.identifier.Contains("strait") && smr.name.Contains("Strait")) smr.sharedMesh = ball.mesh;
                                else if (ball.mesh != null && ball.identifier.Contains("saturn") && smr.name.Contains("Saturn")) smr.sharedMesh = ball.mesh;
                                else if (ball.mesh != null && ball.identifier.Contains("regular") && (!smr.name.Contains("Strait") && !smr.name.Contains("Saturn") && smr.name.Contains("mesh001"))) smr.sharedMesh = ball.mesh;
                            }

                            //Nitro hook
                            if (ball.identifier.Contains("nitro") && (ball.mesh != null || ball.tex != null))
                            {
                                if (ball.tex != null && (!smr.name.Contains("Outline")))
                                {
                                    if (ball.tex != null && ball.identifier.Contains("detective") && smr.name.Contains("Detective")) ApplyTexture(smr, ball.tex);
                                    else if (ball.tex != null && ball.identifier.Contains("regular") && (!smr.name.Contains("Detective") && smr.name.Contains("cuff"))) ApplyTexture(smr, ball.tex);
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
                identifier = _identifier;
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

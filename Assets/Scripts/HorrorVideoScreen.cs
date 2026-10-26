using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HorrorVideoScreen : MonoBehaviour
{
    [SerializeField] private Texture2D[] frames;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private Light screenLight;

    private Renderer screenRenderer;
    private MaterialPropertyBlock propBlock;
    private int currentFrameIndex = -1;
    private Texture2D[] runtimeFallbackFrames;

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public void Setup(Texture2D[] videoFrames, Light linkedLight, float fps = 8f)
    {
        frames = videoFrames;
        screenLight = linkedLight;
        framesPerSecond = fps;
        EnsureFramesAvailable();
        ApplyFrame(0);
    }

    private void OnEnable()
    {
        screenRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        EnsureFramesAvailable();
        ApplyFrame(0);

#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
        EditorApplication.update += EditorTick;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
#endif
    }

#if UNITY_EDITOR
    private void EditorTick()
    {
        if (!Application.isPlaying && this != null && isActiveAndEnabled)
        {
            TickAnimation(EditorApplication.timeSinceStartup);
        }
    }
#endif

    private void Update()
    {
        double t = Application.isPlaying ? Time.timeAsDouble : Time.realtimeSinceStartupAsDouble;
        TickAnimation(t);
    }

    private void TickAnimation(double timeSeconds)
    {
        Texture2D[] activeFrames = GetActiveFrames();
        if (activeFrames == null || activeFrames.Length == 0)
        {
            return;
        }

        int idx = Mathf.FloorToInt((float)(timeSeconds * framesPerSecond)) % activeFrames.Length;
        if (idx < 0)
        {
            idx += activeFrames.Length;
        }

        if (idx != currentFrameIndex)
        {
            ApplyFrame(idx);
        }
    }

    private Texture2D[] GetActiveFrames()
    {
        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            return frames;
        }

        EnsureFramesAvailable();
        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            return frames;
        }

        return runtimeFallbackFrames;
    }

    private void EnsureFramesAvailable()
    {
        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            return;
        }

#if UNITY_EDITOR
        Texture2D[] loaded = new Texture2D[16];
        int valid = 0;
        for (int i = 1; i <= 16; i++)
        {
            string frameName = i < 10 ? ("Frame_0" + i) : ("Frame_" + i);
            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/Objects/VideoFrames/" + frameName + ".png");
            if (t != null)
            {
                loaded[valid++] = t;
            }
        }
        if (valid > 0)
        {
            frames = new Texture2D[valid];
            System.Array.Copy(loaded, frames, valid);
            return;
        }
#endif

        if (runtimeFallbackFrames == null || runtimeFallbackFrames.Length == 0 || runtimeFallbackFrames[0] == null)
        {
            runtimeFallbackFrames = GenerateProceduralHorrorFrames(12, 160, 90);
        }
    }

    private static Texture2D[] GenerateProceduralHorrorFrames(int count, int w, int h)
    {
        Texture2D[] result = new Texture2D[count];
        for (int f = 0; f < count; f++)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Color32[] cols = new Color32[w * h];

            bool isGlitch = (f == 3 || f == 7 || f == 10);
            bool isStatic = (f == 0 || f == 6);
            float figScale = isGlitch ? 1.55f : (0.4f + 0.65f * (f / (float)count));
            int scanBarY = (f * 13) % h;

            for (int y = 0; y < h; y++)
            {
                float ny = (y - h * 0.5f) / (h * 0.5f);
                bool scanline = (y % 2 == 0);
                bool bar = Mathf.Abs(y - scanBarY) <= 2;

                for (int x = 0; x < w; x++)
                {
                    float nx = (x - w * 0.5f) / (w * 0.5f);
                    float r, g, b;

                    if (isStatic)
                    {
                        float n = Mathf.PerlinNoise(x * 0.37f + f * 17.1f, y * 0.37f + f * 31.7f);
                        r = g = b = 30f + n * 185f;
                    }
                    else
                    {
                        float depth = Mathf.Max(Mathf.Abs(nx) * 0.85f, Mathf.Abs(ny));
                        float wall = Mathf.Clamp((depth - 0.1f) * 85f, 8f, 70f);
                        r = wall * 0.95f;
                        g = wall * 0.88f;
                        b = wall * 0.82f;

                        // Silueta acercándose en el centro
                        float hx = nx / (0.12f * figScale);
                        float hy = (ny - 0.18f * figScale) / (0.16f * figScale);
                        float bx = nx / (0.22f * figScale);
                        float by = (ny + 0.25f * figScale) / (0.42f * figScale);

                        if ((hx * hx + hy * hy) < 1f || (bx * bx + by * by) < 1f)
                        {
                            r = 4f;
                            g = 3f;
                            b = 5f;

                            // Ojos rojos brillantes
                            float eyeY = 0.19f * figScale;
                            float eyeDx = 0.045f * figScale;
                            float eyeR = 0.022f * figScale;
                            float dL = (nx + eyeDx) * (nx + eyeDx) + (ny - eyeY) * (ny - eyeY);
                            float dR = (nx - eyeDx) * (nx - eyeDx) + (ny - eyeY) * (ny - eyeY);
                            if (dL < eyeR * eyeR || dR < eyeR * eyeR)
                            {
                                r = 250f;
                                g = 30f;
                                b = 25f;
                            }
                        }

                        if (isGlitch)
                        {
                            r = Mathf.Clamp(r * 2.5f + 85f, 0f, 255f);
                            g *= 0.25f;
                            b *= 0.25f;
                        }
                    }

                    if (bar)
                    {
                        r = Mathf.Min(255f, r + 80f);
                        g = Mathf.Min(255f, g + 80f);
                        b = Mathf.Min(255f, b + 80f);
                    }
                    if (scanline)
                    {
                        r *= 0.72f;
                        g *= 0.72f;
                        b *= 0.72f;
                    }

                    cols[y * w + x] = new Color32((byte)r, (byte)g, (byte)b, 255);
                }
            }

            tex.SetPixels32(cols);
            tex.Apply(false);
            result[f] = tex;
        }
        return result;
    }

    private void ApplyFrame(int idx)
    {
        Texture2D[] activeFrames = GetActiveFrames();
        if (activeFrames == null || activeFrames.Length == 0)
        {
            return;
        }

        currentFrameIndex = Mathf.Clamp(idx, 0, activeFrames.Length - 1);
        Texture2D tex = activeFrames[currentFrameIndex];
        if (tex == null)
        {
            return;
        }

        if (screenRenderer == null)
        {
            screenRenderer = GetComponent<Renderer>();
        }
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        bool isRedGlitch = (currentFrameIndex == 4 || currentFrameIndex == 5 || currentFrameIndex == 9 || currentFrameIndex == 10 || currentFrameIndex == 13 || currentFrameIndex == 14);
        Color emisColor = isRedGlitch
            ? new Color(2.4f, 0.45f, 0.4f, 1f)
            : new Color(1.35f, 1.4f, 1.45f, 1f);

        if (screenRenderer != null)
        {
            if (screenRenderer.sharedMaterial != null)
            {
                screenRenderer.sharedMaterial.SetTexture(BaseMapId, tex);
                screenRenderer.sharedMaterial.SetTexture(MainTexId, tex);
                screenRenderer.sharedMaterial.SetTexture(EmissionMapId, tex);
                screenRenderer.sharedMaterial.SetColor(EmissionColorId, emisColor);
            }

            screenRenderer.GetPropertyBlock(propBlock);
            propBlock.SetTexture(BaseMapId, tex);
            propBlock.SetTexture(MainTexId, tex);
            propBlock.SetTexture(EmissionMapId, tex);
            propBlock.SetColor(EmissionColorId, emisColor);
            screenRenderer.SetPropertyBlock(propBlock);
        }

        if (screenLight != null)
        {
            screenLight.color = isRedGlitch
                ? new Color(0.95f, 0.2f, 0.18f)
                : new Color(0.65f, 0.75f, 0.88f);
            screenLight.intensity = isRedGlitch ? 3.5f : 2.1f;
        }
    }
}

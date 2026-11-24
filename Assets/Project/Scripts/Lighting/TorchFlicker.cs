using UnityEngine;

// Makes a torch light and its flame flicker together.
// Uses smooth Perlin noise (not Random) so it looks like real fire, and every torch
// gets its own random offset so they never flicker in sync.
// Shadows are handed out by TorchShadowManager, only the torches closest to the camera get them.
public class TorchFlicker : MonoBehaviour
{
    [Header("References")]
    public Light torchLight;
    public Renderer flameRenderer;

    [Header("Light")]
    public float baseIntensity = 3f;
    public float baseRange = 7f;
    public Color coolColor = new Color(1f, 0.45f, 0.15f);
    public Color hotColor = new Color(1f, 0.65f, 0.3f);

    [Header("Flicker")]
    [Tooltip("How fast the flicker moves")]
    public float flickerSpeed = 3f;
    [Range(0f, 1f)] public float intensityVariation = 0.25f;
    [Range(0f, 1f)] public float rangeVariation = 0.08f;
    [Tooltip("How far the light wobbles around, this makes the shadows sway")]
    public float positionJitter = 0.015f;

    [Header("Flame")]
    [Range(0f, 1f)] public float flameScaleVariation = 0.12f;

    [Header("Shadows")]
    [Range(0f, 1f)] public float shadowStrength = 0.85f;

    // other systems (wind, the player walking past, ...) can push this up or down later
    public float IntensityMultiplier { get; set; } = 1f;

    private float seed;
    private Vector3 lightBasePosition;
    private Vector3 flameBaseScale;
    private float shadowWeight;
    private MaterialPropertyBlock flameProperties;

    private static readonly int FlameIntensityId = Shader.PropertyToID("_FlameIntensity");
    private static readonly int SeedId = Shader.PropertyToID("_Seed");

    void Awake()
    {
        seed = Random.Range(0f, 1000f);

        if (torchLight != null)
            lightBasePosition = torchLight.transform.localPosition;

        if (flameRenderer != null)
        {
            flameBaseScale = flameRenderer.transform.localScale;
            flameProperties = new MaterialPropertyBlock();
            flameRenderer.GetPropertyBlock(flameProperties);
            flameProperties.SetFloat(SeedId, seed);
            flameRenderer.SetPropertyBlock(flameProperties);
        }

        // no shadows until the manager says this torch is close enough
        SetShadowWeight(0f);
    }

    void OnEnable()
    {
        TorchShadowManager.Register(this);
    }

    void OnDisable()
    {
        TorchShadowManager.Unregister(this);
    }

    void Update()
    {
        float t = Time.time * flickerSpeed;

        // two layers of noise: a slow breathing and a faster crackle
        float slow = Mathf.PerlinNoise(seed, t * 0.35f);
        float fast = Mathf.PerlinNoise(seed + 37.1f, t * 1.7f);
        float flicker = slow * 0.65f + fast * 0.35f; // 0..1, mostly around 0.5

        float strength = (flicker - 0.5f) * 2f; // -1..1

        if (torchLight != null)
        {
            torchLight.intensity = baseIntensity * IntensityMultiplier * (1f + strength * intensityVariation);
            torchLight.range = baseRange * (1f + strength * rangeVariation);
            torchLight.color = Color.Lerp(coolColor, hotColor, flicker);

            Vector3 wobble = new Vector3(
                Mathf.PerlinNoise(seed + 11.3f, t) - 0.5f,
                Mathf.PerlinNoise(seed + 23.7f, t) - 0.5f,
                Mathf.PerlinNoise(seed + 41.9f, t) - 0.5f) * (positionJitter * 2f);
            torchLight.transform.localPosition = lightBasePosition + wobble;
        }

        if (flameRenderer != null)
        {
            float flameStrength = 1f + strength * flameScaleVariation;
            flameRenderer.transform.localScale = new Vector3(
                flameBaseScale.x,
                flameBaseScale.y * flameStrength * Mathf.Max(IntensityMultiplier, 0f),
                flameBaseScale.z);

            flameRenderer.GetPropertyBlock(flameProperties);
            flameProperties.SetFloat(FlameIntensityId, Mathf.Lerp(0.9f, 1.1f, flicker));
            flameRenderer.SetPropertyBlock(flameProperties);
        }
    }

    // 0 = no shadows, 1 = full shadows. Called by TorchShadowManager, fades in and out
    public void SetShadowWeight(float weight)
    {
        shadowWeight = weight;
        if (torchLight == null)
            return;

        torchLight.shadows = shadowWeight > 0.01f ? LightShadows.Soft : LightShadows.None;
        torchLight.shadowStrength = shadowStrength * shadowWeight;
    }

    public float ShadowWeight => shadowWeight;
}

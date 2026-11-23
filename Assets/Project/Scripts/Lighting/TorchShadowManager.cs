using System.Collections.Generic;
using UnityEngine;

// Point light shadows are expensive (6 shadow maps per torch) and URP only has room for a few.
// This gives shadows only to the torches closest to the camera and fades them in and out,
// all other torches still light the room, just without shadows.
// You don't have to place this anywhere: the first torch that turns on creates it.
public class TorchShadowManager : MonoBehaviour
{
    [Tooltip("How many torches may cast shadows at the same time")]
    public int maxShadowedTorches = 4;
    [Tooltip("Torches further away than this never get shadows")]
    public float maxShadowDistance = 20f;
    [Tooltip("Seconds between checking which torches are closest")]
    public float refreshInterval = 0.25f;
    [Tooltip("How fast shadows fade in and out")]
    public float fadeSpeed = 3f;

    private static TorchShadowManager instance;
    private static readonly List<TorchFlicker> torches = new List<TorchFlicker>();

    private readonly HashSet<TorchFlicker> shadowed = new HashSet<TorchFlicker>();
    private Vector3 cameraPosition;
    private float refreshTimer;

    public static void Register(TorchFlicker torch)
    {
        if (!torches.Contains(torch))
            torches.Add(torch);

        if (instance == null && Application.isPlaying)
        {
            GameObject managerObject = new GameObject("TorchShadowManager");
            instance = managerObject.AddComponent<TorchShadowManager>();
        }
    }

    public static void Unregister(TorchFlicker torch)
    {
        torches.Remove(torch);
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            cameraPosition = cam.transform.position;
            PickClosestTorches();
        }

        // fade every torch towards its target
        float step = fadeSpeed * Time.deltaTime;
        for (int i = 0; i < torches.Count; i++)
        {
            TorchFlicker torch = torches[i];
            float target = shadowed.Contains(torch) ? 1f : 0f;
            if (!Mathf.Approximately(torch.ShadowWeight, target))
                torch.SetShadowWeight(Mathf.MoveTowards(torch.ShadowWeight, target, step));
        }
    }

    private void PickClosestTorches()
    {
        torches.Sort(CompareDistance);

        shadowed.Clear();
        float maxSqr = maxShadowDistance * maxShadowDistance;
        for (int i = 0; i < torches.Count && shadowed.Count < maxShadowedTorches; i++)
        {
            if ((torches[i].transform.position - cameraPosition).sqrMagnitude > maxSqr)
                break;
            shadowed.Add(torches[i]);
        }
    }

    private int CompareDistance(TorchFlicker a, TorchFlicker b)
    {
        float da = (a.transform.position - cameraPosition).sqrMagnitude;
        float db = (b.transform.position - cameraPosition).sqrMagnitude;
        return da.CompareTo(db);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}

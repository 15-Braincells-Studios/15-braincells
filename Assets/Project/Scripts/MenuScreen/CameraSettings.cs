using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraSettings : MonoBehaviour
{
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 110f;

    private CinemachineCamera vcam;

    private void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        Apply(PlayerPrefs.GetFloat("FOV", defaultFOV));
    }

    public void Apply(float fov)
    {
        vcam.Lens.FieldOfView = Mathf.Clamp(fov, minFOV, maxFOV);
    }
}
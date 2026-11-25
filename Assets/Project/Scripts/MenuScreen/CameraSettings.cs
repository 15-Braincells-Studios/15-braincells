using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSettings : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 110f;

    private Camera playerCamera;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();

        float savedFOV = PlayerPrefs.GetFloat("FOV", defaultFOV);

        savedFOV = Mathf.Clamp(savedFOV, minFOV, maxFOV);

        playerCamera.fieldOfView = savedFOV;
    }
}
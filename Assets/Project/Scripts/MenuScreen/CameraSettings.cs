using UnityEngine;

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

        if (playerCamera == null)
        {
            Debug.LogError("CameraSettings: No Camera component found!");
            return;
        }

        LoadFOV();
    }

    private void LoadFOV()
    {
        float savedFOV = PlayerPrefs.GetFloat("FOV", defaultFOV);

        savedFOV = Mathf.Clamp(savedFOV, minFOV, maxFOV);

        playerCamera.fieldOfView = savedFOV;
    }
}
using UnityEngine;

// Sits on the InspectStage prefab. ItemInspector finds it in the scene,
// so the player prefab doesn't need a link to the stage.
public class InspectStage : MonoBehaviour
{
    public Camera inspectCamera;
    public Transform inspectPivot;

    // fill the fields when the script is added in the editor
    void Reset()
    {
        inspectCamera = GetComponentInChildren<Camera>(true);

        Transform pivot = transform.Find("InspectPivot");
        if (pivot != null)
            inspectPivot = pivot;
    }
}

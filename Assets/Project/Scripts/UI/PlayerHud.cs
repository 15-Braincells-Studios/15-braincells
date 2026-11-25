using System.Collections.Generic;
using UnityEngine;

// Sits on the Player HUD (crosshair and interact prompts).
// Menus call PlayerHud.Hide(this) when they open and PlayerHud.Show(this) when they close.
// The HUD only comes back once no menu wants it hidden anymore, so menus that are
// open at the same time (for example dialogue + pause menu) don't undo each other.
// HideCrosshair / ShowCrosshair work the same way, but only for the crosshair,
// so prompts like "press E to talk" stay visible.
[RequireComponent(typeof(CanvasGroup))]
public class PlayerHud : MonoBehaviour
{
    [Tooltip("The crosshair object. If left empty, a child called \"Crosshair\" is used.")]
    [SerializeField] private GameObject crosshair;

    private static PlayerHud instance;
    private static readonly HashSet<Object> hiddenBy = new HashSet<Object>();
    private static readonly HashSet<Object> crosshairHiddenBy = new HashSet<Object>();

    private CanvasGroup group;

    private void Awake()
    {
        instance = this;
        group = GetComponent<CanvasGroup>();

        if (crosshair == null)
        {
            Transform found = transform.Find("Crosshair");
            if (found != null)
                crosshair = found.gameObject;
        }

        // Fresh start when a scene loads
        hiddenBy.Clear();
        crosshairHiddenBy.Clear();
        Refresh();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // The whole HUD: crosshair and all prompts
    public static void Hide(Object who)
    {
        hiddenBy.Add(who);
        Refresh();
    }

    public static void Show(Object who)
    {
        hiddenBy.Remove(who);
        Refresh();
    }

    // Only the crosshair
    public static void HideCrosshair(Object who)
    {
        crosshairHiddenBy.Add(who);
        Refresh();
    }

    public static void ShowCrosshair(Object who)
    {
        crosshairHiddenBy.Remove(who);
        Refresh();
    }

    private static void Refresh()
    {
        if (instance == null)
            return;

        bool visible = hiddenBy.Count == 0;

        instance.group.alpha = visible ? 1f : 0f;

        // An invisible HUD shouldn't catch clicks meant for the menu behind it
        instance.group.blocksRaycasts = visible;

        if (instance.crosshair != null)
            instance.crosshair.SetActive(crosshairHiddenBy.Count == 0);
    }
}

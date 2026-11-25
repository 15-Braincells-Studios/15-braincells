using UnityEngine;

namespace Project.Scripts.Core
{
    // Carries the case picked in the main menu into the game scene. Static, so it survives the scene load.
    public static class SelectedCase
    {
        // True once the game was started from the main menu. Then the menu decides the case, not the CaseManager's Inspector field.
        public static bool ChosenInMenu;

        // The fixed case to play, or null for a random case.
        public static TextAsset FixedCase;

        public static void Random()
        {
            ChosenInMenu = true;
            FixedCase = null;
        }

        public static void Fixed(TextAsset caseJson)
        {
            ChosenInMenu = true;
            FixedCase = caseJson;
        }
    }
}

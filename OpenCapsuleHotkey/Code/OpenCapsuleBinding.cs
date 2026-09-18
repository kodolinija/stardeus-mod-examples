using Game;
using Game.Input;
using HarmonyLib;
using UnityEngine;

namespace OpenCapsuleHotkey {
    public static class OpenCapsuleBinding {
        // ActionType is an enum that lives in the game code, so a mod cannot add a named
        // value to it. It is backed by a byte though, so any unused number can be cast to it.
        // The game uses the low numbers, so pick a high one (max 255), and make sure it does
        // not clash with other mods that add bindings the same way.
        //
        // The number is also the name of the binding: the saved key override and the
        // translation key of the label in Settings > Input are both made of it.
        // See "input.200" in Translations/English.csv.
        public const ActionType Action = (ActionType) 200;

        // The game calls private static methods with this attribute right after it loads
        // the mod DLL. That is after the default bindings are created and before the
        // overrides saved by the player are applied, so a binding added here will show up
        // in Settings > Input, can be rebound there, and the new key will be remembered.
        // Adding a binding any later than this would log an error and lose the saved key.
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize() {
            The.Bindings.Add(MultiBinding.CreateFor(Action)
                // The header this binding will be listed under in Settings > Input
                .WithUIGroup("shortcuts")
                // Check the other bindings in Settings > Input before choosing a default,
                // two actions on the same key will cause a "Duplicate Binding" warning
                .Bind("<Keyboard>/g"));

            // The bindings above are recreated every time the game boots, but Harmony patches
            // stay applied for as long as the process lives. This method can run more than
            // once per process (it does in the Unity Editor of the game developers, on every
            // Play), and patching again would not replace the patches, it would stack them:
            // every Postfix would run twice, then three times, and so on.
            if (Harmony.HasAnyPatches(HarmonyId)) {
                return;
            }
            OpenCapsulePatches.Initialize();
            // The game only applies Harmony patches from its own code, mods have to apply
            // theirs. If anything in here throws (for example when a game update renames
            // a patched method), the game will disable the mod and tell the player about it.
            new Harmony(HarmonyId).PatchAll(typeof(OpenCapsuleBinding).Assembly);
        }

        // Has to be unique, Harmony uses it to tell apart the patches of different mods
        private const string HarmonyId = "OpenCapsuleHotkey";
    }
}

using System;
using System.Collections.Generic;
using Game;
using Game.Data;
using Game.Input;
using Game.Systems.UIUX;
using Game.UI;
using HarmonyLib;

namespace OpenCapsuleHotkey {
    public static class OpenCapsulePatches {
        // OrderOpenCapsuleAction.OrderOpen is private, so we get to it via reflection instead
        // of copying what it does. The delegate is created once, calling it is as cheap as
        // calling the method directly.
        private delegate bool OrderOpenFn(List<Obj> objs, bool dryRun, out int cnt);
        private static OrderOpenFn orderOpen;

        public static void Initialize() {
            var method = AccessTools.Method(typeof(OrderOpenCapsuleAction), "OrderOpen");
            if (method == null) {
                throw new MissingMethodException(nameof(OrderOpenCapsuleAction), "OrderOpen");
            }
            orderOpen = (OrderOpenFn) Delegate.CreateDelegate(typeof(OrderOpenFn), method);
        }

        // Runs after the game is done checking its own shortcuts. The game does not get here
        // while the player is typing into a text field, a screen block is showing, or the
        // game is not loaded, so none of that has to be checked again.
        [HarmonyPatch(typeof(UserInputShortcutsHandler),
            nameof(UserInputShortcutsHandler.HandleShortcuts))]
        private static class HandleShortcutsPatch {
            private static void Postfix() {
                // This runs every frame, keep the code before this check to the minimum
                if (!The.Bindings.IsJustPressed(OpenCapsuleBinding.Action)) {
                    return;
                }
                // Same conditions the game has for its own selection shortcuts
                if (!Ready.Input || UIShowing.FullScreenWidget) {
                    return;
                }
                var selected = A.S.Query.GetSelectedObjs.Ask();
                if (selected == null || selected.Count == 0) {
                    return;
                }
                // Returns false if nothing that was selected could be opened
                if (!orderOpen(selected, false, out _)) {
                    UISounds.PlayActionDenied();
                }
            }
        }

        // The patches below add the key glyph to the "Open Capsule" button, the same way
        // the Draft button shows its key. They run after the original method has created
        // the button, __result is what the original method has returned.
        [HarmonyPatch(typeof(OrderOpenCapsuleAction), "Create")]
        private static class CreatePatch {
            private static void Postfix(WBase __result) {
                ShowGlyph(__result);
            }
        }

        [HarmonyPatch(typeof(OrderOpenCapsuleAction), "CreateMulti")]
        private static class CreateMultiPatch {
            private static void Postfix(WBase __result) {
                ShowGlyph(__result);
            }
        }

        private static void ShowGlyph(WBase widget) {
            if (widget is WBottomBarButton button) {
                // The glyph will be hidden if the player has removed the key in the settings
                button.SetShortcutGlyph(OpenCapsuleBinding.Action);
            }
        }
    }
}

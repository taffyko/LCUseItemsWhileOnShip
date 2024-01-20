using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using System.Reflection;
using System.Collections.Generic;
using System;
using GameNetcodeStuff;
using System.Reflection.Emit;

namespace UseItemsWhileOnShip;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public partial class Plugin : BaseUnityPlugin {
    internal const string modGUID = PluginInfo.PLUGIN_GUID;
    internal const string modName = PluginInfo.PLUGIN_NAME;
    internal const string modVersion = PluginInfo.PLUGIN_VERSION;
    internal readonly Harmony harmony = new Harmony(modGUID);
    internal static ManualLogSource log;
    internal static List<Action> cleanupActions = new List<Action>();

    static Plugin() {
        log = BepInEx.Logging.Logger.CreateLogSource(modName);
    }

    private void Awake() {
        log.LogInfo($"Loading {modGUID}");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    private void OnDestroy() {
        #if DEBUG
        harmony?.UnpatchSelf();
        foreach (var action in cleanupActions) {
            action();
        }
        log.LogInfo($"Unloading {modGUID}");
        #endif
    }
}


[HarmonyPatch]
class Patches {
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
    [HarmonyPatch(typeof(PlayerControllerB), "SetHoverTipAndCurrentInteractTrigger")]
    private static IEnumerable<CodeInstruction> PatchGameStateCheck(IEnumerable<CodeInstruction> instructions) {
        var fieldInfo = typeof(GameNetworkManager).GetField("gameHasStarted");
        foreach (var instruction in instructions) {
            if (instruction.opcode == OpCodes.Ldfld && instruction.operand == (object)fieldInfo) {
                yield return new CodeInstruction(OpCodes.Pop);
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
            } else {
                yield return instruction;
            }
        }
    }
}
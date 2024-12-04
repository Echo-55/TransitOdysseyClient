using System;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace TransitOdysseyClient.Patches;

[HarmonyPatch]
public class Player_OnGameSessionEnd_Patch
{
    // public virtual void OnGameSessionEnd(ExitStatus exitStatus, float pastTime, string locationId, string exitName)
    public static MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("EFT.Player");
        return AccessTools.Method(type, "OnGameSessionEnd",
            new[] { typeof(ExitStatus), typeof(float), typeof(string), typeof(string) });
    }

    public static bool Prefix(ExitStatus exitStatus, float pastTime, string locationId, string exitName,
        Player __instance)
    {
        switch (exitStatus)
        {
            case ExitStatus.Survived:
                TransitOdysseyPlugin.Instance.UnlockNextLocation(locationId);
                break;
            case ExitStatus.Killed:
                TransitOdysseyPlugin.Instance.PlayerDied();
                break;
            case ExitStatus.Left:
                if (TransitOdysseyPlugin.Instance.ShouldResetOnLeftRaidStatus.Value)
                    TransitOdysseyPlugin.Instance.PlayerDied();
                break;
            case ExitStatus.Runner:
                if (TransitOdysseyPlugin.Instance.ShouldResetOnRunnerStatus.Value)
                    TransitOdysseyPlugin.Instance.PlayerDied();
                break;
            case ExitStatus.MissingInAction:
                if (TransitOdysseyPlugin.Instance.ShouldResetOnMIAStatus.Value)
                    TransitOdysseyPlugin.Instance.PlayerDied();
                break;
            case ExitStatus.Transit:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(exitStatus), exitStatus, null);
        }

        return true;
    }
}
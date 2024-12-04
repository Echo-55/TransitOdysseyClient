using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.UI.Matchmaker;
using HarmonyLib;
using TransitOdysseyClient.Data;

namespace TransitOdysseyClient.Patches;

[HarmonyPatch]
public static class MatchMakerSelectionLocationScreen_method_10_Patch
{
    private static ELocation StartLocation => TransitOdysseyPlugin.StartLocation.Value;

    private static MatchMakerSelectionLocationScreen MatchMakerSelectionLocationScreenInstance { get; set; }

    public static List<ELocation> UnlockedLocations
    {
        get => TransitOdysseyPlugin.UnlockedLocations;
        set => TransitOdysseyPlugin.UnlockedLocations = value;
    }

    public static MethodBase TargetMethod()
    {
        var type = typeof(MatchMakerSelectionLocationScreen);
        var method = type.GetMethod("method_10", AccessTools.all, null,
            new Type[] { typeof(LocationSettingsClass.Location) }, null);
        if (method == null)
        {
            TransitOdysseyPlugin.Logger.LogError("Target method not found!");
        }

        return method;
    }

    public static bool Prefix(LocationSettingsClass.Location location, MatchMakerSelectionLocationScreen __instance)
    {
        if (!MatchMakerSelectionLocationScreenInstance)
        {
            MatchMakerSelectionLocationScreenInstance = __instance;
        }

        var locationId = location.Id;
        if (!Enum.TryParse(locationId, true, out ELocation eLocation))
        {
            TransitOdysseyPlugin.Logger.LogInfo($"Could not parse location {locationId}");
            return true;
        }

        if (locationId == StartLocation.ToIdString())
        {
            location.Enabled = true;
            location.Locked = false;
        }
        else if (UnlockedLocations.Contains(eLocation, new ELocationComparer()))
        {
            location.Enabled = true;
            location.Locked = false;
        }
        else
        {
            location.Enabled = false;
            location.Locked = true;
        }

        return true;
    }

    private class ELocationComparer : IEqualityComparer<ELocation>
    {
        public bool Equals(ELocation x, ELocation y)
        {
            return x.ToIdString() == y.ToIdString();
        }

        public int GetHashCode(ELocation obj)
        {
            return obj.GetHashCode();
        }
    }
}
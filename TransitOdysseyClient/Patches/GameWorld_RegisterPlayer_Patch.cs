using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using TransitOdysseyClient.Data;

namespace TransitOdysseyClient.Patches;

[HarmonyPatch]
public class GameWorld_RegisterPlayer_Patch
{
    public static MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("EFT.GameWorld");
        return AccessTools.Method(type, "RegisterPlayer", new[] { typeof(IPlayer) });
    }

    public static bool Prefix(IPlayer iPlayer)
    {
        ELocation startLocation = TransitOdysseyPlugin.StartLocation.Value;

        var player = iPlayer as Player;
        if (!player)
        {
            TransitOdysseyPlugin.Logger.LogInfo("Could not cast to Player");
            TransitOdysseyPlugin.CurrentPlayerLocation = startLocation;
            return true;
        }

        GameWorld gameWorld = Singleton<GameWorld>.Instance;
        if (player != gameWorld.MainPlayer) return true;

        var currentLocation = player.Location;
        if (string.IsNullOrEmpty(currentLocation))
        {
            TransitOdysseyPlugin.Logger.LogInfo("Player location is null or empty");
            TransitOdysseyPlugin.CurrentPlayerLocation = startLocation;
            return true;
        }

        if (!Enum.TryParse(currentLocation, true, out ELocation eLocation))
        {
            TransitOdysseyPlugin.Logger.LogInfo($"Could not parse location {currentLocation}");
            TransitOdysseyPlugin.CurrentPlayerLocation = startLocation;
            return true;
        }

        TransitOdysseyPlugin.Logger.LogInfo($"Player spawned at {eLocation}");
        TransitOdysseyPlugin.CurrentPlayerLocation = eLocation;

        return true;
    }
}
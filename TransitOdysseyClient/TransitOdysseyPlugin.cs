using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFTConfiguration.Attributes;
using HarmonyLib;
using TransitOdysseyClient.Data;

namespace TransitOdysseyClient;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class TransitOdysseyPlugin : BaseUnityPlugin
{
    private const string ConfigSection = "Transit Odyssey";
    private const ELocation DefaultStartLocation = ELocation.RezervBase;

    // dict for associating location enum values with config entries
    private Dictionary<ELocation, ConfigEntry<bool>> _locationConfigEntries;

    public static TransitOdysseyPlugin Instance;

    public static ConfigEntry<ELocation> StartLocation;

    private ConfigEntry<bool> UnlockAllLocations;
    private ConfigEntry<bool> Factory4DayUnlocked;
    private ConfigEntry<bool> Factory4NightUnlocked;
    private ConfigEntry<bool> CustomsUnlocked;
    private ConfigEntry<bool> WoodsUnlocked;
    private ConfigEntry<bool> ShorelineUnlocked;
    private ConfigEntry<bool> InterchangeUnlocked;
    private ConfigEntry<bool> RezervBaseUnlocked;
    private ConfigEntry<bool> LaboratoryUnlocked;
    private ConfigEntry<bool> LighthouseUnlocked;
    private ConfigEntry<bool> TarkovStreetsUnlocked;
    private ConfigEntry<bool> SandboxUnlocked;
    private ConfigEntry<bool> SandboxHighUnlocked;
    private ConfigEntry<bool> DevelopUnlocked;
    private ConfigEntry<bool> TerminalUnlocked;

    public ConfigEntry<bool> ShouldResetOnLeftRaidStatus;
    public ConfigEntry<bool> ShouldResetOnRunnerStatus;
    public ConfigEntry<bool> ShouldResetOnMIAStatus;

    public static List<ELocation> UnlockedLocations;

    internal new static ManualLogSource Logger;

    public static ELocation CurrentPlayerLocation { get; set; }

    private void Awake()
    {
        Instance = this;

        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        RegisterConfigEntries(ConfigSection, DefaultStartLocation);

        Config.SettingChanged += Config_SettingChanged;

        _locationConfigEntries = new Dictionary<ELocation, ConfigEntry<bool>>
        {
            { ELocation.Factory4_Day, Factory4DayUnlocked },
            { ELocation.Factory4_Night, Factory4NightUnlocked },
            { ELocation.BigMap, CustomsUnlocked },
            { ELocation.Woods, WoodsUnlocked },
            { ELocation.Shoreline, ShorelineUnlocked },
            { ELocation.Interchange, InterchangeUnlocked },
            { ELocation.RezervBase, RezervBaseUnlocked },
            { ELocation.Laboratory, LaboratoryUnlocked },
            { ELocation.Lighthouse, LighthouseUnlocked },
            { ELocation.TarkovStreets, TarkovStreetsUnlocked },
            { ELocation.Sandbox, SandboxUnlocked },
            { ELocation.Sandbox_High, SandboxHighUnlocked },
            { ELocation.Develop, DevelopUnlocked },
            { ELocation.Terminal, TerminalUnlocked }
        };

        // Add the start location
        UnlockedLocations = [StartLocation.Value];

        // Add locations based on boolean config values and the start location
        foreach (var entry in _locationConfigEntries)
        {
            if (entry.Value.Value || entry.Key == StartLocation.Value)
            {
                UnlockedLocations.Add(entry.Key);
            }
        }

        // Add all locations if UnlockAllLocations is true
        if (UnlockAllLocations.Value)
        {
            UnlockedLocations = [..Enum.GetValues(typeof(ELocation)).Cast<ELocation>()];
        }

        // Patch methods
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }

    public void UnlockNextLocation(string locationId)
    {
        // TODO: Implement a better way to determine the next location to unlock/when to unlock
        // Option 1. Unlock the next location if the player has survived the raid from any location
        // Option 2. Determine the next location based on the current location


        // For now, just unlock the next location in the enum

        // Determine the next location to unlock
        ELocation nextLocation = UnlockedLocations.Last() + 1;
        if (!Enum.IsDefined(typeof(ELocation), nextLocation))
        {
            Logger.LogInfo("No more locations to unlock");
            return;
        }

        // Update the config entry bool value
        if (_locationConfigEntries.TryGetValue(nextLocation, out var configEntry))
        {
            configEntry.Value = true;
        }

        // Add the next location to the unlocked locations
        UnlockedLocations.Add(nextLocation);

        // Save the config
        Config.Save();
    }

    public void PlayerDied()
    {
        Logger.LogInfo("Player died");
        // Reset the current player location to the start location
        CurrentPlayerLocation = StartLocation.Value;
        // clear all unlocked locations other than the start location
        UnlockedLocations = [StartLocation.Value];

        // Update the config entries
        foreach (var entry in _locationConfigEntries)
        {
            if (entry.Key == StartLocation.Value)
            {
                entry.Value.Value = true;
            }
            else
            {
                entry.Value.Value = false;
            }
        }

        // save unlocked locations
        Config.Save();
    }

    private ConfigEntry<T> CreateAdvancedConfigEntry<T>(string section, string key, T defaultValue,
        string description = null)
    {
        var attrs = new ConfigurationManagerAttributes()
        {
            IsAdvanced = true
        };
        return Config.Bind(section, key, defaultValue, new ConfigDescription(description, null, attrs));
    }

    private void RegisterConfigEntries(string configSection, ELocation defaultStartLocation)
    {
        // Basic config entries
        StartLocation = Config.Bind(configSection, "Set Start Location", defaultStartLocation);

        // Advanced config entries
        UnlockAllLocations =
            CreateAdvancedConfigEntry(configSection, "Unlock All Locations", false, "Unlock all locations");
        Factory4DayUnlocked = CreateAdvancedConfigEntry(configSection, "Unlock Factory 4 Day", false,
            "Unlock Factory 4 Day location");
        Factory4NightUnlocked = CreateAdvancedConfigEntry(configSection, "Unlock Factory 4 Night", false,
            "Unlock Factory 4 Night location");
        CustomsUnlocked = CreateAdvancedConfigEntry(configSection, "Unlock Customs", false, "Unlock Customs location");
        WoodsUnlocked = CreateAdvancedConfigEntry(configSection, "Unlock Woods", false, "Unlock Woods location");
        ShorelineUnlocked =
            CreateAdvancedConfigEntry(configSection, "Unlock Shoreline", false, "Unlock Shoreline location");
        InterchangeUnlocked =
            CreateAdvancedConfigEntry(configSection, "Unlock Interchange", false, "Unlock Interchange location");
        RezervBaseUnlocked =
            CreateAdvancedConfigEntry(configSection, "Unlock Rezerv Base", false, "Unlock Rezerv Base location");
        LaboratoryUnlocked =
            CreateAdvancedConfigEntry(configSection, "Unlock Laboratory", false, "Unlock Laboratory location");
        LighthouseUnlocked =
            CreateAdvancedConfigEntry(configSection, "Unlock Lighthouse", false, "Unlock Lighthouse location");
        TarkovStreetsUnlocked = CreateAdvancedConfigEntry(configSection, "Unlock Tarkov Streets", false,
            "Unlock Tarkov Streets location");
        SandboxUnlocked = CreateAdvancedConfigEntry(configSection, "Unlock Sandbox", false, "Unlock Sandbox location");
        SandboxHighUnlocked =
            CreateAdvancedConfigEntry(configSection, "Unlock Sandbox High", false, "Unlock Sandbox High location");
        DevelopUnlocked = CreateAdvancedConfigEntry(configSection, "Unlock Develop", false, "Unlock Develop location");
        TerminalUnlocked =
            CreateAdvancedConfigEntry(configSection, "Unlock Terminal", false, "Unlock Terminal location");

        ShouldResetOnLeftRaidStatus = CreateAdvancedConfigEntry(configSection, "Reset On Left Raid Status", true,
            "Reset unlocked locations on Left raid status");
        ShouldResetOnRunnerStatus = CreateAdvancedConfigEntry(configSection, "Reset On Runner Status", true,
            "Reset unlocked locations on Runner status");
        ShouldResetOnMIAStatus = CreateAdvancedConfigEntry(configSection,
            "Reset On Missing In Action Status", true, "Reset unlocked locations on Missing In Action status");
    }

    private void Config_SettingChanged(object sender, SettingChangedEventArgs e)
    {
        var settingActions = new Dictionary<ConfigEntry<bool>, Action>
        {
            { Factory4DayUnlocked, () => UpdateLocation(Factory4DayUnlocked, ELocation.Factory4_Day) },
            { Factory4NightUnlocked, () => UpdateLocation(Factory4NightUnlocked, ELocation.Factory4_Night) },
            { CustomsUnlocked, () => UpdateLocation(CustomsUnlocked, ELocation.BigMap) },
            { WoodsUnlocked, () => UpdateLocation(WoodsUnlocked, ELocation.Woods) },
            { ShorelineUnlocked, () => UpdateLocation(ShorelineUnlocked, ELocation.Shoreline) },
            { InterchangeUnlocked, () => UpdateLocation(InterchangeUnlocked, ELocation.Interchange) },
            { RezervBaseUnlocked, () => UpdateLocation(RezervBaseUnlocked, ELocation.RezervBase) },
            { LaboratoryUnlocked, () => UpdateLocation(LaboratoryUnlocked, ELocation.Laboratory) },
            { LighthouseUnlocked, () => UpdateLocation(LighthouseUnlocked, ELocation.Lighthouse) },
            { TarkovStreetsUnlocked, () => UpdateLocation(TarkovStreetsUnlocked, ELocation.TarkovStreets) },
            { SandboxUnlocked, () => UpdateLocation(SandboxUnlocked, ELocation.Sandbox) },
            { SandboxHighUnlocked, () => UpdateLocation(SandboxHighUnlocked, ELocation.Sandbox_High) },
            { DevelopUnlocked, () => UpdateLocation(DevelopUnlocked, ELocation.Develop) },
            { TerminalUnlocked, () => UpdateLocation(TerminalUnlocked, ELocation.Terminal) }
        };

        if (e.ChangedSetting == StartLocation)
        {
            if (!UnlockedLocations.Contains(StartLocation.Value))
            {
                UnlockedLocations.Add(StartLocation.Value);
            }
        }
        else if (e.ChangedSetting == UnlockAllLocations)
        {
            if (UnlockAllLocations.Value)
            {
                UnlockedLocations = [..Enum.GetValues(typeof(ELocation)).Cast<ELocation>()];
            }
            else
            {
                UnlockedLocations = [StartLocation.Value];
            }
        }
        else if (settingActions.TryGetValue((ConfigEntry<bool>)e.ChangedSetting, out Action action))
        {
            action();
        }
    }

    private static void UpdateLocation(ConfigEntry<bool> configEntry, ELocation location)
    {
        if (configEntry.Value)
        {
            UnlockedLocations.Add(location);
        }
        else
        {
            UnlockedLocations.Remove(location);
        }
    }
}
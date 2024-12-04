using System;

namespace TransitOdysseyClient.Data;

public enum ELocation
{
    Develop,
    Woods,
    // ReSharper disable once InconsistentNaming
    Factory4_Day,
    // ReSharper disable once InconsistentNaming
    Factory4_Night,
    BigMap,
    Shoreline,
    Interchange,
    RezervBase,
    Laboratory,
    Lighthouse,
    TarkovStreets,
    Sandbox,
    // ReSharper disable once InconsistentNaming
    Sandbox_High,
    Terminal
}

public static class LocationExtensions
{
    public static string ToIdString(this ELocation eLocation)
    {
        return eLocation switch
        {
            ELocation.Develop => "develop",
            ELocation.Woods => "Woods",
            ELocation.Factory4_Day => "factory4_day",
            ELocation.Factory4_Night => "factory4_night",
            ELocation.BigMap => "bigmap",
            ELocation.Shoreline => "Shoreline",
            ELocation.Interchange => "Interchange",
            ELocation.RezervBase => "RezervBase",
            ELocation.Laboratory => "laboratory",
            ELocation.Lighthouse => "Lighthouse",
            ELocation.TarkovStreets => "TarkovStreets",
            ELocation.Sandbox => "Sandbox",
            ELocation.Sandbox_High => "Sandbox_high",
            ELocation.Terminal => "Terminal",
            _ => throw new ArgumentOutOfRangeException(nameof(eLocation), eLocation, null)
        };
    }
}
using System.Collections.Generic;
using UnityEngine;
using MSCLoader;

namespace VehicleDoorsOverhauled
{
  public class VehicleDoorsOverhauled : Mod
  {
    public override string ID => "VehicleDoorsOverhauled"; // Your (unique) mod ID 
    public override string Name => "Vehicle Doors Overhauled"; // Your mod name
    public override string Author => "casper-3"; // Name of the Author (your name)
    public override string Version => "1.1.0"; // Version
    public override string Description => "Overhauls vehicle door controls to use left and right mouse buttons."; // Short description of your mod
    public override Game SupportedGames => Game.MyWinterCar; //Supported Games

    private readonly List<VehiclePatcher> vanillaPatchers = new List<VehiclePatcher>()
    {
      new GifuPatcher("Gifu", () => GameObject.Find("GIFU(750/450psi)").transform),
      new MachtwagenPatcher("Machtwagen", () => GameObject.Find("JOBS").transform.Find("TAXIJOB/MACHTWAGEN")),
    };

    private readonly List<KeyValuePair<string, VehiclePatcher>> modPatchers = new()
    {
      new("SecondMachtwagen", new MachtwagenPatcher("Machtwagen (Second Machtwagen)", () => GameObject.Find("SECONDMACHTWAGEN").transform)),
      new("Second Gifu", new GifuPatcher("Gifu (Second Gifu)", () => GameObject.Find("GIFU(650/350psi)").transform)),
    };

    public override void ModSetup()
    {
      SetupFunction(Setup.ModSettings, Mod_Settings);
      SetupFunction(Setup.ModSettingsLoaded, Mod_SettingsLoaded);
      SetupFunction(Setup.OnLoad, Mod_OnLoad);
      SetupFunction(Setup.PostLoad, Mod_PostLoad);
    }

    private void Mod_Settings()
    {
      foreach (VehiclePatcher patcher in vanillaPatchers)
      {
        patcher.CreateSettings();
      }

      foreach (var entry in modPatchers)
      {
        entry.Value.CreateSettings();
      }
    }

    private void Mod_SettingsLoaded()
    {
      foreach (var entry in modPatchers)
      {
        if (!ModLoader.IsModPresent(entry.Key))
        {
          entry.Value.HideSettings();
        }
      }
    }

    private void Mod_OnLoad()
    {
      foreach (VehiclePatcher patcher in vanillaPatchers)
      {
        if (patcher.IsEnabled)
          patcher.Patch();
      }
    }

    private void Mod_PostLoad()
    {
      foreach (var entry in modPatchers)
      {
        if (ModLoader.IsModPresent(entry.Key) && entry.Value.IsEnabled)
          entry.Value.Patch();
      }
    }
  }
}
using MSCLoader;
using System;

namespace VehicleDoorsOverhauled
{
  public class VehicleDoorsOverhauled : Mod
  {
    public override string ID => "VehicleDoorsOverhauled"; // Your (unique) mod ID 
    public override string Name => "Vehicle Doors Overhauled"; // Your mod name
    public override string Author => "casper-3"; // Name of the Author (your name)
    public override string Version => "1.3.0"; // Version
    public override string Description => "Overhauls vehicle door controls to use left and right mouse buttons."; // Short description of your mod
    public override Game SupportedGames => Game.MyWinterCar | Game.MySummerCar; //Supported Games

    public override void ModSetup()
    {
      SetupFunction(Setup.ModSettings, Mod_Settings);
      SetupFunction(Setup.OnLoad, Mod_OnLoad);
      SetupFunction(Setup.PostLoad, Mod_PostLoad);
    }

    private void Mod_Settings()
    {
      foreach (VehiclePatcher patcher in PatcherRegistry.VanillaPatchers)
      {
        patcher.CreateSettings();
      }

      foreach (var entry in PatcherRegistry.ModPatchers)
      {
        if (!ModLoader.IsModPresent(entry.Key)) continue;

        entry.Value.CreateSettings();
      }
    }

    private void Mod_OnLoad()
    {
      foreach (VehiclePatcher patcher in PatcherRegistry.VanillaPatchers)
      {
        if (patcher.IsEnabled)
        {
          try
          {
            patcher.Patch();
          }
          catch (Exception ex)
          {
            ModConsole.LogError($"Failed to patch {patcher.VehicleName}: {ex}");
          }
        }
      }
    }

    private void Mod_PostLoad()
    {
      foreach (var entry in PatcherRegistry.ModPatchers)
      {
        if (!ModLoader.IsModPresent(entry.Key)) continue;

        VehiclePatcher patcher = entry.Value;
        if (patcher.IsEnabled)
        {
          try
          {
            patcher.Patch();
          }
          catch (Exception ex)
          {
            ModConsole.LogError($"Failed to patch {patcher.VehicleName}: {ex}");
          }
        }
      }
    }
  }
}

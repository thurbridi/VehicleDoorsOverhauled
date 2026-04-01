using System;
using System.Collections.Generic;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  static class PatcherRegistry
  {
    public static List<VehiclePatcher> VanillaPatchers =>
      ModLoader.CurrentGame == Game.MyWinterCar ? mwcVanillaPatchers : mscVanillaPatchers;

    public static List<KeyValuePair<string, VehiclePatcher>> ModPatchers =>
      ModLoader.CurrentGame == Game.MyWinterCar ? mwcModPatchers : mscModPatchers;

    private static readonly List<VehiclePatcher> mscVanillaPatchers = new List<VehiclePatcher>()
    {
      new SatsumaPatcher("Satsuma", () => GameObject.Find("SATSUMA(557kg, 248)").transform),
      new MSCGifuPatcher("Gifu", () => GameObject.Find("GIFU(750/450psi)").transform),
      new KekmetPatcher("Kekmet", () => GameObject.Find("KEKMET(350-400psi)").transform),
      new FerndalePatcher("Ferndale", () => GameObject.Find("FERNDALE(1630kg)").transform),
      new HayosikoPatcher("Hayosiko", () => GameObject.Find("HAYOSIKO(1500kg, 250)").transform),
      new RusckoPatcher("Ruscko", () => GameObject.Find("RCO_RUSCKO12(270)").transform),
      new FittanPatcher("Fittan", () => GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN")),
    };

    private static List<KeyValuePair<string, VehiclePatcher>> mscModPatchers = new List<KeyValuePair<string, VehiclePatcher>>()
    {
      new("Saker", new SakerPatcher("Saker", () => GameObject.Find("SAKER(1350kg)").transform)),
    };

    private static readonly List<VehiclePatcher> mwcVanillaPatchers = new List<VehiclePatcher>()
    {
      new GifuPatcher("Gifu", () => GameObject.Find("GIFU(750/450psi)").transform),
      new MachtwagenPatcher("Machtwagen", () => GameObject.Find("JOBS").transform.Find("TAXIJOB/MACHTWAGEN")),
      new RivettPatcher("Rivett", () => GameObject.Find("CORRIS").transform),
      new BachglotzPatcher("Bachglotz", () => GameObject.Find("BACHGLOTZ(1905kg)").transform),
      new KekmetPatcher("Kekmet", () => GameObject.Find("KEKMET(350-400psi)").transform),
      new HeppaPatcher("Heppa", () => GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/HEPPA")),
      new SorbetPatcher("Sorbet", () => GameObject.Find("SORBET(190-200psi)").transform),
    };

    private static readonly List<KeyValuePair<string, VehiclePatcher>> mwcModPatchers = new()
    {
      new("SecondMachtwagen", new MachtwagenPatcher("Machtwagen (Second Machtwagen)", () => GameObject.Find("SECONDMACHTWAGEN").transform)),
      new("Second Gifu", new GifuPatcher("Gifu (Second Gifu)", () => GameObject.Find("GIFU(650/350psi)").transform)),
    };
  }
}
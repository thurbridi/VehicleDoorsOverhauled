using MSCLoader;
using HutongGames.PlayMaker;
using UnityEngine;
using System;

namespace VehicleDoorsReworked
{
    public class VehicleDoorsReworked : Mod
    {
        public override string ID => "VehicleDoorsReworked"; // Your (unique) mod ID 
        public override string Name => "VehicleDoorsReworked"; // Your mod name
        public override string Author => "casper-3"; // Name of the Author (your name)
        public override string Version => "0.1.0"; // Version
        public override string Description => "Reworked vehicle door controls to use both mouse buttons."; // Short description of your mod
        public override Game SupportedGames => Game.MyWinterCar; //Supported Games
        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        private void Mod_OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            SorbetPatcher.Patch();
            MachtwagenPatcher.Patch();
            BachglotzPatcher.Patch();
            GifuPatcher.Patch();
            KekmetPatcher.Patch();
            RivettPatcher.Patch();
        }
    }
}
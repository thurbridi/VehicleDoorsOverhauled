using MSCLoader;

namespace VehicleDoorsOverhauled
{
    public class VehicleDoorsOverhauled : Mod
    {
        public override string ID => "VehicleDoorsOverhauled"; // Your (unique) mod ID 
        public override string Name => "Vehicle Doors Overhauled"; // Your mod name
        public override string Author => "casper-3"; // Name of the Author (your name)
        public override string Version => "0.1.0"; // Version
        public override string Description => "Overhauls vehicle door controls to use left and right mouse buttons."; // Short description of your mod
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
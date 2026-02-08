using MSCLoader;

namespace VehicleDoorsOverhauled
{
    public class VehicleDoorsOverhauled : Mod
    {
        public override string ID => "VehicleDoorsOverhauled"; // Your (unique) mod ID 
        public override string Name => "Vehicle Doors Overhauled"; // Your mod name
        public override string Author => "casper-3"; // Name of the Author (your name)
        public override string Version => "1.0.0"; // Version
        public override string Description => "Overhauls vehicle door controls to use left and right mouse buttons."; // Short description of your mod
        public override Game SupportedGames => Game.MyWinterCar; //Supported Games

        private SettingsCheckBox shouldPatchSorbet, shouldPatchMachtwagen, shouldPatchBachglotz, shouldPatchGifu, shouldPatchKekmet,
        shouldPatchRivett;

        public override void ModSetup()
        {
            SetupFunction(Setup.ModSettings, Mod_Settings);
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        private void Mod_Settings()
        {
            Settings.AddHeader("Vehicle Patches");
            Settings.AddText("<size=18>Choose which vehicles to apply the door overhaul to. <b>CHANGES REQUIRE RELOAD</b>.</size>");
            shouldPatchSorbet = Settings.AddCheckBox(settingID: "shouldPatchSorbet", name: "Patch Sorbet", value: true);
            shouldPatchMachtwagen = Settings.AddCheckBox(settingID: "shouldPatchMachtwagen", name: "Patch Machtwagen", value: true);
            shouldPatchBachglotz = Settings.AddCheckBox(settingID: "shouldPatchBachglotz", name: "Patch Bachglotz", value: true);
            shouldPatchGifu = Settings.AddCheckBox(settingID: "shouldPatchGifu", name: "Patch Gifu", value: true);
            shouldPatchKekmet = Settings.AddCheckBox(settingID: "shouldPatchKekmet", name: "Patch Kekmet", value: true);
            shouldPatchRivett = Settings.AddCheckBox(settingID: "shouldPatchRivett", name: "Patch Rivett", value: true);
        }

        private void Mod_OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            if (shouldPatchSorbet.GetValue())
                SorbetPatcher.Patch();
            if (shouldPatchMachtwagen.GetValue())
                MachtwagenPatcher.Patch();
            if (shouldPatchBachglotz.GetValue())
                BachglotzPatcher.Patch();
            if (shouldPatchGifu.GetValue())
                GifuPatcher.Patch();
            if (shouldPatchKekmet.GetValue())
                KekmetPatcher.Patch();
            if (shouldPatchRivett.GetValue())
                RivettPatcher.Patch();
        }
    }
}
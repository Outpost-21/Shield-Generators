using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ShieldGen
{
    public class ShieldGenMod : Mod
    {
        public static ShieldGenMod mod;
        public static ShieldGenSettings settings;

        public Vector2 optionsScrollPosition;
        public float optionsViewRectHeight;

        internal static string VersionDir => Path.Combine(mod.Content.ModMetaData.RootDir.FullName, "Version.txt");
        public static string CurrentVersion { get; private set; }

        public ShieldGenMod(ModContentPack content) : base(content)
        {
            mod = this;
            settings = GetSettings<ShieldGenSettings>();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            CurrentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            LogUtil.LogMessage($"{CurrentVersion} ::");

            if (Prefs.DevMode)
            {
                File.WriteAllText(VersionDir, CurrentVersion);
            }

            Harmony harmony = new Harmony("Neronix17.ShieldGen.RimWorld");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory() => "Shield Generators";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            bool flag = optionsViewRectHeight > inRect.height;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - (flag ? 26f : 0f), optionsViewRectHeight);
            Widgets.BeginScrollView(inRect, ref optionsScrollPosition, viewRect);
            Listing_Standard listing = new Listing_Standard();
            Rect rect = new Rect(viewRect.x, viewRect.y, viewRect.width, 999999f);
            listing.Begin(rect);
            // ============================ CONTENTS ================================
            DoOptionsCategoryContents(listing);
            // ======================================================================
            optionsViewRectHeight = listing.CurHeight;
            listing.End();
            Widgets.EndScrollView();
        }

        public void DoOptionsCategoryContents(Listing_Standard listing)
        {
            listing.Note("You will need to restart the game for most of these settings to take effect.", GameFont.Tiny);
            listing.GapLine();
            listing.CheckboxEnhanced("Disable Cooling System", "If enabled, this completely disables the cooling on the larger shield generators, instead they'll behave like they're always adequetly cooled. The cooling related buildings and pipes can still be built, they just won't do anything useful.", ref settings.disableCooling);
            listing.CheckboxEnhanced("Disable Cooling Plug Effects", "If enabled, this will prevent the cooling plugs from spawning their visual effects as they heat up. Useful for those playing on a Potato 3000.", ref settings.disableVisuals);
        }
    }
}

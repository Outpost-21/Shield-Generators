using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ShieldGen
{
    public class ShieldGenSettings : ModSettings
    {
        public bool verboseLogging = false;

        public bool disableVisuals = false;

        public bool disableCooling = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref disableVisuals, "disableVisuals");
            Scribe_Values.Look(ref disableCooling, "disableCooling");
        }

        public bool IsValidSetting(string input)
        {
            if (GetType().GetFields().Where(p => p.FieldType == typeof(bool)).Any(i => i.Name == input))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<string> GetEnabledSettings
        {
            get
            {
                return GetType().GetFields().Where(p => p.FieldType == typeof(bool) && (bool)p.GetValue(this)).Select(p => p.Name);
            }
        }
    }
}

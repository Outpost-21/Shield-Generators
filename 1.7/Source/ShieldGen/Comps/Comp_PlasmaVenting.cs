using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using PipeSystem;

namespace ShieldGen
{
    public class Comp_PlasmaVenting : ThingComp
    {
        public CompProperties_PlasmaVenting Props => (CompProperties_PlasmaVenting)props;

        public CompResourceStorage storageComp;

        public CompResourceStorage StorageComp { get { if(storageComp == null) { storageComp = parent.TryGetComp<CompResourceStorage>(); } return storageComp; } }

        public override void CompTick()
        {
            base.CompTick();
            if(StorageComp.AmountStored > 0f)
            {
                PlasmaFlare(Mathf.Clamp(StorageComp.AmountStored, 1f, Props.ventPerTickLimit));
            }
        }

        public void PlasmaFlare(float amnt)
        {
            if (amnt > Props.flareMin)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(parent.Position, Props.radius, false))
                {
                    if (!cell.InBounds(parent.Map) || cell == parent.Position)
                    {
                        continue;
                    }
                    FireUtility.TryStartFireIn(cell, parent.Map, amnt / 10f, null);
                }
            }
            VentPlasma(amnt);
        }

        public void VentPlasma(float amnt)
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(parent.Position, Props.radius, false))
            {
                if (!cell.InBounds(parent.Map))
                {
                    continue;
                }
                if (Rand.Chance(0.05f) && !ShieldGenMod.settings.disableVisuals)
                {
                    FleckMaker.ThrowHeatGlow(cell, parent.Map, amnt / 50f);
                }
                GenTemperature.PushHeat(cell, parent.Map, amnt / 10f);
            }
            StorageComp.DrawResource(Mathf.Min(amnt, StorageComp.AmountStored));
        }
    }
}

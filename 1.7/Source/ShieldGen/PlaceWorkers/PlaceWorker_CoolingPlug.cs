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
    public class PlaceWorker_CoolingPlug : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            CompProperties_PlasmaVenting comp = def.GetCompProperties<CompProperties_PlasmaVenting>();
            if(comp != null)
            {
                GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(center, comp.radius, true).ToList(), GenTemperature.ColorRoomHot);
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            CompProperties_PlasmaVenting comp = ((ThingDef)checkingDef).GetCompProperties<CompProperties_PlasmaVenting>();
            foreach(IntVec3 cell in GenRadial.RadialCellsAround(loc, comp.radius, false))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }
                if (cell.GetThingList(map).Any(t => t.def == checkingDef))
                {
                    return "CannotPlaceAdjacentSameDef".Translate();
                }
            }
            return true;
        }
    }
}

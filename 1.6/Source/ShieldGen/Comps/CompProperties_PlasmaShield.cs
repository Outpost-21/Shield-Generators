using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using TabulaRasa;

namespace ShieldGen
{
    public class CompProperties_PlasmaShield : TabulaRasa.CompProperties_Shield
    {
        public CompProperties_PlasmaShield()
        {
            compClass = typeof(Comp_PlasmaShield);
        }

        public float plasmaPerStress = 1f;

        public float basePlasmaGen = 10f;
    }
}

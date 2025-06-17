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
    public class CompProperties_PlasmaVenting : CompProperties
    {
        public CompProperties_PlasmaVenting() => compClass = typeof(Comp_PlasmaVenting);

        public float ventPerTickLimit = 20f;

        public float radius = 2.9f; // 2.9 makes a 5x5 around it, exactly what I need.

        public float flareMin = 10f;
    }
}

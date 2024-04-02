using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using TabulaRasa;
using PipeSystem;

namespace ShieldGen
{
    public class Comp_PlasmaShield : Comp_Shield
    {
        public new CompProperties_PlasmaShield Props => (CompProperties_PlasmaShield)props;

        public CompResourceTrader compResourceTrader;

        public CompResourceTrader CompResourceTrader { get { if (compResourceTrader == null) { compResourceTrader = parent.TryGetComp<CompResourceTrader>(); } return compResourceTrader; } }

        public override bool Active => base.Active && (!CompResourceTrader.PipeNet.storages.NullOrEmpty() || ShieldGenMod.settings.disableCooling);

        public override void CompTick()
        {
            base.CompTick();
            if (Active && !ShieldGenMod.settings.disableCooling)
            {
                DistributePlasma(Props.basePlasmaGen);
                if (CurStressLevel > 0)
                {
                    DistributePlasma(CurStressLevel * Props.plasmaPerStress);
                }
            }
        }

        public void DistributePlasma(float amnt)
        {
            float stored;
            CompResourceTrader.PipeNet.DistributeAmongStorage(amnt, out stored);
            if (stored < amnt)
            {
                OverloadShield();
            }
        }
    }
}

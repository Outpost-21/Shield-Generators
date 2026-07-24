using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TabulaRasa;
using Verse;

namespace ShieldGen.Patches
{
    public static class Patch_ShieldOffsetGravshipRotation
    {
        private static readonly HashSet<string> OffsetShieldDefNames = new HashSet<string>
        {
            "ShieldGen_Shield_Durex",
            "ShieldGen_Shield_Trojan",
            "ShieldGen_Shield_Astroglide"
        };

        private static readonly Dictionary<Comp_Shield, int> lastGravshipRotations = new Dictionary<Comp_Shield, int>();
        private static readonly Dictionary<Gravship, int> pendingLandingRotations = new Dictionary<Gravship, int>();
        private static readonly FieldInfo gravshipThingsField = typeof(Gravship).GetField("things", BindingFlags.Instance | BindingFlags.NonPublic);

        private static int? currentSpawningGravshipRotation;

        public static bool AppliesTo(Comp_Shield shield)
        {
            return shield != null
                && shield.parent != null
                && shield.parent.def != null
                && OffsetShieldDefNames.Contains(shield.parent.def.defName);
        }

        public static void EnsureCurrentRotation(Comp_Shield shield)
        {
            if (AppliesTo(shield) && !lastGravshipRotations.ContainsKey(shield))
            {
                lastGravshipRotations[shield] = CurrentRotation(shield).AsInt;
            }
        }

        public static void ResetCurrentRotation(Comp_Shield shield)
        {
            if (AppliesTo(shield) && !lastGravshipRotations.ContainsKey(shield))
            {
                lastGravshipRotations[shield] = CurrentRotation(shield).AsInt;
            }
        }

        public static void RotateOffsetIfNeeded(Comp_Shield shield)
        {
            if (!AppliesTo(shield) || !currentSpawningGravshipRotation.HasValue)
            {
                return;
            }

            Rot4 currentRotation = CurrentRotation(shield);
            int previousRotation;
            if (!lastGravshipRotations.TryGetValue(shield, out previousRotation))
            {
                lastGravshipRotations[shield] = currentRotation.AsInt;
                return;
            }
            if (previousRotation == currentRotation.AsInt)
            {
                return;
            }

            IntVec3 offset = new IntVec3(shield.shieldOffsetX, 0, shield.shieldOffsetY);
            IntVec3 rotatedOffset = offset.RotatedBy(Rot4.GetRelativeRotation(new Rot4(previousRotation), currentRotation));
            shield.shieldOffsetX = rotatedOffset.x;
            shield.shieldOffsetY = rotatedOffset.z;
            lastGravshipRotations[shield] = currentRotation.AsInt;
        }

        public static void CaptureGravshipThings(Gravship gravship)
        {
            if (gravship == null || gravshipThingsField == null)
            {
                return;
            }

            IDictionary things = gravshipThingsField.GetValue(gravship) as IDictionary;
            if (things == null)
            {
                return;
            }

            foreach (DictionaryEntry entry in things)
            {
                Thing thing = entry.Key as Thing;
                if (thing == null)
                {
                    continue;
                }

                Comp_Shield shield = thing.TryGetComp<Comp_Shield>();
                if (AppliesTo(shield))
                {
                    lastGravshipRotations[shield] = gravship.Rotation.AsInt;
                }
            }
        }

        public static void BeginSpawningGravship(Gravship gravship)
        {
            Rot4 rotation = gravship != null ? gravship.Rotation : Rot4.North;
            int pendingRotation;
            if (gravship != null && pendingLandingRotations.TryGetValue(gravship, out pendingRotation))
            {
                rotation = new Rot4(pendingRotation);
                pendingLandingRotations.Remove(gravship);
            }
            currentSpawningGravshipRotation = rotation.AsInt;
        }

        public static void EndSpawningGravship()
        {
            currentSpawningGravshipRotation = null;
        }

        public static void SetPendingLandingRotation(Gravship gravship, Rot4 landingRotation)
        {
            if (gravship != null)
            {
                pendingLandingRotations[gravship] = landingRotation.AsInt;
            }
        }

        public static void ExposeLastRotation(Comp_Shield shield)
        {
            if (!AppliesTo(shield))
            {
                return;
            }

            int lastRotation = -1;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                EnsureCurrentRotation(shield);
                lastGravshipRotations.TryGetValue(shield, out lastRotation);
            }

            Scribe_Values.Look(ref lastRotation, "shieldOffsetLastGravshipRotation", -1);

            if (Scribe.mode == LoadSaveMode.LoadingVars && lastRotation >= 0)
            {
                lastGravshipRotations[shield] = lastRotation;
            }
        }

        private static Rot4 CurrentRotation(Comp_Shield shield)
        {
            if (currentSpawningGravshipRotation.HasValue)
            {
                return new Rot4(currentSpawningGravshipRotation.Value);
            }

            int lastRotation;
            if (lastGravshipRotations.TryGetValue(shield, out lastRotation))
            {
                return new Rot4(lastRotation);
            }

            return shield.parent.Rotation;
        }
    }

    [HarmonyPatch(typeof(WorldComponent_GravshipController), "RemoveGravshipFromMap")]
    public static class Patch_GravshipController_RemoveGravshipFromMap
    {
        public static void Postfix(Gravship __result)
        {
            Patch_ShieldOffsetGravshipRotation.CaptureGravshipThings(__result);
        }
    }

    [HarmonyPatch(typeof(WorldComponent_GravshipController), nameof(WorldComponent_GravshipController.InitiateLanding))]
    public static class Patch_GravshipController_InitiateLanding
    {
        public static void Prefix(Gravship gravship, Rot4 landingRot)
        {
            Patch_ShieldOffsetGravshipRotation.SetPendingLandingRotation(gravship, landingRot);
        }
    }

    [HarmonyPatch(typeof(GravshipPlacementUtility), "SpawnNonPawnThings")]
    public static class Patch_GravshipPlacementUtility_SpawnNonPawnThings
    {
        public static void Prefix(Gravship gravship)
        {
            Patch_ShieldOffsetGravshipRotation.BeginSpawningGravship(gravship);
        }

        public static void Postfix()
        {
            Patch_ShieldOffsetGravshipRotation.EndSpawningGravship();
        }
    }

    [HarmonyPatch(typeof(Comp_Shield), nameof(Comp_Shield.PostSpawnSetup))]
    public static class Patch_CompShield_PostSpawnSetup
    {
        public static void Postfix(Comp_Shield __instance)
        {
            Patch_ShieldOffsetGravshipRotation.EnsureCurrentRotation(__instance);
            Patch_ShieldOffsetGravshipRotation.RotateOffsetIfNeeded(__instance);
        }
    }

    [HarmonyPatch(typeof(Comp_Shield), nameof(Comp_Shield.PostExposeData))]
    public static class Patch_CompShield_PostExposeData
    {
        public static void Postfix(Comp_Shield __instance)
        {
            Patch_ShieldOffsetGravshipRotation.ExposeLastRotation(__instance);
        }
    }

    [HarmonyPatch(typeof(Comp_Shield), "set_SetShieldOffsetX")]
    public static class Patch_CompShield_SetShieldOffsetX
    {
        public static void Postfix(Comp_Shield __instance)
        {
            Patch_ShieldOffsetGravshipRotation.ResetCurrentRotation(__instance);
        }
    }

    [HarmonyPatch(typeof(Comp_Shield), "set_SetShieldOffsetY")]
    public static class Patch_CompShield_SetShieldOffsetY
    {
        public static void Postfix(Comp_Shield __instance)
        {
            Patch_ShieldOffsetGravshipRotation.ResetCurrentRotation(__instance);
        }
    }
}

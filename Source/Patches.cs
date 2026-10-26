using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CaravanCarriers
{
    // Vanilla caravan speed: every humanlike is a rider, rideable animals are sorted best-first, each rider takes the
    // best one left (riders without one count as 1.0), and the caravan uses the average. Three calls in the first loop
    // are swapped so carriers stand in for animals. Everything after that, including the speed tooltip, is vanilla's
    // own code working on vanilla's own variables, so the tooltip stays consistent. Identical in 1.6 and 1.7.
    [HarmonyPatch(typeof(CaravanTicksPerMoveUtility), nameof(CaravanTicksPerMoveUtility.GetTicksPerMove),
        new[] { typeof(List<Pawn>), typeof(float), typeof(float), typeof(bool), typeof(StringBuilder) })]
    public static class Patch_GetTicksPerMove
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo raceProps = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.RaceProps));
            MethodInfo humanlike = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike));
            MethodInfo rideable = AccessTools.Method(typeof(CaravanRideableUtility), nameof(CaravanRideableUtility.IsCaravanRideable), new[] { typeof(Pawn) });
            MethodInfo getStat = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue), new[] { typeof(Thing), typeof(StatDef), typeof(bool), typeof(int) });

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int riderCheck = -1;
            int rideableCheck = -1;
            int factorRead = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (riderCheck < 0 && i + 1 < codes.Count && codes[i].Calls(raceProps) && codes[i + 1].Calls(humanlike))
                {
                    riderCheck = i;
                }
                else if (riderCheck >= 0 && rideableCheck < 0 && codes[i].Calls(rideable))
                {
                    rideableCheck = i;
                }
                else if (rideableCheck >= 0 && factorRead < 0 && codes[i].Calls(getStat))
                {
                    factorRead = i;
                }
            }
            if (riderCheck < 0 || rideableCheck < 0 || factorRead < 0)
            {
                Log.Error("[Caravan Carriers] CaravanTicksPerMoveUtility.GetTicksPerMove is not shaped as expected; carriers will not speed up caravans.");
                return codes;
            }

            // pawn.RaceProps.Humanlike -> CountsAsRider(pawn). Same stack: Pawn in, bool out. Labels stay on the first.
            codes[riderCheck].opcode = OpCodes.Call;
            codes[riderCheck].operand = AccessTools.Method(typeof(CaravanCarry), nameof(CaravanCarry.CountsAsRider));
            codes[riderCheck + 1].opcode = OpCodes.Nop;
            codes[riderCheck + 1].operand = null;
            // pawn.IsCaravanRideable() -> CanBeRidden(pawn)
            codes[rideableCheck].operand = AccessTools.Method(typeof(CaravanCarry), nameof(CaravanCarry.CanBeRidden));
            // pawn.GetStatValue(CaravanRidingSpeedFactor, ...) -> RideFactor(pawn, CaravanRidingSpeedFactor, ...)
            codes[factorRead].operand = AccessTools.Method(typeof(CaravanCarry), nameof(CaravanCarry.RideFactor));
            return codes;
        }
    }

    // The caravan and transport dialogs draw a "rideable" icon only next to animals. Give carriers the same icon,
    // with a tooltip saying what they give.
    [HarmonyPatch(typeof(TransferableUIUtility), nameof(TransferableUIUtility.DoExtraIcons))]
    public static class Patch_DoExtraIcons
    {
        private const float IconWidth = 24f;

        public static void Postfix(Transferable trad, Rect rect, ref float curX)
        {
            if (!(trad.AnyThing is Pawn pawn) || !pawn.RaceProps.Humanlike)
            {
                return;
            }
            float factor = CaravanCarry.CarryFactor(pawn);
            if (factor <= 1f)
            {
                return;
            }
            Rect iconRect = new Rect(curX - IconWidth, (rect.height - IconWidth) / 2f, IconWidth, IconWidth);
            curX -= IconWidth;
            GUI.DrawTexture(iconRect, CarrierIcon.Texture);
            if (Mouse.IsOver(iconRect))
            {
                TooltipHandler.TipRegion(iconRect, "CaravanCarriers_CarrierTip".Translate(factor.ToStringPercent()));
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class CarrierIcon
    {
        public static readonly Texture2D Texture = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Rideable");
    }
}

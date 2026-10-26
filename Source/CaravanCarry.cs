using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CaravanCarriers
{
    // A carrier is a humanlike that carries one caravan member the way a rideable animal does.
    public static class CaravanCarry
    {
        // The speed factor a carrier gives the member it carries; 1 means it is not a carrier.
        // Its own caravan riding speed stat counts (genes or race can raise it), and with the size rule on, any adult
        // at or above the size threshold gets at least the configured factor.
        public static float CarryFactor(Pawn pawn)
        {
            if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike || pawn.Downed || !pawn.DevelopmentalStage.Adult())
            {
                return 1f;
            }
            float factor = pawn.GetStatValue(StatDefOf.CaravanRidingSpeedFactor);
            CaravanCarriersSettings settings = CaravanCarriersMod.settings;
            if (settings.sizeRule && pawn.BodySize >= settings.minBodySize && settings.sizeFactor > factor)
            {
                factor = settings.sizeFactor;
            }
            return factor;
        }

        public static bool IsCarrier(Pawn pawn)
        {
            return CarryFactor(pawn) > 1f;
        }

        // The three methods below are swapped into CaravanTicksPerMoveUtility.GetTicksPerMove.

        // Replaces pawn.RaceProps.Humanlike in the rider count: a carrier is not a rider, just as a horse is not.
        public static bool CountsAsRider(Pawn pawn)
        {
            return pawn.RaceProps.Humanlike && !IsCarrier(pawn);
        }

        // Replaces pawn.IsCaravanRideable().
        public static bool CanBeRidden(Pawn pawn)
        {
            return pawn.IsCaravanRideable() || IsCarrier(pawn);
        }

        // Replaces pawn.GetStatValue(StatDefOf.CaravanRidingSpeedFactor) for the ridden pawn.
        public static float RideFactor(Thing thing, StatDef stat, bool applyPostProcess, int cacheStaleAfterTicks)
        {
            if (thing is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                return CarryFactor(pawn);
            }
            return thing.GetStatValue(stat, applyPostProcess, cacheStaleAfterTicks);
        }
    }
}

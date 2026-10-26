using HarmonyLib;
using UnityEngine;
using Verse;

namespace CaravanCarriers
{
    public class CaravanCarriersSettings : ModSettings
    {
        // Big pawns carry even without a caravan riding speed stat of their own.
        public bool sizeRule = true;
        public float minBodySize = 2f;
        public float sizeFactor = 1.3f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref sizeRule, "sizeRule", true);
            Scribe_Values.Look(ref minBodySize, "minBodySize", 2f);
            Scribe_Values.Look(ref sizeFactor, "sizeFactor", 1.3f);
        }
    }

    public class CaravanCarriersMod : Mod
    {
        public static CaravanCarriersSettings settings;

        public CaravanCarriersMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<CaravanCarriersSettings>();
            new Harmony("DanZinagri.CaravanCarriers").PatchAll(typeof(CaravanCarriersMod).Assembly);
        }

        public override string SettingsCategory()
        {
            return "Caravan Carriers";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("A carrier carries one caravan member the way a rideable animal does, and is not counted as a rider itself. Changes apply straight away.");
            listing.Gap();
            listing.Label("Any adult whose caravan riding speed is above 100% is a carrier and uses that value. Genes or a race can raise it.");
            listing.Gap();
            listing.CheckboxLabeled("Big pawns carry too", ref settings.sizeRule, "Lets large pawns carry without a caravan riding speed of their own.");
            if (settings.sizeRule)
            {
                listing.Label("Minimum body size: " + settings.minBodySize.ToString("0.0"));
                settings.minBodySize = Mathf.Round(listing.Slider(settings.minBodySize, 1.2f, 5f) * 10f) / 10f;
                listing.Label("Caravan speed for the member carried: " + settings.sizeFactor.ToStringPercent());
                settings.sizeFactor = Mathf.Round(listing.Slider(settings.sizeFactor, 1.05f, 2f) * 20f) / 20f;
                GUI.color = Color.gray;
                listing.Label("For comparison, vanilla horses give 160% and most other rideable animals 130%. A carrier with a higher caravan riding speed of its own keeps that instead.");
                GUI.color = Color.white;
            }
            listing.End();
        }
    }
}

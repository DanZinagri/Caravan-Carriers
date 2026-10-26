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
            return "CaravanCarriers_SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("CaravanCarriers_Intro".Translate());
            listing.Gap();
            listing.Label("CaravanCarriers_StatRule".Translate());
            listing.Gap();
            listing.CheckboxLabeled("CaravanCarriers_SizeRule".Translate(), ref settings.sizeRule, "CaravanCarriers_SizeRuleTip".Translate());
            if (settings.sizeRule)
            {
                listing.Label("CaravanCarriers_MinBodySize".Translate(settings.minBodySize.ToString("0.0")));
                settings.minBodySize = Mathf.Round(listing.Slider(settings.minBodySize, 1.2f, 5f) * 10f) / 10f;
                listing.Label("CaravanCarriers_SizeFactor".Translate(settings.sizeFactor.ToStringPercent()));
                settings.sizeFactor = Mathf.Round(listing.Slider(settings.sizeFactor, 1.05f, 2f) * 20f) / 20f;
                GUI.color = Color.gray;
                listing.Label("CaravanCarriers_SizeFactorNote".Translate());
                GUI.color = Color.white;
            }
            listing.End();
        }
    }
}

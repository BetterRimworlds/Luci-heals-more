﻿using RimWorld;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LHM
{
    public class HediffCompProperties_HealPermanentWounds : HediffCompProperties
    {
        public HediffCompProperties_HealPermanentWounds()
        {
            compClass = typeof(HediffComp_HealPermanentWounds);
        }
    }

    public class HediffComp_HealPermanentWounds : HediffComp
    {
        private int ticksToHeal;
        private List<string> chronicConditions = new List<string>()
        {
            "BadBack", "Frail", "Cataract", "Blindness", "HearingLoss", "Dementia", "Alzheimers",
            "Asthma", "HeartArteryBlockage", "Carcinoma", "TraumaSavant", "Cirrhosis",
            "ChemicalDamageSevere", "ChemicalDamageModerate"
        };

        public HediffCompProperties_HealPermanentWounds Props => (HediffCompProperties_HealPermanentWounds)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
            if (Settings.Get().debugHealingSpeed)
            {
                ticksToHeal = 3000;
            }
            else
            {
                ticksToHeal = Rand.Range(4 * 60000, 6 * 60000); // one day = 60'000 ticks
            }

        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                AffectPawnsAge();
                ResetTicksToHeal();
            }
        }

        private void TryHealRandomPermanentWound()
        {
            //TODO: there should be a way to find chronic diseases without this list for better mod compatibility
            var selectHediffsQuery = from hd in base.Pawn.health.hediffSet.hediffs
                         where hd.IsPermanent() || chronicConditions.Contains(hd.def.defName)
                         select hd;
            if (selectHediffsQuery.Any()) {
                selectHediffsQuery.TryRandomElement(out Hediff hediff);

                if (hediff != null)
                {
                    float meanHeal = 0.2f;
                    float rndHealPercent = meanHeal + (Rand.Gaussian() * meanHeal / 2f); // heal % is normaly distributed between 10 % and 30 %

                    float bodyPartMaxHP = hediff.Part.def.GetMaxHealth(hediff.pawn);
                    float healAmount = bodyPartMaxHP * rndHealPercent;
                    if (healAmount < 0.1f) healAmount = 0.1f;
                    if (hediff.Severity - healAmount < 0.1f) base.Pawn.health.hediffSet.hediffs.Remove(hediff);
                    else hediff.Severity -= healAmount;

                }

                if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
                {
                    Messages.Message("MessagePermanentWoundHealed".Translate(parent.LabelCap,
                        base.Pawn.LabelShort,
                        hediff.Label), base.Pawn, MessageTypeDefOf.PositiveEvent, true);
                }
            }
        }

        private void AffectPawnsAge()
        {
            if (base.Pawn.RaceProps.Humanlike)
            {
                if (base.Pawn.ageTracker.AgeBiologicalYears > 25)
                {
                    int biologicalYears;
                    int biologicalQuadrums;
                    int biologicalDays;
                    float num4;

                    base.Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out num4);

                    string ageBefore = "AgeBiological".Translate(new object[] { biologicalYears, biologicalQuadrums, biologicalDays });
                    long diffFromOptimalAge = base.Pawn.ageTracker.AgeBiologicalTicks - 25 * 60 * 60000;
                    base.Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);

                    base.Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out num4);
                    string ageAfter = "AgeBiological".Translate(new object[] { biologicalYears, biologicalQuadrums, biologicalDays });

                    if (base.Pawn.IsColonist && Settings.Get().showAgingMessages)
                    {
                        Messages.Message("MessageAgeReduced".Translate(new object[]
                            {
                                base.Pawn.LabelShort,
                                ageBefore,
                                ageAfter
                            }), MessageTypeDefOf.PositiveEvent);
                        Messages.Message("MessageAgeReduced".Translate(parent.LabelCap, base.Pawn.LabelShort, ageBefore, ageAfter), 
                            base.Pawn, MessageTypeDefOf.PositiveEvent, true);
                    }

                }
                else if (base.Pawn.ageTracker.AgeBiologicalYears < 25) // if 25 do nothing, if younger that 25, that mature faster towards 25
                {
                    base.Pawn.ageTracker.AgeBiologicalTicks += (long)(15 * 60000); // get one quadrum older
                }

            }
            else // if not humanlike then opimal age is the start of the third stage
            {
                int lifeStage = base.Pawn.ageTracker.CurLifeStageIndex;
                long startOfThirdStage = (long)(base.Pawn.RaceProps.lifeStageAges[2].minAge * 60 * 60000);
                long diffFromOptimalAge = base.Pawn.ageTracker.AgeBiologicalTicks - startOfThirdStage;
                if (lifeStage >= 3) // then need to become younger
                {
                    base.Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);
                }
                else // in that case mature faster towards 3rd stage
                {
                    base.Pawn.ageTracker.AgeBiologicalTicks += (long)(5 * 60000); // get 5 days older
                }

            }

        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }
    }

    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<Settings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Luci heals more!";
        }
    }

    class Settings : ModSettings
    {
        public bool showAgingMessages = false;
        public bool debugHealingSpeed = false;

        public static Settings Get()
        {
            return LoadedModManager.GetMod<LHM.Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            options.CheckboxLabeled("Show aging messages", checkOn: ref showAgingMessages, tooltip: "Show notification every time age was affected by luci");
            options.CheckboxLabeled("Debug luci healing", checkOn: ref debugHealingSpeed, tooltip: "Luci heal procs way more often");

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showAgingMessages, "showAgingMessages", false);
            Scribe_Values.Look(ref debugHealingSpeed, "debugHealingSpeed", false);
        }
    }

}

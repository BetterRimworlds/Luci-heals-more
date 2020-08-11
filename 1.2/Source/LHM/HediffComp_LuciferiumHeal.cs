﻿using RimWorld;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace LHM
{
    public class HediffComp_LuciferiumHeal : HediffComp
    {
        private const int DaysInYear = 60;
        private const int OptimalAge = 25;
        private const float MinimalHealthAmount = 0.01f;

        private int ticksToHeal;

        public HediffCompProperties_LuciferiumHeal Props => (HediffCompProperties_LuciferiumHeal) props;

        public HashSet<string> AdditionalHedifsToHeal { get; } = new HashSet<string>()
        {
            "ChemicalDamageSevere", "ChemicalDamageModerate", "Cirrhosis"
        };

        public HediffComp_LuciferiumHeal()
        {
<<<<<<< HEAD
            if(ticksToHeal > 6 * GenDate.TicksPerDay) ResetTicksToHeal();
=======
            if (ticksToHeal > 6 * GenDate.TicksPerDay) ResetTicksToHeal();
>>>>>>> 9bef1a6... wip
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
<<<<<<< HEAD
            // next heal event will happen after an hour in the debug mode or after 4 to 6 days (uniformly distributed) normaly
            ticksToHeal = Settings.Get().enableDebugHealingSpeed ? 2500 : Rand.Range(4 * GenDate.TicksPerDay, 6 * GenDate.TicksPerDay); 
=======
            // next heal event will happen after an hour in the debug mode or after 4 to 6 days (uniformly distributed) in normal mode
            ticksToHeal = Settings.Get().debugHealingSpeed ? 2500 : Rand.Range(4 * GenDate.TicksPerDay, 6 * GenDate.TicksPerDay);
>>>>>>> 9bef1a6... wip
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
<<<<<<< HEAD
            if (ticksToHeal >= 6 * GenDate.TicksPerDay) ResetTicksToHeal();
=======
            if (ticksToHeal >= 6 * GenDate.TicksPerDay)
                ResetTicksToHeal();
>>>>>>> 9bef1a6... wip
            else if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                if (Settings.Get().shouldAffectAge) AffectPawnsAge();
                ResetTicksToHeal();
            }
        }

        private void TryHealRandomPermanentWound()
        {
            var selectHediffsQuery = from hd in Pawn.health.hediffSet.hediffs
                                     where 
                                         hd.IsPermanent() 
                                         || hd.def.chronic 
                                         || AdditionalHedifsToHeal.Contains(hd.def.defName) 
                                         || (hd.def.defName.Equals("TraumaSavant") && Settings.Get().healTraumaSavant)
                                     select hd;

            if (selectHediffsQuery.TryRandomElement(out Hediff hediff))
            {
                float meanHeal = 0.2f;
                float healDeviation = 0.1f;
                float rndHealPercentValue = meanHeal + Rand.Gaussian() * healDeviation; // heal % is normaly distributed between 10 % and 30 %

                float bodyPartMaxHP = hediff.Part.def.GetMaxHealth(hediff.pawn);
                float rawHealAmount = hediff.IsPermanent() ? bodyPartMaxHP * rndHealPercentValue : rndHealPercentValue;
                float healAmount = (rawHealAmount < MinimalHealthAmount) ? MinimalHealthAmount : rawHealAmount;

                if (hediff.Severity - healAmount < MinimalHealthAmount)
                    HandleLowSeverity(hediff);
                else
                    hediff.Severity -= healAmount;
            }

            TryRegrowMissingBodypart();
        }

        private void HandleLowSeverity(Hediff hediff)
        {
            Pawn.health.RemoveHediff(hediff);
            if (PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("MessagePermanentWoundHealed".Translate(
                        parent.LabelCap,
                        Pawn.LabelShort,
                        hediff.Label,
                        Pawn.Named("PAWN")
                    ),
                    Pawn, MessageTypeDefOf.PositiveEvent, true
                );
            }
        }

        private void AffectPawnsAge()
        {
            if (Pawn.RaceProps.Humanlike)
            {
<<<<<<< HEAD
                if (Pawn.ageTracker.AgeBiologicalYears > 25) ReduceAgeOfHumanlike();
                else if (Pawn.ageTracker.AgeBiologicalYears < 25) Pawn.ageTracker.AgeBiologicalTicks += (long)(15 * GenDate.TicksPerDay); // get one quadrum older
=======
                if (Pawn.ageTracker.AgeBiologicalYears > OptimalAge)
                    ReduceAgeOfHumanlike();
                else if (Pawn.ageTracker.AgeBiologicalYears < OptimalAge)
                    Pawn.ageTracker.AgeBiologicalTicks += (long)(15 * GenDate.TicksPerDay); // get one quadrum older
>>>>>>> 9bef1a6... wip
            }
            else // if not humanlike then optimal age is the start of the third stage
            {
                int lifeStage = Pawn.ageTracker.CurLifeStageIndex;
<<<<<<< HEAD
                long startOfThirdStage = (long)(Pawn.RaceProps.lifeStageAges[2].minAge * 60 * GenDate.TicksPerDay);
=======
                long startOfThirdStage = (long)(Pawn.RaceProps.lifeStageAges[2].minAge * DaysInYear * GenDate.TicksPerDay);
>>>>>>> 9bef1a6... wip
                long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - startOfThirdStage;

                if (lifeStage >= 3 && diffFromOptimalAge > 0) // then need to become younger
                {
                    Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);
                }
                else // in that case mature faster towards 3rd stage
                {
                    Pawn.ageTracker.AgeBiologicalTicks += (long)(5 * GenDate.TicksPerDay); // get 5 days older
                }
            }

        }

        private void ReduceAgeOfHumanlike()
        {
            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out int biologicalYears, out int biologicalQuadrums, out int biologicalDays, out float biologicalHours);

            string ageBefore = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);
            long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - OptimalAge * DaysInYear * GenDate.TicksPerDay;
            Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);

            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out biologicalHours);
            string ageAfter = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);

            if (Pawn.IsColonist && Settings.Get().showAgingMessages)
            {
                Messages.Message("MessageAgeReduced".Translate(
                        Pawn.LabelShort,
                        ageBefore,
                        ageAfter
                    ),
                    MessageTypeDefOf.PositiveEvent, true
                );
            }
        }

        private void TryRegrowMissingBodypart()
        {
            HediffDef regrowingHediffDef = LHM_HediffDefOf.RegrowingBodypart;
            BodyPartRecord missingPart = FindBiggestMissingBodyPart();

            if(regrowingHediffDef == null)
            {
                Log.Warning("HediffDef for regrowing bodypart is not loaded");
            }

            if(missingPart != null)
            {
                Pawn.health.RestorePart(missingPart);
                Pawn.health.AddHediff(HediffMaker.MakeHediff(regrowingHediffDef, Pawn, missingPart));
                Log.Message("Regrowing Hediff added");
            }
        }

        private BodyPartRecord FindBiggestMissingBodyPart()
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (Hediff_MissingPart missingPartsCommonAncestor in Pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (
                    !Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(missingPartsCommonAncestor.Part) && 
                    (bodyPartRecord == null || missingPartsCommonAncestor.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren))
                {
                    bodyPartRecord = missingPartsCommonAncestor.Part;
                }
            }
            return bodyPartRecord;
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }
    }

}

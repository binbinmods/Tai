using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Tai.CustomFunctions;
using static Tai.Plugin;
using static Tai.DescriptionFunctions;
using static Tai.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Tai
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }
            string traitName = traitData.TraitName;
            string traitId = _trait;


            if (_trait == trait0)
            {
                // When you play a Shadow Spell, apply 2 Sanctify to everyone. When you play a Holy Spell, apply 2 Dark to everyone (once per turn).

                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (CanIncrementTraitActivations(traitId) && (_castedCard.HasCardType(Enums.CardType.Holy_Spell) || _castedCard.HasCardType(Enums.CardType.Shadow_Spell)))// && MatchManager.Instance.energyJustWastedByHero > 0)
                {
                    LogDebug($"Handling Trait {traitId}: {traitName}");

                    if (_castedCard.HasCardType(Enums.CardType.Shadow_Spell))
                    {
                        ApplyAuraCurseToAll("sanctify", 2, AppliesTo.Global, sourceCharacter: _character, useCharacterMods: true);
                    }
                    if (_castedCard.HasCardType(Enums.CardType.Holy_Spell))
                    {
                        ApplyAuraCurseToAll("dark", 2, AppliesTo.Global, sourceCharacter: _character, useCharacterMods: true);
                    }
                    IncrementTraitActivations(traitId);
                }
            }


            else if (_trait == trait2a)
            {
                // trait2a
                // Double's Sanctify's effectiveness, but Sanctify explodes at 38 charges, dealing 2 Shadow Damage per charge. Dark explosions deal Holy Damage.

            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // Sanctify +2, Dark +2
                LogDebug($"Handling Trait {traitId}: {traitName}");

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // When dark or Sanctify explode, randomly apply energize and dark or inspire and sanctify to a random hero
                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                // On hit, apply 2 Dark and 2 Sanctify. Dark and Sanctify explosions deal 30% more damage.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                _target.SetAuraTrait(_character, "sanctify", 2);
                _target.SetAuraTrait(_character, "dark", 2);
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait2a:
                // Double's Sanctify's effectiveness, but 
                // Sanctify explodes at 38 charges, dealing 2 Shadow Damage per charge. 
                // Dark explosions deal Holy Damage.


                // trait2b:
                // Sanctify +2, Dark +2

                // trait 4a;
                // When dark or Sanctify explode, randomly apply energize and dark or inspire and sanctify to a random hero

                // trait 4b:
                // Dark and Sanctify explosions deal 30% more damage.

                case "sanctify":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Global))
                    {

                        __result.HealAttackerPerStack *= 2;
                        __result.ResistModifiedPercentagePerStack *= 2;
                        __result.ResistModifiedPercentagePerStack2 *= 2;
                        __result.ExplodeAtStacks = 38;
                        __result.DamageWhenConsumedPerCharge = 2;
                        __result.DamageTypeWhenConsumed = Enums.DamageType.Shadow;
                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Global))
                    {
                        __result.DamageWhenConsumedPerCharge *= 1.3f;
                    }
                    break;
                case "dark":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Heroes))
                    {
                        __result.DamageTypeWhenConsumed = Enums.DamageType.Holy;
                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Heroes))
                    {
                        __result.DamageWhenConsumedPerCharge *= 1.3f;
                    }
                    break;
            }
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        // public static void HealAuraCursePrefix(ref Character __instance, AuraCurseData AC, ref int __state)
        // {
        //     LogInfo($"HealAuraCursePrefix {subclassName}");
        //     string traitOfInterest = trait4b;
        //     if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth"))
        //     {
        //         __state = Mathf.FloorToInt(__instance.GetAuraCharges("stealth") * 0.25f);
        //         // __instance.SetAuraTrait(null, "stealth", 1);

        //     }

        // }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        // public static void HealAuraCursePostfix(ref Character __instance, AuraCurseData AC, int __state)
        // {
        //     LogInfo($"HealAuraCursePrefix {subclassName}");
        //     string traitOfInterest = trait4b;
        //     if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth") && __state > 0)
        //     {
        //         // __state = __instance.GetAuraCharges("stealth");
        //         __instance.SetAuraTrait(null, "stealth", __state);
        //     }

        // }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPostfix(
            Character __instance,
            Enums.EventActivation theEvent,
            Character target = null,
            int auxInt = 0,
            string auxString = "")
        {
            if (theEvent == Enums.EventActivation.AuraCurseSet && __instance.Alive && __instance != null && AtOManager.Instance.TeamHaveTrait(trait4a))
            {
                int nCharges = __instance.GetAuraCharges(auxString) + auxInt;
                bool ACExplodes = nCharges >= GetAuraCurseData(auxString).ExplodeAtStacks;
                if (ACExplodes)
                {
                    LogDebug($"{auxString} explodes on {__instance.Id} with {__instance.GetAuraCharges(auxString)} + {auxInt} charges.");
                    // When dark or Sanctify explode, randomly apply energize and dark or inspire and sanctify to a random hero
                    Hero[] teamHeroes = MatchManager.Instance.GetTeamHero();
                    Character randomCharacter = GetRandomCharacter(teamHeroes);

                    if (MatchManager.Instance.GetRandomIntRange(0, 2) == 0)
                    {
                        // Apply energize and dark
                        randomCharacter.SetAuraTrait(__instance, "energize", 1);
                        randomCharacter.SetAuraTrait(__instance, "dark", 5);
                    }
                    else
                    {
                        // Apply inspire and sanctify
                        randomCharacter.SetAuraTrait(__instance, "inspire", 1);
                        randomCharacter.SetAuraTrait(__instance, "sanctify", 5);
                    }


                }

            }

        }





        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CardData), nameof(CardData.SetDescriptionNew))]
        // public static void SetDescriptionNewPostfix(ref CardData __instance, bool forceDescription = false, Character character = null, bool includeInSearch = true)
        // {
        //     // LogInfo("executing SetDescriptionNewPostfix");
        //     if (__instance == null)
        //     {
        //         LogDebug("Null Card");
        //         return;
        //     }
        //     if (!Globals.Instance.CardsDescriptionNormalized.ContainsKey(__instance.Id))
        //     {
        //         LogError($"missing card Id {__instance.Id}");
        //         return;
        //     }


        //     if (__instance.CardName == "Mind Maze")
        //     {
        //         StringBuilder stringBuilder1 = new StringBuilder();
        //         LogDebug($"Current description for {__instance.Id}: {stringBuilder1}");
        //         string currentDescription = Globals.Instance.CardsDescriptionNormalized[__instance.Id];
        //         stringBuilder1.Append(currentDescription);
        //         // stringBuilder1.Replace($"When you apply", $"When you play a Mind Spell\n or apply");
        //         stringBuilder1.Replace($"Lasts one turn", $"Lasts two turns");
        //         BinbinNormalizeDescription(ref __instance, stringBuilder1);
        //     }
        // }

    }
}


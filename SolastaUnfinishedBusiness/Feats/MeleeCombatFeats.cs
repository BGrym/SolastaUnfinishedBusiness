﻿using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static RuleDefinitions.RollContext;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.WeaponTypeDefinitions;

namespace SolastaUnfinishedBusiness.Feats;

internal static class MeleeCombatFeats
{
    internal static void CreateFeats([NotNull] List<FeatDefinition> feats)
    {
        var featBladeMastery = BuildBladeMastery();
        var featCrusherStr = BuildCrusherStr();
        var featCrusherCon = BuildCrusherCon();
        var featDefensiveDuelist = BuildDefensiveDuelist();
        var featFellHanded = BuildFellHanded();
        var featPiercerDex = BuildPiercerDex();
        var featPiercerStr = BuildPiercerStr();
        var featPowerAttack = BuildPowerAttack();
        var featRecklessAttack = BuildRecklessAttack();
        var featSavageAttack = BuildSavageAttack();
        var featSlasherStr = BuildSlasherStr();
        var featSlasherDex = BuildSlasherDex();
        var featSpearMastery = BuildSpearMastery();

        feats.AddRange(
            featBladeMastery,
            featCrusherStr,
            featCrusherCon,
            featDefensiveDuelist,
            featFellHanded,
            featPiercerDex,
            featPiercerStr,
            featPowerAttack,
            featRecklessAttack,
            featSavageAttack,
            featSlasherDex,
            featSlasherStr,
            featSpearMastery);

        var featGroupCrusher = GroupFeats.MakeGroup("FeatGroupCrusher", GroupFeats.Crusher,
            featCrusherStr,
            featCrusherCon);

        var featGroupSlasher = GroupFeats.MakeGroup("FeatGroupSlasher", GroupFeats.Slasher,
            featSlasherDex,
            featSlasherStr);

        GroupFeats.FeatGroupDefenseCombat.AddFeats(
            featDefensiveDuelist);

        GroupFeats.FeatGroupPiercer.AddFeats(
            featPiercerDex,
            featPiercerStr);

        GroupFeats.FeatGroupUnarmoredCombat.AddFeats(
            featGroupCrusher);

        GroupFeats.MakeGroup("FeatGroupMeleeCombat", null,
            GroupFeats.FeatGroupElementalTouch,
            GroupFeats.FeatGroupPiercer,
            FeatDefinitions.DauntingPush,
            FeatDefinitions.DistractingGambit,
            FeatDefinitions.TripAttack,
            featBladeMastery,
            featDefensiveDuelist,
            featFellHanded,
            featPowerAttack,
            featRecklessAttack,
            featSavageAttack,
            featSpearMastery,
            featGroupCrusher,
            featGroupSlasher);
    }

    #region Defensive Duelist

    private static FeatDefinition BuildDefensiveDuelist()
    {
        const string NAME = "FeatDefensiveDuelist";

        var conditionDefensiveDuelist = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Feat)
            .SetFeatures(FeatureDefinitionAttributeModifierBuilder
                .Create($"AttributeModifier{NAME}")
                .SetGuiPresentationNoContent(true)
                .SetModifier(
                    FeatureDefinitionAttributeModifier.AttributeModifierOperation.AddProficiencyBonus,
                    AttributeDefinitions.ArmorClass)
                .AddToDB())
            .SetSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        var powerDefensiveDuelist = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}")
            .SetGuiPresentation(NAME, Category.Feat)
            .SetUsesFixed(ActivationTime.Reaction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder
                        .Create()
                        .SetConditionForm(
                            conditionDefensiveDuelist,
                            ConditionForm.ConditionOperation.Add,
                            true,
                            true)
                        .Build())
                    .Build())
            .SetCustomSubFeatures(new ValidatorsPowerUse(ValidatorsCharacter.MainHandIsFinesseWeapon))
            .AddToDB();

        return FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(powerDefensiveDuelist)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    #endregion

    #region Reckless Attack

    private static FeatDefinition BuildRecklessAttack()
    {
        return FeatDefinitionWithPrerequisitesBuilder
            .Create("FeatRecklessAttack")
            .SetGuiPresentation("RecklessAttack", Category.Action)
            .SetFeatures(ActionAffinityBarbarianRecklessAttack)
            .SetValidators(ValidatorsFeat.ValidateNotClass(CharacterClassDefinitions.Barbarian))
            .AddToDB();
    }

    #endregion

    #region Savage Attack

    private static FeatDefinition BuildSavageAttack()
    {
        return FeatDefinitionBuilder
            .Create("FeatSavageAttack")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionDieRollModifierBuilder
                    .Create("DieRollModifierFeatSavageAttackNonMagic")
                    .SetGuiPresentationNoContent(true)
                    .SetModifiers(AttackDamageValueRoll, 1, 1, 1, "Feat/&FeatSavageAttackReroll")
                    .AddToDB(),
                FeatureDefinitionDieRollModifierBuilder
                    .Create("DieRollModifierFeatSavageAttackMagic")
                    .SetGuiPresentationNoContent(true)
                    .SetModifiers(MagicDamageValueRoll, 1, 1, 1, "Feat/&FeatSavageAttackReroll")
                    .AddToDB())
            .AddToDB();
    }

    #endregion

    #region Spear Mastery

    private static FeatDefinition BuildSpearMastery()
    {
        const string NAME = "FeatSpearMastery";
        const string REACH_CONDITION = $"Condition{NAME}Reach";

        var validWeapon = ValidatorsWeapon.IsOfWeaponType(SpearType);

        var conditionFeatSpearMasteryReach = ConditionDefinitionBuilder
            .Create(REACH_CONDITION)
            .SetGuiPresentation($"Power{NAME}Reach", Category.Feature, ConditionDefinitions.ConditionGuided)
            .SetPossessive()
            .SetSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
            .SetFeatures(FeatureDefinitionBuilder
                .Create($"Feature{NAME}Reach")
                .SetGuiPresentationNoContent(true)
                .SetCustomSubFeatures(new IncreaseMeleeAttackReach(1, validWeapon,
                    ValidatorsCharacter.HasAnyOfConditions(REACH_CONDITION)))
                .AddToDB())
            .AddToDB();

        var powerFeatSpearMasteryReach = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Reach")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite($"Power{NAME}Reach", Resources.SpearMasteryReach, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetParticleEffectParameters(SpellDefinitions.Shield)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(
                        conditionFeatSpearMasteryReach,
                        ConditionForm.ConditionOperation.Add,
                        true,
                        true)
                    .Build())
                .UseQuickAnimations()
                .Build())
            .AddToDB();

        var conditionDamage = ConditionDefinitionBuilder
            .Create($"Condition{NAME}Damage")
            .SetGuiPresentationNoContent(true)
            .SetFeatures(FeatureDefinitionAdditionalDamageBuilder
                .Create($"AdditionalDamage{NAME}")
                .SetGuiPresentationNoContent(true)
                .SetNotificationTag("SpearMastery")
                .SetDamageValueDetermination(AdditionalDamageValueDetermination.SameAsBaseWeaponDie)
                .SetIgnoreCriticalDoubleDice(true)
                // .SetTargetCondition(conditionFeatSpearMasteryCharge, AdditionalDamageTriggerCondition.TargetHasCondition)
                //Adding any property so that custom restricted context would trigger
                .SetRequiredProperty(RestrictedContextRequiredProperty.Weapon)
                .SetCustomSubFeatures(new RestrictedContextValidator((_, _, character, _, ranged, mode, _) =>
                    (OperationType.Set, !ranged && validWeapon(mode, null, character))))
                .AddToDB())
            .AddToDB();

        IEnumerator AddCondition(GameLocationCharacter attacker, GameLocationCharacter defender,
            GameLocationBattleManager manager, GameLocationActionManager actionManager, ReactionRequest request)
        {
            var character = attacker.RulesetCharacter;
            var rulesetCondition = RulesetCondition.CreateActiveCondition(character.Guid, conditionDamage,
                DurationType.Round, 1, TurnOccurenceType.StartOfTurn, character.Guid, string.Empty);

            character.AddConditionOfCategory(AttributeDefinitions.TagCombat, rulesetCondition);

            yield break;
        }

        IEnumerator RemoveCondition(GameLocationCharacter attacker, GameLocationCharacter defender,
            GameLocationBattleManager manager, GameLocationActionManager actionManager, ReactionRequest request)
        {
            attacker.RulesetCharacter.RemoveAllConditionsOfCategoryAndType(AttributeDefinitions.TagCombat,
                conditionDamage.Name);

            yield break;
        }

        var conditionFeatSpearMasteryCharge = ConditionDefinitionBuilder
            .Create($"Condition{NAME}Charge")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionGuided)
            .SetPossessive()
            .SetFeatures(FeatureDefinitionBuilder
                .Create($"Feature{NAME}")
                .SetGuiPresentationNoContent(true)
                .SetCustomSubFeatures(new CanMakeAoOOnReachEntered
                {
                    AccountAoOImmunity = true,
                    WeaponValidator = validWeapon,
                    BeforeReaction = AddCondition,
                    AfterReaction = RemoveCondition
                })
                .AddToDB())
            .AddToDB();

        var powerFeatSpearMasteryCharge = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Charge")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite($"Power{NAME}Charge", Resources.SpearMasteryCharge, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                .SetTargetingData(Side.Ally, RangeType.Self, 1, TargetType.Self)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(conditionFeatSpearMasteryCharge,
                        ConditionForm.ConditionOperation.Add, true, false)
                    .Build())
                .UseQuickAnimations()
                .Build())
            .AddToDB();

        return FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                powerFeatSpearMasteryReach,
                powerFeatSpearMasteryCharge,
                FeatureDefinitionAttackModifierBuilder
                    .Create($"AttackModifier{NAME}")
                    .SetGuiPresentation(Category.Feature)
                    .SetAttackRollModifier(1)
                    .SetCustomSubFeatures(
                        new RestrictedContextValidator((_, _, character, _, ranged, mode, _) =>
                            (OperationType.Set, !ranged && validWeapon(mode, null, character))),
                        new UpgradeWeaponDice((_, _) => (1, DieType.D8, DieType.D10), validWeapon))
                    .AddToDB())
            .AddToDB();
    }

    #endregion

    #region Helpers

    private sealed class ModifyAttackModeForWeaponTypeFilter : IModifyAttackModeForWeapon
    {
        private readonly string _sourceName;
        private readonly List<WeaponTypeDefinition> _weaponTypeDefinition = new();

        public ModifyAttackModeForWeaponTypeFilter(string sourceName,
            params WeaponTypeDefinition[] weaponTypeDefinition)
        {
            _sourceName = sourceName;
            _weaponTypeDefinition.AddRange(weaponTypeDefinition);
        }

        public void ModifyAttackMode(RulesetCharacter character, [CanBeNull] RulesetAttackMode attackMode)
        {
            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            if (damage == null)
            {
                return;
            }

            if (attackMode.sourceDefinition is not ItemDefinition { IsWeapon: true } sourceDefinition ||
                !_weaponTypeDefinition.Contains(sourceDefinition.WeaponDescription.WeaponTypeDefinition))
            {
                return;
            }

            attackMode.ToHitBonus += 1;
            attackMode.ToHitBonusTrends.Add(new TrendInfo(1, FeatureSourceType.CharacterFeature, _sourceName, null));
        }
    }

    #endregion

    #region Common Features

    private static readonly FeatureDefinitionPower PowerFeatCrusherHit = FeatureDefinitionPowerBuilder
        .Create("PowerFeatCrusherHit")
        .SetGuiPresentationNoContent(true)
        .SetUsesFixed(ActivationTime.NoCost, RechargeRate.TurnStart)
        .SetShowCasting(false)
        .SetEffectDescription(EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Enemy, RangeType.Self, 1, TargetType.IndividualsUnique)
            .SetDurationData(DurationType.Instantaneous)
            .SetEffectForms(EffectFormBuilder
                .Create()
                .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 1)
                .Build())
            .Build())
        .AddToDB();

    private static readonly FeatureDefinition FeatureFeatCrusher = FeatureDefinitionAdditionalDamageBuilder
        .Create("FeatureFeatCrusher")
        .SetGuiPresentationNoContent(true)
        .SetTriggerCondition(ExtraAdditionalDamageTriggerCondition.UsePowerReaction)
        .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
        .SetDamageDice(DieType.D1, 0)
        .SetSpecificDamageType(PowerFeatCrusherHit.Name) // use specific type to pass power name to UsePowerReaction
        .SetRequiredProperty(RestrictedContextRequiredProperty.MeleeWeapon)
        .SetCustomSubFeatures(
            new RestrictedContextValidator((_, _, _, _, ranged, mode, _) =>
                (OperationType.Set,
                    !ranged && ValidatorsWeapon.IsOfDamageType(DamageTypeBludgeoning)(mode, null, null))))
        .AddToDB();

    private static readonly FeatureDefinition FeatureFeatCrusherCriticalHit = FeatureDefinitionAdditionalDamageBuilder
        .Create("FeatureFeatCrusherCriticalHit")
        .SetGuiPresentationNoContent(true)
        .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
        .SetDamageDice(DieType.D1, 0)
        .SetNotificationTag(GroupFeats.Crusher)
        .SetCustomSubFeatures(
            new RestrictedContextValidator((_, _, character, _, ranged, mode, _) =>
                (OperationType.Set,
                    !ranged && ValidatorsWeapon.IsOfDamageType(DamageTypeBludgeoning)(mode, null, character))),
            new AfterAttackEffectFeatCrusher(
                ConditionDefinitionBuilder
                    .Create("ConditionFeatCrusherCriticalHit")
                    .SetGuiPresentation("FeatCrusherStr", Category.Feat)
                    .SetSpecialDuration(DurationType.Round, 1)
                    .SetPossessive()
                    .SetFeatures(
                        FeatureDefinitionCombatAffinityBuilder
                            .Create("CombatAffinityFeatCrusher")
                            .SetGuiPresentation("ConditionFeatCrusherCriticalHit", Category.Condition)
                            .SetAttackOnMeAdvantage(AdvantageType.Advantage)
                            .AddToDB())
                    .AddToDB(),
                DamageTypeBludgeoning))
        .AddToDB();

    private static readonly FeatureDefinition FeatureFeatPiercer = FeatureDefinitionBuilder
        .Create("FeatureFeatPiercer")
        .SetGuiPresentationNoContent(true)
        .SetCustomSubFeatures(
            new BeforeAttackEffectFeatPiercer(
                ConditionDefinitionBuilder
                    .Create("ConditionFeatPiercerNonMagic")
                    .SetGuiPresentationNoContent(true)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetSpecialDuration(DurationType.Round, 1)
                    .SetSpecialInterruptions(ConditionInterruption.Attacked)
                    .SetFeatures(
                        FeatureDefinitionDieRollModifierBuilder
                            .Create("DieRollModifierFeatPiercerNonMagic")
                            .SetGuiPresentation("ConditionFeatPiercerNonMagic", Category.Condition)
                            .SetModifiers(AttackDamageValueRoll, 1, 1, 1, "Feat/&FeatPiercerReroll")
                            .AddToDB())
                    .AddToDB(),
                DamageTypePiercing),
            new CustomAdditionalDamageFeatPiercer(
                FeatureDefinitionAdditionalDamageBuilder
                    .Create("AdditionalDamageFeatPiercer")
                    .SetGuiPresentation(Category.Feature)
                    .SetNotificationTag(GroupFeats.Piercer)
                    .SetDamageValueDetermination(AdditionalDamageValueDetermination.SameAsBaseWeaponDie)
                    .SetIgnoreCriticalDoubleDice(true)
                    .AddToDB(),
                DamageTypePiercing))
        .AddToDB();

    private static readonly FeatureDefinition FeatureFeatSlasher = FeatureDefinitionBuilder
        .Create("FeatureFeatSlasher")
        .SetGuiPresentationNoContent(true)
        .SetCustomSubFeatures(
            new AfterAttackEffectFeatSlasher(
                ConditionDefinitionBuilder
                    .Create("ConditionFeatSlasherHit")
                    .SetGuiPresentation(Category.Condition)
                    .SetConditionType(ConditionType.Detrimental)
                    .SetSpecialDuration(DurationType.Round, 1)
                    .SetPossessive()
                    .SetFeatures(
                        FeatureDefinitionMovementAffinityBuilder
                            .Create("MovementAffinityFeatSlasher")
                            .SetGuiPresentation("ConditionFeatSlasherHit", Category.Condition)
                            .SetBaseSpeedAdditiveModifier(-2)
                            .AddToDB())
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create("ConditionFeatSlasherCriticalHit")
                    .SetGuiPresentation(Category.Condition)
                    .SetConditionType(ConditionType.Detrimental)
                    .SetSpecialDuration(DurationType.Round, 1)
                    .SetPossessive()
                    .SetFeatures(
                        FeatureDefinitionCombatAffinityBuilder
                            .Create("CombatAffinityFeatSlasher")
                            .SetGuiPresentation("ConditionFeatSlasherCriticalHit", Category.Condition)
                            .SetMyAttackAdvantage(AdvantageType.Disadvantage)
                            .AddToDB())
                    .AddToDB(),
                DamageTypeSlashing))
        .AddToDB();

    #endregion

    #region Crusher

    private static FeatDefinition BuildCrusherStr()
    {
        return FeatDefinitionBuilder
            .Create("FeatCrusherStr")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Einar,
                FeatureFeatCrusher,
                FeatureFeatCrusherCriticalHit,
                PowerFeatCrusherHit)
            .SetFeatFamily(GroupFeats.Crusher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Strength, 13)
            //.SetCustomSubFeatures(ValidatorsCharacter.MainHandIsOfDamageType(DamageTypeBludgeoning))
            .AddToDB();
    }

    private static FeatDefinition BuildCrusherCon()
    {
        return FeatDefinitionBuilder
            .Create("FeatCrusherCon")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Arun,
                FeatureFeatCrusher,
                FeatureFeatCrusherCriticalHit,
                PowerFeatCrusherHit)
            .SetFeatFamily(GroupFeats.Crusher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Constitution, 13)
            //.SetCustomSubFeatures(ValidatorsCharacter.MainHandIsOfDamageType(DamageTypeBludgeoning))
            .AddToDB();
    }

    private sealed class AfterAttackEffectFeatCrusher : IAfterAttackEffect
    {
        private readonly ConditionDefinition _criticalConditionDefinition;
        private readonly string _damageType;

        internal AfterAttackEffectFeatCrusher(
            ConditionDefinition criticalConditionDefinition,
            string damageType)
        {
            _criticalConditionDefinition = criticalConditionDefinition;
            _damageType = damageType;
        }

        public void AfterOnAttackHit(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode attackMode,
            ActionModifier attackModifier)
        {
            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            if (damage == null || damage.DamageType != _damageType)
            {
                return;
            }

            if (outcome is not RollOutcome.CriticalSuccess)
            {
                return;
            }

            var rulesetCondition = RulesetCondition.CreateActiveCondition(
                defender.RulesetCharacter.Guid,
                _criticalConditionDefinition,
                DurationType.Round,
                1,
                TurnOccurenceType.StartOfTurn,
                attacker.RulesetCharacter.Guid,
                attacker.RulesetCharacter.CurrentFaction.Name);

            defender.RulesetCharacter.AddConditionOfCategory(AttributeDefinitions.TagCombat, rulesetCondition);
        }
    }

    #endregion

    #region Blade Mastery

    private static FeatDefinition BuildBladeMastery()
    {
        const string NAME = "FeatBladeMastery";

        var weaponTypes = new[] { ShortswordType, LongswordType, ScimitarType, RapierType, GreatswordType };

        var validWeapon = ValidatorsWeapon.IsOfWeaponType(weaponTypes);

        var conditionBladeMastery = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Feat)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(FeatureDefinitionAttributeModifierBuilder
                .Create($"AttributeModifier{NAME}")
                .SetGuiPresentationNoContent(true)
                .SetModifier(
                    FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                    AttributeDefinitions.ArmorClass,
                    1)
                .AddToDB())
            .AddToDB();

        var powerBladeMastery = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}")
            .SetGuiPresentation(NAME, Category.Feat)
            .SetUsesFixed(ActivationTime.Reaction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder
                        .Create()
                        .SetConditionForm(
                            conditionBladeMastery,
                            ConditionForm.ConditionOperation.Add,
                            true,
                            true)
                        .Build())
                    .Build())
            .SetCustomSubFeatures(
                new RestrictedContextValidator((_, _, character, _, ranged, mode, _) =>
                    (OperationType.Set, !ranged && validWeapon(mode, null, character))))
            .AddToDB();

        return FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(powerBladeMastery)
            .SetCustomSubFeatures(
                new OnComputeAttackModifierFeatBladeMastery(weaponTypes),
                new ModifyAttackModeForWeaponTypeFilter($"Feature/&ModifyAttackMode{NAME}Title", weaponTypes))
            .AddToDB();
    }


    private sealed class OnComputeAttackModifierFeatBladeMastery : IOnComputeAttackModifier
    {
        private readonly List<WeaponTypeDefinition> _weaponTypeDefinition = new();

        public OnComputeAttackModifierFeatBladeMastery(params WeaponTypeDefinition[] weaponTypeDefinition)
        {
            _weaponTypeDefinition.AddRange(weaponTypeDefinition);
        }

        public void ComputeAttackModifier(
            RulesetCharacter myself,
            RulesetCharacter defender,
            BattleDefinitions.AttackProximity attackProximity,
            RulesetAttackMode attackMode,
            ref ActionModifier attackModifier)
        {
            if (attackProximity != BattleDefinitions.AttackProximity.PhysicalRange &&
                attackProximity != BattleDefinitions.AttackProximity.PhysicalReach)
            {
                return;
            }

            if (attackMode.actionType != ActionDefinitions.ActionType.Reaction)
            {
                return;
            }

            if (!ValidatorsWeapon.IsOfWeaponType(_weaponTypeDefinition.ToArray())(attackMode, null, null))
            {
                return;
            }

            attackModifier.attackAdvantageTrends.Add(
                new TrendInfo(1, FeatureSourceType.Feat, "Feature/&ModifyAttackModeFeatBladeMasteryTitle", null));
        }
    }

    #endregion

    #region Fell Handed

    private static FeatDefinition BuildFellHanded()
    {
        const string NAME = "FeatFellHanded";

        var weaponTypes = new[] { BattleaxeType, GreataxeType, HandaxeType, MaulType, WarhammerType };

        var fellHandedAdvantage = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Advantage")
            .SetGuiPresentation(NAME, Category.Feat, $"Feature/&Power{NAME}AdvantageDescription", hidden: true)
            .SetUsesFixed(ActivationTime.Reaction)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetMotionForm(MotionForm.MotionType.FallProne)
                    .Build())
                .Build())
            .AddToDB();

        var feat = FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetCustomSubFeatures(
                new AfterAttackEffectFeatFellHanded(fellHandedAdvantage, weaponTypes),
                new ModifyAttackModeForWeaponTypeFilter(
                    $"Feature/&ModifyAttackMode{NAME}Title", weaponTypes))
            .AddToDB();

        return feat;
    }

    private sealed class AfterAttackEffectFeatFellHanded : IAfterAttackEffect
    {
        private const string SuretyText = "Feedback/&FeatFeatFellHandedDisadvantage";
        private const string SuretyTitle = "Feat/&FeatFellHandedTitle";
        private const string SuretyDescription = "Feature/&PowerFeatFellHandedDisadvantageDescription";
        private readonly List<WeaponTypeDefinition> _weaponTypeDefinition = new();
        private readonly DamageForm damage;
        private readonly FeatureDefinitionPower power;

        public AfterAttackEffectFeatFellHanded(FeatureDefinitionPower power,
            params WeaponTypeDefinition[] weaponTypeDefinition)
        {
            this.power = power;
            _weaponTypeDefinition.AddRange(weaponTypeDefinition);

            damage = new DamageForm
            {
                DamageType = DamageTypeBludgeoning, DieType = DieType.D1, DiceNumber = 0, BonusDamage = 0
            };
        }

        public void AfterOnAttackHit(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode attackMode,
            ActionModifier attackModifier)
        {
            if (attackMode.sourceDefinition is not ItemDefinition { IsWeapon: true } sourceDefinition ||
                !_weaponTypeDefinition.Contains(sourceDefinition.WeaponDescription.WeaponTypeDefinition))
            {
                return;
            }

            var rulesetAttacker = attacker.RulesetCharacter;
            var rulesetDefender = defender.RulesetCharacter;
            var modifier = attackMode.ToHitBonus + attackModifier.AttackRollModifier;

            switch (attackModifier.AttackAdvantageTrend)
            {
                case > 0 when outcome is RollOutcome.Success or RollOutcome.CriticalSuccess:
                    var lowerRoll = Math.Min(Global.FirstAttackRoll, Global.SecondAttackRoll);

                    var lowOutcome =
                        GameLocationBattleManagerTweaks.GetAttackResult(lowerRoll, modifier, rulesetDefender);

                    Gui.Game.GameConsole.AttackRolled(
                        rulesetAttacker,
                        rulesetDefender,
                        power,
                        lowOutcome,
                        lowerRoll + modifier,
                        lowerRoll,
                        modifier,
                        attackModifier.AttacktoHitTrends,
                        new List<TrendInfo>());

                    if (lowOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
                    {
                        var usablePower = UsablePowersProvider.Get(power, rulesetAttacker);
                        ServiceRepository.GetService<IRulesetImplementationService>()
                            .InstantiateEffectPower(rulesetAttacker, usablePower, false)
                            .ApplyEffectOnCharacter(rulesetDefender, true, defender.LocationPosition);

                        GameConsoleHelper.LogCharacterAffectedByCondition(rulesetDefender,
                            ConditionDefinitions.ConditionProne);
                    }

                    break;
                case < 0 when outcome is RollOutcome.Failure or RollOutcome.CriticalFailure:
                    var higherRoll = Math.Max(Global.FirstAttackRoll, Global.SecondAttackRoll);

                    var strength = rulesetAttacker.GetAttribute(AttributeDefinitions.Strength)
                        .CurrentValue;
                    var strengthMod = AttributeDefinitions.ComputeAbilityScoreModifier(strength);

                    if (strengthMod <= 0)
                    {
                        break;
                    }

                    var higherOutcome =
                        GameLocationBattleManagerTweaks.GetAttackResult(higherRoll, modifier, rulesetDefender);

                    if (higherOutcome is not (RollOutcome.Success or RollOutcome.CriticalSuccess))
                    {
                        break;
                    }

                    GameConsoleHelper.LogCharacterAffectsTarget(rulesetAttacker, rulesetDefender,
                        SuretyTitle, SuretyText, tooltipContent: SuretyDescription);

                    damage.BonusDamage = strengthMod;
                    RulesetActor.InflictDamage(
                        strengthMod,
                        damage,
                        DamageTypeBludgeoning,
                        new RulesetImplementationDefinitions.ApplyFormsParams { targetCharacter = rulesetDefender },
                        rulesetDefender,
                        false,
                        attacker.Guid,
                        false,
                        attackMode.AttackTags,
                        new RollInfo(DieType.D1, new List<int>(), strengthMod),
                        true,
                        out _);

                    break;
            }
        }
    }

    #endregion

    #region Piercer

    private static FeatDefinition BuildPiercerDex()
    {
        return FeatDefinitionBuilder
            .Create("FeatPiercerDex")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Misaye,
                FeatureFeatPiercer)
            .SetFeatFamily(GroupFeats.Piercer)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    private static FeatDefinition BuildPiercerStr()
    {
        return FeatDefinitionBuilder
            .Create("FeatPiercerStr")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Einar,
                FeatureFeatPiercer)
            .SetFeatFamily(GroupFeats.Piercer)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Strength, 13)
            .AddToDB();
    }

    private sealed class BeforeAttackEffectFeatPiercer : IBeforeAttackEffect
    {
        private readonly ConditionDefinition _conditionDefinition;
        private readonly string _damageType;

        internal BeforeAttackEffectFeatPiercer(ConditionDefinition conditionDefinition, string damageType)
        {
            _conditionDefinition = conditionDefinition;
            _damageType = damageType;
        }

        public void BeforeOnAttackHit(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode attackMode,
            ActionModifier attackModifier)
        {
            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            if (damage == null || damage.DamageType != _damageType)
            {
                return;
            }

            var rulesetCondition = RulesetCondition.CreateActiveCondition(
                attacker.RulesetCharacter.Guid,
                _conditionDefinition,
                DurationType.Round,
                1,
                TurnOccurenceType.EndOfTurn,
                attacker.RulesetCharacter.Guid,
                attacker.RulesetCharacter.CurrentFaction.Name);

            attacker.RulesetCharacter.AddConditionOfCategory(AttributeDefinitions.TagCombat, rulesetCondition);
        }
    }

    private sealed class CustomAdditionalDamageFeatPiercer : CustomAdditionalDamage
    {
        private readonly string _damageType;

        public CustomAdditionalDamageFeatPiercer(IAdditionalDamageProvider provider, string damageType) : base(provider)
        {
            _damageType = damageType;
        }

        internal override bool IsValid(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget,
            out CharacterActionParams reactionParams)
        {
            reactionParams = null;

            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            return criticalHit && damage != null && damage.DamageType == _damageType;
        }
    }

    #endregion

    #region Power Attack

    private static FeatDefinition BuildPowerAttack()
    {
        var concentrationProvider = new StopPowerConcentrationProvider("PowerAttack",
            "Tooltip/&PowerAttackConcentration",
            Sprites.GetSprite("PowerAttackConcentrationIcon", Resources.PowerAttackConcentrationIcon, 64, 64));

        var conditionPowerAttackTrigger = ConditionDefinitionBuilder
            .Create("ConditionPowerAttackTrigger")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(FeatureDefinitionBuilder
                .Create("TriggerFeaturePowerAttack")
                .SetGuiPresentationNoContent(true)
                .SetCustomSubFeatures(concentrationProvider)
                .AddToDB())
            .AddToDB();

        var conditionPowerAttack = ConditionDefinitionBuilder
            .Create("ConditionPowerAttack")
            .SetGuiPresentation("FeatPowerAttack", Category.Feat, ConditionDefinitions.ConditionHeraldOfBattle)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                FeatureDefinitionBuilder
                    .Create("ModifyAttackModeForWeaponFeatPowerAttack")
                    .SetGuiPresentation("FeatPowerAttack", Category.Feat)
                    .SetCustomSubFeatures(new ModifyAttackModeForWeaponFeatPowerAttack())
                    .AddToDB())
            .AddToDB();

        var powerAttack = FeatureDefinitionPowerBuilder
            .Create("PowerAttack")
            .SetGuiPresentation("Feat/&FeatPowerAttackTitle",
                Gui.Format("Feat/&FeatPowerAttackDescription", Main.Settings.DeadEyeAndPowerAttackBaseValue.ToString()),
                Sprites.GetSprite("PowerAttackIcon", Resources.PowerAttackIcon, 128, 64))
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttackTrigger, ConditionForm.ConditionOperation.Add)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttack, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetCustomSubFeatures(
                new ValidatorsPowerUse(ValidatorsCharacter.HasNoneOfConditions(conditionPowerAttack.Name)))
            .AddToDB();

        Global.PowersThatIgnoreInterruptions.Add(powerAttack);

        var powerTurnOffPowerAttack = FeatureDefinitionPowerBuilder
            .Create("PowerTurnOffPowerAttack")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Round, 1)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttackTrigger, ConditionForm.ConditionOperation.Remove)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttack, ConditionForm.ConditionOperation.Remove)
                        .Build())
                .Build())
            .AddToDB();

        Global.PowersThatIgnoreInterruptions.Add(powerTurnOffPowerAttack);
        concentrationProvider.StopPower = powerTurnOffPowerAttack;

        return FeatDefinitionBuilder
            .Create("FeatPowerAttack")
            .SetGuiPresentation("Feat/&FeatPowerAttackTitle",
                Gui.Format("Feat/&FeatPowerAttackDescription", Main.Settings.DeadEyeAndPowerAttackBaseValue.ToString()))
            .SetFeatures(
                powerAttack,
                powerTurnOffPowerAttack
            )
            .AddToDB();
    }

    private sealed class ModifyAttackModeForWeaponFeatPowerAttack : IModifyAttackModeForWeapon
    {
        public void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode)
        {
            if (!ValidatorsWeapon.IsMelee(attackMode) && !ValidatorsWeapon.IsUnarmedWeapon(character, attackMode))
            {
                return;
            }

            SrdAndHouseRulesContext.ModifyAttackModeAndDamage(character, "Feat/&FeatPowerAttackTitle", attackMode);
        }
    }

    #endregion

    #region Slasher

    private static FeatDefinition BuildSlasherDex()
    {
        return FeatDefinitionBuilder
            .Create("FeatSlasherDex")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Misaye,
                FeatureFeatSlasher)
            .SetFeatFamily(GroupFeats.Slasher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    private static FeatDefinition BuildSlasherStr()
    {
        return FeatDefinitionBuilder
            .Create("FeatSlasherStr")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Einar,
                FeatureFeatSlasher)
            .SetFeatFamily(GroupFeats.Slasher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Strength, 13)
            .AddToDB();
    }

    private sealed class AfterAttackEffectFeatSlasher : IAfterAttackEffect
    {
        private readonly ConditionDefinition _conditionDefinition;
        private readonly ConditionDefinition _criticalConditionDefinition;
        private readonly string _damageType;

        internal AfterAttackEffectFeatSlasher(
            ConditionDefinition conditionDefinition,
            ConditionDefinition criticalConditionDefinition,
            string damageType)
        {
            _conditionDefinition = conditionDefinition;
            _criticalConditionDefinition = criticalConditionDefinition;
            _damageType = damageType;
        }

        public void AfterOnAttackHit(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode attackMode,
            ActionModifier attackModifier)
        {
            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            if (damage == null || damage.DamageType != _damageType)
            {
                return;
            }

            RulesetCondition rulesetCondition;

            if (outcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
            {
                rulesetCondition = RulesetCondition.CreateActiveCondition(
                    attacker.RulesetCharacter.Guid,
                    _conditionDefinition,
                    DurationType.Round,
                    1,
                    TurnOccurenceType.StartOfTurn,
                    attacker.RulesetCharacter.Guid,
                    attacker.RulesetCharacter.CurrentFaction.Name);

                defender.RulesetCharacter.AddConditionOfCategory(AttributeDefinitions.TagCombat, rulesetCondition);
            }

            if (outcome is not RollOutcome.CriticalSuccess)
            {
                return;
            }

            rulesetCondition = RulesetCondition.CreateActiveCondition(
                defender.RulesetCharacter.Guid,
                _criticalConditionDefinition,
                DurationType.Round,
                0,
                TurnOccurenceType.StartOfTurn,
                attacker.RulesetCharacter.Guid,
                attacker.RulesetCharacter.CurrentFaction.Name);

            defender.RulesetCharacter.AddConditionOfCategory(AttributeDefinitions.TagCombat, rulesetCondition);
        }
    }

    #endregion
}

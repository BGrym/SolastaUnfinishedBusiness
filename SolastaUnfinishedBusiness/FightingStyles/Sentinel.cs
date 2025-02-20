﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class Sentinel : AbstractFightingStyle
{
    private const string SentinelName = "Sentinel";

    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create(SentinelName)
        .SetGuiPresentation(Category.FightingStyle, DatabaseHelper.CharacterSubclassDefinitions.MartialMountaineer)
        .SetFeatures(FeatureDefinitionBuilder
            .Create("OnAttackHitEffectFeatSentinel")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(
                AttacksOfOpportunity.CanIgnoreDisengage,
                AttacksOfOpportunity.SentinelFeatMarker,
                new OnAttackHitEffectFeatSentinel(CustomConditionsContext.StopMovement))
            .AddToDB())
        .AddToDB();

    internal override List<FeatureDefinitionFightingStyleChoice> FightingStyleChoice => new()
    {
        FightingStyleChampionAdditional, FightingStyleFighter, FightingStylePaladin, FightingStyleRanger
    };

    private sealed class OnAttackHitEffectFeatSentinel : IAfterAttackEffect
    {
        private readonly ConditionDefinition _conditionSentinelStopMovement;

        internal OnAttackHitEffectFeatSentinel(ConditionDefinition conditionSentinelStopMovement)
        {
            _conditionSentinelStopMovement = conditionSentinelStopMovement;
        }

        public void AfterOnAttackHit(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode attackMode,
            ActionModifier attackModifier)
        {
            if (outcome != RollOutcome.Success && outcome != RollOutcome.CriticalSuccess)
            {
                return;
            }

            if (attackMode is not { ActionType: ActionDefinitions.ActionType.Reaction })
            {
                return;
            }

            if (attackMode.AttackTags.Contains(AttacksOfOpportunity.NotAoOTag))
            {
                return;
            }

            var character = defender.RulesetCharacter;

            character.AddConditionOfCategory(AttributeDefinitions.TagCombat,
                RulesetCondition.CreateActiveCondition(character.Guid,
                    _conditionSentinelStopMovement,
                    DurationType.Round,
                    1,
                    TurnOccurenceType.StartOfTurn,
                    attacker.Guid,
                    string.Empty
                ));
        }
    }
}

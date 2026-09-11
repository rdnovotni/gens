using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>Sends a real, repeatable goodwill gift to a Foreign People (§6: "sending luxury goods, wine,
/// or fine craftsmanship... for a modest, real Standing improvement, at Treasury cost rather than
/// negotiation risk"). Deliberately gate-exempt — unlike <see cref="ProposeFrontierTreatyCommand"/>,
/// this command never requires the §5 language gate to clear; an envoy who can actually communicate
/// simply lands the gift better (see <see cref="SendDiplomaticGiftCommands.Mutate"/>).</summary>
public sealed record SendDiplomaticGiftCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> EnvoyCharacterId,
    RuntimeId<Actor> ForeignPeopleActorId,
    Money GiftValue) : ICommand;

/// <summary>Emitted whenever a <see cref="SendDiplomaticGiftCommand"/> is accepted.</summary>
public sealed record DiplomaticGiftSentEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    Money GiftValue,
    int GoodwillGained,
    HouseStandingLevel PreviousStanding,
    HouseStandingLevel NewStanding,
    string? CausationId) : IDomainEvent
{
    public string Type => "diplomacy.diplomaticGiftSent";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), ForeignPeopleActorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SendDiplomaticGiftCommand"/> (ADR 0006). Built
/// against a <see cref="CultureLanguageMap"/>, matching <see
/// cref="AcquireLanguageCommands.BuildPipeline"/>'s "caller-loaded content" shape.</summary>
public static class SendDiplomaticGiftCommands
{
    public static readonly ValidationErrorCode EnvoyNotFound = new("diplomacy.sendGift.envoyNotFound");
    public static readonly ValidationErrorCode EnvoyDeceased = new("diplomacy.sendGift.envoyDeceased");
    public static readonly ValidationErrorCode EnvoyNotOfHousehold = new("diplomacy.sendGift.envoyNotOfHousehold");
    public static readonly ValidationErrorCode UnknownForeignPeople = new("diplomacy.sendGift.unknownForeignPeople");
    public static readonly ValidationErrorCode GiftValueOutOfRange = new("diplomacy.sendGift.giftValueOutOfRange");
    public static readonly ValidationErrorCode InsufficientFunds = new("diplomacy.sendGift.insufficientFunds");

    public static CommandPipeline<WorldState, SendDiplomaticGiftCommand> CreatePipeline(CultureLanguageMap cultureLanguages)
    {
        if (cultureLanguages is null)
            throw new ArgumentNullException(nameof(cultureLanguages));

        return new CommandPipeline<WorldState, SendDiplomaticGiftCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, cultureLanguages),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, SendDiplomaticGiftCommand command)
    {
        if (!state.Characters.TryGet(command.EnvoyCharacterId, out var envoy))
            return EnvoyNotFound;
        if (!envoy!.IsAlive)
            return EnvoyDeceased;
        if (envoy.Household != command.HouseholdId)
            return EnvoyNotOfHousehold;
        if (!state.Actors.TryGet(command.ForeignPeopleActorId, out var actor) || actor!.ActorType != LivingWorldActorType.ForeignPeople ||
            !ForeignPeopleQueries.TryGet(state, command.ForeignPeopleActorId, out _))
        {
            return UnknownForeignPeople;
        }
        if (command.GiftValue < Money.FromDenarii(FrontierDiplomacyCatalog.MinimumGiftDenarii))
            return GiftValueOutOfRange;

        var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance < command.GiftValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SendDiplomaticGiftCommand command, CultureLanguageMap cultureLanguages)
    {
        var events = new List<IDomainEvent>();

        var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(account, -command.GiftValue),
                new LedgerPosting(LedgerAccountKey.Mint, command.GiftValue),
            },
            reference: $"diplomacy.sendDiplomaticGift:{command.CommandId.ToTaggedString()}"));

        var giftUnits = command.GiftValue.RawValue / Money.FromDenarii(FrontierDiplomacyCatalog.MinimumGiftDenarii).RawValue;
        var baseGoodwill = (int)giftUnits * FrontierDiplomacyCatalog.GiftGoodwillPerMinimumUnit;

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, command.EnvoyCharacterId, command.HouseholdId, command.ForeignPeopleActorId, cultureLanguages);
        var goodwillGained = Math.Max(0, baseGoodwill + baseGoodwill * quality.QualityModifierPercent / 100);

        var key = new PerPeopleStandingKey(command.HouseholdId, command.ForeignPeopleActorId);
        var previous = PerPeopleStandingResolver.GetEffective(state, command.HouseholdId, command.ForeignPeopleActorId).Standing;
        var updated = PerPeopleStandingMutator.Apply(state, key, goodwillGained, command.SubmittedDate);

        LivingWorldActorTieringService.RecordContactAndPromote(state, command.ForeignPeopleActorId, command.SubmittedDate);
        RivalDossierRefresh.Refresh(
            state, command.ForeignPeopleActorId, command.SubmittedDate,
            $"A diplomatic gift worth {command.GiftValue.ToDisplayString()} denarii was sent, improving standing.");

        events.Add(new DiplomaticGiftSentEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.ForeignPeopleActorId,
            command.GiftValue, goodwillGained, previous, updated.Standing, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}

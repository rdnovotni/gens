using Gens.Simulation.Characters;
using Gens.Simulation.Cultures;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Languages;

public sealed class LanguagesTests
{
    private static readonly GameDate StartDate = new(0);

    // ---- LanguageFamily / LanguageDefinition --------------------------------------------------

    [Test]
    public void FamilyConstructorRejectsAnEmptyMemberList()
    {
        Assert.Throws<ArgumentException>(() =>
            new LanguageFamily(new DefinitionId<LanguageFamily>("test-family"), "Test", Array.Empty<DefinitionId<LanguageDefinition>>()));
    }

    [Test]
    public void FamilyConstructorRejectsAnIsolateWithMoreThanOneMember()
    {
        var a = new DefinitionId<LanguageDefinition>("test-lang-a");
        var b = new DefinitionId<LanguageDefinition>("test-lang-b");
        Assert.Throws<ArgumentException>(() =>
            new LanguageFamily(new DefinitionId<LanguageFamily>("test-family"), "Test", new[] { a, b }, isIsolate: true));
    }

    [Test]
    public void DefinitionConstructorRejectsEmptyName()
    {
        Assert.Throws<ArgumentException>(() =>
            new LanguageDefinition(new DefinitionId<LanguageDefinition>("test-lang"), " ", new DefinitionId<LanguageFamily>("test-family")));
    }

    // ---- LanguageCatalog ------------------------------------------------------------------------

    [Test]
    public void CatalogRejectsALanguageReferencingAnUnknownFamily()
    {
        var language = new LanguageDefinition(
            new DefinitionId<LanguageDefinition>("test-lang"), "Test", new DefinitionId<LanguageFamily>("missing-family"));

        Assert.Throws<ArgumentException>(() => new LanguageCatalog(new[] { language }, Array.Empty<LanguageFamily>()));
    }

    [Test]
    public void CatalogRejectsDuplicateFamilyIds()
    {
        var familyId = new DefinitionId<LanguageFamily>("test-family");
        var family = new LanguageFamily(familyId, "Test", new[] { new DefinitionId<LanguageDefinition>("test-lang") });

        Assert.Throws<ArgumentException>(() => new LanguageCatalog(Array.Empty<LanguageDefinition>(), new[] { family, family }));
    }

    [Test]
    public void SharesNonIsolateFamilyIsTrueForTwoCelticLanguages()
    {
        var catalog = KnownWorldLanguages.BuildCatalog();
        Assert.That(catalog.SharesNonIsolateFamily(KnownWorldLanguages.Gaulish, KnownWorldLanguages.Brythonic), Is.True);
    }

    [Test]
    public void SharesNonIsolateFamilyIsFalseAcrossDifferentFamilies()
    {
        var catalog = KnownWorldLanguages.BuildCatalog();
        Assert.That(catalog.SharesNonIsolateFamily(KnownWorldLanguages.Latin, KnownWorldLanguages.GreekKoine), Is.False);
    }

    [Test]
    public void SharesNonIsolateFamilyIsFalseForBasqueEvenAgainstItself()
    {
        var catalog = KnownWorldLanguages.BuildCatalog();
        Assert.That(catalog.SharesNonIsolateFamily(KnownWorldLanguages.BasqueAquitanian, KnownWorldLanguages.BasqueAquitanian), Is.False);
    }

    // ---- KnownWorldLanguages / CultureLanguageMap ------------------------------------------------

    [Test]
    public void KnownWorldLanguageCatalogHasNoDuplicatesAndEveryFamilyResolves()
    {
        var catalog = KnownWorldLanguages.BuildCatalog();
        Assert.Multiple(() =>
        {
            Assert.That(catalog.LanguageCount, Is.GreaterThan(0));
            Assert.That(catalog.FamilyCount, Is.GreaterThan(0));
        });
    }

    [Test]
    public void CultureLanguageMapResolvesRomanToLatinAndGallicToGaulish()
    {
        var map = CultureLanguageMap.BuildKnownWorldMap();
        Assert.Multiple(() =>
        {
            Assert.That(map.Resolve(KnownWorldCultures.Roman), Is.EqualTo(KnownWorldLanguages.Latin));
            Assert.That(map.Resolve(KnownWorldCultures.Gallic), Is.EqualTo(KnownWorldLanguages.Gaulish));
        });
    }

    [Test]
    public void CultureLanguageMapReturnsNullForAnHonestlyUnmappedCulture()
    {
        var map = CultureLanguageMap.BuildKnownWorldMap();
        Assert.That(map.Resolve(KnownWorldCultures.Blemmyes), Is.Null);
    }

    [Test]
    public void CultureLanguageMapRejectsADuplicateCultureEntry()
    {
        var entries = new[]
        {
            new KeyValuePair<DefinitionId<Simulation.Identity.Culture>, DefinitionId<LanguageDefinition>>(
                KnownWorldCultures.Roman, KnownWorldLanguages.Latin),
            new KeyValuePair<DefinitionId<Simulation.Identity.Culture>, DefinitionId<LanguageDefinition>>(
                KnownWorldCultures.Roman, KnownWorldLanguages.GreekKoine),
        };
        Assert.Throws<ArgumentException>(() => new CultureLanguageMap(entries));
    }

    // ---- LanguageProficiency / AcquireLanguageCommand --------------------------------------------

    [Test]
    public void AcquireLanguageGrantsANewProficiencyEntry()
    {
        var (state, characterId) = OneRomanCharacter();
        var pipeline = AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap());

        var result = pipeline.Execute(state, new AcquireLanguageCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId,
            KnownWorldLanguages.Latin, FluencyTier.FluentNative, LanguageAcquisitionMethod.NativeOrigin));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(LanguageProficiencyQueries.HasConversationalOrBetter(state, characterId, KnownWorldLanguages.Latin), Is.True);
        });
    }

    [Test]
    public void AcquireLanguageUpdatesAnExistingEntryInsteadOfDuplicatingIt()
    {
        var (state, characterId) = OneRomanCharacter();
        var pipeline = AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap());

        pipeline.Execute(state, new AcquireLanguageCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId,
            KnownWorldLanguages.GreekKoine, FluencyTier.Basic, LanguageAcquisitionMethod.FormalEducation));
        pipeline.Execute(state, new AcquireLanguageCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId,
            KnownWorldLanguages.GreekKoine, FluencyTier.FluentNative, LanguageAcquisitionMethod.FormalEducation));

        var entries = LanguageProficiencyQueries.ForCharacter(state, characterId);
        Assert.Multiple(() =>
        {
            Assert.That(entries.Count, Is.EqualTo(1));
            Assert.That(entries[0].FluencyTier, Is.EqualTo(FluencyTier.FluentNative));
        });
    }

    [Test]
    public void AcquireLanguageRejectsNativeOriginForANonNativeLanguage()
    {
        var (state, characterId) = OneRomanCharacter();
        var pipeline = AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap());

        var result = pipeline.Execute(state, new AcquireLanguageCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId,
            KnownWorldLanguages.GreekKoine, FluencyTier.FluentNative, LanguageAcquisitionMethod.NativeOrigin));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(AcquireLanguageCommands.NativeOriginRequiresNativeLanguage));
        });
    }

    [Test]
    public void AcquireLanguageRejectsAnUnknownLanguage()
    {
        var (state, characterId) = OneRomanCharacter();
        var pipeline = AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap());

        var result = pipeline.Execute(state, new AcquireLanguageCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId,
            new DefinitionId<LanguageDefinition>("not-a-real-language"), FluencyTier.Basic, LanguageAcquisitionMethod.FormalEducation));

        Assert.That(result.Error, Is.EqualTo(AcquireLanguageCommands.UnknownLanguage));
    }

    [Test]
    public void HasConversationalOrBetterIsFalseWithNoTrackedEntry()
    {
        var (state, characterId) = OneRomanCharacter();
        Assert.That(LanguageProficiencyQueries.HasConversationalOrBetter(state, characterId, KnownWorldLanguages.Latin), Is.False);
    }

    // ---- LiteracyRecord / SetLiteracyCommand -------------------------------------------------------

    [Test]
    public void SetLiteracyRecordsAFactForANamedCharacter()
    {
        var (state, characterId) = OneRomanCharacter();
        var result = SetLiteracyCommands.Pipeline.Execute(state, new SetLiteracyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId, true, LiteracyDerivation.LearningAttribute));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(LiteracyQueries.TryGet(state, characterId, out var record), Is.True);
            Assert.That(record.IsLiterate, Is.True);
        });
    }

    [Test]
    public void SetLiteracyOverwritesAnExistingRecordRatherThanDuplicatingIt()
    {
        var (state, characterId) = OneRomanCharacter();
        SetLiteracyCommands.Pipeline.Execute(state, new SetLiteracyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId, true, LiteracyDerivation.LearningAttribute));
        SetLiteracyCommands.Pipeline.Execute(state, new SetLiteracyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId, false, LiteracyDerivation.LearningAttribute));

        LiteracyQueries.TryGet(state, characterId, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(state.LiteracyRecords.Count, Is.EqualTo(1));
            Assert.That(record.IsLiterate, Is.False);
        });
    }

    [Test]
    public void SetLiteracyRejectsAnUnknownCharacter()
    {
        var state = new WorldState(StartDate);
        var result = SetLiteracyCommands.Pipeline.Execute(state, new SetLiteracyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, state.CharacterIds.Issue(), true, LiteracyDerivation.LearningAttribute));

        Assert.That(result.Error, Is.EqualTo(SetLiteracyCommands.CharacterNotFound));
    }

    // ---- InterpresAppointment / AppointInterpresCommand --------------------------------------------

    [Test]
    public void AppointInterpresRequiresConversationalProficiencyInEveryCoveredLanguage()
    {
        var (state, householdId, appointeeId) = OneHouseholdWithACharacter();

        var result = AppointInterpresCommands.Pipeline.Execute(state, new AppointInterpresCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, appointeeId, new[] { KnownWorldLanguages.Gaulish }));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(AppointInterpresCommands.InsufficientProficiency));
        });
    }

    [Test]
    public void AppointInterpresSucceedsOnceTheAppointeeHoldsConversationalProficiency()
    {
        var (state, householdId, appointeeId) = OneHouseholdWithACharacter();
        GrantConversational(state, appointeeId, KnownWorldLanguages.Gaulish);

        var result = AppointInterpresCommands.Pipeline.Execute(state, new AppointInterpresCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, appointeeId, new[] { KnownWorldLanguages.Gaulish }));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(InterpresQueries.CoversLanguage(state, householdId, KnownWorldLanguages.Gaulish), Is.True);
        });
    }

    // ---- DiplomacyLanguageGateEvaluator (§6, §10) ---------------------------------------------------

    [Test]
    public void GateClearsOnTheNegotiatorsOwnFluency()
    {
        var (state, negotiatorId) = OneRomanCharacter();
        GrantConversational(state, negotiatorId, KnownWorldLanguages.Germanic);

        var result = DiplomacyLanguageGateEvaluator.Evaluate(state, negotiatorId, KnownWorldLanguages.Germanic, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cleared, Is.True);
            Assert.That(result.GateClearedBy, Is.EqualTo(LanguageGateClearedBy.NegotiatorFluency));
        });
    }

    [Test]
    public void GateClearsThroughAFormalInterpresAppointment()
    {
        var (state, householdId, negotiatorId) = OneHouseholdWithACharacter();
        var interpreterId = state.CharacterIds.Issue();
        state.Characters.Add(interpreterId, CharacterTestFixtures.Minimal(interpreterId, nomen: "Interpres", household: householdId));
        GrantConversational(state, interpreterId, KnownWorldLanguages.Germanic);
        AppointInterpresCommands.Pipeline.Execute(state, new AppointInterpresCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, interpreterId, new[] { KnownWorldLanguages.Germanic }));

        var result = DiplomacyLanguageGateEvaluator.Evaluate(state, negotiatorId, KnownWorldLanguages.Germanic, householdId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cleared, Is.True);
            Assert.That(result.GateClearedBy, Is.EqualTo(LanguageGateClearedBy.InterpresPresent));
            Assert.That(result.InterpresCharacterId, Is.EqualTo(interpreterId));
        });
    }

    [Test]
    public void GateClearsThroughAnInformalHouseholdMemberPerSevenOwnFlexibility()
    {
        var (state, householdId, negotiatorId) = OneHouseholdWithACharacter();
        var informalId = state.CharacterIds.Issue();
        state.Characters.Add(informalId, CharacterTestFixtures.Minimal(informalId, nomen: "Informal", household: householdId));
        GrantConversational(state, informalId, KnownWorldLanguages.Germanic);

        var result = DiplomacyLanguageGateEvaluator.Evaluate(state, negotiatorId, KnownWorldLanguages.Germanic, householdId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cleared, Is.True);
            Assert.That(result.GateClearedBy, Is.EqualTo(LanguageGateClearedBy.InterpresPresent));
            Assert.That(result.InterpresCharacterId, Is.EqualTo(informalId));
        });
    }

    [Test]
    public void GateDoesNotClearWithNoFluencyAndNoHousehold()
    {
        var (state, negotiatorId) = OneRomanCharacter();
        var result = DiplomacyLanguageGateEvaluator.Evaluate(state, negotiatorId, KnownWorldLanguages.Germanic, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cleared, Is.False);
            Assert.That(result.GateClearedBy, Is.EqualTo(LanguageGateClearedBy.None));
        });
    }

    // ---- InteractionLanguageBarrier (§6 soft penalty) -----------------------------------------------

    [Test]
    public void SeverityIsNoneWithNoTrackedProficiencyEitherSide()
    {
        var (state, first) = OneRomanCharacter();
        var secondId = state.CharacterIds.Issue();
        state.Characters.Add(secondId, CharacterTestFixtures.Minimal(secondId, nomen: "Second"));

        Assert.That(InteractionLanguageBarrier.Severity(state, first, secondId), Is.EqualTo(LanguageBarrierSeverity.None));
    }

    [Test]
    public void SeverityIsNoneWhenBothShareConversationalOrBetter()
    {
        var (state, first) = OneRomanCharacter();
        var secondId = state.CharacterIds.Issue();
        state.Characters.Add(secondId, CharacterTestFixtures.Minimal(secondId, nomen: "Second"));
        GrantConversational(state, first, KnownWorldLanguages.Latin);
        GrantConversational(state, secondId, KnownWorldLanguages.Latin);

        Assert.That(InteractionLanguageBarrier.Severity(state, first, secondId), Is.EqualTo(LanguageBarrierSeverity.None));
    }

    [Test]
    public void SeverityIsHaltingWhenTheSharedTierIsOnlyBasic()
    {
        var (state, first) = OneRomanCharacter();
        var secondId = state.CharacterIds.Issue();
        state.Characters.Add(secondId, CharacterTestFixtures.Minimal(secondId, nomen: "Second"));

        var pipeline = AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap());
        pipeline.Execute(state, new AcquireLanguageCommand(
            state.CommandIds.Issue(), "player", StartDate, null, first, KnownWorldLanguages.Gaulish, FluencyTier.FluentNative,
            LanguageAcquisitionMethod.FormalEducation));
        pipeline.Execute(state, new AcquireLanguageCommand(
            state.CommandIds.Issue(), "player", StartDate, null, secondId, KnownWorldLanguages.Gaulish, FluencyTier.Basic,
            LanguageAcquisitionMethod.FormalEducation));

        Assert.That(InteractionLanguageBarrier.Severity(state, first, secondId), Is.EqualTo(LanguageBarrierSeverity.Halting));
    }

    [Test]
    public void SeverityIsNoSharedLanguageWhenBothHaveTrackedLanguagesButNoOverlap()
    {
        var (state, first) = OneRomanCharacter();
        var secondId = state.CharacterIds.Issue();
        state.Characters.Add(secondId, CharacterTestFixtures.Minimal(secondId, nomen: "Second"));
        GrantConversational(state, first, KnownWorldLanguages.Latin);
        GrantConversational(state, secondId, KnownWorldLanguages.Germanic);

        Assert.That(InteractionLanguageBarrier.Severity(state, first, secondId), Is.EqualTo(LanguageBarrierSeverity.NoSharedLanguage));
    }

    // ---- Save/load round trip --------------------------------------------------------------------

    [Test]
    public void LanguageStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, characterId) = OneHouseholdWithACharacter();
        GrantConversational(state, characterId, KnownWorldLanguages.Gaulish);
        SetLiteracyCommands.Pipeline.Execute(state, new SetLiteracyCommand(
            state.CommandIds.Issue(), "player", StartDate, null, characterId, true, LiteracyDerivation.LearningAttribute));
        AppointInterpresCommands.Pipeline.Execute(state, new AppointInterpresCommand(
            state.CommandIds.Issue(), "player", StartDate, null, householdId, characterId, new[] { KnownWorldLanguages.Gaulish }));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.LanguageProficiencies.Count, Is.EqualTo(state.LanguageProficiencies.Count));
            Assert.That(restored.LiteracyRecords.Count, Is.EqualTo(state.LiteracyRecords.Count));
            Assert.That(restored.InterpresAppointments.Count, Is.EqualTo(state.InterpresAppointments.Count));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    // ---- Shared fixtures ----------------------------------------------------------------------------

    private static (WorldState State, RuntimeId<Character> CharacterId) OneRomanCharacter()
    {
        var state = new WorldState(StartDate);
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        return (state, characterId);
    }

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Character> CharacterId) OneHouseholdWithACharacter()
    {
        var state = new WorldState(StartDate);
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, household: householdId));
        return (state, householdId, characterId);
    }

    private static void GrantConversational(WorldState state, RuntimeId<Character> characterId, DefinitionId<LanguageDefinition> languageId) =>
        AcquireLanguageCommands.BuildPipeline(KnownWorldLanguages.BuildCatalog(), CultureLanguageMap.BuildKnownWorldMap()).Execute(
            state, new AcquireLanguageCommand(
                state.CommandIds.Issue(), "player", StartDate, null, characterId, languageId, FluencyTier.Conversational,
                LanguageAcquisitionMethod.FormalEducation));
}

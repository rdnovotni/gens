#nullable enable

using Gens.Presentation.Adapters;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Characters;
using Gens.Simulation.Queries;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class CharacterDetailAdapterTests
{
    [Test]
    public void AdaptShapesEveryFieldFromTheShellsRealQuery()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var characterId = CampaignTestFixtures.AddAdultHouseholdMember(
            shell, praenomen: "Marcus", nomen: "Aurelius", skills: new LaborSkills(30, 20, 10, 40, 15));
        shell.Submit(AssignDutyCommands.Pipeline, new AssignDutyCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            characterId, shell.HouseholdId, DutySlot.FieldHand));

        var projection = shell.Query(new CharacterDetailQuery(characterId), "player");
        var viewModel = new CharacterDetailAdapter().Adapt(projection);

        Assert.That(viewModel.NameLabel, Is.EqualTo("Marcus Aurelius"));
        Assert.That(viewModel.SubtitleLabel, Does.Contain("Adult").And.Contain("RomanCitizen").And.Contain("FieldHand"));
        Assert.That(viewModel.AttributesLabel, Does.Contain("Diplomacy 10"));
        Assert.That(viewModel.SkillsLabel, Does.Contain("Fieldwork 30").And.Contain("Culinary 40"));
        Assert.That(viewModel.ConditionLabel, Does.Contain("Health 80").And.Contain("Loyalty 50"));
    }

    [Test]
    public void SubtitleReadsDeceasedForANonLivingCharacterAndOmitsTheDutySegment()
    {
        var projection = new CharacterDetailProjection(
            CharacterId: "character_0000000", FullName: "Marcus Aurelius", Sex: Sex.Male, AgeInYears: 60,
            Stage: LifecycleStage.Elderly, IsAlive: false, LegalStatus: LegalStatus.RomanCitizen,
            SocialClass: SocialClass.Plebeian, Attributes: new CoreAttributes(10, 10, 10, 10, 10),
            Skills: new LaborSkills(10, 10, 10, 10, 10), Condition: new Condition(0, 0, 0, 0, 0),
            DutySlot: null, TraitIds: System.Array.Empty<string>(), CurrentSpouseId: null,
            MotherId: null, FatherId: null, VisualProfile: MinimalProfile());

        var viewModel = new CharacterDetailAdapter().Adapt(projection);

        Assert.That(viewModel.SubtitleLabel, Is.EqualTo("60 · Deceased · RomanCitizen"));
    }

    [Test]
    public void BindingAppliesEveryNamedLabel()
    {
        var root = new VisualElement();
        root.Add(new Label { name = CharacterDetailBinding.NameLabelName });
        root.Add(new Label { name = CharacterDetailBinding.SubtitleLabelName });
        root.Add(new Label { name = CharacterDetailBinding.AttributesLabelName });
        root.Add(new Label { name = CharacterDetailBinding.SkillsLabelName });
        root.Add(new Label { name = CharacterDetailBinding.ConditionLabelName });
        var viewModel = new CharacterDetailViewModel("Marcus Aurelius", "sub", "attrs", "skills", "condition");

        CharacterDetailBinding.Apply(root, viewModel);

        Assert.That(root.Q<Label>(CharacterDetailBinding.NameLabelName).text, Is.EqualTo("Marcus Aurelius"));
        Assert.That(root.Q<Label>(CharacterDetailBinding.ConditionLabelName).text, Is.EqualTo("condition"));
    }

    private static CharacterVisualProfile MinimalProfile() => new()
    {
        Height = Height.Average,
        Build = Build.Average,
        FacialStructure = FacialStructure.Oval,
        Complexion = Complexion.Olive,
        HairColor = HairColor.Brown,
        HairStyle = HairStyle.Cropped,
        EyeColor = EyeColor.Brown,
        NotableFeatures = System.Array.Empty<NotableFeature>(),
        Portrait = PortraitRecipeGenerator.Generate(
            Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, System.Array.Empty<NotableFeature>()),
    };
}

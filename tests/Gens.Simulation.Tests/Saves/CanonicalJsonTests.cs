using System.Text.Json.Serialization;
using Gens.Simulation.Identity;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

public sealed class CanonicalJsonTests
{
    [Test]
    public void Fixed64RoundTripsThroughItsRawScaledValueNotDecimalText()
    {
        var value = Fixed64.FromInt(3);

        var bytes = CanonicalJson.SerializeToCanonicalBytes(value);

        Assert.That(
            System.Text.Encoding.UTF8.GetString(bytes),
            Is.EqualTo((3 * Fixed64.ScaleFactor).ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Assert.That(CanonicalJson.Deserialize<Fixed64>(bytes), Is.EqualTo(value));
    }

    [Test]
    public void GameDateRoundTripsThroughRawTotalMonths()
    {
        var date = new GameDate(42);

        var bytes = CanonicalJson.SerializeToCanonicalBytes(date);

        Assert.That(System.Text.Encoding.UTF8.GetString(bytes), Is.EqualTo("42"));
        Assert.That(CanonicalJson.Deserialize<GameDate>(bytes), Is.EqualTo(date));
    }

    [Test]
    public void RuntimeIdRoundTripsThroughItsTaggedStringForm()
    {
        var state = new Gens.Simulation.State.WorldState(new GameDate(0));
        var id = state.CharacterIds.Issue();

        var bytes = CanonicalJson.SerializeToCanonicalBytes(id);

        Assert.That(System.Text.Encoding.UTF8.GetString(bytes), Is.EqualTo("\"char_0000000\""));
        Assert.That(CanonicalJson.Deserialize<RuntimeId<Character>>(bytes), Is.EqualTo(id));
    }

    [Test]
    public void DefinitionIdRoundTripsThroughItsPlainStringValue()
    {
        var id = new DefinitionId<Good>("grain");

        var bytes = CanonicalJson.SerializeToCanonicalBytes(id);

        Assert.That(System.Text.Encoding.UTF8.GetString(bytes), Is.EqualTo("\"grain\""));
        Assert.That(CanonicalJson.Deserialize<DefinitionId<Good>>(bytes), Is.EqualTo(id));
    }

    [Test]
    public void Pcg32StateRoundTripsWithExplicitFieldOrder()
    {
        var state = new Pcg32State(1, 123UL, 456UL);

        var bytes = CanonicalJson.SerializeToCanonicalBytes(state);

        Assert.That(
            System.Text.Encoding.UTF8.GetString(bytes),
            Is.EqualTo("{\"algorithmVersion\":1,\"state\":123,\"increment\":456}"));
        Assert.That(CanonicalJson.Deserialize<Pcg32State>(bytes), Is.EqualTo(state));
    }

    [Test]
    public void SerializingIdenticalValuesTwiceProducesByteIdenticalOutput()
    {
        var dto = new SamplePropertyOrderDto { B = 2, A = 1, C = 3 };

        var first = CanonicalJson.SerializeToCanonicalBytes(dto);
        var second = CanonicalJson.SerializeToCanonicalBytes(dto);

        Assert.That(second, Is.EqualTo(first));
        Assert.That(System.Text.Encoding.UTF8.GetString(first), Is.EqualTo("{\"a\":1,\"b\":2,\"c\":3}"));
    }

    [Test]
    public void OutputHasNoIndentationAndNoByteOrderMark()
    {
        var bytes = CanonicalJson.SerializeToCanonicalBytes(new GameDate(1));

        Assert.That(bytes[0], Is.Not.EqualTo(0xEF));
        Assert.That(System.Text.Encoding.UTF8.GetString(bytes), Does.Not.Contain("\n"));
    }

    [Test]
    public void Sha256HexIsDeterministicAndLowercase()
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes("hello");

        var first = CanonicalJson.Sha256Hex(bytes);
        var second = CanonicalJson.Sha256Hex(bytes);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Is.EqualTo(first.ToLowerInvariant()));
        Assert.That(first.Length, Is.EqualTo(64));
    }

    private sealed record SamplePropertyOrderDto
    {
        [JsonPropertyOrder(1)]
        public required int B { get; init; }

        [JsonPropertyOrder(0)]
        public required int A { get; init; }

        [JsonPropertyOrder(2)]
        public required int C { get; init; }
    }
}

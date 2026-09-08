using System.Text;
using Gens.Localization;
using NUnit.Framework;

namespace Gens.Localization.Tests;

[TestFixture]
public sealed class LocalizationServiceTests
{
    [Test]
    public void SelectedLocaleFallsBackAndValidatesParameters()
    {
        var service = Service(); service.AddJson("fr", Json("{\"hello\":\"Bonjour\"}")); service.SetLocale("fr");
        Assert.Multiple(() => { Assert.That(service.Get("hello"), Is.EqualTo("Bonjour")); Assert.That(service.Get("age", new Dictionary<string, object?> { ["age"] = 12 }), Is.EqualTo("Age 12")); Assert.Throws<FormatException>(() => service.Get("age")); });
    }
    [Test] public void MissingKeyAndPseudoLocaleAreVisible() { var service = Service(); Assert.That(service.Get("missing"), Is.EqualTo("⟦missing:missing⟧")); service.SetLocale("qps-ploc"); string value = service.Get("hello"); Assert.Multiple(() => { Assert.That(value, Does.StartWith("[")); Assert.That(value.Length, Is.GreaterThan("Hello".Length * 1.3)); }); }
    private static LocalizationService Service() { var service = new LocalizationService(developmentMode: true); service.AddJson("en", Json("{\"hello\":\"Hello\",\"age\":\"Age {age}\"}")); return service; }
    private static MemoryStream Json(string value) => new(Encoding.UTF8.GetBytes(value));
}

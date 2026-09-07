using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class ScreenGoldenTests
{
    private static readonly string[] Names = { "main-menu", "new-game", "settings", "credits", "roster", "character", "estate", "report", "confirmation", "wax-seal" };

    [TestCaseSource(nameof(Names))]
    public void ReferenceRenderIsPinnedPng(string name)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Goldens", name + ".png");
        byte[] bytes = File.ReadAllBytes(path);
        Assert.Multiple(() => { Assert.That(bytes.Length, Is.GreaterThan(4_000)); Assert.That(bytes.Take(8), Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })); });
    }
}

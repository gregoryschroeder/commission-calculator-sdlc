using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace CommissionCalculator.Web.Tests;

// WCAG 2.2 AA criteria the stylesheet alone can break (plan.md, Principle VI). Colours are read
// from the tokens declared on :root, so the checks follow the stylesheet rather than a copy of it.
public sealed partial class StylesheetAccessibilityTests
{
    private static readonly string Css = File.ReadAllText(StylesheetPath());

    [Fact, Trait("Requirement", "FR-021")]
    public void TextAndLinksHaveAtLeastFourPointFiveToOneContrast()
    {
        Assert.True(Contrast("--color-text", "--color-background") >= 4.5, "body and table text (WCAG 1.4.3)");
        Assert.True(Contrast("--color-link", "--color-background") >= 4.5, "link text (WCAG 1.4.3)");
    }

    [Fact, Trait("Requirement", "FR-021")]
    public void FocusIndicatorAndControlBordersHaveAtLeastThreeToOneContrast()
    {
        Assert.True(Contrast("--color-focus", "--color-background") >= 3.0, "focus indicator (WCAG 1.4.11)");
        Assert.True(Contrast("--color-border", "--color-background") >= 3.0, "select and button borders (WCAG 1.4.11)");
    }

    [Theory, Trait("Requirement", "FR-021")]
    [InlineData("select")]
    [InlineData("button")]
    [InlineData(".skip-link")]
    public void ControlsAreAtLeastTwentyFourPixelsSquare(string selector)
    {
        var declarations = Rules().Where(rule => rule.Selectors.Contains(selector)).SelectMany(rule => rule.Declarations).ToList();

        Assert.True(Pixels(declarations, "min-height") >= 24, $"{selector} min-height (WCAG 2.5.8)");
        Assert.True(Pixels(declarations, "min-width") >= 24, $"{selector} min-width (WCAG 2.5.8)");
    }

    [Fact, Trait("Requirement", "FR-021")]
    public void NothingIsFixedOrStickySoFocusIsNeverObscured() =>
        Assert.DoesNotContain(Rules().SelectMany(rule => rule.Declarations),
            declaration => declaration.Property == "position" && declaration.Value is "fixed" or "sticky");

    [Fact, Trait("Requirement", "FR-021")]
    public void NoFixedHeightOrHiddenOverflowSoTextCanBeRespaced() =>
        Assert.DoesNotContain(Rules().SelectMany(rule => rule.Declarations),
            declaration => declaration.Property == "height" || (declaration.Property == "overflow" && declaration.Value == "hidden"));

    private sealed record Declaration(string Property, string Value);

    private sealed record Rule(IReadOnlyList<string> Selectors, IReadOnlyList<Declaration> Declarations);

    private static List<Rule> Rules() =>
        RuleBlock().Matches(CommentPattern().Replace(Css, ""))
            .Select(match => new Rule(
                match.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                match.Groups[2].Value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(declaration => declaration.Split(':', 2, StringSplitOptions.TrimEntries))
                    .Where(parts => parts.Length == 2)
                    .Select(parts => new Declaration(parts[0], parts[1]))
                    .ToList()))
            .ToList();

    private static double Contrast(string foregroundToken, string backgroundToken)
    {
        var (lighter, darker) = (Luminance(Token(foregroundToken)), Luminance(Token(backgroundToken)));
        if (lighter < darker)
        {
            (lighter, darker) = (darker, lighter);
        }

        return (lighter + 0.05) / (darker + 0.05);
    }

    private static string Token(string name) =>
        Rules().Where(rule => rule.Selectors.Contains(":root")).SelectMany(rule => rule.Declarations)
            .SingleOrDefault(declaration => declaration.Property == name)?.Value
        ?? throw new Xunit.Sdk.XunitException($"The stylesheet declares no colour token {name} on :root.");

    // WCAG relative luminance of a #rrggbb colour.
    private static double Luminance(string hex)
    {
        double Channel(int offset)
        {
            var value = int.Parse(hex.AsSpan(1 + offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(0)) + (0.7152 * Channel(2)) + (0.0722 * Channel(4));
    }

    private static double Pixels(IEnumerable<Declaration> declarations, string property)
    {
        var value = declarations.LastOrDefault(declaration => declaration.Property == property)?.Value;
        return value is not null && value.EndsWith("px", StringComparison.Ordinal)
            ? double.Parse(value[..^2], CultureInfo.InvariantCulture)
            : 0;
    }

    private static string StylesheetPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", "CommissionCalculator.Web", "wwwroot", "css", "site.css");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("site.css was not found above the test output directory.");
    }

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}")]
    private static partial Regex RuleBlock();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CommentPattern();
}

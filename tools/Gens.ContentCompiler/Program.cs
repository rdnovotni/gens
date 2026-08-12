using System.Globalization;
using Gens.ContentCompiler.Commands;

return Dispatch(args);

static int Dispatch(string[] args)
{
    if (args.Length == 0)
    {
        PrintUsage();
        return 2;
    }

    try
    {
        return args[0] switch
        {
            "validate" => ValidateCommand.Run(RequirePositional(args, 1, "contentRoot")),
            "compile" => CompileCommand.Run(RequirePositional(args, 1, "contentRoot"), RequirePositional(args, 2, "outputPath")),
            "inspect" => InspectCommand.Run(RequirePositional(args, 1, "contentRoot"), RequirePositional(args, 2, "definitionId")),
            "diff" => DiffCommand.Run(RequirePositional(args, 1, "packA"), RequirePositional(args, 2, "packB")),
            "run-campaign" => RunCampaignCommand.Run(
                ulong.Parse(RequireOption(args, "--seed") ?? "1", CultureInfo.InvariantCulture),
                int.Parse(RequireOption(args, "--months") ?? "0", CultureInfo.InvariantCulture),
                RequireOption(args, "--out") ?? throw new ArgumentException("--out is required."),
                RequireOption(args, "--content-hash") ?? "unspecified"),
            "replay" => ReplayCommand.Run(RequirePositional(args, 1, "savePath")),
            "verify-save" => VerifySaveCommand.Run(RequirePositional(args, 1, "savePath")),
            "migrate-save" => MigrateSaveCommand.Run(
                RequirePositional(args, 1, "savePath"),
                RequireOption(args, "--out") ?? throw new ArgumentException("--out is required.")),
            "new-campaign" => NewCampaignCommand.Run(
                ulong.Parse(RequireOption(args, "--seed") ?? "1", CultureInfo.InvariantCulture),
                int.Parse(RequireOption(args, "--start-months") ?? "0", CultureInfo.InvariantCulture),
                RequireOption(args, "--region") ?? throw new ArgumentException("--region is required."),
                RequireOption(args, "--content") ?? throw new ArgumentException("--content is required."),
                RequireOption(args, "--out") ?? throw new ArgumentException("--out is required."),
                RequireOption(args, "--start-profile"),
                RequireOption(args, "--ruleset") ?? "default",
                RequireOption(args, "--difficulty") ?? "standard"),
            "advance" => AdvanceCommand.Run(
                RequirePositional(args, 1, "savePath"),
                int.Parse(RequireOption(args, "--months") ?? "1", CultureInfo.InvariantCulture),
                RequireOption(args, "--out") ?? throw new ArgumentException("--out is required."),
                RequireOption(args, "--report-out")),
            "submit-command" => SubmitCommand.Run(
                RequirePositional(args, 1, "savePath"),
                RequireOption(args, "--actor") ?? throw new ArgumentException("--actor is required."),
                RequireOption(args, "--type") ?? throw new ArgumentException("--type is required."),
                RequireOption(args, "--payload") ?? "{}",
                int.Parse(RequireOption(args, "--due-in-months") ?? "0", CultureInfo.InvariantCulture),
                RequireOption(args, "--out") ?? throw new ArgumentException("--out is required.")),
            "report" => ReportCommand.Run(RequirePositional(args, 1, "reportPath")),
            "save" => SaveCommand.Run(
                RequirePositional(args, 1, "savePath"),
                RequireOption(args, "--out") ?? throw new ArgumentException("--out is required.")),
            "load" => LoadCommand.Run(RequirePositional(args, 1, "savePath")),
            "compare-hashes" => CompareHashesCommand.Run(
                RequirePositional(args, 1, "pathA"),
                RequirePositional(args, 2, "pathB")),
            "inspect-state" => InspectStateCommand.Run(RequirePositional(args, 1, "savePath")),
            "help" or "--help" or "-h" => PrintUsageAndReturnZero(),
            _ => UnknownCommand(args[0]),
        };
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}

static string RequirePositional(string[] args, int index, string name)
{
    if (index >= args.Length || args[index].StartsWith("--", StringComparison.Ordinal))
        throw new ArgumentException($"Missing required positional argument '{name}'.");
    return args[index];
}

static string? RequireOption(string[] args, string flag)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == flag)
            return args[i + 1];
    }

    return null;
}

static int UnknownCommand(string command)
{
    Console.Error.WriteLine($"Unknown command '{command}'.");
    PrintUsage();
    return 2;
}

static int PrintUsageAndReturnZero()
{
    PrintUsage();
    return 0;
}

static void PrintUsage()
{
    Console.Error.WriteLine("""
        Usage: gens-content <command> [args]

        Content commands:
          validate <contentRoot>
          compile <contentRoot> <outputPath>
          inspect <contentRoot> <definitionId>
          diff <compiledPackA> <compiledPackB>

        Save/campaign commands:
          run-campaign --seed <n> --months <n> --out <path> [--content-hash <hash>]
          replay <savePath>
          verify-save <savePath>
          migrate-save <savePath> --out <path>

        Headless campaign shell commands (Phase 4):
          new-campaign --seed <n> --region <id> --content <compiledPackPath> --out <path>
                       [--start-months <n>] [--start-profile <id>] [--ruleset <id>] [--difficulty <id>]
          advance <savePath> [--months <n>] --out <path> [--report-out <path>]
          submit-command <savePath> --actor <id> --type <name> [--payload <json>]
                         [--due-in-months <n>] --out <path>
          report <reportPath>
          save <savePath> --out <path>
          load <savePath>
          compare-hashes <pathA> <pathB>
          inspect-state <savePath>
        """);
}

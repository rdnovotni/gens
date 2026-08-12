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
        """);
}

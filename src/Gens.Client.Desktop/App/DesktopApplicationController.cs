using Gens.Application.Campaign;
using Gens.Art;
using Gens.Audio;
using Gens.Client.Desktop.Platform;
using Gens.Client.Desktop.Settings;
using Gens.Presentation;
using Gens.Presentation.Models;
using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.UI;

namespace Gens.Client.Desktop.App;

public enum ScreenId { MainMenu, NewGameSetup, Settings, Credits, HouseholdRoster, CharacterDetail, EstateSettlement, MonthlyReport }
public enum ModalKind { None, Information, Confirmation, WaxSeal }
public sealed record ModalState(ModalKind Kind, string Title, string Body, CampaignHouseholdAction? Action = null);

/// <summary>Testable application/navigation coordinator. It owns exactly one session and only snapshot view models.</summary>
public sealed class DesktopApplicationController
{
    private readonly IApplicationPaths paths;
    private readonly SettingsService settingsService;
    private IReadOnlyList<IDomainEvent> lastReportEvents = Array.Empty<IDomainEvent>();
    private ScreenId? backScreen;
    private bool returnToMenuPending;

    public DesktopApplicationController(IApplicationPaths paths, AudioEngine? audio = null, Action<string, Exception?>? settingsLog = null)
    {
        this.paths = paths;
        paths.EnsureRequiredDirectories();
        settingsService = new(paths, settingsLog);
        Settings = settingsService.Current;
        Audio = audio ?? new AudioEngine(new NullAudioBackend("Native audio backend is unavailable; continuing silently."));
        ApplyAudioSettings();
        CurrentScreen = ScreenId.MainMenu;
        Log("Native client initialized.");
    }

    private CampaignSession? CurrentCampaign { get; set; }
    public bool HasActiveCampaign => CurrentCampaign is not null;
    public CampaignPresentation? Presentation { get; private set; }
    public DesktopSettings Settings { get; private set; }
    public AudioEngine Audio { get; }
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
    public ScreenId CurrentScreen { get; private set; }
    public ModalState? Modal { get; private set; }
    public bool QuitRequested { get; private set; }
    public string? SelectedCharacterId { get; private set; }
    public IReadOnlyList<string> Logs => logs;
    public bool HasSave => File.Exists(paths.Quicksave);
    public ulong? StateHash => CurrentCampaign?.ComputeStateHash();
    public string PortraitCachePath => Path.Combine(paths.Cache, "visuals");
    private readonly List<string> logs = new();

    public void Navigate(ScreenId screen)
    {
        if (IsGameplay(screen) && CurrentCampaign is null) throw new InvalidOperationException("A campaign is required for gameplay navigation.");
        CurrentScreen = screen; Modal = null; Log($"Navigate: {screen}");
    }

    public void StartNew(string region, string difficulty, ulong seed = 1)
    {
        ReplaceCampaign(CampaignSession.CreateNew(new CampaignStartOptions(seed, region, difficulty), out IReadOnlyList<IDomainEvent> history));
        lastReportEvents = history; CurrentScreen = ScreenId.HouseholdRoster; Log($"Campaign created: seed={seed}, region={region}, difficulty={difficulty}.");
    }

    public void Load()
    {
        try
        {
            if (!HasSave) { ShowInformation("Load Campaign", "No valid quicksave exists yet."); return; }
            ReplaceCampaign(CampaignSession.Load(paths.Quicksave, out var manifest)); lastReportEvents = Array.Empty<IDomainEvent>();
            CurrentScreen = ScreenId.HouseholdRoster; Log($"Campaign loaded: format={manifest.SaveFormatVersion}, game={manifest.GameVersion}.");
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or NotSupportedException)
        { ShowInformation("Load Failed", ex.Message); Log($"Load failed: {ex.Message}"); }
    }

    public void Save()
    {
        try
        {
            RequireCampaign().Save(paths.Quicksave, "0.1.0"); ShowInformation("Campaign Saved", $"Saved to {paths.Quicksave}."); Log("Campaign saved.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { ShowInformation("Save Failed", ex.Message); Log($"Save failed: {ex.Message}"); }
    }

    public void AdvanceMonth()
    {
        CampaignSession campaign = RequireCampaign(); lastReportEvents = campaign.AdvanceMonth(); CurrentScreen = ScreenId.MonthlyReport;
        Log($"Month advanced: hash={campaign.ComputeStateHash():x16}.");
    }

    public void OpenCharacter(string id) { SelectedCharacterId = id; backScreen = ScreenId.HouseholdRoster; CurrentScreen = ScreenId.CharacterDetail; Log($"Character opened: {id}."); }
    public void Back() { if (Modal is not null) { CancelModal(); return; } if (CurrentScreen == ScreenId.CharacterDetail) CurrentScreen = backScreen ?? ScreenId.HouseholdRoster; else if (!IsGameplay(CurrentScreen)) CurrentScreen = ScreenId.MainMenu; }

    public void RequestAction(CampaignHouseholdAction action)
    {
        ConfirmationModel preview = RequirePresentation().Preview(action);
        if (!preview.IsAvailable) { ShowInformation("Not Available", preview.ErrorCode ?? "This action is not currently available."); return; }
        Modal = new(preview.IsWaxSeal ? ModalKind.WaxSeal : ModalKind.Confirmation, preview.Title, preview.Body, action);
    }

    public void ConfirmModal()
    {
        ModalState? pending = Modal; Modal = null;
        if (returnToMenuPending) { returnToMenuPending = false; ConfirmMainMenu(); return; }
        if (pending?.Action is not { } action) return;
        CommandResult result = RequireCampaign().SubmitAction(action);
        CurrentScreen = ScreenId.EstateSettlement;
        ShowInformation(result.Accepted ? "Command Accepted" : "Command Rejected", result.Accepted ? "The household records were updated." : result.Error?.Code ?? "Rejected.");
        Log($"Submit {action}: {(result.Accepted ? "accepted" : "rejected")}.");
    }

    public void CancelModal() { Modal = null; returnToMenuPending = false; }
    public void RequestMainMenu() { returnToMenuPending = true; Modal = new(ModalKind.Confirmation, "Return to Main Menu", "Unsaved progress will be lost unless you save first."); }
    public void ConfirmMainMenu()
    {
        CurrentCampaign = null; Presentation = null; SelectedCharacterId = null; lastReportEvents = Array.Empty<IDomainEvent>(); Modal = null; CurrentScreen = ScreenId.MainMenu; Log("Campaign session cleared; returned to main menu.");
    }
    public void RequestQuit()
    {
        QuitRequested = true;
        try { settingsService.Save(Settings); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Log($"Settings could not be saved on quit: {ex.Message}"); }
        Log("Clean shutdown requested.");
    }
    public void SetUiScale(float scale) => UpdateSettings(Settings with { Display = Settings.Display with { UiScale = scale } }, "Display");
    public void SetReducedMotion(bool value) => UpdateSettings(Settings with { Accessibility = Settings.Accessibility with { ReducedMotion = value, Motion = value ? MotionMode.Reduced : MotionMode.Full } }, "Accessibility");
    public void SetHighContrast(bool value) => UpdateSettings(Settings with { Accessibility = Settings.Accessibility with { HighContrast = value } }, "Accessibility");
    public void SetLocale(string locale) => UpdateSettings(Settings with { Language = Settings.Language with { Locale = locale } }, "Language");
    public void SetConsoleEnabled(bool value) => UpdateSettings(Settings with { Developer = Settings.Developer with { ConsoleEnabled = value } }, "Developer");
    public void SetAiArtEnabled(bool value) => UpdateSettings(Settings with { Art = Settings.Art with { AiGenerationEnabled = value, Provider = value ? "mock" : "none" } }, "Art");
    public void SetExternalArtConsent(bool value) => UpdateSettings(Settings with { Art = Settings.Art with { ExternalGenerationConsent = value } }, "Art");
    public void SetAudioVolume(AudioBus bus, float value)
    {
        AudioSettings audio = bus switch
        {
            AudioBus.Master => Settings.Audio with { MasterVolume = value },
            AudioBus.Music => Settings.Audio with { MusicVolume = value },
            AudioBus.Ambience => Settings.Audio with { AmbienceVolume = value },
            AudioBus.Effects => Settings.Audio with { EffectsVolume = value },
            AudioBus.UI => Settings.Audio with { UiVolume = value },
            _ => Settings.Audio,
        };
        UpdateSettings(Settings with { Audio = audio }, "Audio"); ApplyAudioSettings();
    }
    public void SetAudioMuted(bool value) { UpdateSettings(Settings with { Audio = Settings.Audio with { Muted = value } }, "Audio"); ApplyAudioSettings(); }

    public InkBarModel InkBar() => RequirePresentation().InkBar();
    public HouseholdRosterModel Roster() => RequirePresentation().HouseholdRoster();
    public CharacterDetailModel Character() => RequirePresentation().CharacterDetail(SelectedCharacterId ?? throw new InvalidOperationException("No character is selected."));
    public EstateSettlementModel Estate() => RequirePresentation().EstateSettlement();
    public MonthlyReportModel Report() => RequirePresentation().MonthlyReport(lastReportEvents);

    public string ExecuteConsoleCommand(string line)
    {
        string[] parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries); if (parts.Length == 0) return string.Empty;
        try
        {
            string output = parts[0].ToLowerInvariant() switch
            {
                "help" => "help | state | hash | replay | query <ink|roster|estate|report> | submit <rites|festival> | save | load | advance | clear",
                "state" => CurrentCampaign is null ? "No active campaign." : $"screen={CurrentScreen} date={InkBar().Date} household={InkBar().GensName}",
                "hash" => CurrentCampaign is null ? "No active campaign." : $"{CurrentCampaign.ComputeStateHash():x16}",
                "replay" => ReplayConsole(),
                "query" => QueryConsole(parts),
                "submit" => SubmitConsole(parts),
                "save" => RunConsole(Save, "Save requested."),
                "load" => RunConsole(Load, "Load requested."),
                "advance" => RunConsole(AdvanceMonth, "Month advanced."),
                "clear" => ClearConsole(),
                _ => $"Unknown command '{parts[0]}'. Type help.",
            };
            if (output.Length > 0) Log($"> {line}\n{output}"); return output;
        }
        catch (Exception ex) { string message = $"Command failed: {ex.Message}"; Log(message); return message; }
    }

    private string QueryConsole(string[] parts) => parts.Length < 2 ? "Usage: query <ink|roster|estate|report>" : parts[1].ToLowerInvariant() switch
    {
        "ink" => $"{InkBar().GensName} | {InkBar().Date} | {InkBar().Treasury} | {InkBar().Dignitas}",
        "roster" => string.Join("; ", Roster().Rows.Select(static r => $"{r.CharacterId} {r.Name}")),
        "estate" => $"{Estate().SettlementStage}; holdings={Estate().Holdings.Count}",
        "report" => $"{Report().Date}; headlines={Report().Headlines.Count}",
        _ => "Unknown query.",
    };
    private string SubmitConsole(string[] parts)
    {
        if (parts.Length < 2) return "Usage: submit <rites|festival>";
        CampaignHouseholdAction action = parts[1].Equals("festival", StringComparison.OrdinalIgnoreCase) ? CampaignHouseholdAction.FundFestival : CampaignHouseholdAction.CycleRitesBudget;
        CommandResult result = RequireCampaign().SubmitAction(action); return result.Accepted ? "Accepted." : $"Rejected: {result.Error?.Code}";
    }
    private string ReplayConsole()
    {
        string diagnosticsPath = Path.Combine(paths.Cache, "replay-diagnostics.gens");
        ReplayDiagnosticsResult result = RequireCampaign().VerifyDeterministicReplay(diagnosticsPath, "0.1.0");
        return result.Matches
            ? $"Replay verified: {result.HashBeforeSave:x16}."
            : $"Replay mismatch: {result.HashBeforeSave:x16} != {result.HashAfterReload:x16}.";
    }
    private string ClearConsole() { logs.Clear(); return string.Empty; }
    private void UpdateSettings(DesktopSettings value, string domain) { settingsService.Update(value, domain); DesktopSettings previous = Settings; Settings = settingsService.Current; SettingsChanged?.Invoke(this, new(previous, Settings, domain)); }
    private void ApplyAudioSettings()
    {
        Audio.SetBusVolume(AudioBus.Master, Settings.Audio.MasterVolume); Audio.SetMuted(AudioBus.Master, Settings.Audio.Muted);
        Audio.SetBusVolume(AudioBus.Music, Settings.Audio.MusicVolume); Audio.SetBusVolume(AudioBus.Ambience, Settings.Audio.AmbienceVolume);
        Audio.SetBusVolume(AudioBus.Effects, Settings.Audio.EffectsVolume); Audio.SetBusVolume(AudioBus.UI, Settings.Audio.UiVolume);
    }
    private static string RunConsole(Action action, string response) { action(); return response; }
    private void ReplaceCampaign(CampaignSession session) { CurrentCampaign = session; Presentation = new(session); SelectedCharacterId = null; }
    private CampaignSession RequireCampaign() => CurrentCampaign ?? throw new InvalidOperationException("No active campaign.");
    private CampaignPresentation RequirePresentation() => Presentation ?? throw new InvalidOperationException("No active campaign.");
    private void ShowInformation(string title, string body) => Modal = new(ModalKind.Information, title, body);
    private void Log(string value) { logs.Add($"[{DateTimeOffset.Now:HH:mm:ss}] {value}"); if (logs.Count > 200) logs.RemoveAt(0); }
    private static bool IsGameplay(ScreenId id) => id is ScreenId.HouseholdRoster or ScreenId.CharacterDetail or ScreenId.EstateSettlement or ScreenId.MonthlyReport;
}

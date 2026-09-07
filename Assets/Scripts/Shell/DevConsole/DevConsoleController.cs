#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Gens.Presentation.Shell.DevConsole;

/// <summary>
/// The DFHack-style dev/debug console (Settings' "Enable Dev Console" toggle): a second,
/// always-present <see cref="UIDocument"/> that overlays the running app regardless of which screen
/// <see cref="GensAppController"/> currently has mounted — it never touches that controller's own
/// document, so it survives every one of that controller's <c>root.Clear()</c> screen swaps untouched.
/// Every command it runs reaches the campaign only through <see cref="CampaignShell.Query{TProjection}"/>
/// / <see cref="CampaignShell.Submit{TCommand}"/> (ADR 0013) via <see cref="DevConsoleContext"/> — this
/// controller itself never reads or writes <c>WorldState</c>.
/// </summary>
public sealed class DevConsoleController : MonoBehaviour
{
    [SerializeField]
    private UIDocument document = null!;

    [SerializeField]
    private VisualTreeAsset consoleAsset = null!;

    [SerializeField]
    private CampaignShellBehaviour shellBehaviour = null!;

    /// <summary>Shared with <see cref="GensAppController"/>'s Settings screen wiring — the console can
    /// only ever open when this pref is non-zero.</summary>
    public const string EnabledPrefKey = "DevConsoleEnabled";

    private readonly IReadOnlyList<IDevConsoleCommand> _commands = DevConsoleCommandRegistry.BuildDefault();
    private readonly List<string> _history = new();

    private VisualElement _root = null!;
    private ScrollView _scrollView = null!;
    private TextField _inputField = null!;
    private DevConsoleContext _context = null!;
    private int _historyIndex;
    private bool _isOpen;

    private void Awake() => UnityEngine.Application.logMessageReceived += OnUnityLogMessage;

    private void OnDestroy() => UnityEngine.Application.logMessageReceived -= OnUnityLogMessage;

    private void Start()
    {
        _context = new DevConsoleContext(shellBehaviour, ClearScrollback);

        var instance = consoleAsset.CloneTree();
        instance.style.flexGrow = 1;
        document.rootVisualElement.Add(instance);

        _root = document.rootVisualElement.Q<VisualElement>("dev-console-root")
            ?? throw new InvalidOperationException($"{nameof(DevConsoleController)} requires 'dev-console-root' in its UXML.");
        _scrollView = _root.Q<ScrollView>("dev-console-scrollback")
            ?? throw new InvalidOperationException($"{nameof(DevConsoleController)} requires 'dev-console-scrollback' in its UXML.");
        _inputField = _root.Q<TextField>("dev-console-input")
            ?? throw new InvalidOperationException($"{nameof(DevConsoleController)} requires 'dev-console-input' in its UXML.");

        _root.style.display = DisplayStyle.None;
        _inputField.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);

        AppendLine("Dev console ready. Type 'help' for a list of commands.");
    }

    /// <summary>Polls the Input System's keyboard device directly rather than a UI Toolkit
    /// <see cref="KeyDownEvent"/> on either app panel's root, so the toggle works regardless of which
    /// of the two panels (this console's, or <see cref="GensAppController"/>/<see
    /// cref="GensUIController"/>'s) currently holds keyboard focus — no <c>.inputactions</c> asset or
    /// action map needed for a single global binding.</summary>
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard is null || !keyboard.backquoteKey.wasPressedThisFrame)
            return;

        if (!_isOpen && !IsAvailable())
            return;

        Toggle();
    }

    /// <summary>The safety gate: the Settings toggle alone is never enough to open the console in a
    /// shipped build, so a stray enabled pref left over from a development build can never surface it
    /// in a release build.</summary>
    private static bool IsAvailable() =>
        PlayerPrefs.GetInt(EnabledPrefKey, 0) != 0 && (Debug.isDebugBuild || UnityEngine.Application.isEditor);

    private void Toggle()
    {
        _isOpen = !_isOpen;
        _root.style.display = _isOpen ? DisplayStyle.Flex : DisplayStyle.None;
        _inputField.SetValueWithoutNotify(string.Empty);

        if (_isOpen)
            _inputField.schedule.Execute(() => _inputField.Focus());
    }

    private void OnInputKeyDown(KeyDownEvent evt)
    {
        switch (evt.keyCode)
        {
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                SubmitLine();
                evt.StopPropagation();
                break;
            case KeyCode.UpArrow:
                CycleHistory(-1);
                evt.StopPropagation();
                break;
            case KeyCode.DownArrow:
                CycleHistory(1);
                evt.StopPropagation();
                break;
            case KeyCode.Escape:
                Toggle();
                evt.StopPropagation();
                break;
            case KeyCode.BackQuote:
                // Consumed by the global toggle in Update() instead; prevent it also being typed.
                evt.StopPropagation();
                break;
        }
    }

    private void SubmitLine()
    {
        var line = _inputField.value?.Trim() ?? string.Empty;
        _inputField.SetValueWithoutNotify(string.Empty);
        if (line.Length == 0)
            return;

        _history.Add(line);
        _historyIndex = _history.Count;
        AppendLine($"> {line}");

        var (verb, args) = DevConsoleLineParser.Parse(line)!.Value;
        var command = _commands.FirstOrDefault(c => string.Equals(c.Name, verb, StringComparison.OrdinalIgnoreCase));
        if (command is null)
        {
            AppendLine($"Unknown command '{verb}'. Type 'help' for a list of commands.");
            return;
        }

        try
        {
            var output = command.Execute(_context, args);
            if (!string.IsNullOrEmpty(output))
                AppendLine(output);
        }
        catch (Exception ex)
        {
            AppendLine($"Error: {ex.Message}");
        }
    }

    private void CycleHistory(int direction)
    {
        if (_history.Count == 0)
            return;

        _historyIndex = Mathf.Clamp(_historyIndex + direction, 0, _history.Count);
        var value = _historyIndex < _history.Count ? _history[_historyIndex] : string.Empty;
        _inputField.SetValueWithoutNotify(value);
        _inputField.cursorIndex = value.Length;
    }

    private void OnUnityLogMessage(string logMessage, string stackTrace, LogType type)
    {
        if (_scrollView is null)
            return;

        AppendLine($"[{type}] {logMessage}");
    }

    private void AppendLine(string text)
    {
        _scrollView.Add(new Label(text));
        _scrollView.schedule.Execute(() => _scrollView.scrollOffset = new Vector2(0, float.MaxValue));
    }

    private void ClearScrollback() => _scrollView.Clear();
}

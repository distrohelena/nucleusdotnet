using System;
using System.Runtime.InteropServices;

namespace Nucleus.ConsoleEngine;

/// <summary>
/// Detects terminal capabilities (ANSI, cursor control) so console-based apps can adapt per host.
/// </summary>
public sealed class TerminalEnvironment {
    const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    const int STD_OUTPUT_HANDLE = -11;

    TerminalEnvironment(bool useAnsiSequences, RendererPreference preference, bool virtualTerminalEnabled, bool fallbackFromAnsiPreference) {
        UseAnsiSequences = useAnsiSequences;
        Preference = preference;
        VirtualTerminalEnabled = virtualTerminalEnabled;
        FallbackFromAnsiPreference = fallbackFromAnsiPreference;
    }

    /// <summary>
    /// True when the renderer should emit ANSI escape sequences; otherwise prefer the basic renderer.
    /// </summary>
    public bool UseAnsiSequences { get; }

    /// <summary>
    /// Gets the renderer preference requested by the operator.
    /// </summary>
    public RendererPreference Preference { get; }

    /// <summary>
    /// Indicates whether <c>ENABLE_VIRTUAL_TERMINAL_PROCESSING</c> is active on Windows.
    /// </summary>
    public bool VirtualTerminalEnabled { get; }

    /// <summary>
    /// True when ANSI was requested explicitly but the terminal could not satisfy it.
    /// </summary>
    public bool FallbackFromAnsiPreference { get; }

    public static TerminalEnvironment Detect(RendererPreference preference) {
        bool supportsAnsi = DetermineAnsiSupport(out bool vtEnabled);
        bool useAnsi = preference switch {
            RendererPreference.Basic => false,
            RendererPreference.Ansi => supportsAnsi,
            _ => supportsAnsi
        };

        bool fallback = preference == RendererPreference.Ansi && !supportsAnsi;
        return new TerminalEnvironment(useAnsi, preference, vtEnabled, fallback);
    }

    static bool DetermineAnsiSupport(out bool virtualTerminalEnabled) {
        virtualTerminalEnabled = false;

        if (IsOutputRedirected()) {
            return false;
        }

        if (OperatingSystem.IsWindows()) {
            virtualTerminalEnabled = TryEnableVirtualTerminalProcessing();
            return virtualTerminalEnabled;
        }

        string term = Environment.GetEnvironmentVariable("TERM") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(term)) {
            return false;
        }

        return !string.Equals(term, "dumb", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsOutputRedirected() {
        try {
            return Console.IsOutputRedirected;
        } catch {
            return true;
        }
    }

    static bool TryEnableVirtualTerminalProcessing() {
        IntPtr stdout = GetStdHandle(STD_OUTPUT_HANDLE);
        if (stdout == IntPtr.Zero || stdout == new IntPtr(-1)) {
            return false;
        }

        if (!GetConsoleMode(stdout, out uint mode)) {
            return false;
        }

        if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == ENABLE_VIRTUAL_TERMINAL_PROCESSING) {
            return true;
        }

        return SetConsoleMode(stdout, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }

    [DllImport("kernel32.dll")]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}

/// <summary>
/// Declares the available rendering strategies for console UIs.
/// </summary>
public enum RendererPreference {
    Auto,
    Ansi,
    Basic
}

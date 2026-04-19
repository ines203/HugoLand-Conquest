using System.Runtime.InteropServices;

namespace HugoLandWpf;

/// <summary>
/// Génère des sons de jeu musicaux via les API Windows (pas de fichiers externes requis).
/// </summary>
public static class SoundManager
{
    [DllImport("kernel32.dll")]
    private static extern bool Beep(int frequency, int duration);

    private static bool _enabled = true;

    public static bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    // ── Sons de jeu ──────────────────────────────────────

    /// <summary>Clic léger — confirmation d'action</summary>
    public static void PlayClick() => Play(() =>
    {
        Beep(880, 40);
        Beep(1100, 40);
    });

    /// <summary>Déplacement d'armée — pas militaires</summary>
    public static void PlayMove() => Play(() =>
    {
        Beep(440, 60);
        Beep(550, 60);
    });

    /// <summary>Construction — marteau sur enclume</summary>
    public static void PlayBuild() => Play(() =>
    {
        Beep(300, 80);
        Beep(400, 60);
        Beep(500, 80);
    });

    /// <summary>Combat — choc de lames</summary>
    public static void PlayCombat() => Play(() =>
    {
        Beep(200, 60);
        Beep(150, 80);
        Beep(250, 100);
    });

    /// <summary>Fin de tour — cloche</summary>
    public static void PlayEndTurn() => Play(() =>
    {
        Beep(660, 100);
        Beep(880, 150);
    });

    /// <summary>Renforcement — tambour</summary>
    public static void PlayReinforce() => Play(() =>
    {
        Beep(220, 80);
        Beep(220, 80);
        Beep(330, 120);
    });

    /// <summary>Scission — deux notes séparées</summary>
    public static void PlaySplit() => Play(() =>
    {
        Beep(660, 80);
        Thread.Sleep(30);
        Beep(440, 80);
    });

    /// <summary>Victoire — fanfare</summary>
    public static void PlayVictory() => Play(() =>
    {
        int[] notes = { 523, 659, 784, 1047 };
        foreach (var n in notes)
        {
            Beep(n, 150);
            Thread.Sleep(20);
        }
        Beep(1047, 400);
    });

    /// <summary>Défaite — descente triste</summary>
    public static void PlayDefeat() => Play(() =>
    {
        int[] notes = { 440, 370, 311, 277 };
        foreach (var n in notes)
        {
            Beep(n, 180);
            Thread.Sleep(10);
        }
    });

    /// <summary>Erreur — buzz</summary>
    public static void PlayError() => Play(() =>
    {
        Beep(200, 100);
        Beep(180, 150);
    });

    // ── Utilitaire ───────────────────────────────────────

    private static void Play(Action sound)
    {
        if (!_enabled) return;
        Task.Run(sound); // non-bloquant
    }
}

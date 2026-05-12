using System.Runtime.InteropServices;

namespace HugoLandWpf;

/// <summary>
/// Sons de jeu générés via l'API Windows Beep.
/// Aucun fichier audio externe requis.
/// </summary>
public static class SoundManager
{
    [DllImport("kernel32.dll")]
    private static extern bool Beep(int frequency, int duration);

    public static bool Enabled { get; set; } = true;

    // ── Sons ──────────────────────────────────────────────

    /// <summary>Clic léger — sélection / confirmation</summary>
    public static void PlayClick() => Play(() =>
    {
        Beep(1047, 30); // Do
        Beep(1319, 40); // Mi
    });

    /// <summary>Déplacement — pas sur herbe</summary>
    public static void PlayMove() => Play(() =>
    {
        Beep(392, 50);  // Sol
        Beep(523, 60);  // Do
    });

    /// <summary>Construction — 3 notes montantes</summary>
    public static void PlayBuild() => Play(() =>
    {
        Beep(523, 60);  // Do
        Beep(659, 60);  // Mi
        Beep(784, 80);  // Sol
    });

    /// <summary>Combat — choc sourd</summary>
    public static void PlayCombat() => Play(() =>
    {
        Beep(220, 50);
        Beep(185, 80);
        Beep(165, 100);
    });

    /// <summary>Fin de tour — cloche grave</summary>
    public static void PlayEndTurn() => Play(() =>
    {
        Beep(523, 80);  // Do
        Beep(784, 120); // Sol
    });

    /// <summary>Renforcement — tambour</summary>
    public static void PlayReinforce() => Play(() =>
    {
        Beep(196, 70);
        Thread.Sleep(20);
        Beep(196, 70);
        Thread.Sleep(20);
        Beep(294, 100);
    });

    /// <summary>Scission — deux notes séparées</summary>
    public static void PlaySplit() => Play(() =>
    {
        Beep(784, 60);  // Sol
        Thread.Sleep(30);
        Beep(523, 60);  // Do
    });

    /// <summary>Victoire — fanfare majeure</summary>
    public static void PlayVictory() => Play(() =>
    {
        int[] notes = { 523, 659, 784, 1047, 1319 };
        foreach (var n in notes)
        {
            Beep(n, 120);
            Thread.Sleep(15);
        }
        Beep(1319, 500);
    });

    /// <summary>Défaite — descente mineure triste</summary>
    public static void PlayDefeat() => Play(() =>
    {
        int[] notes = { 494, 440, 392, 349 };
        foreach (var n in notes)
        {
            Beep(n, 160);
            Thread.Sleep(10);
        }
        Beep(262, 400);
    });

    /// <summary>Erreur</summary>
    public static void PlayError() => Play(() =>
    {
        Beep(220, 80);
        Thread.Sleep(20);
        Beep(196, 120);
    });

    private static void Play(Action sound)
    {
        if (!Enabled) return;
        Task.Run(sound);
    }
}

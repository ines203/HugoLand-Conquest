using System.Windows.Media;

namespace HugoLandWpf;

/// <summary>
/// Gère la musique de fond du jeu.
/// strategyMusic.mp3  — menu principal
/// mediavalMusic.mp3  — plateau de jeu (tour par tour)
/// battleMusic.mp3    — combat en cours
/// </summary>
public static class MusicManager
{
    private static MediaPlayer _player = new MediaPlayer();
    private static string _currentTrack = "";
    private static bool _enabled = true;
    private static double _volume = 0.4;

    public static bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (!value) Stop();
        }
    }

    public static double Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0.0, 1.0);
            _player.Volume = _volume;
        }
    }

    // ── Pistes ──────────────────────────────────────────

    public static void PlayMenu()
        => Play("strategyMusic.mp3");

    public static void PlayGame()
        => Play("mediavalMusic.mp3");

    public static void PlayBattle()
        => Play("battleMusic.mp3");

    public static void Stop()
    {
        _player.Stop();
        _currentTrack = "";
    }

    public static void Pause()
        => _player.Pause();

    public static void Resume()
    {
        if (_enabled) _player.Play();
    }

    // ── Utilitaire ───────────────────────────────────────

    private static void Play(string filename)
    {
        if (!_enabled) return;

        // Ne pas redémarrer si c'est déjà la bonne piste
        if (_currentTrack == filename) return;

        try
        {
            _currentTrack = filename;

            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets", "Music", filename);

            _player.Stop();
            _player.Open(new Uri(path, UriKind.Absolute));
            _player.Volume = _volume;
            _player.Play();

            // Boucle infinie
            _player.MediaEnded -= OnMediaEnded;
            _player.MediaEnded += OnMediaEnded;
        }
        catch { /* fichier absent, on ignore */ }
    }

    private static void OnMediaEnded(object? sender, EventArgs e)
    {
        _player.Position = TimeSpan.Zero;
        _player.Play();
    }
}

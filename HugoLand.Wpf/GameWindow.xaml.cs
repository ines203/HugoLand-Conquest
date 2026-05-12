using HugoLand.Core.Services;
using HugoLandCore.Models;
using HugoLandCore.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HugoLandWpf;

public partial class GameWindow : Window
{
    private Game _game = null!;
    private Player _currentPlayer = null!;
    private MilitaryDetachment? _selectedArmy;
    private bool _movingMode = false;
    private int _gameId;
    private readonly GameViewModel _vm = new();

    private const int CELL = 64;
    private const int RADIUS = 16;

    // Couleurs terrain (fallback si image absente)
    private static readonly Dictionary<TerrainType, Color> TerrainFill = new()
    {
        { TerrainType.Plaine,   Color.FromRgb(45,  90,  39)  },
        { TerrainType.Foret,    Color.FromRgb(26,  58,  16)  },
        { TerrainType.Montagne, Color.FromRgb(92,  74,  53)  },
        { TerrainType.Mer,      Color.FromRgb(14,  58,  92)  }
    };

    private static readonly Dictionary<TerrainType, Color> TerrainBorder = new()
    {
        { TerrainType.Plaine,   Color.FromRgb(55,  110, 48)  },
        { TerrainType.Foret,    Color.FromRgb(35,  75,  22)  },
        { TerrainType.Montagne, Color.FromRgb(110, 90,  65)  },
        { TerrainType.Mer,      Color.FromRgb(20,  75,  115) }
    };

    // Images terrain
    private static readonly Dictionary<TerrainType, string> TerrainImages = new()
    {
        { TerrainType.Plaine,   "pack://application:,,,/Assets/terrain_plaine.png"   },
        { TerrainType.Foret,    "pack://application:,,,/Assets/terrain_foret.png"    },
        { TerrainType.Montagne, "pack://application:,,,/Assets/terrain_montagne.png" },
        { TerrainType.Mer,      "pack://application:,,,/Assets/terrain_mer.png"      }
    };

    private readonly ImageBrush _fogBrush =
    new ImageBrush(new BitmapImage(
        new Uri("pack://application:,,,/Assets/fog.png")));

    private static System.Windows.Media.Imaging.BitmapImage? LoadTerrainImage(TerrainType terrain)
    {
        try
        {
            var img = new System.Windows.Media.Imaging.BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(TerrainImages[terrain]);
            img.DecodePixelWidth = 64;
            img.DecodePixelHeight = 64;
            img.EndInit();
            return img;
        }
        catch { return null; }
    }

    public GameWindow(int gameId)
    {
        InitializeComponent();
        _gameId = gameId;
        DataContext = _vm;
        ChargerPartie();
        DessinerPlateau();
        MettreAJourPanneau();
        MusicManager.PlayGame();
    }

    private void ChargerPartie()
    {
        _game = App.Context.Games
            .Include(g => g.Map).ThenInclude(m => m.Territories)
                .ThenInclude(t => t.Detachment).ThenInclude(d => d!.Player)
            .Include(g => g.Map).ThenInclude(m => m.Territories)
                .ThenInclude(t => t.Installation)
            .Include(g => g.Players).ThenInclude(p => p.Detachments)
                .ThenInclude(d => d.Territory)
            .Include(g => g.Players).ThenInclude(p => p.Installations)
                .ThenInclude(i => i.Territory)
            .First(g => g.Id == _gameId);

        _currentPlayer = _game.Players.ToList()[_game.CurrentPlayerIndex];
    }

    private void DessinerPlateau()
    {
        UpdateFogOfWar();
        CanvasJeu.Children.Clear();
        var map = _game.Map!;
        CanvasJeu.Width  = map.Width  * CELL + 1;
        CanvasJeu.Height = map.Height * CELL + 1;

        foreach (var t in map.Territories)
            DessinerCase(t);
    }

    private void DessinerCase(Territory t)
    {
        int playerId = _currentPlayer.Id;

        bool visible = t.IsVisibleTo(playerId);
        bool explored = t.IsExploredBy(playerId);

        double left = t.X * CELL;
        double top  = t.Y * CELL;

        // Fond terrain avec image
        var terrainImg = LoadTerrainImage(t.Terrain);
        Brush terrainBrush = terrainImg != null
            ? (Brush)new ImageBrush(terrainImg) { Stretch = Stretch.UniformToFill }
            : new SolidColorBrush(TerrainFill[t.Terrain]);

        var rect = new Rectangle
        {
            Width = CELL - 1, Height = CELL - 1,
            Fill   = terrainBrush,
            Stroke = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
            StrokeThickness = 0.5,
            Tag = t
        };
        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);
        rect.MouseLeftButtonDown += Cell_Click;
        CanvasJeu.Children.Add(rect);

        if (!explored)
        {
            var fog = new Rectangle
            {
                Width = CELL - 1,
                Height = CELL - 1,
                Fill = _fogBrush,
                Opacity = 1.0,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(fog, left);
            Canvas.SetTop(fog, top);
            CanvasJeu.Children.Add(fog);

            return;
        }

        // tuiles explorées mais pas dans le range
        // (on connait le terrain mais on sait pas si ennemi)
        if (!visible)
        {
            var shroud = new Rectangle
            {
                Width = CELL - 1,
                Height = CELL - 1,
                Fill = _fogBrush,
                Opacity = 0.33,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shroud, left);
            Canvas.SetTop(shroud, top);
            CanvasJeu.Children.Add(shroud);
        }

        // Highlight déplacement possible
        if (_movingMode && _selectedArmy?.Territory != null)
        {
            bool adj  = Math.Abs(t.X - _selectedArmy.Territory.X) +
                        Math.Abs(t.Y - _selectedArmy.Territory.Y) == 1;
            bool nrj  = _selectedArmy.Energy >= t.TerrainEnergyCost;
            bool pass = t.Terrain != TerrainType.Mer;

            if (adj && nrj && pass)
            {
                var glow = new Rectangle
                {
                    Width = CELL - 1, Height = CELL - 1,
                    Fill = new SolidColorBrush(Color.FromArgb(60, 59, 130, 246)),
                    Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                    StrokeThickness = 1.5,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(glow, left);
                Canvas.SetTop(glow, top);
                CanvasJeu.Children.Add(glow);
            }
        }

        // Installation (cadre coloré)
        if (t.Installation != null)
        {
            bool own = t.Installation.PlayerId == _currentPlayer.Id;
            var isBorder = new Rectangle
            {
                Width = CELL - 1, Height = CELL - 1,
                Fill = Brushes.Transparent,
                Stroke = t.Installation.Type == InstallationType.Fortification
                    ? (own ? new SolidColorBrush(Color.FromRgb(245, 158, 11))
                           : new SolidColorBrush(Color.FromRgb(220, 38, 38)))
                    : (own ? new SolidColorBrush(Color.FromRgb(99, 102, 241))
                           : new SolidColorBrush(Color.FromRgb(239, 68, 68))),
                StrokeThickness = t.Installation.Type == InstallationType.Fortification ? 2.5 : 1.5,
                StrokeDashArray = t.Installation.Type == InstallationType.Camp
                    ? new DoubleCollection { 3, 2 } : null,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(isBorder, left);
            Canvas.SetTop(isBorder, top);
            CanvasJeu.Children.Add(isBorder);
        }

        // Détachement militaire
        if (t.Detachment != null && visible)
        {
            bool own   = t.Detachment.PlayerId == _currentPlayer.Id;
            bool canAct = t.Detachment.Actionable && t.Detachment.Force >= 10;
            bool selected = _selectedArmy?.Id == t.Detachment.Id;

            // Cercle de sélection externe
            if (selected)
            {
                var selRing = new Ellipse
                {
                    Width = CELL - 4, Height = CELL - 4,
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(Color.FromRgb(250, 204, 21)),
                    StrokeThickness = 2,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(selRing, left + 2);
                Canvas.SetTop(selRing, top + 2);
                CanvasJeu.Children.Add(selRing);
            }

            // Fond du cercle
            Color baseColor = own
                ? (canAct ? Color.FromRgb(29, 78, 216) : Color.FromRgb(15, 40, 100))
                : Color.FromRgb(185, 28, 28);

            var circle = new Ellipse
            {
                Width = CELL - 10, Height = CELL - 10,
                Fill = new SolidColorBrush(baseColor),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(circle, left + 5);
            Canvas.SetTop(circle, top + 5);
            CanvasJeu.Children.Add(circle);

            // Nombre de soldats
            var lbl = new TextBlock
            {
                Text = t.Detachment.Force >= 100
                    ? t.Detachment.Force.ToString()
                    : t.Detachment.Force.ToString(),
                Foreground = Brushes.White,
                FontSize = t.Detachment.Force >= 100 ? 9 : 11,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                IsHitTestVisible = false
            };
            lbl.Measure(new Size(100, 100));
            Canvas.SetLeft(lbl, left + (CELL - lbl.DesiredSize.Width) / 2);
            Canvas.SetTop(lbl, top + (CELL - lbl.DesiredSize.Height) / 2);
            CanvasJeu.Children.Add(lbl);

            // Barre d'énergie (petits points verts en bas)
            for (int e = 0; e < 5; e++)
            {
                var dot = new Ellipse
                {
                    Width = 5, Height = 5,
                    Fill = e < t.Detachment.Energy
                        ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                        : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(dot, left + 4 + e * 8);
                Canvas.SetTop(dot, top + CELL - 10);
                CanvasJeu.Children.Add(dot);
            }
        }
    }

    private void Cell_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Rectangle { Tag: Territory t }) return;

        if (_movingMode && _selectedArmy != null)
        {
            int dx = t.X - _selectedArmy.Territory!.X;
            int dy = t.Y - _selectedArmy.Territory!.Y;

            if (Math.Abs(dx) + Math.Abs(dy) != 1)
            { AjouterJournal("Not an adjacent territory."); return; }
            if (t.Terrain == TerrainType.Mer)
            { AjouterJournal("The sea is impassable."); return; }
            if (_selectedArmy.Energy < t.TerrainEnergyCost)
            { AjouterJournal($"Not enough energy (cost: {t.TerrainEnergyCost})."); return; }

            int dir = (dx, dy) switch
            {
                (0, -1) => 0, (1, 0) => 1, (0, 1) => 2, (-1, 0) => 3, _ => -1
            };
            if (dir >= 0)
            {
                bool hasEnemy = _game.Players
                    .Where(p => p.Id != _currentPlayer.Id)
                    .Any(p => p.Detachments.Any(d => d.TerritoryId == t.Id));
                if (hasEnemy) MusicManager.PlayBattle();
                App.ArmyService.TryMove(_selectedArmy.Id, dir);
                SoundManager.PlayMove();
                FlashCell(t.X * CELL, t.Y * CELL, Color.FromRgb(59, 130, 246));
                AjouterJournal($"Army moved to ({t.X},{t.Y}).");
            }

            _movingMode = false;
            _selectedArmy = null;
            MusicManager.PlayGame();
            Rafraichir();
            return;
        }

        // Sélection d'armée
        if (t.Detachment != null &&
            t.Detachment.PlayerId == _currentPlayer.Id &&
            t.Detachment.Actionable && t.Detachment.Force >= 10)
        {
            _selectedArmy = t.Detachment;
            MettreAJourPanneau();
            DessinerPlateau();
        }
    }
    private void MettreAJourPanneau()
    {
        _vm.UpdateFromGame(_game, _currentPlayer);
        _vm.UpdateSelectedArmy(_selectedArmy, _currentPlayer);

        var firstPlayerId = _game.Players.OrderBy(p => p.Id).Select(p => p.Id).First();
        bool isFirstPlayer = _currentPlayer.Id == firstPlayerId;

        GradStop1.Color = isFirstPlayer ? Color.FromRgb(29, 78, 216) : Color.FromRgb(153, 27, 27);
        GradStop2.Color = isFirstPlayer ? Color.FromRgb(13, 42, 112) : Color.FromRgb(80, 10, 10);
    }
    private void BtnDeplacer_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedArmy == null) return;
        _movingMode = true;
        DessinerPlateau();
        AjouterJournal("Click on an adjacent territory to move.");
    }

    private void BtnCamp_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedArmy == null) return;
        App.InstallationService.BuildCamp(_selectedArmy);
        SoundManager.PlayClick();
        AjouterJournal($"Camp built at ({_selectedArmy.Territory?.X},{_selectedArmy.Territory?.Y}).");
        Rafraichir();
    }

    private void BtnFort_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedArmy == null) return;
        App.InstallationService.UpgradeToFort(_selectedArmy);
        SoundManager.PlayBuild();
        AjouterJournal($"Fortification erected at ({_selectedArmy.Territory?.X},{_selectedArmy.Territory?.Y}).");
        Rafraichir();
    }

    private void BtnRenforcer_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedArmy == null) return;
        var dlg = new RenforcerDialog(_currentPlayer.Gold);
        if (dlg.ShowDialog() == true && dlg.Montant > 0)
        {
            App.ArmyService.ReinforceArmy(_currentPlayer, _selectedArmy, dlg.Montant);
            SoundManager.PlayClick();
            AjouterJournal($"+{dlg.Montant} troops recruited.");
            Rafraichir();
        }
    }

    private void BtnScinder_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedArmy == null) return;
        var dlg = new ScinderDialog(_selectedArmy.Force);
        if (dlg.ShowDialog() == true)
        {
            App.ArmyService.TrySplit(_selectedArmy.Id, dlg.Direction, dlg.Montant);
            SoundManager.PlayClick();
            AjouterJournal($"Army split: {dlg.Montant} troops sent.");
            Rafraichir();
        }
    }

    private void BtnPasser_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedArmy == null) return;
        _selectedArmy.Actionable = false;

        // Historique
        App.Context.PlayerActions.Add(new HugoLandCore.Models.PlayerAction
        {
            Description = $"Pass action for army at ({_selectedArmy.Territory?.X},{_selectedArmy.Territory?.Y})",
            Timestamp = DateTime.Now,
            PlayerId = _currentPlayer.Id
        });

        App.Context.SaveChanges();
        AjouterJournal("Army passed.");
        _selectedArmy = null;
        Rafraichir();
    }

    private void BtnFinTour_Click(object sender, RoutedEventArgs e)
    {
        // Snapshot
        App.Context.Snapshots.Add(new TurnSnapshot
        {
            TurnNumber = _game.TurnNumber, PlayerId = _currentPlayer.Id, GameId = _game.Id,
            Gold = _currentPlayer.Gold,
            ArmyCount = _currentPlayer.Detachments.Count(d => d.Force >= 10),
            TotalSoldiers = _currentPlayer.Detachments.Sum(d => d.Force),
            FortificationCount = _currentPlayer.Installations.Count(i => i.Type == InstallationType.Fortification)
        });

        // Rotation
        var players = _game.Players.ToList();
        _game.CurrentPlayerIndex = (_game.CurrentPlayerIndex + 1) % players.Count;
        _game.TurnNumber++;
        foreach (var p in _game.Players)
            foreach (var a in p.Detachments)
                a.Actionable = a.Force >= 10;
        App.Context.SaveChanges();

        ChargerPartie();
        _currentPlayer = _game.Players.ToList()[_game.CurrentPlayerIndex];

        // Vérif dette
        if (_currentPlayer.DebtTurns >= 5)
        {
            SoundManager.PlayDefeat();
            MessageBox.Show($"{_currentPlayer.Name} is bankrupt and eliminated!",
                "Defeat", MessageBoxButton.OK, MessageBoxImage.Warning);
            _game.Status = GameStatus.Finished;
            App.Context.SaveChanges();
            new MainWindow().Show(); Close(); return;
        }

        // Économie
        foreach (var a in _currentPlayer.Detachments.ToList())
            App.ArmyService.GainEnergy(a.Id);
        App.EconomyService.CollectRevenue(_currentPlayer.Id);
        App.EconomyService.PayMaintenance(_currentPlayer.Id);

        ChargerPartie();

        // Victoire
        var opp = _game.Players.First(p => p.Id != _currentPlayer.Id);
        if (!opp.Detachments.Any(d => d.Force >= 10))
        {
            SoundManager.PlayVictory();
            MessageBox.Show($"🏆 {_currentPlayer.Name} wins the war!",
                "Victory!", MessageBoxButton.OK, MessageBoxImage.Information);
            _game.Status = GameStatus.Finished;
            App.Context.SaveChanges();
            new MainWindow().Show(); Close(); return;
        }

        _selectedArmy = null;
        _movingMode   = false;
        DessinerPlateau();
        MettreAJourPanneau();
        SoundManager.PlayEndTurn();
        AjouterJournal($"─── Turn {_game.TurnNumber} : {_currentPlayer.Name} ───");
    }

    private void Rafraichir()
    {
        _selectedArmy = null;
        _movingMode   = false;
        ChargerPartie();
        DessinerPlateau();
        MettreAJourPanneau();
    }

    private void AjouterJournal(string msg)
    {
        _vm.AddLog(msg);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        App.Context.SaveChanges();
        MusicManager.Stop();
    }

    private void BtnQuit_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Return to main menu?\nThe game will be saved.",
            "Quit Game",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            App.Context.SaveChanges();
            new MainWindow().Show();
            this.Close();
        }
    }

    // ── Animation flash sur une case ──────────────
    private void FlashCell(double left, double top, Color color)
    {
        var flash = new Rectangle
        {
            Width = CELL - 1, Height = CELL - 1,
            Fill = new SolidColorBrush(Color.FromArgb(0, color.R, color.G, color.B)),
            IsHitTestVisible = false
        };
        Canvas.SetLeft(flash, left);
        Canvas.SetTop(flash, top);
        CanvasJeu.Children.Add(flash);

        var anim = new ColorAnimation
        {
            From     = Color.FromArgb(160, color.R, color.G, color.B),
            To       = Color.FromArgb(0,   color.R, color.G, color.B),
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase()
        };

        var brush = new SolidColorBrush(Color.FromArgb(0, color.R, color.G, color.B));
        flash.Fill = brush;

        anim.Completed += (s, e) => CanvasJeu.Children.Remove(flash);
        brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
    }

    /// <summary>
    /// mettre a jour fog of war 
    /// </summary>
    private void UpdateFogOfWar()
    {
        int playerId = _currentPlayer.Id;

        // Clear ONLY this player's visibility
        foreach (var t in _game.Map!.Territories)
        {
            t.VisibleByPlayers.Remove(playerId);
        }

        foreach (var army in _currentPlayer.Detachments)
        {
            if (army.Territory != null)
                RevealTerritory(army.Territory, 2);
        }

        foreach (var inst in _currentPlayer.Installations)
        {
            if (inst.Territory != null)
                RevealTerritory(inst.Territory, 1);
        }
    }
    private void RevealTerritory(Territory center, int radius)
    {
        int playerId = _currentPlayer.Id;

        foreach (var t in _game.Map!.Territories)
        {
            int dist = Math.Abs(t.X - center.X)
                     + Math.Abs(t.Y - center.Y);

            if (dist <= radius)
            {
                t.VisibleByPlayers.Add(playerId);
                t.ExploredByPlayers.Add(playerId);
            }
        }
    }
}

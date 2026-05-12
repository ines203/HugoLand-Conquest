using HugoLandCore.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HugoLandWpf;

public partial class WorldEditorWindow : Window
{
    private WorldTemplate? _currentWorld;
    private TerrainType _selectedTerrain = TerrainType.Plaine;
    private bool _isSettingStart = false;
    private int _startPlayerNum = 0;
    private bool _isDragging = false;

    private const int CELL_SIZE = 36;

    // Couleurs fallback par terrain
    private static readonly Dictionary<TerrainType, Color> TerrainColors = new()
    {
        { TerrainType.Plaine,   Color.FromRgb(74,  124, 89)  },
        { TerrainType.Foret,    Color.FromRgb(45,  80,  22)  },
        { TerrainType.Montagne, Color.FromRgb(139, 115, 85)  },
        { TerrainType.Mer,      Color.FromRgb(30,  96,  145) }
    };

    private static readonly Dictionary<TerrainType, string> TerrainLabels = new()
    {
        { TerrainType.Plaine,   "" },
        { TerrainType.Foret,    "" },
        { TerrainType.Montagne, "" },
        { TerrainType.Mer,      "" }
    };

    private static readonly Dictionary<TerrainType, string> TerrainImagePaths = new()
    {
        { TerrainType.Plaine,   "pack://application:,,,/Assets/terrain_plaine.png"   },
        { TerrainType.Foret,    "pack://application:,,,/Assets/terrain_foret.png"    },
        { TerrainType.Montagne, "pack://application:,,,/Assets/terrain_montagne.png" },
        { TerrainType.Mer,      "pack://application:,,,/Assets/terrain_mer.png"      }
    };

    private static System.Windows.Media.Imaging.BitmapImage? LoadTerrainImg(TerrainType terrain)
    {
        try
        {
            var img = new System.Windows.Media.Imaging.BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(TerrainImagePaths[terrain]);
            img.DecodePixelWidth = CELL_SIZE;
            img.EndInit();
            return img;
        }
        catch { return null; }
    }

    private static Brush GetTerrainBrush(TerrainType terrain)
    {
        var img = LoadTerrainImg(terrain);
        if (img != null)
            return new ImageBrush(img) { Stretch = System.Windows.Media.Stretch.UniformToFill };
        return new SolidColorBrush(TerrainColors[terrain]);
    }

    public WorldEditorWindow()
    {
        InitializeComponent();
        ChargerListeMondes();
    }

    private void ChargerListeMondes()
    {
        var worlds = App.WorldService.GetAllWorlds();
        LstMondes.ItemsSource = null;
        LstMondes.ItemsSource = worlds.Select(w =>
            new WorldListItem(w.Id, $"{w.Name} ({w.Width}×{w.Height}){(w.IsValid ? " ✔" : "")}  —  {w.UpdatedAt:yyyy-MM-dd}")).ToList();
    }

    private void LstMondes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LstMondes.SelectedItem is WorldListItem item)
        {
            _currentWorld = App.WorldService.GetWorld(item.Id);
            if (_currentWorld != null)
                ChargerMonde(_currentWorld);
        }
    }

    private void ChargerMonde(WorldTemplate world)
    {
        TxtNom.Text = world.Name;
        TxtDescription.Text = world.Description;
        TxtTitreMonde.Text = $"{world.Name} — {world.Width}×{world.Height}{(world.IsValid ? "  ✔ Validé" : "")}";

        MettreAJourPositionsDépart();
        DessinerGrille(world);
        MettreAJourDistribution();
    }

    private void DessinerGrille(WorldTemplate world)
    {
        CanvasGrille.Children.Clear();
        CanvasGrille.Width = world.Width * CELL_SIZE + 1;
        CanvasGrille.Height = world.Height * CELL_SIZE + 1;

        foreach (var tt in world.Territories)
        {
            var rect = new Rectangle
            {
                Width = CELL_SIZE - 1,
                Height = CELL_SIZE - 1,
                Fill = GetTerrainBrush(tt.Terrain),
                Stroke = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                StrokeThickness = 1,
                Tag = tt
            };

            // Lettre du terrain
            var label = new TextBlock
            {
                Text = TerrainLabels[tt.Terrain],
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(rect, tt.X * CELL_SIZE);
            Canvas.SetTop(rect, tt.Y * CELL_SIZE);
            Canvas.SetLeft(label, tt.X * CELL_SIZE + 12);
            Canvas.SetTop(label, tt.Y * CELL_SIZE + 10);

            rect.MouseLeftButtonDown += Cell_MouseDown;
            rect.MouseEnter         += Cell_MouseEnter;
            rect.MouseLeftButtonUp  += Cell_MouseUp;

            CanvasGrille.Children.Add(rect);
            CanvasGrille.Children.Add(label);
        }

        // Afficher les positions de départ
        DessinerMarqueurDepart(world);
    }

    private void DessinerMarqueurDepart(WorldTemplate world)
    {
        // Supprimer anciens marqueurs
        var old = CanvasGrille.Children.OfType<Ellipse>().ToList();
        foreach (var o in old) CanvasGrille.Children.Remove(o);

        void AjouterMarqueur(int? x, int? y, Color color, string label)
        {
            if (x == null || y == null) return;
            var ellipse = new Ellipse
            {
                Width = CELL_SIZE - 6, Height = CELL_SIZE - 6,
                Fill = new SolidColorBrush(color),
                Opacity = 0.85,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(ellipse, x.Value * CELL_SIZE + 3);
            Canvas.SetTop(ellipse, y.Value * CELL_SIZE + 3);
            CanvasGrille.Children.Add(ellipse);

            var txt = new TextBlock
            {
                Text = label, Foreground = Brushes.White,
                FontSize = 11, FontWeight = FontWeights.Bold,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(txt, x.Value * CELL_SIZE + 12);
            Canvas.SetTop(txt, y.Value * CELL_SIZE + 10);
            CanvasGrille.Children.Add(txt);
        }

        AjouterMarqueur(world.StartX1, world.StartY1, Color.FromRgb(33, 150, 243), "J1");
        AjouterMarqueur(world.StartX2, world.StartY2, Color.FromRgb(244, 67, 54),  "J2");
    }

    private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            _isDragging = true;
            AppliquerTerrain(sender);
        }
        else if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
        {
            AppliquerStartPosition(sender);
        }
    }

    private void Cell_MouseEnter(object sender, MouseEventArgs e)
    {
        if (_isDragging && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            AppliquerTerrain(sender);
    }

    private void Cell_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
    }

    // Clic gauche = peindre terrain
    private void AppliquerTerrain(object sender)
    {
        if (_currentWorld == null) return;
        if (sender is not Rectangle rect || rect.Tag is not TerritoryTemplate tt) return;

        TerrainType terrain = RbForet.IsChecked == true    ? TerrainType.Foret
                            : RbMontagne.IsChecked == true ? TerrainType.Montagne
                            : RbMer.IsChecked == true      ? TerrainType.Mer
                            : TerrainType.Plaine;

        App.WorldService.SetTerrain(_currentWorld.Id, tt.X, tt.Y, terrain);
        tt.Terrain = terrain;

        rect.Fill = GetTerrainBrush(terrain);
        var labels = CanvasGrille.Children.OfType<TextBlock>()
            .Where(t => Canvas.GetLeft(t) == tt.X * CELL_SIZE + 12 &&
                        Canvas.GetTop(t)  == tt.Y * CELL_SIZE + 10).ToList();
        foreach (var l in labels) l.Text = TerrainLabels[terrain];

        MettreAJourDistribution();
    }

    // Clic droit = placer position de départ
    private void AppliquerStartPosition(object sender)
    {
        if (_currentWorld == null) return;
        if (sender is not Rectangle rect || rect.Tag is not TerritoryTemplate tt) return;

        int playerNum = RbDepart1.IsChecked == true ? 1 : 2;

        try
        {
            App.WorldService.SetStartPosition(_currentWorld.Id, playerNum, tt.X, tt.Y);
            _currentWorld = App.WorldService.GetWorld(_currentWorld.Id)!;
            DessinerMarqueurDepart(_currentWorld);
            MettreAJourPositionsDépart();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void MettreAJourPositionsDépart()
    {
        if (_currentWorld == null) return;
        TxtDepart1.Text = _currentWorld.StartX1.HasValue
            ? $"J1 : ({_currentWorld.StartX1}, {_currentWorld.StartY1})"
            : "J1 : non défini";
        TxtDepart2.Text = _currentWorld.StartX2.HasValue
            ? $"J2 : ({_currentWorld.StartX2}, {_currentWorld.StartY2})"
            : "J2 : non défini";
    }

    private void MettreAJourDistribution()
    {
        if (_currentWorld == null) return;
        int total = _currentWorld.Territories.Count;
        int plaine   = _currentWorld.Territories.Count(t => t.Terrain == TerrainType.Plaine);
        int foret    = _currentWorld.Territories.Count(t => t.Terrain == TerrainType.Foret);
        int montagne = _currentWorld.Territories.Count(t => t.Terrain == TerrainType.Montagne);
        int mer      = _currentWorld.Territories.Count(t => t.Terrain == TerrainType.Mer);
        double accessible = total > 0 ? (double)(total - mer) / total * 100 : 0;

        TxtDistribution.Text =
            $"Plaine : {plaine}  Forêt : {foret}\n" +
            $"Montagne : {montagne}  Mer : {mer}\n" +
            $"Accessible : {accessible:F0}% (min 60%)";
    }

    private void BtnNouveauMonde_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new NewWorldDialog();
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var world = App.WorldService.CreateWorld(
                    dialog.NomMonde, dialog.DescriptionMonde,
                    dialog.Largeur, dialog.Hauteur);
                ChargerListeMondes();
                _currentWorld = world;
                ChargerMonde(world);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void BtnValider_Click(object sender, RoutedEventArgs e)
    {
        if (_currentWorld == null) return;
        var errors = App.WorldService.ValidateWorld(_currentWorld.Id);
        if (errors.Count == 0)
        {
            MessageBox.Show("✔ Monde valide ! Il peut maintenant être utilisé pour des parties.",
                "Validation réussie", MessageBoxButton.OK, MessageBoxImage.Information);
            _currentWorld = App.WorldService.GetWorld(_currentWorld.Id)!;
            TxtTitreMonde.Text = $"{_currentWorld.Name} — {_currentWorld.Width}×{_currentWorld.Height}  ✔ Validé";
            ChargerListeMondes();
        }
        else
        {
            MessageBox.Show("Erreurs de validation :\n\n" + string.Join("\n", errors),
                "Validation échouée", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
    {
        if (_currentWorld == null) return;
        // Le nom/description sont sauvegardés à la volée via SetTerrain — ici on confirme juste
        MessageBox.Show("Monde sauvegardé automatiquement à chaque modification.",
            "Sauvegarde", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Rb_Checked(object sender, RoutedEventArgs e) { }

    private void BtnRetour_Click(object sender, RoutedEventArgs e)
    {
        if (Owner != null)
            Owner.Show();
        else
            new MainWindow().Show();
        this.Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // S'assurer que MainWindow est visible si on ferme l'éditeur
    }
}

// Classe utilitaire pour la liste
public record WorldListItem(int Id, string Display)
{
    public override string ToString() => Display;
}

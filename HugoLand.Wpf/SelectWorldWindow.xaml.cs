using HugoLandCore.Models;
using System.Windows;
using System.Windows.Controls;

namespace HugoLandWpf;

public partial class SelectWorldWindow : Window
{
    public WorldTemplate? SelectedWorld { get; private set; }
    private readonly List<WorldTemplate> _worlds;

    public SelectWorldWindow(List<WorldTemplate> worlds)
    {
        InitializeComponent();
        _worlds = worlds;
        LstMondes.ItemsSource = worlds.Select(w =>
            $"{w.Name}  ({w.Width}×{w.Height})").ToList();
    }

    private void BtnChoisir_Click(object sender, RoutedEventArgs e)
    {
        if (LstMondes.SelectedIndex < 0)
        {
            MessageBox.Show("Please select a world.", "Selection needed",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        SelectedWorld = _worlds[LstMondes.SelectedIndex];
        DialogResult = true;
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}

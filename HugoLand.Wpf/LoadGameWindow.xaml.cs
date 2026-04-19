using HugoLandCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace HugoLandWpf;

public partial class LoadGameWindow : Window
{
    public int? SelectedGameId { get; private set; }
    private List<Game> _games = new();

    public LoadGameWindow()
    {
        InitializeComponent();
        ChargerParties();
    }

    private void ChargerParties()
    {
        _games = App.Context.Games
            .Include(g => g.Players)
            .OrderByDescending(g => g.TurnNumber)
            .ToList();

        LstParties.ItemsSource = _games.Select(g =>
            $"{g.Name}  — Tour {g.TurnNumber}, {(g.Status == GameStatus.InProgress ? "En cours" : "Terminée")}").ToList();
    }

    private void BtnCharger_Click(object sender, RoutedEventArgs e)
    {
        if (LstParties.SelectedIndex < 0)
        {
            MessageBox.Show("Veuillez sélectionner une partie.", "Sélection requise",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        SelectedGameId = _games[LstParties.SelectedIndex].Id;
        DialogResult = true;
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}

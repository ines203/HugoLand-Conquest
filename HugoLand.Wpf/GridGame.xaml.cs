using System.Windows;

namespace HugoLandWpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BtnNouvellePartie_Click(object sender, RoutedEventArgs e)
    {
        var worlds = App.WorldService.GetAllWorlds()
            .Where(w => w.IsValid).ToList();

        if (!worlds.Any())
        {
            MessageBox.Show(
                "Aucun monde validé disponible.\nCréez et validez un monde dans l'éditeur d'abord.",
                "Aucun monde disponible",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selection = new SelectWorldWindow(worlds);
        if (selection.ShowDialog() == true && selection.SelectedWorld != null)
        {
            var creation = new CreatePlayersWindow();
            if (creation.ShowDialog() == true)
            {
                var game = App.WorldService.CreateGameFromWorld(
                    selection.SelectedWorld,
                    creation.Player1Name,
                    creation.Player2Name);

                var gameWindow = new GameWindow(game.Id);
                gameWindow.Show();
                this.Hide();
            }
        }
    }

    private void BtnChargerPartie_Click(object sender, RoutedEventArgs e)
    {
        var loadWindow = new LoadGameWindow();
        if (loadWindow.ShowDialog() == true && loadWindow.SelectedGameId.HasValue)
        {
            var gameWindow = new GameWindow(loadWindow.SelectedGameId.Value);
            gameWindow.Show();
            this.Hide();
        }
    }

    private void BtnEditeurMonde_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var editor = new WorldEditorWindow();
            editor.Owner = this;
            editor.Show();
            this.Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening World Editor:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "World Editor Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void BtnQuitter_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}

using HugoLandCore.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HugoLandWpf;

public partial class GameHistoryWindow : Window
{
    private readonly StatisticsService _stats;
    private bool _initialized = false;  // garde pour éviter Filter_Changed prématuré

    public GameHistoryWindow()
    {
        InitializeComponent();
        _stats = new StatisticsService(App.Context);

        try { LoadGlobalStats(); }
        catch (Exception ex)
        {
            MessageBox.Show($"Stats error:\n{ex.Message}\n\n{ex.StackTrace}", "Stats Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        try { LoadHistory(); }
        catch (Exception ex)
        {
            MessageBox.Show($"History error:\n{ex.Message}\n\n{ex.StackTrace}", "History Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        _initialized = true;
    }

    private void LoadGlobalStats()
    {
        var gs = _stats.GetGlobalStats();
        TxtTotalGames.Text = gs.TotalGames.ToString();
        TxtWinRate.Text    = $"{gs.WinRate:F0}%";
        TxtAvgTurns.Text   = $"{gs.AvgTurnsPerVictory:F1}";
        TxtAvgGold.Text    = $"{gs.AvgGoldPerGame:F0}";
    }

    private void LoadHistory()
    {
        string? filterResult = (CmbFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() switch
        {
            "Wins only"  => "wins",
            _            => null
        };

        string sortBy = (CmbSort.SelectedItem as ComboBoxItem)?.Content?.ToString() switch
        {
            "Most turns"   => "turns",
            "Fewest turns" => "turns",
            _              => "date"
        };

        bool descending = (CmbSort.SelectedItem as ComboBoxItem)?.Content?.ToString() switch
        {
            "Oldest"       => false,
            "Fewest turns" => false,
            _              => true
        };

        var history = _stats.GetGameHistory(filterResult, null, sortBy, descending);

        GameListPanel.Children.Clear();

        if (!history.Any())
        {
            GameListPanel.Children.Add(new TextBlock
            {
                Text = "No finished games found.",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12, Margin = new Thickness(16)
            });
            return;
        }

        foreach (var entry in history)
        {
            var row = BuildHistoryRow(entry);
            GameListPanel.Children.Add(row);

            // Séparateur
            GameListPanel.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Height = 1
            });
        }
    }

    private Border BuildHistoryRow(GameHistoryEntry entry)
    {
        var grid = new Grid { Margin = new Thickness(16, 10, 16, 10) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });

        // Nom de la partie
        var nameBlock = new TextBlock
        {
            Text = entry.GameName,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12, FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(nameBlock, 0);

        // Taille carte
        var mapBlock = new TextBlock
        {
            Text = entry.MapSize,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            FontFamily = new FontFamily("Consolas"), FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(mapBlock, 1);

        // Résultat
        var resultBlock = new TextBlock
        {
            Text = entry.Result,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153)),
            FontFamily = new FontFamily("Consolas"), FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(resultBlock, 2);

        // Tours
        var turnsBlock = new TextBlock
        {
            Text = $"{entry.TotalTurns} turns",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontFamily = new FontFamily("Consolas"), FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(turnsBlock, 3);

        // Bouton rapport
        var reportBtn = new Button
        {
            Content = "Report",
            Background = new SolidColorBrush(Color.FromRgb(15, 46, 95)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            Foreground = new SolidColorBrush(Color.FromRgb(147, 197, 253)),
            FontFamily = new FontFamily("Consolas"), FontSize = 10,
            Padding = new Thickness(8, 3, 8, 3),
            Tag = entry.GameId
        };
        reportBtn.Click += (s, e) =>
        {
            if (s is Button btn && btn.Tag is int gameId)
            {
                var report = new GameReportWindow(gameId);
                report.Owner = this;
                report.ShowDialog();
            }
        };
        Grid.SetColumn(reportBtn, 4);

        grid.Children.Add(nameBlock);
        grid.Children.Add(mapBlock);
        grid.Children.Add(resultBlock);
        grid.Children.Add(turnsBlock);
        grid.Children.Add(reportBtn);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
        };
        border.Child = grid;

        // Hover
        border.MouseEnter += (s, e) =>
            border.Background = new SolidColorBrush(Color.FromArgb(20, 59, 130, 246));
        border.MouseLeave += (s, e) =>
            border.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        return border;
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized) return;
        LoadHistory();
    }
    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadGlobalStats();
        LoadHistory();
    }
    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}

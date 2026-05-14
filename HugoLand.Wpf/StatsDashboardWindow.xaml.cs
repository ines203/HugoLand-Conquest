using HugoLandCore.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HugoLandWpf;

public partial class StatsDashboardWindow : Window
{
    private readonly int _gameId;
    private readonly StatisticsService _statsService;

    public StatsDashboardWindow(int gameId)
    {
        InitializeComponent();
        _gameId = gameId;
        _statsService = new StatisticsService(App.Context);
        Refresh();
    }

    /// <summary>Rafraîchit toutes les statistiques — à appeler après chaque tour.</summary>
    public void Refresh()
    {
        var (p1, p2) = _statsService.GetLiveDashboard(_gameId);
        var game = App.Context.Games.Find(_gameId);
        if (game == null) return;

        TxtTurn.Text = $"— Turn {game.TurnNumber}";

        // Noms
        TxtP1Name.Text = p1.Name;
        TxtP2Name.Text = p2.Name;
        TxtLegendP1.Text = p1.Name;
        TxtLegendP2.Text = p2.Name;

        // Armées
        TxtP1Armies.Text = p1.ArmyCount.ToString();
        TxtP2Armies.Text = p2.ArmyCount.ToString();

        // Force
        TxtP1Force.Text = p1.TotalForce.ToString();
        TxtP2Force.Text = p2.TotalForce.ToString();

        // Fortifications
        TxtP1Forts.Text = p1.FortCount.ToString();
        TxtP2Forts.Text = p2.FortCount.ToString();

        // Or
        TxtP1Gold.Text = p1.Gold.ToString();
        TxtP2Gold.Text = p2.Gold.ToString();

        // Économie
        TxtP1Rev.Text   = $"+{p1.Revenue} rev";
        TxtP1Maint.Text = $"-{p1.Maintenance} maint";
        TxtP2Rev.Text   = $"+{p2.Revenue} rev";
        TxtP2Maint.Text = $"-{p2.Maintenance} maint";

        // Dettes
        TxtP1Debt.Visibility = p1.DebtTurns > 0 ? Visibility.Visible : Visibility.Collapsed;
        TxtP1Debt.Text = $"⚠ Debt {p1.DebtTurns}/5";
        TxtP2Debt.Visibility = p2.DebtTurns > 0 ? Visibility.Visible : Visibility.Collapsed;
        TxtP2Debt.Text = $"⚠ Debt {p2.DebtTurns}/5";

        // Graphique
        DrawForceChart();
    }

    private void DrawForceChart()
    {
        ChartCanvas.Children.Clear();

        var history = _statsService.GetForceHistory(_gameId);
        if (history.Count < 2) return;

        double w = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 700;
        double h = 130;
        double padL = 30, padR = 10, padT = 10, padB = 20;

        int maxForce = Math.Max(1,
            history.Max(s => Math.Max(s.ForceP1, s.ForceP2)));

        double chartW = w - padL - padR;
        double chartH = h - padT - padB;

        // Grille horizontale
        for (int i = 0; i <= 4; i++)
        {
            double y = padT + chartH - (chartH * i / 4.0);
            var line = new Line
            {
                X1 = padL, Y1 = y, X2 = w - padR, Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                StrokeThickness = 0.5
            };
            ChartCanvas.Children.Add(line);

            if (i % 2 == 0)
            {
                var lbl = new TextBlock
                {
                    Text = ((int)(maxForce * i / 4.0)).ToString(),
                    Foreground = new SolidColorBrush(Color.FromArgb(100, 148, 163, 184)),
                    FontSize = 8,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas")
                };
                Canvas.SetLeft(lbl, 0);
                Canvas.SetTop(lbl, y - 6);
                ChartCanvas.Children.Add(lbl);
            }
        }

        // Lignes de données
        DrawLine(history, s => s.ForceP1, maxForce, chartW, chartH, padL, padT, padB, h,
                 Color.FromRgb(96, 165, 250));  // bleu P1
        DrawLine(history, s => s.ForceP2, maxForce, chartW, chartH, padL, padT, padB, h,
                 Color.FromRgb(248, 113, 113)); // rouge P2
    }

    private void DrawLine(
        List<HugoLandCore.Services.ForceSnapshot> history,
        Func<HugoLandCore.Services.ForceSnapshot, int> selector,
        int maxForce, double chartW, double chartH,
        double padL, double padT, double padB, double h,
        Color color)
    {
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round
        };

        int n = history.Count;
        for (int i = 0; i < n; i++)
        {
            double x = padL + chartW * i / Math.Max(1, n - 1);
            double val = selector(history[i]);
            double y = padT + chartH - (chartH * val / maxForce);
            polyline.Points.Add(new System.Windows.Point(x, y));
        }

        ChartCanvas.Children.Add(polyline);
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}

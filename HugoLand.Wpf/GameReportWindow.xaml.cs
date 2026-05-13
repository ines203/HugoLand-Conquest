using HugoLandCore.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HugoLandWpf;

public partial class GameReportWindow : Window
{
    public GameReportWindow(int gameId)
    {
        InitializeComponent();
        var service = new StatisticsService(App.Context);
        var report = service.GetGameReport(gameId);
        if (report != null)
            PopulateReport(report);
    }

    private void PopulateReport(GameReport r)
    {
        // En-tête
        TxtGameName.Text  = r.GameName;
        TxtWinner.Text    = r.Winner;
        TxtTurns.Text     = $"{r.TotalTurns} turns";

        // Stats finales
        TxtWinnerName2.Text  = r.Winner;
        TxtWinnerStats.Text  =
            $"Gold: {r.WinnerGold}\nForce: {r.WinnerForce}\nForts: {r.WinnerForts}";
        TxtLoserName.Text    = r.Loser;
        TxtLoserStats.Text   =
            $"Gold: {r.LoserGold}\nForce: {r.LoserForce}\nForts: {r.LoserForts}";

        // Top 5 actions
        TopActionsPanel.Children.Clear();
        int rank = 1;
        int maxCount = r.TopActions.Count > 0 ? r.TopActions.Max(a => a.Count) : 1;
        foreach (var (action, count) in r.TopActions)
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            var rankTxt = new TextBlock
            {
                Text = $"#{rank}",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontSize = 11, FontFamily = new FontFamily("Consolas")
            };
            Grid.SetColumn(rankTxt, 0);

            var nameTxt = new TextBlock
            {
                Text = action,
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                FontSize = 11, FontFamily = new FontFamily("Consolas")
            };
            Grid.SetColumn(nameTxt, 1);

            // Barre de progression
            double ratio = maxCount > 0 ? (double)count / maxCount : 0;
            var barBg = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                CornerRadius = new CornerRadius(2),
                Height = 10, VerticalAlignment = VerticalAlignment.Center
            };
            var barFill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                CornerRadius = new CornerRadius(2),
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = ratio * 180
            };
            var barGrid = new Grid();
            barGrid.Children.Add(barBg);
            barGrid.Children.Add(barFill);
            Grid.SetColumn(barGrid, 2);

            var countTxt = new TextBlock
            {
                Text = count.ToString(),
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                FontSize = 11, FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(countTxt, 3);

            row.Children.Add(rankTxt);
            row.Children.Add(nameTxt);
            row.Children.Add(barGrid);
            row.Children.Add(countTxt);
            TopActionsPanel.Children.Add(row);
            rank++;
        }

        // Heatmap des combats
        DrawHeatmap(r.CombatHeatmap);

        // Graphique de force
        DrawForceChart(r.ForceHistory, r.Winner, r.Loser);
    }

    private void DrawHeatmap(List<(int X, int Y, int CombatCount)> data)
    {
        HeatmapCanvas.Children.Clear();
        if (!data.Any()) return;

        double w = 740, h = 190;
        int maxX = data.Max(d => d.X) + 1;
        int maxY = data.Max(d => d.Y) + 1;
        int maxC = data.Max(d => d.CombatCount);

        double cellW = Math.Min(30, w / maxX);
        double cellH = Math.Min(25, h / maxY);

        foreach (var (x, y, count) in data)
        {
            double intensity = maxC > 0 ? (double)count / maxC : 0;
            byte r = (byte)(200 * intensity);
            byte g = (byte)(30 * (1 - intensity));

            var rect = new Rectangle
            {
                Width = cellW - 1, Height = cellH - 1,
                Fill = new SolidColorBrush(Color.FromArgb((byte)(80 + 175 * intensity), r, g, 30)),
                ToolTip = $"({x},{y}): {count} combat(s)"
            };
            Canvas.SetLeft(rect, x * cellW);
            Canvas.SetTop(rect, y * cellH);
            HeatmapCanvas.Children.Add(rect);

            if (count > 0)
            {
                var lbl = new TextBlock
                {
                    Text = count.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 8, FontFamily = new FontFamily("Consolas"),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(lbl, x * cellW + cellW / 2 - 4);
                Canvas.SetTop(lbl, y * cellH + cellH / 2 - 6);
                HeatmapCanvas.Children.Add(lbl);
            }
        }
    }

    private void DrawForceChart(List<ForceSnapshot> history, string p1Name, string p2Name)
    {
        // Légende
        ChartLegend.Children.Clear();
        AddLegendItem(ChartLegend, p1Name, Color.FromRgb(96, 165, 250));
        AddLegendItem(ChartLegend, p2Name, Color.FromRgb(248, 113, 113));

        ForceChartCanvas.Children.Clear();
        if (history.Count < 2) return;

        double w = 740, h = 130;
        double padL = 30, padR = 10, padT = 10, padB = 20;
        double chartW = w - padL - padR;
        double chartH = h - padT - padB;
        int maxForce = Math.Max(1, history.Max(s => Math.Max(s.ForceP1, s.ForceP2)));

        // Grille
        for (int i = 0; i <= 4; i++)
        {
            double y = padT + chartH - (chartH * i / 4.0);
            ForceChartCanvas.Children.Add(new Line
            {
                X1 = padL, Y1 = y, X2 = w - padR, Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255)),
                StrokeThickness = 0.5
            });
        }

        DrawChartLine(ForceChartCanvas, history, s => s.ForceP1, maxForce,
                      chartW, chartH, padL, padT, w, Color.FromRgb(96, 165, 250));
        DrawChartLine(ForceChartCanvas, history, s => s.ForceP2, maxForce,
                      chartW, chartH, padL, padT, w, Color.FromRgb(248, 113, 113));
    }

    private static void AddLegendItem(Panel panel, string name, Color color)
    {
        panel.Children.Add(new Rectangle
        {
            Width = 16, Height = 3, Fill = new SolidColorBrush(color),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0)
        });
        panel.Children.Add(new TextBlock
        {
            Text = name, Foreground = new SolidColorBrush(color),
            FontSize = 10, FontFamily = new FontFamily("Consolas"),
            Margin = new Thickness(0, 0, 16, 0)
        });
    }

    private static void DrawChartLine(Canvas canvas, List<ForceSnapshot> history,
        Func<ForceSnapshot, int> selector, int maxForce,
        double chartW, double chartH, double padL, double padT,
        double totalW, Color color)
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
        canvas.Children.Add(polyline);
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}

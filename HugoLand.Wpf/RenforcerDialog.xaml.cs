using System.Windows;
using System.Windows.Input;

namespace HugoLandWpf;

public partial class RenforcerDialog : Window
{
    public int Montant { get; private set; }
    private readonly int _or;

    public RenforcerDialog(int or)
    {
        InitializeComponent();
        _or = or;
        TxtOr.Text = $"Available gold: {or}";
        TxtMontant.TextChanged += (s, e) =>
        {
            if (int.TryParse(TxtMontant.Text, out int n) && n > 0)
                TxtCout.Text = $"Cost: {n * 2} gold";
            else
                TxtCout.Text = "Cost: 0 gold";
        };
        TxtMontant.Focus();
    }

    private void TxtMontant_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Valider();
    }

    private void BtnRecruter_Click(object sender, RoutedEventArgs e)
        => Valider();

    private void Valider()
    {
        if (!int.TryParse(TxtMontant.Text, out int n) || n <= 0)
        {
            MessageBox.Show("Please enter a valid number.", "Invalid",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (n * 2 > _or)
        {
            MessageBox.Show($"Not enough gold. You need {n * 2} gold but only have {_or}.",
                "Not enough gold", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Montant = n;
        DialogResult = true;
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}

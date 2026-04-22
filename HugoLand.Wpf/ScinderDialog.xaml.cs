using System.Windows;

namespace HugoLandWpf;

public partial class ScinderDialog : Window
{
    public int Montant { get; private set; }
    public int Direction { get; private set; } // 0=N,1=E,2=S,3=O
    private readonly int _force;

    public ScinderDialog(int force)
    {
        InitializeComponent();
        _force = force;
        TxtForce.Text = $"Current force : {force}";
    }

    private void BtnScinder_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TxtMontant.Text, out int n) || n < 10)
        { MessageBox.Show("Minimum 10 soldier.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (n >= _force)
        { MessageBox.Show("There must be at least 1 soldier left.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        Montant = n;
        Direction = RbN.IsChecked == true ? 0
                  : RbE.IsChecked == true ? 1
                  : RbS.IsChecked == true ? 2
                  : 3;
        DialogResult = true;
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

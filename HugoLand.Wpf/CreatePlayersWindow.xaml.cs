using System.Windows;

namespace HugoLandWpf;

public partial class CreatePlayersWindow : Window
{
    public string Player1Name { get; private set; } = "Player 1";
    public string Player2Name { get; private set; } = "Player 2";
    public bool IsPlayer2AI  { get; private set; } = false;

    public CreatePlayersWindow()
    {
        InitializeComponent();
    }

    private void BtnDemarrer_Click(object sender, RoutedEventArgs e)
    {
        Player1Name = string.IsNullOrWhiteSpace(TxtJoueur1.Text) ? "Player 1" : TxtJoueur1.Text.Trim();

        if (IsPlayer2AI)
            Player2Name = "AI";
        else
            Player2Name = string.IsNullOrWhiteSpace(TxtJoueur2.Text) ? "Player 2" : TxtJoueur2.Text.Trim();

        DialogResult = true;
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ChkIA_Checked(object sender, RoutedEventArgs e)
    {
        IsPlayer2AI = true;
        if (TxtJoueur2 != null)
        {
            TxtJoueur2.Text = "AI";
            TxtJoueur2.IsEnabled = false;
        }
    }

    private void ChkIA_Unchecked(object sender, RoutedEventArgs e)
    {
        IsPlayer2AI = false;
        if (TxtJoueur2 != null)
        {
            TxtJoueur2.Text = "";
            TxtJoueur2.IsEnabled = true;
        }
    }
}

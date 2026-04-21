using System.Windows;

namespace HugoLandWpf;

public partial class NewWorldDialog : Window
{
    public string NomMonde { get; private set; } = "";
    public string DescriptionMonde { get; private set; } = "";
    public int Largeur { get; private set; }
    public int Hauteur { get; private set; }

    public NewWorldDialog()
    {
        InitializeComponent();
    }

    private void BtnCreer_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNom.Text))
        {
            MessageBox.Show("The name is obligatory.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(TxtLargeur.Text, out int w) || w < 10 || w > 50 ||
            !int.TryParse(TxtHauteur.Text, out int h) || h < 10 || h > 50)
        {
            MessageBox.Show("The dimensions must be between 10 and 50.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        NomMonde = TxtNom.Text.Trim();
        DescriptionMonde = TxtDescription.Text.Trim();
        Largeur = w;
        Hauteur = h;
        DialogResult = true;
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}

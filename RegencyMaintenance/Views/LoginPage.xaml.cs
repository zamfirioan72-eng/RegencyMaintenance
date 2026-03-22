using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using RegencyMaintenance.Services;

namespace RegencyMaintenance.Views;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
        this.Loaded += LoginPage_Loaded;
    }

    private async void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        await DataService.Instance.LoadAsync();
        var last = DataService.Instance.LastOperator;
        if (!string.IsNullOrEmpty(last))
        {
            LastOperatorText.Text = $"Ultimo operatore: {last}";
            LastOperatorText.Visibility = Visibility.Visible;
            OperatorNameBox.Text = last;
        }
        OperatorNameBox.Focus(FocusState.Programmatic);
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        DoLogin();
    }

    private void OperatorNameBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            DoLogin();
    }

    private void DoLogin()
    {
        var name = OperatorNameBox.Text.Trim().ToUpper();
        if (string.IsNullOrEmpty(name))
        {
            ErrorText.Text = "Inserisci il nome dell'operatore!";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        ErrorText.Visibility = Visibility.Collapsed;
        DataService.Instance.CurrentOperator = name;
        Frame.Navigate(typeof(MainPage), name);
    }
}

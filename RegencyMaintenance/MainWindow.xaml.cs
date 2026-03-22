using Microsoft.UI.Xaml;
using RegencyMaintenance.Views;

namespace RegencyMaintenance;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.ExtendsContentIntoTitleBar = true;
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
        RootFrame.Navigate(typeof(LoginPage));
    }
}

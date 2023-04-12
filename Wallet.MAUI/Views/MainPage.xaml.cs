using System.Windows.Input;
using Wallet.MAUI.ViewModel;

namespace Wallet.MAUI;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}
}



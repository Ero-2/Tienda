namespace Tienda
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Esto permite que Shell.Current.GoToAsync("AccountPage") funcione
            Routing.RegisterRoute(nameof(Views.AccountPage), typeof(Views.AccountPage));
        }
    }
}

namespace OKKT25
{

    public partial class App : Application
    {
        [Obsolete]
        public App()
        {
            InitializeComponent();

            Application.Current.UserAppTheme = AppTheme.Dark;

            MainPage = new AppShell();
        }
    }

}

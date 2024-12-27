using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Space_battle.Model;
using Space_battle.View;

namespace Space_battle.View
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private Game game;
        public MainPage()
        {
            InitializeComponent();
        }

        private void CreateTheGame(object sender, RoutedEventArgs e)
        {
            game = new Game(false);
            NavigationService.Navigate(game);
            game.StartGame();
        }

        private void JoinTheGame(object sender, RoutedEventArgs e)
        {
            game = new Game(true);
            NavigationService.Navigate(game);
            game.StartGame();
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
namespace Arcadia.UI
{
    public class GameWheelItems
    {
        public Canvas GameWheelCanvas { get; set; }

        public GameWheelItems()
        {
            // Create game wheel items
            var gameWheelItem1 = new GameWheelItem();
            gameWheelItem1.Title = "Game 1";
            gameWheelItem1.Image = new BitmapImage(new Uri("pack://application:,,,/Assets/Game1.png"));

            var gameWheelItem2 = new GameWheelItem();
            gameWheelItem2.Title = "Game 2";
            gameWheelItem2.Image = new BitmapImage(new Uri("pack://application:,,,/Assets/Game2.png"));

            // Add game wheel items to canvas
            GameWheelCanvas.Children.Add(gameWheelItem1);
            GameWheelCanvas.Children.Add(gameWheelItem2);
        }
    }
}

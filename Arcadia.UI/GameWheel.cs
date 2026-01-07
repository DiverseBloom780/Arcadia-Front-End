using System;
using System.Windows;
using System.Windows.Controls;
namespace Arcadia.UI
{
    public class GameWheel : UserControl
    {
        public GameWheel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Create game wheel UI components
            var gameWheelCanvas = new Canvas();
            gameWheelCanvas.Width = 800;
            gameWheelCanvas.Height = 600;

            // Add game wheel items
            var gameWheelItems = new GameWheelItems();
            gameWheelItems.GameWheelCanvas = gameWheelCanvas;

            // Add game wheel animations
            var gameWheelAnimations = new GameWheelAnimations();
            gameWheelAnimations.GameWheelCanvas = gameWheelCanvas;

            // Add game wheel interactions
            var gameWheelInteractions = new GameWheelInteractions();
            gameWheelInteractions.GameWheelCanvas = gameWheelCanvas;
        }
    }
}

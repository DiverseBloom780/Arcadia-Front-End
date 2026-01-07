using System;
using System.Windows;
using System.Windows.Controls;
namespace Arcadia.UI
{
    public class GameWheelInteractions
    {
        public Canvas GameWheelCanvas { get; set; }

        public GameWheelInteractions()
        {
            // Handle mouse wheel events
            GameWheelCanvas.MouseWheel += GameWheelCanvas_MouseWheel;

            // Handle mouse click events
            GameWheelCanvas.MouseDown += GameWheelCanvas_MouseDown;
        }

        private void GameWheelCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Handle mouse wheel event
        }

        private void GameWheelCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Handle mouse click event
        }
    }
}

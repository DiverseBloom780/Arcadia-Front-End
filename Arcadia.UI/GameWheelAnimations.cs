using System;
using System.Windows;
using System.Windows.Controls;
namespace Arcadia.UI
{
    public class GameWheelAnimations
    {
        public Canvas GameWheelCanvas { get; set; }

        public GameWheelAnimations()
        {
            // Create game wheel animations
            var animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 360;
            animation.Duration = new Duration(TimeSpan.FromSeconds(1));
            animation.RepeatBehavior = RepeatBehavior.Forever;

            // Apply animation to game wheel canvas
            GameWheelCanvas.BeginAnimation(RotateTransform.AngleProperty, animation);
        }
    }
}

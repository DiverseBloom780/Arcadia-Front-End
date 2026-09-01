using System;
using Arcadia.Core.Models;

namespace Arcadia.Core
{
    /// <summary>
    /// Helper class that calculates the position (X, Y), scale, and opacity 
    /// for items in the Game Wheel based on the chosen WheelMode.
    /// </summary>
    public static class WheelRenderer
    {
        public struct ItemLayout
        {
            public double X;
            public double Y;
            public double Scale;
            public double Opacity;
            public int ZIndex;
        }

        /// <summary>
        /// Calculates the layout for a single item in the wheel.
        /// </summary>
        /// <param name="index">The index of the item in the list.</param>
        /// <param name="selectedIndex">The currently selected index (can be fractional for smooth animation).</param>
        /// <param name="totalItems">Total number of items in the wheel.</param>
        /// <param name="mode">The WheelMode layout style.</param>
        /// <param name="viewportWidth">Width of the rendering area.</param>
        /// <param name="viewportHeight">Height of the rendering area.</param>
        /// <returns>An ItemLayout struct containing the calculated properties.</returns>
        public static ItemLayout CalculateItemLayout(
            int index, 
            double selectedIndex, 
            int totalItems, 
            WheelMode mode, 
            double viewportWidth, 
            double viewportHeight)
        {
            // Calculate distance from center (0 = center, -1 = one above, 1 = one below)
            double distance = index - selectedIndex;
            
            // Handle wrapping for circular lists
            double halfTotal = totalItems / 2.0;
            if (distance > halfTotal) distance -= totalItems;
            if (distance < -halfTotal) distance += totalItems;

            ItemLayout layout = new ItemLayout
            {
                Scale = 1.0,
                Opacity = 1.0,
                ZIndex = 100 - (int)Math.Abs(distance * 10),
                X = 0,
                Y = 0
            };

            // Smooth falloff calculation for scale and opacity
            double absDist = Math.Abs(distance);
            
            // Maximum visible items distance from center
            double maxVisibleDist = 7.0; 

            if (absDist > maxVisibleDist)
            {
                layout.Opacity = 0;
                return layout;
            }

            switch (mode)
            {
                case WheelMode.Horizontal:
                    layout.X = distance * (viewportWidth * 0.15);
                    layout.Y = 0;
                    layout.Scale = Math.Max(0.5, 1.0 - (absDist * 0.1));
                    layout.Opacity = Math.Max(0.0, 1.0 - (absDist * 0.2));
                    break;
                    
                case WheelMode.Vertical:
                    layout.X = 0;
                    layout.Y = distance * (viewportHeight * 0.15);
                    layout.Scale = Math.Max(0.5, 1.0 - (absDist * 0.1));
                    layout.Opacity = Math.Max(0.0, 1.0 - (absDist * 0.2));
                    break;
                    
                case WheelMode.Curved:
                    layout.X = distance * (viewportWidth * 0.12);
                    layout.Y = Math.Pow(absDist, 2) * (viewportHeight * 0.02);
                    layout.Scale = Math.Max(0.4, 1.0 - (absDist * 0.15));
                    layout.Opacity = Math.Max(0.0, 1.0 - (absDist * 0.25));
                    break;
                    
                case WheelMode.Angled:
                    // Classic Hyperspin vertical right-side angled wheel
                    double curveX = Math.Pow(absDist, 1.5) * 20.0;
                    layout.X = curveX; 
                    layout.Y = distance * (viewportHeight * 0.12);
                    layout.Scale = Math.Max(0.3, 1.0 - (absDist * 0.12));
                    layout.Opacity = Math.Max(0.0, 1.0 - (absDist * 0.18));
                    break;
            }

            return layout;
        }
    }
}

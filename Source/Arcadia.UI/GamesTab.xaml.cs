using System.Collections.Generic;
using System.Windows.Controls;
using Arcadia.Core.Models; // Required for List<Game>

namespace Arcadia.UI.Tabs
{
    public partial class GamesTab : UserControl
    {
        private readonly List<Game> _games;
        
        // FIX: Constructor now accepts List<Game>
        public GamesTab(List<Game> games)
        {
            InitializeComponent();
            _games = games;
            // TODO: Add logic to render game wheel based on _games
        }
        
        // Safety parameterless constructor if needed by XAML preview
        public GamesTab() : this(new List<Game>())
        {
        }
    }
}
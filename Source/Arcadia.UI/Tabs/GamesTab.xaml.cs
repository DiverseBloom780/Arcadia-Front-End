using System.Collections.Generic;
using System.Windows.Controls;
using Arcadia.Core.Models; 

namespace Arcadia.UI.Tabs
{
    public partial class GamesTab : UserControl
    {
        private readonly List<Game> _games;
        
        // Constructor now accepts List<Game>
        public GamesTab(List<Game> games)
        {
            InitializeComponent();
            _games = games;
        }
        
        public GamesTab() : this(new List<Game>())
        {
        }
    }
}
using System.Collections.Generic;
using System.Windows.Controls;
using Arcadia.Core.Models;

namespace Arcadia.UI.Tabs
{
    public partial class GamesTab : UserControl
    {
        private readonly List<Game> _games;

        public GamesTab(List<Game> games)
        {
            InitializeComponent();
            _games = games;
        }

        // Parameterless constructor for XAML designer
        public GamesTab() : this(new List<Game>()) { }
    }
}

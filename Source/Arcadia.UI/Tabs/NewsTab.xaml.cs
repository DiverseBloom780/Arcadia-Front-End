using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Arcadia.UI.Tabs
{
    public partial class NewsTab : UserControl
    {
        private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(20) };
        private static readonly string EpicUrl = "https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions?locale=en-US&country=US&allowCountries=US";
        private static readonly string GogUrl = "https://catalog.gog.com/v1/catalog?countryCode=US&locale=en-US&limit=20&order=desc%3Adiscount&productType=in%3Agame";

        public NewsTab()
        {
            InitializeComponent();
            Loaded += async (_, _) => await RefreshAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await RefreshAsync();

        private async Task RefreshAsync()
        {
            StatusText.Text = "Refreshing live deals and news...";
            try
            {
                var results = await Task.WhenAll(
                    SafeLoadAsync(LoadEpicAsync, "Epic giveaways unavailable"),
                    SafeLoadAsync(LoadGogAsync, "GOG sale feed unavailable"),
                    SafeLoadAsync(LoadUpcomingAsync, "Upcoming-games feed unavailable"),
                    SafeLoadAsync(() => LoadEmulationAsync("shadPS4", "https://github.com/shadps4-emu/shadPS4/releases.atom"), "shadPS4 feed unavailable"),
                    SafeLoadAsync(() => LoadEmulationAsync("RPCS3", "https://github.com/RPCS3/rpcs3/releases.atom"), "RPCS3 feed unavailable"),
                    SafeLoadAsync(() => LoadEmulationAsync("SharpEmu", "https://github.com/sharpemu/sharpemu/commits/main.atom"), "SharpEmu feed unavailable"));
                EpicList.ItemsSource = results[0];
                GogList.ItemsSource = results[1];
                UpcomingList.ItemsSource = results[2];
                EmulationList.ItemsSource = results[3].Concat(results[4]).Concat(results[5]).ToList();
                StatusText.Text = $"Updated {DateTime.Now:g}  •  Double-click an item to open its source.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Some live sources could not be reached: {ex.Message}";
            }
        }

        private static async Task<List<DiscoverItem>> SafeLoadAsync(Func<Task<List<DiscoverItem>>> loader, string fallback)
        {
            try
            {
                return await loader();
            }
            catch
            {
                return new List<DiscoverItem> { new(fallback, "Check your connection or refresh later.", "", "") };
            }
        }

        private static async Task<List<DiscoverItem>> LoadEpicAsync()
        {
            using var doc = JsonDocument.Parse(await Client.GetStringAsync(EpicUrl));
            var list = new List<DiscoverItem>();
            foreach (var item in doc.RootElement.GetProperty("data").GetProperty("Catalog").GetProperty("searchStore").GetProperty("elements").EnumerateArray())
            {
                if (!item.TryGetProperty("promotions", out var promotions) || !promotions.TryGetProperty("promotionalOffers", out var offers) || offers.GetArrayLength() == 0)
                    continue;
                var price = item.GetProperty("price").GetProperty("totalPrice");
                if (price.GetProperty("discountPrice").GetInt32() != 0) continue;
                var endDate = offers[0].GetProperty("promotionalOffers")[0].GetProperty("endDate").GetDateTime().ToLocalTime();
                list.Add(new DiscoverItem(item.GetProperty("title").GetString() ?? "Unknown game", $"FREE UNTIL {endDate:g}", item.GetProperty("description").GetString() ?? "", $"https://store.epicgames.com/en-US/p/{item.GetProperty("productSlug").GetString()}"));
            }
            return list.Take(10).ToList();
        }

        private static async Task<List<DiscoverItem>> LoadGogAsync()
        {
            using var doc = JsonDocument.Parse(await Client.GetStringAsync(GogUrl));
            var list = new List<DiscoverItem>();
            foreach (var item in doc.RootElement.GetProperty("products").EnumerateArray())
            {
                var price = item.GetProperty("price");
                var discount = price.GetProperty("discount").GetString() ?? "";
                if (!discount.StartsWith("-")) continue;
                list.Add(new DiscoverItem(item.GetProperty("title").GetString() ?? "Unknown game", $"{discount}  •  {price.GetProperty("final").GetString()} (was {price.GetProperty("base").GetString()})", "GOG sale", item.GetProperty("storeLink").GetString() ?? "https://www.gog.com/"));
            }
            return list.Take(20).ToList();
        }

        private static async Task<List<DiscoverItem>> LoadUpcomingAsync()
        {
            var items = await LoadAtomAsync("PC game news", "https://www.pcgamer.com/rss/");
            var keywords = new[] { "release", "announce", "upcoming", "revealed", "launch", "coming" };
            var filtered = items.Where(item => keywords.Any(word => item.Title.Contains(word, StringComparison.OrdinalIgnoreCase))).ToList();
            return (filtered.Count > 0 ? filtered : items).Take(15).ToList();
        }

        private static async Task<List<DiscoverItem>> LoadEmulationAsync(string name, string url)
        {
            var items = await LoadAtomAsync(name, url);
            foreach (var item in items) item.Subtitle = name + "  •  " + item.Subtitle;
            return items.Take(10).ToList();
        }

        private static async Task<List<DiscoverItem>> LoadAtomAsync(string source, string url)
        {
            var xml = XDocument.Parse(await Client.GetStringAsync(url));
            XNamespace atom = "http://www.w3.org/2005/Atom";
            return xml.Root?.Name.LocalName == "feed"
                ? xml.Root.Elements(atom + "entry").Select(entry => new DiscoverItem(
                    entry.Element(atom + "title")?.Value.Trim() ?? "Untitled",
                    $"{source}  •  {entry.Element(atom + "updated")?.Value}", "",
                    entry.Elements(atom + "link").FirstOrDefault()?.Attribute("href")?.Value ?? "")).ToList()
                : new List<DiscoverItem>();
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox list && list.SelectedItem is DiscoverItem item && Uri.TryCreate(item.Url, UriKind.Absolute, out var uri))
                Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
        }

        private sealed class DiscoverItem
        {
            public string Title { get; }
            public string Subtitle { get; set; }
            public string Description { get; }
            public string Url { get; }
            public DiscoverItem(string title, string subtitle, string description, string url) => (Title, Subtitle, Description, Url) = (title, subtitle, description, url);
        }
    }
}

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RegencyMaintenance.Helpers;
using RegencyMaintenance.Services;

namespace RegencyMaintenance.Views;

public sealed partial class RoomDialog : ContentDialog
{
    private readonly string _roomName;
    private readonly bool _isCommonSpace;

    public RoomDialog(string roomName)
    {
        this.InitializeComponent();
        _roomName = roomName;
        _isCommonSpace = roomName == "SPAZIO COMUNE";
        this.Title = _isCommonSpace ? "SPAZIO COMUNE" : $"Stanza {roomName}";
        BuildTabs();
    }

    private void BuildTabs()
    {
        MainTabView.TabItems.Clear();

        if (_isCommonSpace)
            BuildCommonSpaceTabs();
        else
            BuildRoomTabs();
    }

    private void BuildRoomTabs()
    {
        MainTabView.TabItems.Add(CreateNoteTab("STANZA", "STANZA"));
        MainTabView.TabItems.Add(CreateNoteTab("BAGNO", "BAGNO"));
        MainTabView.TabItems.Add(CreateFilterTab());
        MainTabView.TabItems.Add(CreateBathroomFaucetTab());
        MainTabView.TabItems.Add(CreateBatteryTab());
    }

    private void BuildCommonSpaceTabs()
    {
        var simpleTabs = new[]
        {
            "ZONE COMUNI", "CUCINA", "BAR", "RICEVIMENTO",
            "CENTRALE IDRICA", "CENTRALE MEDIA TENSIONE",
            "CENTRALE ALTA TENSIONE", "CED",
            "LOCALE QUADRI CENTRALE ANTINCENDIO", "Note"
        };

        foreach (var tab in simpleTabs)
            MainTabView.TabItems.Add(CreateNoteTab(tab, tab));

        MainTabView.TabItems.Add(CreateFilterTab());
        MainTabView.TabItems.Add(CreateHydronicTab());
        MainTabView.TabItems.Add(CreateBatteryTab());
        MainTabView.TabItems.Add(CreateBathroomFaucetTab());
    }

    private TabViewItem CreateNoteTab(string header, string category)
    {
        var noteBox = new TextBox
        {
            PlaceholderText = "Inserisci note intervento...",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 120,
            Margin = new Thickness(0, 0, 0, 12)
        };

        var saveBtn = new Button
        {
            Content = "💾 SALVA NOTE",
            Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x4C, 0xAF, 0x50)),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Padding = new Thickness(16, 10, 16, 10),
            CornerRadius = new CornerRadius(6)
        };
        saveBtn.Click += async (s, e) =>
        {
            await DataService.Instance.AddInterventionAsync(_roomName, category, noteBox.Text.Trim());
            noteBox.Text = string.Empty;
            ShowSaveConfirmation(saveBtn);
        };

        var panel = new StackPanel { Padding = new Thickness(16), Spacing = 8 };
        panel.Children.Add(new TextBlock
        {
            Text = $"Note per {header}:",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            FontSize = 14
        });
        panel.Children.Add(noteBox);
        panel.Children.Add(saveBtn);

        return new TabViewItem { Header = header, Content = new ScrollViewer { Content = panel } };
    }

    private TabViewItem CreateFilterTab()
    {
        var header = _isCommonSpace ? "FILTRI ARIA CONDIZIONATA" : "FILTRI";
        var rooms = DataService.GetAllFilterRooms();

        var panel = new StackPanel { Padding = new Thickness(16), Spacing = 12 };

        panel.Children.Add(new TextBlock
        {
            Text = "Filtri Aria Condizionata - Ciclo: 20 giorni",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            FontSize = 14
        });

        panel.Children.Add(new TextBlock
        {
            Text = "🟢 OK (<15gg)  🟠 TRA POCO (15-19gg)  🔴 DA PULIRE (≥20gg)",
            FontSize = 12,
            Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x60, 0x60, 0x60))
        });

        var wrapPanel = new WrapGrid { Orientation = Orientation.Horizontal, MaximumRowsOrColumns = 8 };

        foreach (var room in rooms)
        {
            var btn = CreateFilterRoomButton(room, DataService.Instance.RoomFilterDates,
                async (r) =>
                {
                    var today = DateHelper.TodayIso;
                    await DataService.Instance.SetRoomFilterDateAsync(r, today);
                    await DataService.Instance.AddInterventionAsync(r, "FILTRO ARIA CONDIZIONATA", $"Filtro pulito - {DateHelper.TodayFormatted}");
                    BuildTabs();
                    MainTabView.SelectedIndex = _isCommonSpace ? 10 : 2;
                });
            wrapPanel.Children.Add(btn);
        }

        var scrollForGrid = new ScrollViewer { Content = wrapPanel, MaxHeight = 300 };
        panel.Children.Add(scrollForGrid);

        // Set all today button
        var setAllBtn = new Button
        {
            Content = "📅 IMPOSTA OGGI PER TUTTE LE STANZE",
            Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x21, 0x96, 0xF3)),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Padding = new Thickness(14, 10, 14, 10),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 8, 0, 0)
        };
        setAllBtn.Click += async (s, e) =>
        {
            var today = DateHelper.TodayIso;
            foreach (var room in rooms)
                await DataService.Instance.SetRoomFilterDateAsync(room, today);
            await DataService.Instance.AddInterventionAsync("TUTTI", "FILTRO ARIA CONDIZIONATA", $"Tutti i filtri puliti - {DateHelper.TodayFormatted}");
            BuildTabs();
            MainTabView.SelectedIndex = _isCommonSpace ? 10 : 2;
        };
        panel.Children.Add(setAllBtn);

        // Export button
        var exportBtn = new Button
        {
            Content = "📄 ESPORTA STATO FILTRI IN TXT",
            Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x78, 0x90, 0x9C)),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Padding = new Thickness(14, 10, 14, 10),
            CornerRadius = new CornerRadius(6)
        };
        exportBtn.Click += async (s, e) => await ExportFilterStatusAsync(rooms, DataService.Instance.RoomFilterDates, "filtri");
        panel.Children.Add(exportBtn);

        return new TabViewItem { Header = header, Content = new ScrollViewer { Content = panel } };
    }

    private Button CreateFilterRoomButton(string room,
        Dictionary<string, Models.RoomFilterData> dates,
        Func<string, Task> onClickAsync)
    {
        string color;
        string statusEmoji;
        int days = 0;

        if (dates.TryGetValue(room, out var fd) && !string.IsNullOrEmpty(fd.LastCleaning))
        {
            days = DateHelper.DaysSince(fd.LastCleaning);
            if (days >= 20) { color = "#FFCCCC"; statusEmoji = "🔴"; }
            else if (days >= 15) { color = "#FFE6CC"; statusEmoji = "🟠"; }
            else { color = "#CCFFCC"; statusEmoji = "🟢"; }
        }
        else
        {
            color = "#E0E0E0";
            statusEmoji = "❓";
        }

        var btn = new Button
        {
            Content = $"{statusEmoji} {room}\n{(days > 0 ? $"{days}gg" : "Mai")}",
            Background = BrushFromHex(color),
            Foreground = new SolidColorBrush(Colors.Black),
            FontSize = 12,
            Padding = new Thickness(10, 8, 10, 8),
            Margin = new Thickness(3),
            CornerRadius = new CornerRadius(6),
            MinWidth = 80
        };
        btn.Click += async (s, e) => await onClickAsync(room);
        return btn;
    }

    private TabViewItem CreateHydronicTab()
    {
        var components = DataService.GetHydronicComponents();
        var panel = new StackPanel { Padding = new Thickness(16), Spacing = 8 };

        panel.Children.Add(new TextBlock
        {
            Text = "Filtri Y Moduli Idronici - Ciclo: 6 mesi",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold, FontSize = 14
        });
        panel.Children.Add(new TextBlock
        {
            Text = "🟢 OK (<4 mesi)  🟠 ATTENZIONE (4-5 mesi)  🔴 DA SOSTITUIRE (≥6 mesi)",
            FontSize = 12
        });

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Header
        AddGridRow(grid, 0, "COMPONENTE", "ULTIMA MANUTENZIONE", "MESI", "STATO", "AZIONE",
            isBold: true);

        int row = 1;
        foreach (var comp in components)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            string lastDate = "Mai";
            double months = 0;
            string status = "Non registrato";
            string color = "#E0E0E0";

            if (DataService.Instance.HydronicFilterDates.TryGetValue(comp, out var hd) && !string.IsNullOrEmpty(hd.LastMaintenance))
            {
                lastDate = DateHelper.IsoToItalian(hd.LastMaintenance);
                months = DateHelper.MonthsBetween(hd.LastMaintenance);
                if (months >= 6) { status = "🔴 DA SOSTITUIRE"; color = "#FFCCCC"; }
                else if (months >= 4) { status = "🟠 ATTENZIONE"; color = "#FFE6CC"; }
                else { status = "🟢 OK"; color = "#CCFFCC"; }
            }

            var bgBrush = BrushFromHex(color);

            var compText = new TextBlock { Text = comp, FontSize = 12, Padding = new Thickness(4), TextWrapping = TextWrapping.Wrap };
            var dateText = new TextBlock { Text = lastDate, FontSize = 12, Padding = new Thickness(4) };
            var monthsText = new TextBlock { Text = $"{months:F1}", FontSize = 12, Padding = new Thickness(4) };
            var statusText = new TextBlock { Text = status, FontSize = 12, Padding = new Thickness(4), Background = bgBrush };

            string compCapture = comp;
            var actionBtn = new Button
            {
                Content = "Aggiorna",
                FontSize = 11, Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x4C, 0xAF, 0x50)),
                Foreground = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(4), Margin = new Thickness(2)
            };
            actionBtn.Click += async (s, e) =>
            {
                await DataService.Instance.SetHydronicFilterDateAsync(compCapture, DateHelper.TodayIso);
                await DataService.Instance.AddInterventionAsync("SPAZIO COMUNE", "FILTRO Y MODULO IDRONICO", $"Sostituito: {compCapture} - {DateHelper.TodayFormatted}");
                BuildTabs();
                MainTabView.SelectedIndex = 11;
            };

            Grid.SetRow(compText, row); Grid.SetColumn(compText, 0);
            Grid.SetRow(dateText, row); Grid.SetColumn(dateText, 1);
            Grid.SetRow(monthsText, row); Grid.SetColumn(monthsText, 2);
            Grid.SetRow(statusText, row); Grid.SetColumn(statusText, 3);
            Grid.SetRow(actionBtn, row); Grid.SetColumn(actionBtn, 4);

            grid.Children.Add(compText);
            grid.Children.Add(dateText);
            grid.Children.Add(monthsText);
            grid.Children.Add(statusText);
            grid.Children.Add(actionBtn);

            row++;
        }

        panel.Children.Add(grid);
        return new TabViewItem { Header = "FILTRI Y MODULI IDRONICI", Content = new ScrollViewer { Content = panel } };
    }

    private TabViewItem CreateBatteryTab()
    {
        var rooms = DataService.GetBatteryRooms();
        var panel = new StackPanel { Padding = new Thickness(16), Spacing = 8 };

        panel.Children.Add(new TextBlock
        {
            Text = "Batterie Porte Stanze - Ciclo: 365 giorni",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold, FontSize = 14
        });
        panel.Children.Add(new TextBlock
        {
            Text = "🟢 OK (<300gg)  🟠 ATTENZIONE (300-364gg)  🔴 DA SOSTITUIRE (≥365gg)",
            FontSize = 12
        });

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        AddGridRow(grid, 0, "STANZA", "ULTIMA SOSTITUZIONE", "GIORNI", "STATO", "AZIONE", isBold: true);

        int row = 1;
        foreach (var room in rooms)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            string lastDate = "Mai";
            int days = 0;
            string status = "Non registrato";
            string color = "#E0E0E0";

            if (DataService.Instance.BatteryDates.TryGetValue(room, out var bd) && !string.IsNullOrEmpty(bd.LastChange))
            {
                lastDate = DateHelper.IsoToItalian(bd.LastChange);
                days = DateHelper.DaysSince(bd.LastChange);
                if (days >= 365) { status = "🔴 DA SOSTITUIRE"; color = "#FFCCCC"; }
                else if (days >= 300) { status = "🟠 ATTENZIONE"; color = "#FFE6CC"; }
                else { status = "🟢 OK"; color = "#CCFFCC"; }
            }

            var bgBrush = BrushFromHex(color);

            var roomText = new TextBlock { Text = room, FontSize = 12, Padding = new Thickness(4) };
            var dateText = new TextBlock { Text = lastDate, FontSize = 12, Padding = new Thickness(4) };
            var daysText = new TextBlock { Text = days > 0 ? days.ToString() : "-", FontSize = 12, Padding = new Thickness(4) };
            var statusText = new TextBlock { Text = status, FontSize = 12, Padding = new Thickness(4), Background = bgBrush };

            string roomCapture = room;
            var actionBtn = new Button
            {
                Content = "Sostituita",
                FontSize = 11, Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x4C, 0xAF, 0x50)),
                Foreground = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(4), Margin = new Thickness(2)
            };
            actionBtn.Click += async (s, e) =>
            {
                await DataService.Instance.SetBatteryDateAsync(roomCapture, DateHelper.TodayIso);
                await DataService.Instance.AddInterventionAsync(roomCapture, "BATTERIA PORTA", $"Batteria sostituita - {DateHelper.TodayFormatted}");
                BuildTabs();
                int idx = _isCommonSpace ? 12 : 4;
                MainTabView.SelectedIndex = idx;
            };

            Grid.SetRow(roomText, row); Grid.SetColumn(roomText, 0);
            Grid.SetRow(dateText, row); Grid.SetColumn(dateText, 1);
            Grid.SetRow(daysText, row); Grid.SetColumn(daysText, 2);
            Grid.SetRow(statusText, row); Grid.SetColumn(statusText, 3);
            Grid.SetRow(actionBtn, row); Grid.SetColumn(actionBtn, 4);

            grid.Children.Add(roomText);
            grid.Children.Add(dateText);
            grid.Children.Add(daysText);
            grid.Children.Add(statusText);
            grid.Children.Add(actionBtn);

            row++;
        }

        panel.Children.Add(grid);

        var exportBtn = new Button
        {
            Content = "📄 ESPORTA IN TXT",
            Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x78, 0x90, 0x9C)),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Padding = new Thickness(12, 8, 12, 8), CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 8, 0, 0)
        };
        exportBtn.Click += async (s, e) => await ExportBatteryStatusAsync(rooms);
        panel.Children.Add(exportBtn);

        return new TabViewItem { Header = "BATTERIE PORTE STANZE", Content = new ScrollViewer { Content = panel } };
    }

    private TabViewItem CreateBathroomFaucetTab()
    {
        var rooms = DataService.GetAllFilterRooms();
        var panel = new StackPanel { Padding = new Thickness(16), Spacing = 12 };

        panel.Children.Add(new TextBlock
        {
            Text = "Rompigetto Bagno - Ciclo: 180 giorni",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold, FontSize = 14
        });
        panel.Children.Add(new TextBlock
        {
            Text = "🟢 OK (<150gg)  🟠 ATTENZIONE (150-179gg)  🔴 DA SOSTITUIRE (≥180gg)",
            FontSize = 12
        });

        var wrapPanel = new WrapGrid { Orientation = Orientation.Horizontal, MaximumRowsOrColumns = 8 };
        foreach (var room in rooms)
        {
            var btn = CreateFaucetRoomButton(room);
            wrapPanel.Children.Add(btn);
        }

        panel.Children.Add(new ScrollViewer { Content = wrapPanel, MaxHeight = 300 });

        var setAllBtn = new Button
        {
            Content = "📅 IMPOSTA OGGI PER TUTTE",
            Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x21, 0x96, 0xF3)),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Padding = new Thickness(14, 10, 14, 10), CornerRadius = new CornerRadius(6)
        };
        setAllBtn.Click += async (s, e) =>
        {
            foreach (var room in rooms)
            {
                await DataService.Instance.SetBathroomFaucetDateAsync(room, DateHelper.TodayIso);
                await DataService.Instance.AddInterventionAsync(room, "ROMPIGETTO BAGNO", $"Rompigetto sostituito - {DateHelper.TodayFormatted}");
            }
            BuildTabs();
            int idx = _isCommonSpace ? 13 : 3;
            MainTabView.SelectedIndex = idx;
        };
        panel.Children.Add(setAllBtn);

        return new TabViewItem { Header = "ROMPIGETTO BAGNO", Content = new ScrollViewer { Content = panel } };
    }

    private Button CreateFaucetRoomButton(string room)
    {
        string color;
        string statusEmoji;
        int days = 0;

        if (DataService.Instance.BathroomFaucetDates.TryGetValue(room, out var fd) && !string.IsNullOrEmpty(fd.LastMaintenance))
        {
            days = DateHelper.DaysSince(fd.LastMaintenance);
            if (days >= 180) { color = "#FFCCCC"; statusEmoji = "🔴"; }
            else if (days >= 150) { color = "#FFE6CC"; statusEmoji = "🟠"; }
            else { color = "#CCFFCC"; statusEmoji = "🟢"; }
        }
        else
        {
            color = "#E0E0E0";
            statusEmoji = "❓";
        }

        string roomCapture = room;
        var btn = new Button
        {
            Content = $"{statusEmoji} {room}\n{(days > 0 ? $"{days}gg" : "Mai")}",
            Background = BrushFromHex(color),
            Foreground = new SolidColorBrush(Colors.Black),
            FontSize = 12,
            Padding = new Thickness(10, 8, 10, 8),
            Margin = new Thickness(3),
            CornerRadius = new CornerRadius(6),
            MinWidth = 80
        };
        btn.Click += async (s, e) =>
        {
            await DataService.Instance.SetBathroomFaucetDateAsync(roomCapture, DateHelper.TodayIso);
            await DataService.Instance.AddInterventionAsync(roomCapture, "ROMPIGETTO BAGNO", $"Rompigetto sostituito - {DateHelper.TodayFormatted}");
            BuildTabs();
            int idx = _isCommonSpace ? 13 : 3;
            MainTabView.SelectedIndex = idx;
        };
        return btn;
    }

    private void AddGridRow(Grid grid, int row, string col1, string col2, string col3, string col4, string col5, bool isBold = false)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var weight = isBold ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal;
        var bg = isBold ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xBB, 0xDE, 0xFB)) : null;

        var texts = new[] { col1, col2, col3, col4, col5 };
        for (int c = 0; c < texts.Length; c++)
        {
            var tb = new TextBlock
            {
                Text = texts[c],
                FontWeight = weight,
                FontSize = 12,
                Padding = new Thickness(4, 6, 4, 6)
            };
            if (bg != null)
                tb.Background = bg;
            Grid.SetRow(tb, row);
            Grid.SetColumn(tb, c);
            grid.Children.Add(tb);
        }
    }

    private async Task ExportFilterStatusAsync(List<string> rooms,
        Dictionary<string, Models.RoomFilterData> dates, string name)
    {
        var lines = new List<string>
        {
            $"=== STATO {name.ToUpper()} ===",
            $"Data: {DateHelper.NowFormatted}",
            string.Empty
        };
        foreach (var room in rooms)
        {
            if (dates.TryGetValue(room, out var fd))
            {
                var days = DateHelper.DaysSince(fd.LastCleaning);
                var status = days >= 20 ? "DA PULIRE" : days >= 15 ? "TRA POCO" : "OK";
                lines.Add($"Stanza {room}: {days} giorni ({DateHelper.IsoToItalian(fd.LastCleaning)}) - {status}");
            }
            else
                lines.Add($"Stanza {room}: Mai registrata");
        }

        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{name}_{DateHelper.TimestampFile}.txt");
        await File.WriteAllLinesAsync(path, lines);

        var dlg = new ContentDialog
        {
            Title = "Esportato",
            Content = $"File salvato in:\n{path}",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dlg.ShowAsync();
    }

    private async Task ExportBatteryStatusAsync(List<string> rooms)
    {
        var lines = new List<string>
        {
            "=== STATO BATTERIE PORTE STANZE ===",
            $"Data: {DateHelper.NowFormatted}",
            string.Empty
        };
        foreach (var room in rooms)
        {
            if (DataService.Instance.BatteryDates.TryGetValue(room, out var bd))
            {
                var days = DateHelper.DaysSince(bd.LastChange);
                var status = days >= 365 ? "DA SOSTITUIRE" : days >= 300 ? "ATTENZIONE" : "OK";
                lines.Add($"Stanza {room}: {days} giorni ({DateHelper.IsoToItalian(bd.LastChange)}) - {status}");
            }
            else
                lines.Add($"Stanza {room}: Mai registrata");
        }

        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"batterie_{DateHelper.TimestampFile}.txt");
        await File.WriteAllLinesAsync(path, lines);

        var dlg = new ContentDialog
        {
            Title = "Esportato",
            Content = $"File salvato in:\n{path}",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dlg.ShowAsync();
    }

    private static SolidColorBrush BrushFromHex(string hexColor)
    {
        var hex = hexColor.TrimStart('#');
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0xFF, r, g, b));
    }

    private static void ShowSaveConfirmation(Button btn)
    {
        var originalContent = btn.Content;
        btn.Content = "✅ Salvato!";
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        timer.Tick += (s, e) =>
        {
            btn.Content = originalContent;
            ((DispatcherTimer)s!).Stop();
        };
        timer.Start();
    }
}

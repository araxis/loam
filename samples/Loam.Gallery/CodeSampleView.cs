using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using Loam;
using Loam.Controls;
using LoamIconButton = Loam.Controls.IconButton;

namespace Loam.Gallery;

/// <summary>Editor-style code sample surface for gallery pages.</summary>
public sealed class CodeSampleView : UserControl
{
    private static readonly FontFamily CodeFont = new("Cascadia Mono, Consolas, monospace");
    private static readonly IBrush EditorBackground = Brush("#111827");
    private static readonly IBrush EditorPanel = Brush("#0F172A");
    private static readonly IBrush EditorBorder = Brush("#273449");
    private static readonly IBrush Text = Brush("#D6E2F0");
    private static readonly IBrush Muted = Brush("#708199");
    private static readonly IBrush Keyword = Brush("#7DD3FC");
    private static readonly IBrush TypeName = Brush("#FBBF24");
    private static readonly IBrush Member = Brush("#C4B5FD");
    private static readonly IBrush String = Brush("#86EFAC");
    private static readonly IBrush Number = Brush("#FDBA74");
    private static readonly IBrush Comment = Brush("#6EE7B7");
    private static readonly IBrush Punctuation = Brush("#A7B4C7");
    private static readonly IBrush Accent = Brush("#38BDF8");
    private static readonly IBrush Warn = Brush("#F59E0B");
    private static readonly IBrush Danger = Brush("#F87171");
    private static readonly TimeSpan CopyStatusDuration = TimeSpan.FromMilliseconds(1400);

    private readonly string _copyText;
    private LoamIconButton? _copyButton;
    private TextBlock? _copyStatus;
    private int _copyStatusVersion;

    private static readonly HashSet<string> Keywords =
    [
        "await", "bool", "break", "case", "class", "const", "default", "else", "false",
        "for", "foreach", "if", "in", "is", "new", "null", "private", "public", "return",
        "static", "string", "true", "var", "void",
    ];

    private static readonly HashSet<string> Types =
    [
        "Alert", "AppBar", "Autocomplete", "Avatar", "AvatarGroup", "Badge", "BarChart", "Border", "BreadcrumbItem", "Breadcrumbs", "ButtonGroup", "Card",
        "CardContent", "CardHeader", "CarouselItem", "Chip", "ChipSet", "Collapse",
        "CheckBox", "Container", "DataGrid", "DatePicker", "DateRangePicker", "DateTime", "DialogService", "DockPanel", "Drawer", "DrawerMode", "ExpansionPanel", "ExpansionPanels", "Fab",
        "Col", "Field", "FieldEditor", "FileUpload", "Form", "Grid", "Hidden", "IconButton", "Item", "Layout", "LineChart", "ResponsiveGrid",
        "Link", "ListItem", "ListSubheader", "LoamButton", "LoamColor", "LoamSize", "MainContent", "MaskedTextField", "Menu", "MenuItem", "MonthCalendar", "NavGroup",
        "NavLink", "NavMenu", "NumericField", "Pagination", "Paper", "PieChart", "Popover", "ProgressCircular",
        "ProgressLinear", "Radio", "Rating", "Ripple", "Select", "SelectItem", "Skeleton", "Slider", "StackPanel",
        "SnackbarOptions", "SnackbarService", "Spacer", "Step", "Stepper", "TabItem", "TableRow", "Tabs", "Text", "TextBlock", "TextBox", "TextField",
        "Switch", "TimePicker", "Timeline", "TimelineItem", "ToggleGroup", "ToggleIconButton", "ToggleItem", "Tooltip", "Variant", "WrapPanel",
    ];

    public CodeSampleView(string title, string code)
    {
        var lines = Normalize(code);
        _copyText = string.Join(Environment.NewLine, lines);
        Content = Build(title, lines);
    }

    private Border Build(string title, string[] lines)
    {
        var rowStack = new StackPanel { Spacing = 0 };
        for (var i = 0; i < lines.Length; i++)
        {
            rowStack.Children.Add(CodeLine(i + 1, lines[i]));
        }

        var scroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new Border
            {
                Background = EditorBackground,
                Padding = new Thickness(0, 12, 18, 16),
                Child = rowStack,
            },
        };

        var root = new DockPanel
        {
            LastChildFill = true,
            Children = { Header(title), scroller },
        };

        return new Border
        {
            Background = EditorBackground,
            BorderBrush = EditorBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Child = root,
        };
    }

    private Border Header(string title)
    {
        var dots = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
            Children =
            {
                Dot(Danger),
                Dot(Warn),
                Dot(Accent),
            },
        };

        var name = new TextBlock
        {
            Text = ToFileName(title),
            FontFamily = CodeFont,
            FontSize = 12,
            Foreground = Text,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var label = new Border
        {
            Background = Brush("#172033"),
            BorderBrush = EditorBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 0),
            Height = 24,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = "C#",
                FontFamily = CodeFont,
                FontSize = 11,
                Foreground = Accent,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            },
        };

        _copyStatus = new TextBlock
        {
            MinWidth = 56,
            Text = string.Empty,
            FontFamily = CodeFont,
            FontSize = 11,
            Foreground = String,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false,
        };

        _copyButton = new LoamIconButton
        {
            Icon = Icons.Material.Filled.ContentCopy,
            Variant = Variant.Outlined,
            Color = LoamColor.Info,
            Size = LoamSize.Small,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AutomationProperties.SetName(_copyButton, "Copy code");
        Tooltip.Set(_copyButton, "Copy code");
        _copyButton.Click += OnCopyClicked;

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { label, _copyButton, _copyStatus },
        };

        var layout = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Margin = new Thickness(14, 0),
            Children = { dots, name, actions },
        };
        Avalonia.Controls.Grid.SetColumn(name, 1);
        Avalonia.Controls.Grid.SetColumn(actions, 2);

        var header = new Border
        {
            Height = 42,
            Background = EditorPanel,
            BorderBrush = EditorBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = layout,
        };
        DockPanel.SetDock(header, Dock.Top);
        return header;
    }

    private async void OnCopyClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is null)
            {
                ShowCopyState("Unavailable", LoamColor.Error, Icons.Material.Filled.Close, "Clipboard unavailable");
                return;
            }

            await clipboard.SetTextAsync(_copyText);
            ShowCopyState("Copied", LoamColor.Success, Icons.Material.Filled.Check, "Code copied");
        }
        catch (Exception)
        {
            ShowCopyState("Failed", LoamColor.Error, Icons.Material.Filled.Close, "Copy failed");
        }
    }

    private void ShowCopyState(string text, LoamColor color, string icon, string automationName)
    {
        if (_copyButton is null || _copyStatus is null)
        {
            return;
        }

        _copyButton.Icon = icon;
        _copyButton.Color = color;
        AutomationProperties.SetName(_copyButton, automationName);

        _copyStatus.Text = text;
        _copyStatus.Foreground = color == LoamColor.Error ? Danger : String;
        _copyStatus.IsVisible = true;

        var version = ++_copyStatusVersion;
        DispatcherTimer.RunOnce(() =>
        {
            if (version == _copyStatusVersion)
            {
                ResetCopyState();
            }
        }, CopyStatusDuration);
    }

    private void ResetCopyState()
    {
        if (_copyButton is not null)
        {
            _copyButton.Icon = Icons.Material.Filled.ContentCopy;
            _copyButton.Color = LoamColor.Info;
            AutomationProperties.SetName(_copyButton, "Copy code");
        }

        if (_copyStatus is not null)
        {
            _copyStatus.Text = string.Empty;
            _copyStatus.IsVisible = false;
        }
    }

    private static Border Dot(IBrush brush) => new()
    {
        Width = 10,
        Height = 10,
        CornerRadius = new CornerRadius(5),
        Background = brush,
    };

    private static Avalonia.Controls.Grid CodeLine(int number, string line)
    {
        var grid = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("54,Auto"),
            MinHeight = 21,
        };

        grid.Children.Add(new TextBlock
        {
            Text = number.ToString("00"),
            FontFamily = CodeFont,
            FontSize = 12,
            LineHeight = 21,
            Foreground = Muted,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 12, 0),
        });

        var code = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { },
        };
        foreach (var token in Tokenize(line))
        {
            code.Children.Add(new TextBlock
            {
                Text = token.Text,
                FontFamily = CodeFont,
                FontSize = 13,
                LineHeight = 21,
                Foreground = token.Brush,
            });
        }

        Avalonia.Controls.Grid.SetColumn(code, 1);
        grid.Children.Add(code);
        return grid;
    }

    private static string[] Normalize(string code)
    {
        var lines = code.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        var indent = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(LeadingSpaces)
            .DefaultIfEmpty(0)
            .Min();

        return lines.Select(line => line.Length >= indent ? line[indent..] : line).ToArray();
    }

    private static IEnumerable<CodeToken> Tokenize(string line)
    {
        for (var i = 0; i < line.Length;)
        {
            if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '/')
            {
                yield return new CodeToken(line[i..], Comment);
                yield break;
            }

            if (char.IsWhiteSpace(line[i]))
            {
                var start = i;
                while (i < line.Length && char.IsWhiteSpace(line[i]))
                {
                    i++;
                }

                yield return new CodeToken(line[start..i], Text);
                continue;
            }

            if (line[i] == '"')
            {
                var start = i++;
                while (i < line.Length)
                {
                    if (line[i] == '"' && line[i - 1] != '\\')
                    {
                        i++;
                        break;
                    }

                    i++;
                }

                yield return new CodeToken(line[start..Math.Min(i, line.Length)], String);
                continue;
            }

            if (char.IsLetter(line[i]) || line[i] == '_')
            {
                var start = i++;
                while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_'))
                {
                    i++;
                }

                var word = line[start..i];
                yield return new CodeToken(word, BrushForWord(word, start > 0 && line[start - 1] == '.'));
                continue;
            }

            if (char.IsDigit(line[i]))
            {
                var start = i++;
                while (i < line.Length && (char.IsDigit(line[i]) || line[i] == '.'))
                {
                    i++;
                }

                yield return new CodeToken(line[start..i], Number);
                continue;
            }

            yield return new CodeToken(line[i++].ToString(), Punctuation);
        }
    }

    private static IBrush BrushForWord(string word, bool memberAccess)
    {
        if (memberAccess)
        {
            return Member;
        }

        if (Keywords.Contains(word))
        {
            return Keyword;
        }

        if (Types.Contains(word) || char.IsUpper(word[0]))
        {
            return TypeName;
        }

        return Text;
    }

    private static int LeadingSpaces(string line)
    {
        var count = 0;
        while (count < line.Length && line[count] == ' ')
        {
            count++;
        }

        return count;
    }

    private static string ToFileName(string title)
    {
        var clean = new string(title.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(clean) ? "Sample.cs" : $"{clean}Sample.cs";
    }

    private static ImmutableSolidColorBrush Brush(string hex) => new(Color.Parse(hex));

    private readonly record struct CodeToken(string Text, IBrush Brush);
}

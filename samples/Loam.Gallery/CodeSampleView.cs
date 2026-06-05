using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;

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

    private static readonly HashSet<string> Keywords =
    [
        "await", "bool", "break", "case", "class", "const", "default", "else", "false",
        "for", "foreach", "if", "in", "is", "new", "null", "private", "public", "return",
        "static", "string", "true", "var", "void",
    ];

    private static readonly HashSet<string> Types =
    [
        "Alert", "Avatar", "Badge", "BarChart", "Border", "ButtonGroup", "Card",
        "CardContent", "CardHeader", "CarouselItem", "Chip", "ChipSet", "Collapse",
        "DataGrid", "DateTime", "DockPanel", "ExpansionPanel", "ExpansionPanels", "Fab",
        "Field", "FileUpload", "Form", "Hidden", "IconButton", "Item", "LineChart",
        "ListItem", "ListSubheader", "LoamButton", "LoamColor", "MonthCalendar", "NavGroup",
        "NavLink", "NavMenu", "Pagination", "Paper", "PieChart", "Popover", "ProgressCircular",
        "ProgressLinear", "Ripple", "Select", "SelectItem", "Skeleton", "Slider", "StackPanel",
        "Step", "Stepper", "TableRow", "Text", "TextBlock", "TextBox", "TextField",
        "Timeline", "TimelineItem", "ToggleGroup", "ToggleIconButton", "ToggleItem", "Variant",
    ];

    public CodeSampleView(string title, string code)
    {
        Content = Build(title, Normalize(code));
    }

    private static Border Build(string title, string[] lines)
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

    private static Border Header(string title)
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

        var layout = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Margin = new Thickness(14, 0),
            Children = { dots, name, label },
        };
        Avalonia.Controls.Grid.SetColumn(name, 1);
        Avalonia.Controls.Grid.SetColumn(label, 2);

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

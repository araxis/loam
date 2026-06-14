using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>One step in a <see cref="Stepper"/>, mirroring the reference API's <c>Step</c>.</summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
    Justification = "Mirrors the reference API's Step with the Loam convention of dropping the legacy prefix.")]
public sealed class Step
{
    /// <summary>Creates an empty step.</summary>
    public Step()
    {
    }

    /// <summary>Creates a step with a title and content.</summary>
    public Step(string title, object? content)
    {
        Title = title;
        Content = content;
    }

    /// <summary>The step's header title.</summary>
    public string? Title { get; set; }

    /// <summary>The step's body content (string or any <see cref="Control"/>).</summary>
    public object? Content { get; set; }

    /// <summary>Whether the step is marked complete.</summary>
    public bool Completed { get; set; }
}

/// <summary>
/// A linear step wizard, mirroring the reference API's <c>Stepper</c>. Shows numbered <see cref="Steps"/>
/// with connectors, the active step's content, and Back/Next (Finish) navigation. The active step is
/// <see cref="ActiveIndex"/>; <see cref="OnCompleted"/> fires when the last step is finished.
/// </summary>
public class Stepper : TemplatedControl
{
    /// <summary>Identifies the <see cref="ActiveIndex"/> property.</summary>
    public static readonly StyledProperty<int> ActiveIndexProperty =
        AvaloniaProperty.Register<Stepper, int>(nameof(ActiveIndex),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private readonly List<(Border Circle, Text Number, Icon Check, Text Title)> _markers = new();
    private StackPanel? _header;
    private ContentControl? _content;
    private Button? _back;
    private Button? _next;
    private bool _coercing;

    /// <summary>Creates the stepper.</summary>
    public Stepper()
    {
        AutomationProperties.SetName(this, "Stepper");
        Steps.CollectionChanged += OnStepsChanged;
    }

    /// <summary>The wizard steps.</summary>
    public ObservableCollection<Step> Steps { get; } = new();

    /// <summary>The active step index (two-way).</summary>
    public int ActiveIndex
    {
        get => GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }

    /// <summary>Invoked when the final step is finished.</summary>
    public Action? OnCompleted { get; set; }

    /// <summary>Advances to the next step, marking the current one complete; finishes on the last step.</summary>
    public void Next()
    {
        if (!IsEnabled || Steps.Count == 0)
        {
            return;
        }

        if (ActiveIndex >= Steps.Count - 1)
        {
            if (ActiveIndex >= 0 && ActiveIndex < Steps.Count)
            {
                Steps[ActiveIndex].Completed = true;
                UpdateActive();
            }

            OnCompleted?.Invoke();
            return;
        }

        Steps[ActiveIndex].Completed = true;
        ActiveIndex++;
    }

    /// <summary>Returns to the previous step.</summary>
    public void Previous()
    {
        if (IsEnabled && ActiveIndex > 0)
        {
            ActiveIndex--;
        }
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Stepper);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _header = e.NameScope.Find("PART_Header") as StackPanel;
        _content = e.NameScope.Find("PART_Content") as ContentControl;
        _back = e.NameScope.Find("PART_Back") as Button;
        _next = e.NameScope.Find("PART_Next") as Button;
        if (_back is not null)
        {
            AutomationProperties.SetName(_back, "Previous step");
            _back.Click += (_, _) => Previous();
        }

        if (_next is not null)
        {
            AutomationProperties.SetName(_next, "Next step");
            _next.Click += (_, _) => Next();
        }

        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActiveIndexProperty)
        {
            CoerceActiveIndex();
            UpdateActive();
            ShowContent();
            UpdateButtons();
        }
        else if (change.Property == IsEnabledProperty)
        {
            UpdateButtons();
            Opacity = IsEnabled ? 1 : Controls.Internal.InteractionAssist.DisabledOpacity(this);
        }
    }

    private void OnStepsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Rebuild();
        CoerceActiveIndex();
        UpdateActive();
        ShowContent();
        UpdateButtons();
    }

    private void Rebuild()
    {
        if (_header is null)
        {
            return;
        }

        _header.Children.Clear();
        _markers.Clear();

        for (var i = 0; i < Steps.Count; i++)
        {
            if (i > 0)
            {
                var connector = new Border
                {
                    Height = 2,
                    Width = 32,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0),
                };
                connector.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Divider));
                _header.Children.Add(connector);
            }

            var number = new Text
            {
                Text = (i + 1).ToString(),
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };
            var check = new Icon
            {
                Data = Icons.Material.Filled.Check,
                Size = LoamSize.Small,
                Color = LoamColor.Inherit,
                IsVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var circle = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Child = new Panel { Children = { number, check } },
            };

            var title = new Text { Text = Steps[i].Title, Typo = Typo.Body2, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };

            var step = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Children = { circle, title } };
            AutomationProperties.SetName(step, Steps[i].Title ?? $"Step {i + 1}");
            _header.Children.Add(step);
            _markers.Add((circle, number, check, title));
        }

        UpdateActive();
        ShowContent();
        UpdateButtons();
    }

    private void CoerceActiveIndex()
    {
        if (_coercing)
        {
            return;
        }

        var value = Steps.Count <= 0 ? 0 : Math.Clamp(ActiveIndex, 0, Steps.Count - 1);
        if (value == ActiveIndex)
        {
            return;
        }

        _coercing = true;
        try
        {
            ActiveIndex = value;
        }
        finally
        {
            _coercing = false;
        }
    }

    private void UpdateActive()
    {
        var count = Math.Min(_markers.Count, Steps.Count);
        for (var i = 0; i < count; i++)
        {
            var (circle, number, check, _) = _markers[i];
            var completed = Steps[i].Completed;
            var emphasized = completed || i == ActiveIndex;

            circle.Bind(Border.BackgroundProperty, this.GetResourceObservable(
                emphasized ? LoamTokens.Primary : LoamTokens.Palette(nameof(LoamPalette.ActionDisabledBackground))));

            var foreground = emphasized ? LoamTokens.PrimaryContrastText : LoamTokens.TextSecondary;
            number.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(foreground));
            check.Bind(Icon.ForegroundProperty, this.GetResourceObservable(foreground));

            check.IsVisible = completed;
            number.IsVisible = !completed;
        }
    }

    private void ShowContent()
    {
        if (_content is not null && ActiveIndex >= 0 && ActiveIndex < Steps.Count)
        {
            _content.Content = Steps[ActiveIndex].Content;
        }
        else if (_content is not null)
        {
            _content.Content = null;
        }
    }

    private void UpdateButtons()
    {
        if (_back is not null)
        {
            _back.IsEnabled = IsEnabled && ActiveIndex > 0;
        }

        if (_next is not null)
        {
            _next.Content = ActiveIndex >= Steps.Count - 1 ? "Finish" : "Next";
            _next.IsEnabled = IsEnabled && Steps.Count > 0;
            AutomationProperties.SetName(_next, ActiveIndex >= Steps.Count - 1 ? "Finish steps" : "Next step");
        }

        AutomationProperties.SetHelpText(this, Steps.Count == 0 ? "No steps" : $"Step {ActiveIndex + 1} of {Steps.Count}");
    }
}

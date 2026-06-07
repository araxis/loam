using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Loam;

namespace Loam.Controls;

/// <summary>
/// A lightweight form container. Use <see cref="Children"/> and optional actions for the standard stacked
/// form layout, or set <see cref="Avalonia.Controls.Decorator.Child"/> for a custom layout.
/// <see cref="Validate"/> runs each <see cref="TextField"/>'s validation and updates <see cref="IsValid"/>.
/// </summary>
public class Form : Decorator
{
    private readonly StackPanel _fieldStack = new();
    private readonly StackPanel _actionStack = new();
    private readonly StackPanel _root = new();
    private readonly StackPanel _headerStack = new();
    private readonly Text _titleText = new() { Typo = Typo.TitleLarge };
    private readonly Text _subtitleText = new() { Typo = Typo.Body2, Color = LoamColor.Secondary, TextWrapping = TextWrapping.Wrap };
    private readonly Text _statusText = new() { Typo = Typo.BodySmall, Color = LoamColor.Secondary, TextWrapping = TextWrapping.Wrap };
    private Button? _submitButton;
    private Button? _resetButton;
    private bool _usingGeneratedChild;
    private bool _updatingChild;
    private bool _submitted;

    /// <summary>Identifies the <see cref="Spacing"/> property.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<Form, double>(nameof(Spacing), 16d);

    /// <summary>Identifies the <see cref="ActionSpacing"/> property.</summary>
    public static readonly StyledProperty<double> ActionSpacingProperty =
        AvaloniaProperty.Register<Form, double>(nameof(ActionSpacing), 10d);

    /// <summary>Identifies the <see cref="FieldWidth"/> property.</summary>
    public static readonly StyledProperty<double> FieldWidthProperty =
        AvaloniaProperty.Register<Form, double>(nameof(FieldWidth), 360d);

    /// <summary>Identifies the <see cref="SubmitText"/> property.</summary>
    public static readonly StyledProperty<string?> SubmitTextProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(SubmitText));

    /// <summary>Identifies the <see cref="ResetText"/> property.</summary>
    public static readonly StyledProperty<string?> ResetTextProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(ResetText));

    /// <summary>Identifies the <see cref="ActionSize"/> property.</summary>
    public static readonly StyledProperty<LoamSize> ActionSizeProperty =
        AvaloniaProperty.Register<Form, LoamSize>(nameof(ActionSize), LoamSize.Medium);

    /// <summary>Identifies the <see cref="SubmitVariant"/> property.</summary>
    public static readonly StyledProperty<Variant> SubmitVariantProperty =
        AvaloniaProperty.Register<Form, Variant>(nameof(SubmitVariant), Variant.Filled);

    /// <summary>Identifies the <see cref="SubmitColor"/> property.</summary>
    public static readonly StyledProperty<LoamColor> SubmitColorProperty =
        AvaloniaProperty.Register<Form, LoamColor>(nameof(SubmitColor), LoamColor.Primary);

    /// <summary>Identifies the <see cref="ResetVariant"/> property.</summary>
    public static readonly StyledProperty<Variant> ResetVariantProperty =
        AvaloniaProperty.Register<Form, Variant>(nameof(ResetVariant), Variant.Text);

    /// <summary>Identifies the <see cref="ResetColor"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ResetColorProperty =
        AvaloniaProperty.Register<Form, LoamColor>(nameof(ResetColor), LoamColor.Primary);

    /// <summary>Identifies the <see cref="ActionsHorizontalAlignment"/> property.</summary>
    public static readonly StyledProperty<HorizontalAlignment> ActionsHorizontalAlignmentProperty =
        AvaloniaProperty.Register<Form, HorizontalAlignment>(nameof(ActionsHorizontalAlignment), HorizontalAlignment.Left);

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(Subtitle));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="SuccessText"/> property.</summary>
    public static readonly StyledProperty<string?> SuccessTextProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(SuccessText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="SubmitIcon"/> property.</summary>
    public static readonly StyledProperty<string?> SubmitIconProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(SubmitIcon));

    /// <summary>Identifies the <see cref="ResetIcon"/> property.</summary>
    public static readonly StyledProperty<string?> ResetIconProperty =
        AvaloniaProperty.Register<Form, string?>(nameof(ResetIcon));

    /// <summary>Identifies the <see cref="IsValid"/> property.</summary>
    public static readonly StyledProperty<bool> IsValidProperty =
        AvaloniaProperty.Register<Form, bool>(nameof(IsValid), defaultValue: true);

    /// <summary>Fields and other controls rendered in the standard form body.</summary>
    public AvaloniaList<Control> Children { get; } = [];

    /// <summary>Additional controls rendered in the standard form action row before generated submit/reset actions.</summary>
    public AvaloniaList<Control> Actions { get; } = [];

    /// <summary>Vertical spacing between generated form fields.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>Horizontal spacing between generated form actions.</summary>
    public double ActionSpacing
    {
        get => GetValue(ActionSpacingProperty);
        set => SetValue(ActionSpacingProperty, value);
    }

    /// <summary>Default width applied to generated fields when the field has no explicit width.</summary>
    public double FieldWidth
    {
        get => GetValue(FieldWidthProperty);
        set => SetValue(FieldWidthProperty, value);
    }

    /// <summary>Optional text for a generated submit/validate button.</summary>
    public string? SubmitText
    {
        get => GetValue(SubmitTextProperty);
        set => SetValue(SubmitTextProperty, value);
    }

    /// <summary>Optional text for a generated reset button.</summary>
    public string? ResetText
    {
        get => GetValue(ResetTextProperty);
        set => SetValue(ResetTextProperty, value);
    }

    /// <summary>Size applied to generated submit and reset actions.</summary>
    public LoamSize ActionSize
    {
        get => GetValue(ActionSizeProperty);
        set => SetValue(ActionSizeProperty, value);
    }

    /// <summary>Variant applied to the generated submit action.</summary>
    public Variant SubmitVariant
    {
        get => GetValue(SubmitVariantProperty);
        set => SetValue(SubmitVariantProperty, value);
    }

    /// <summary>Semantic color applied to the generated submit action.</summary>
    public LoamColor SubmitColor
    {
        get => GetValue(SubmitColorProperty);
        set => SetValue(SubmitColorProperty, value);
    }

    /// <summary>Variant applied to the generated reset action.</summary>
    public Variant ResetVariant
    {
        get => GetValue(ResetVariantProperty);
        set => SetValue(ResetVariantProperty, value);
    }

    /// <summary>Semantic color applied to the generated reset action.</summary>
    public LoamColor ResetColor
    {
        get => GetValue(ResetColorProperty);
        set => SetValue(ResetColorProperty, value);
    }

    /// <summary>Horizontal alignment for the generated action row.</summary>
    public HorizontalAlignment ActionsHorizontalAlignment
    {
        get => GetValue(ActionsHorizontalAlignmentProperty);
        set => SetValue(ActionsHorizontalAlignmentProperty, value);
    }

    /// <summary>Generated form title.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Generated supporting text below <see cref="Title"/>.</summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Neutral helper text shown before submit/reset.</summary>
    public string? HelperText
    {
        get => GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Status text shown after a successful generated submit.</summary>
    public string? SuccessText
    {
        get => GetValue(SuccessTextProperty);
        set => SetValue(SuccessTextProperty, value);
    }

    /// <summary>Status text shown after a failed generated submit.</summary>
    public string? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>Leading icon for the generated submit action.</summary>
    public string? SubmitIcon
    {
        get => GetValue(SubmitIconProperty);
        set => SetValue(SubmitIconProperty, value);
    }

    /// <summary>Leading icon for the generated reset action.</summary>
    public string? ResetIcon
    {
        get => GetValue(ResetIconProperty);
        set => SetValue(ResetIconProperty, value);
    }

    /// <summary>Whether all fields passed validation at the last <see cref="Validate"/>.</summary>
    public bool IsValid
    {
        get => GetValue(IsValidProperty);
        private set => SetValue(IsValidProperty, value);
    }

    /// <summary>Runs after the generated submit button validates the form.</summary>
    public Action<Form>? SubmitAction { get; set; }

    /// <summary>Runs after the generated reset button clears field values and validation state.</summary>
    public Action<Form>? ResetAction { get; set; }

    /// <summary>Raised after the generated submit button validates the form.</summary>
    public event EventHandler? Submitted;

    /// <summary>Raised after the generated reset button clears field values and validation state.</summary>
    public event EventHandler? Reset;

    /// <summary>Initializes a new instance of the <see cref="Form"/> class.</summary>
    public Form()
    {
        Children.CollectionChanged += OnGeneratedCollectionChanged;
        Actions.CollectionChanged += OnGeneratedCollectionChanged;
    }

    /// <summary>Validates every <see cref="TextField"/> in the form and returns whether all are valid.</summary>
    public bool Validate()
    {
        var valid = true;
        foreach (var field in FormFields())
        {
            if (field.Validate() is not null)
            {
                valid = false;
            }
        }

        IsValid = valid;
        _submitted = true;
        SyncGeneratedContent();
        return valid;
    }

    /// <summary>Clears generated text fields and validation state.</summary>
    public void ResetFields()
    {
        foreach (var field in FormFields())
        {
            field.Text = null;
            field.Error = false;
            field.ErrorText = null;
        }

        IsValid = true;
        _submitted = false;
        SyncGeneratedContent();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChildProperty && !_updatingChild)
        {
            _usingGeneratedChild = false;
        }

        if (change.Property == SpacingProperty ||
            change.Property == ActionSpacingProperty ||
            change.Property == FieldWidthProperty ||
            change.Property == SubmitTextProperty ||
            change.Property == ResetTextProperty ||
            change.Property == ActionSizeProperty ||
            change.Property == SubmitVariantProperty ||
            change.Property == SubmitColorProperty ||
            change.Property == ResetVariantProperty ||
            change.Property == ResetColorProperty ||
            change.Property == ActionsHorizontalAlignmentProperty ||
            change.Property == TitleProperty ||
            change.Property == SubtitleProperty ||
            change.Property == HelperTextProperty ||
            change.Property == SuccessTextProperty ||
            change.Property == ErrorTextProperty ||
            change.Property == SubmitIconProperty ||
            change.Property == ResetIconProperty ||
            change.Property == IsEnabledProperty)
        {
            SyncGeneratedContent();
        }
    }

    private void OnGeneratedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncGeneratedContent();
    }

    private void SyncGeneratedContent()
    {
        if (!HasGeneratedContent)
        {
            return;
        }

        if (!_usingGeneratedChild)
        {
            _usingGeneratedChild = true;
            _updatingChild = true;
            try
            {
                Child = _root;
            }
            finally
            {
                _updatingChild = false;
            }
        }

        _root.Orientation = Orientation.Vertical;
        _root.Spacing = Spacing;
        SyncHeader();
        _fieldStack.Orientation = Orientation.Vertical;
        _fieldStack.Spacing = Spacing;

        _fieldStack.Children.Clear();
        foreach (var child in Children)
        {
            ApplyGeneratedFieldMetrics(child);
            _fieldStack.Children.Add(child);
        }

        var hasActions = SyncActionRow();
        _root.Children.Clear();
        if (_headerStack.Children.Count > 0)
        {
            _root.Children.Add(_headerStack);
        }

        if (_fieldStack.Children.Count > 0)
        {
            _root.Children.Add(_fieldStack);
        }

        if (UpdateStatus())
        {
            _root.Children.Add(_statusText);
        }

        if (hasActions)
        {
            _root.Children.Add(_actionStack);
        }

        ApplyGeneratedEnabledState();
    }

    private bool HasGeneratedContent =>
        Children.Count > 0 ||
        Actions.Count > 0 ||
        !string.IsNullOrWhiteSpace(SubmitText) ||
        !string.IsNullOrWhiteSpace(ResetText) ||
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Subtitle) ||
        !string.IsNullOrWhiteSpace(HelperText) ||
        !string.IsNullOrWhiteSpace(SuccessText) ||
        !string.IsNullOrWhiteSpace(ErrorText);

    private void SyncHeader()
    {
        _headerStack.Orientation = Orientation.Vertical;
        _headerStack.Spacing = 4;
        _headerStack.Children.Clear();

        if (!string.IsNullOrWhiteSpace(Title))
        {
            _titleText.Text = Title;
            _headerStack.Children.Add(_titleText);
            AutomationProperties.SetName(this, Title);
        }
        else
        {
            AutomationProperties.SetName(this, "Form");
        }

        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            _subtitleText.Text = Subtitle;
            _headerStack.Children.Add(_subtitleText);
            AutomationProperties.SetHelpText(this, Subtitle);
        }
        else
        {
            AutomationProperties.SetHelpText(this, HelperText);
        }
    }

    private bool UpdateStatus()
    {
        string? text;
        LoamColor color;
        if (_submitted && !IsValid && !string.IsNullOrWhiteSpace(ErrorText))
        {
            text = ErrorText;
            color = LoamColor.Error;
        }
        else if (_submitted && IsValid && !string.IsNullOrWhiteSpace(SuccessText))
        {
            text = SuccessText;
            color = LoamColor.Success;
        }
        else
        {
            text = HelperText;
            color = LoamColor.Secondary;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            _statusText.Text = string.Empty;
            return false;
        }

        _statusText.Text = text;
        _statusText.Color = color;
        return true;
    }

    private bool SyncActionRow()
    {
        if (Actions.Count == 0 &&
            string.IsNullOrWhiteSpace(SubmitText) &&
            string.IsNullOrWhiteSpace(ResetText))
        {
            _actionStack.Children.Clear();
            return false;
        }

        _actionStack.Orientation = Orientation.Horizontal;
        _actionStack.Spacing = ActionSpacing;
        _actionStack.HorizontalAlignment = ActionsHorizontalAlignment;
        _actionStack.Children.Clear();
        _submitButton = null;
        _resetButton = null;

        foreach (var action in Actions)
        {
            _actionStack.Children.Add(action);
        }

        if (!string.IsNullOrWhiteSpace(SubmitText))
        {
            var submit = new Button
            {
                Name = "PART_Submit",
                Content = SubmitText,
                Variant = SubmitVariant,
                Color = SubmitColor,
                Size = ActionSize,
                StartIcon = SubmitIcon,
                IsEnabled = IsEnabled,
            };
            AutomationProperties.SetName(submit, SubmitText);
            AutomationProperties.SetHelpText(submit, HelperText ?? "Submit form");
            submit.Click += (_, _) =>
            {
                if (!IsEnabled)
                {
                    return;
                }

                Validate();
                SubmitAction?.Invoke(this);
                Submitted?.Invoke(this, EventArgs.Empty);
            };
            _submitButton = submit;
            _actionStack.Children.Add(submit);
        }

        if (!string.IsNullOrWhiteSpace(ResetText))
        {
            var reset = new Button
            {
                Name = "PART_Reset",
                Content = ResetText,
                Variant = ResetVariant,
                Color = ResetColor,
                Size = ActionSize,
                StartIcon = ResetIcon,
                IsEnabled = IsEnabled,
            };
            AutomationProperties.SetName(reset, ResetText);
            AutomationProperties.SetHelpText(reset, "Reset form");
            reset.Click += (_, _) =>
            {
                if (!IsEnabled)
                {
                    return;
                }

                ResetFields();
                ResetAction?.Invoke(this);
                Reset?.Invoke(this, EventArgs.Empty);
            };
            _resetButton = reset;
            _actionStack.Children.Add(reset);
        }

        return true;
    }

    private void ApplyGeneratedEnabledState()
    {
        if (_submitButton is not null)
        {
            _submitButton.IsEnabled = IsEnabled;
        }

        if (_resetButton is not null)
        {
            _resetButton.IsEnabled = IsEnabled;
        }
    }

    private void ApplyGeneratedFieldMetrics(Control control)
    {
        if (double.IsNaN(FieldWidth) || FieldWidth <= 0 || !double.IsNaN(control.Width))
        {
            return;
        }

        control.Width = FieldWidth;
    }

    private List<TextField> FormFields()
    {
        var visualFields = this.GetVisualDescendants().OfType<TextField>().ToList();
        if (visualFields.Count > 0)
        {
            return visualFields;
        }

        return Children.SelectMany(DescendantFields).ToList();
    }

    private static IEnumerable<TextField> DescendantFields(Control control)
    {
        if (control is TextField field)
        {
            yield return field;
        }

        IEnumerable<Control> children = control switch
        {
            Panel panel => panel.Children.OfType<Control>(),
            ContentControl { Content: Control content } => [content],
            Decorator { Child: Control child } => [child],
            _ => [],
        };

        foreach (var child in children)
        {
            foreach (var descendantField in DescendantFields(child))
            {
                yield return descendantField;
            }
        }
    }
}

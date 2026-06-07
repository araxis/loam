using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace Loam.Controls;

/// <summary>
/// A <see cref="TextField"/> that formats its input through a <see cref="Mask"/> pattern, mirroring
/// the reference API's masked <c>TextField</c>/<c>Mask</c>. Reuses the <see cref="TextField"/> chrome
/// (inherited style key) and reformats <see cref="TextField.Text"/> as the user types.
/// </summary>
public class MaskedTextField : TextField
{
    /// <summary>Identifies the <see cref="Pattern"/> property.</summary>
    public static readonly StyledProperty<string?> PatternProperty =
        AvaloniaProperty.Register<MaskedTextField, string?>(nameof(Pattern));

    private TextBox? _textBox;
    private bool _masking;
    private bool _pendingEditorMask;

    /// <summary>The mask pattern (see <see cref="Mask"/>). Mirrors the reference API's <c>Mask</c>.</summary>
    public string? Pattern
    {
        get => GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_textBox is not null)
        {
            _textBox.TextChanged -= OnEditorTextChanged;
        }

        base.OnApplyTemplate(e);
        _textBox = e.NameScope.Find("PART_TextBox") as TextBox;
        if (_textBox is not null)
        {
            _textBox.TextChanged += OnEditorTextChanged;
        }

        ApplyMask(Text, updateEditor: true);
        ApplyMaskAutomation();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (!_masking && !string.IsNullOrEmpty(Pattern) &&
            (change.Property == TextProperty || change.Property == PatternProperty))
        {
            if (change.Property == TextProperty && _textBox?.IsFocused == true)
            {
                ScheduleEditorMask();
            }
            else
            {
                ApplyMask(Text, updateEditor: true);
            }
        }

        if (change.Property == PatternProperty || change.Property == LabelProperty ||
            change.Property == HelperTextProperty || change.Property == ErrorTextProperty ||
            change.Property == ErrorProperty)
        {
            ApplyMaskAutomation();
        }
    }

    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_masking && _textBox is not null)
        {
            ScheduleEditorMask();
        }
    }

    private void ScheduleEditorMask()
    {
        if (_pendingEditorMask)
        {
            return;
        }

        _pendingEditorMask = true;
        Dispatcher.UIThread.Post(() =>
        {
            _pendingEditorMask = false;
            if (!_masking && _textBox is not null)
            {
                ApplyMask(_textBox.Text, updateEditor: true);
            }
        }, DispatcherPriority.Background);
    }

    private void ApplyMask(string? raw, bool updateEditor)
    {
        if (string.IsNullOrEmpty(Pattern))
        {
            return;
        }

        var masked = Mask.Apply(raw, Pattern);
        if (masked == Text && (!updateEditor || _textBox?.Text == masked))
        {
            return;
        }

        _masking = true;
        try
        {
            Text = masked;

            if (updateEditor && _textBox is not null)
            {
                if (_textBox.Text != masked)
                {
                    _textBox.Text = masked;
                }

                MoveCaretToEnd(_textBox);
            }
        }
        finally
        {
            _masking = false;
        }
    }

    private static void MoveCaretToEnd(TextBox textBox)
    {
        var length = textBox.Text?.Length ?? 0;
        textBox.CaretIndex = length;
        textBox.SelectionStart = length;
        textBox.SelectionEnd = length;
    }

    private void ApplyMaskAutomation()
    {
        AutomationProperties.SetName(this,
            string.IsNullOrWhiteSpace(Label) ? "Masked text field" : Label);

        var patternHelp = string.IsNullOrWhiteSpace(Pattern) ? null : $"Pattern {Pattern}";
        var stateHelp = Error && !string.IsNullOrWhiteSpace(ErrorText) ? ErrorText : HelperText;
        var help = string.IsNullOrWhiteSpace(patternHelp)
            ? stateHelp
            : string.IsNullOrWhiteSpace(stateHelp)
                ? patternHelp
                : $"{patternHelp}. {stateHelp}";

        AutomationProperties.SetHelpText(this, help);
    }
}

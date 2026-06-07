using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Globalization;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A file selection control, mirroring the reference API's <c>FileUpload</c>. A button opens the platform
/// file picker (via <see cref="TopLevel"/>'s <see cref="IStorageProvider"/>); chosen files are exposed
/// as <see cref="Files"/>, their names shown as chips, and <see cref="FilesSelected"/> is raised.
/// </summary>
public class FileUpload : TemplatedControl
{
    /// <summary>Identifies the <see cref="ButtonText"/> property.</summary>
    public static readonly StyledProperty<string> ButtonTextProperty =
        AvaloniaProperty.Register<FileUpload, string>(nameof(ButtonText), "Upload files");

    /// <summary>Identifies the <see cref="AllowMultiple"/> property.</summary>
    public static readonly StyledProperty<bool> AllowMultipleProperty =
        AvaloniaProperty.Register<FileUpload, bool>(nameof(AllowMultiple), true);

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<FileUpload, Variant>(nameof(Variant), Variant.Outlined);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<FileUpload, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<FileUpload, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="AcceptedFileTypes"/> property.</summary>
    public static readonly StyledProperty<IReadOnlyList<FilePickerFileType>?> AcceptedFileTypesProperty =
        AvaloniaProperty.Register<FileUpload, IReadOnlyList<FilePickerFileType>?>(nameof(AcceptedFileTypes));

    /// <summary>Identifies the <see cref="ShowRemoveButtons"/> property.</summary>
    public static readonly StyledProperty<bool> ShowRemoveButtonsProperty =
        AvaloniaProperty.Register<FileUpload, bool>(nameof(ShowRemoveButtons));

    /// <summary>Identifies the <see cref="ShowClearButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowClearButtonProperty =
        AvaloniaProperty.Register<FileUpload, bool>(nameof(ShowClearButton));

    /// <summary>Identifies the <see cref="ClearText"/> property.</summary>
    public static readonly StyledProperty<string> ClearTextProperty =
        AvaloniaProperty.Register<FileUpload, string>(nameof(ClearText), "Clear");

    /// <summary>Identifies the <see cref="SelectedFileIcon"/> property.</summary>
    public static readonly StyledProperty<string?> SelectedFileIconProperty =
        AvaloniaProperty.Register<FileUpload, string?>(nameof(SelectedFileIcon), Icons.Material.Filled.Article);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<FileUpload, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<FileUpload, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="EmptyText"/> property.</summary>
    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<FileUpload, string?>(nameof(EmptyText), "No files selected");

    /// <summary>Identifies the <see cref="SelectedTextFormat"/> property.</summary>
    public static readonly StyledProperty<string> SelectedTextFormatProperty =
        AvaloniaProperty.Register<FileUpload, string>(nameof(SelectedTextFormat), "{0} files selected");

    /// <summary>Identifies the <see cref="ButtonIcon"/> property.</summary>
    public static readonly StyledProperty<string?> ButtonIconProperty =
        AvaloniaProperty.Register<FileUpload, string?>(nameof(ButtonIcon), Icons.Material.Filled.CloudUpload);

    private Button? _button;
    private Text? _labelText;
    private Text? _helperText;
    private Text? _statusText;
    private WrapPanel? _filesPanel;

    /// <summary>Raised with the picked files (empty if the dialog was cancelled).</summary>
    public event Action<IReadOnlyList<IStorageFile>>? FilesSelected;

    /// <summary>Raised when a selected file chip is removed.</summary>
    public event Action<string>? FileRemoved;

    /// <summary>Raised when the selected files are cleared.</summary>
    public event EventHandler? SelectionCleared;

    /// <summary>The last picked files. Mirrors the reference API's <c>Files</c>.</summary>
    public IReadOnlyList<IStorageFile> Files { get; private set; } = Array.Empty<IStorageFile>();

    /// <summary>The display names of the current selection.</summary>
    public IReadOnlyList<string> FileNames { get; private set; } = Array.Empty<string>();

    /// <summary>The picker button caption. Mirrors the reference API's button content.</summary>
    public string ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    /// <summary>Whether multiple files may be picked. Mirrors the reference API's <c>Multiple</c>.</summary>
    public bool AllowMultiple
    {
        get => GetValue(AllowMultipleProperty);
        set => SetValue(AllowMultipleProperty, value);
    }

    /// <summary>The upload button variant.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>The upload button semantic color.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>The upload button size.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Optional file type filters passed to the platform picker.</summary>
    public IReadOnlyList<FilePickerFileType>? AcceptedFileTypes
    {
        get => GetValue(AcceptedFileTypesProperty);
        set => SetValue(AcceptedFileTypesProperty, value);
    }

    /// <summary>Whether generated file chips show a remove affordance.</summary>
    public bool ShowRemoveButtons
    {
        get => GetValue(ShowRemoveButtonsProperty);
        set => SetValue(ShowRemoveButtonsProperty, value);
    }

    /// <summary>Whether a generated clear button is shown after selected file chips.</summary>
    public bool ShowClearButton
    {
        get => GetValue(ShowClearButtonProperty);
        set => SetValue(ShowClearButtonProperty, value);
    }

    /// <summary>Text for the generated clear button.</summary>
    public string ClearText
    {
        get => GetValue(ClearTextProperty);
        set => SetValue(ClearTextProperty, value);
    }

    /// <summary>Leading icon used by generated selected-file chips.</summary>
    public string? SelectedFileIcon
    {
        get => GetValue(SelectedFileIconProperty);
        set => SetValue(SelectedFileIconProperty, value);
    }

    /// <summary>Generated label shown above the picker action.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Generated helper text shown below the picker action and selected files.</summary>
    public string? HelperText
    {
        get => GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Status text shown when no files are selected. Set to <c>null</c> to hide empty status.</summary>
    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    /// <summary>Status text format used when files are selected. Receives count and joined file names.</summary>
    public string SelectedTextFormat
    {
        get => GetValue(SelectedTextFormatProperty);
        set => SetValue(SelectedTextFormatProperty, value);
    }

    /// <summary>Leading icon for the generated picker button.</summary>
    public string? ButtonIcon
    {
        get => GetValue(ButtonIconProperty);
        set => SetValue(ButtonIconProperty, value);
    }

    /// <summary>Updates the displayed selection (names → chips). Called by the picker; also usable to restore a selection.</summary>
    public void ShowSelection(IReadOnlyList<string> names)
    {
        FileNames = names ?? Array.Empty<string>();
        RenderChips();
    }

    /// <summary>Clears the current selection.</summary>
    public void Clear()
    {
        Files = Array.Empty<IStorageFile>();
        ShowSelection(Array.Empty<string>());
        SelectionCleared?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Removes one displayed file name and the matching picked file, when present.</summary>
    public void RemoveSelection(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || FileNames.Count == 0)
        {
            return;
        }

        var names = FileNames.ToList();
        if (!names.Remove(name))
        {
            return;
        }

        FileNames = names;
        Files = Files.Where(file => !string.Equals(file.Name, name, StringComparison.Ordinal)).ToList();
        RenderChips();
        FileRemoved?.Invoke(name);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ShowRemoveButtonsProperty ||
            change.Property == ShowClearButtonProperty ||
            change.Property == ClearTextProperty ||
            change.Property == SelectedFileIconProperty ||
            change.Property == LabelProperty ||
            change.Property == HelperTextProperty ||
            change.Property == EmptyTextProperty ||
            change.Property == SelectedTextFormatProperty ||
            change.Property == ButtonIconProperty ||
            change.Property == ButtonTextProperty ||
            change.Property == SizeProperty)
        {
            RenderChips();
        }
        else if (change.Property == IsEnabledProperty)
        {
            ApplyEnabledState();
        }
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(FileUpload);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_button is not null)
        {
            _button.Click -= OnButtonClick;
        }

        _button = e.NameScope.Find("PART_Button") as Button;
        _labelText = e.NameScope.Find("PART_Label") as Text;
        _helperText = e.NameScope.Find("PART_Helper") as Text;
        _statusText = e.NameScope.Find("PART_Status") as Text;
        _filesPanel = e.NameScope.Find("PART_Files") as WrapPanel;
        if (_button is not null)
        {
            _button.Click += OnButtonClick;
        }

        ApplyEnabledState();
        RenderChips();
    }

    private async void OnButtonClick(object? sender, RoutedEventArgs args)
    {
        if (!IsEnabled)
        {
            args.Handled = true;
            return;
        }

        await PickAsync();
    }

    private async Task PickAsync()
    {
        if (!IsEnabled)
        {
            return;
        }

        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage is null)
        {
            return;
        }

        var picked = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = AllowMultiple,
            Title = ButtonText,
            FileTypeFilter = AcceptedFileTypes,
        });

        if (picked.Count == 0)
        {
            return;
        }

        Files = picked;
        ShowSelection(picked.Select(f => f.Name).ToList());
        FilesSelected?.Invoke(picked);
    }

    private void RenderChips()
    {
        if (_filesPanel is null)
        {
            return;
        }

        _filesPanel.Children.Clear();
        _filesPanel.IsVisible = FileNames.Count > 0;
        RenderTextAnatomy();
        foreach (var name in FileNames)
        {
            var chip = new Chip
            {
                Text = name,
                Icon = SelectedFileIcon,
                Closeable = ShowRemoveButtons,
                Variant = Variant.Outlined,
                Color = LoamColor.Default,
                Size = Size,
                IsEnabled = IsEnabled,
                Margin = new Thickness(0, 6, 6, 0),
            };
            chip.Closed += (_, _) =>
            {
                if (IsEnabled)
                {
                    RemoveSelection(name);
                }
            };
            _filesPanel.Children.Add(chip);
        }

        if (ShowClearButton && FileNames.Count > 0)
        {
            var clear = new Button
            {
                Content = ClearText,
                StartIcon = Icons.Material.Filled.Close,
                Variant = Variant.Text,
                Color = LoamColor.Primary,
                Size = Size,
                IsEnabled = IsEnabled,
                Margin = new Thickness(0, 6, 0, 0),
            };
            AutomationProperties.SetName(clear, ClearText);
            clear.Click += (_, _) =>
            {
                if (IsEnabled)
                {
                    Clear();
                }
            };
            _filesPanel.Children.Add(clear);
        }

        ApplyEnabledState();
    }

    private void RenderTextAnatomy()
    {
        if (_labelText is not null)
        {
            _labelText.Text = Label;
            _labelText.IsVisible = !string.IsNullOrWhiteSpace(Label);
        }

        if (_helperText is not null)
        {
            _helperText.Text = HelperText;
            _helperText.IsVisible = !string.IsNullOrWhiteSpace(HelperText);
        }

        if (_statusText is not null)
        {
            var status = FileNames.Count == 0 ? EmptyText : FormatSelectedText();
            _statusText.Text = status;
            _statusText.IsVisible = !string.IsNullOrWhiteSpace(status);
        }

        InteractionAssist.SetAutomationName(this, Label, ButtonText, "File upload");
        AutomationProperties.SetHelpText(this, !string.IsNullOrWhiteSpace(HelperText)
            ? HelperText
            : FileNames.Count == 0 ? EmptyText : FormatSelectedText());
        ApplyButtonState();
    }

    private void ApplyEnabledState()
    {
        ApplyButtonState();
        if (_filesPanel is null)
        {
            return;
        }

        foreach (var child in _filesPanel.Children.OfType<Control>())
        {
            child.IsEnabled = IsEnabled;
        }
    }

    private void ApplyButtonState()
    {
        if (_button is null)
        {
            return;
        }

        _button.IsEnabled = IsEnabled;
        InteractionAssist.SetAutomationName(_button, ButtonText, Label, "Upload files");
        AutomationProperties.SetHelpText(_button, !string.IsNullOrWhiteSpace(HelperText)
            ? HelperText
            : FileNames.Count == 0 ? EmptyText : FormatSelectedText());
    }

    private string FormatSelectedText()
    {
        try
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                string.IsNullOrWhiteSpace(SelectedTextFormat) ? "{0} files selected" : SelectedTextFormat,
                FileNames.Count,
                string.Join(", ", FileNames));
        }
        catch (FormatException)
        {
            return $"{FileNames.Count} files selected";
        }
    }
}

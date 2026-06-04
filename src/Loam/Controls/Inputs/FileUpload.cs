using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Platform.Storage;

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

    private Button? _button;
    private WrapPanel? _filesPanel;

    /// <summary>Raised with the picked files (empty if the dialog was cancelled).</summary>
    public event Action<IReadOnlyList<IStorageFile>>? FilesSelected;

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
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(FileUpload);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _button = e.NameScope.Find("PART_Button") as Button;
        _filesPanel = e.NameScope.Find("PART_Files") as WrapPanel;
        if (_button is not null)
        {
            _button.Click += async (_, _) => await PickAsync();
        }

        RenderChips();
    }

    private async Task PickAsync()
    {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage is null)
        {
            return;
        }

        var picked = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = AllowMultiple,
            Title = ButtonText,
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
        foreach (var name in FileNames)
        {
            _filesPanel.Children.Add(new Chip
            {
                Text = name,
                Icon = Icons.Material.Filled.Check,
                Variant = Variant.Outlined,
                Color = LoamColor.Default,
                Margin = new Thickness(0, 6, 6, 0),
            });
        }
    }
}

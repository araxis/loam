namespace Loam.Controls;

/// <summary>The outcome of a dialog, mirroring the reference API's <c>DialogResult</c>.</summary>
public sealed class DialogResult
{
    private DialogResult(bool canceled, object? data)
    {
        Canceled = canceled;
        Data = data;
    }

    /// <summary>Whether the dialog was canceled/dismissed.</summary>
    public bool Canceled { get; }

    /// <summary>The data returned by the dialog (when not canceled).</summary>
    public object? Data { get; }

    /// <summary>A successful result carrying optional data.</summary>
    public static DialogResult Ok(object? data = null) => new(false, data);

    /// <summary>A canceled result.</summary>
    public static DialogResult Cancel() => new(true, null);

    /// <summary>The <see cref="Data"/> cast to <typeparamref name="T"/>, or default.</summary>
    public T? DataAs<T>() => Data is T value ? value : default;
}

/// <summary>Options controlling a dialog's presentation.</summary>
public sealed class DialogOptions
{
    /// <summary>Fixed dialog width (otherwise sized to content, capped).</summary>
    public double? Width { get; init; }

    /// <summary>Whether clicking the scrim cancels the dialog (default true).</summary>
    public bool DismissOnScrimClick { get; init; } = true;

    /// <summary>The default options.</summary>
    public static DialogOptions Default { get; } = new();
}

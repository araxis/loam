using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>
/// A handle passed to dialog content so it can close itself with a result, mirroring the reference API's
/// dialog instance / <c>Dialog.Close</c>.
/// </summary>
public sealed class DialogInstance
{
    private readonly TaskCompletionSource<DialogResult> _completion;
    private readonly Action _dismiss;

    internal DialogInstance(TaskCompletionSource<DialogResult> completion, Action dismiss)
    {
        _completion = completion;
        _dismiss = dismiss;
    }

    /// <summary>Closes the dialog with the given result.</summary>
    public void Close(DialogResult result)
    {
        _dismiss();
        _completion.TrySetResult(result);
    }

    /// <summary>Closes the dialog with a successful result.</summary>
    public void Ok(object? data = null) => Close(DialogResult.Ok(data));

    /// <summary>Closes the dialog as canceled.</summary>
    public void Cancel() => Close(DialogResult.Cancel());
}

/// <summary>
/// Shows modal dialogs over the current window, mirroring the reference API's <c>IDialogService</c>. Create
/// one with <see cref="DialogService.For"/> (no provider component required — it uses the window's
/// overlay layer).
/// </summary>
public interface IDialogService
{
    /// <summary>Shows a dialog whose content is built from a <see cref="DialogInstance"/>; resolves when closed.</summary>
    Task<DialogResult> ShowAsync(string? title, Func<DialogInstance, Control> content, DialogOptions? options = null);

    /// <summary>Shows a yes/no confirmation; resolves <c>true</c> when confirmed.</summary>
    Task<bool> ConfirmAsync(string title, string message, string okText = "OK", string cancelText = "Cancel");

    /// <summary>
    /// Shows a message box with up to three buttons, mirroring the reference API's <c>ShowMessageBox</c>. Resolves
    /// <c>true</c> (yes), <c>false</c> (no), or <c>null</c> (cancel/dismiss). Omit <paramref name="noText"/>/
    /// <paramref name="cancelText"/> to hide those buttons.
    /// </summary>
    Task<bool?> MessageBoxAsync(string title, string message, string yesText = "OK", string? noText = null, string? cancelText = null);
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class InputTests
{
    private static void Show(Control content)
    {
        new Window { Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static Border Part(Control control, string name)
    {
        control.ApplyTemplate();
        return control.GetVisualDescendants().OfType<Border>().First(b => b.Name == name);
    }

    private static List<Loam.Controls.Button> ToggleSegments(ToggleGroup group) =>
        group.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Where(button => button.Name == "PART_Segment")
            .ToList();

    private static Loam.Controls.Button ToggleSegment(ToggleGroup group, string text) =>
        ToggleSegments(group).Single(button => (string?)button.Content == text);

    private static Border ButtonRoot(Loam.Controls.Button button)
    {
        button.ApplyTemplate();
        return button.GetVisualDescendants().OfType<Border>().First();
    }

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    [AvaloniaFact]
    public void CheckBox_checked_fills_box_with_color()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var checkBox = new Loam.Controls.CheckBox { Color = LoamColor.Primary, IsChecked = true };
        Show(checkBox);

        var box = Part(checkBox, "PART_Box");
        ((ISolidColorBrush)box.Background!).Color.ShouldBe(Color.Parse("#6750A4"));
        checkBox.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>().First().IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void CheckBox_unchecked_is_outlined_and_transparent()
    {
        var checkBox = new Loam.Controls.CheckBox { IsChecked = false };
        Show(checkBox);

        var box = Part(checkBox, "PART_Box");
        ((ISolidColorBrush)box.Background!).Color.A.ShouldBe((byte)0);
        box.BorderThickness.ShouldBe(new Thickness(2));
    }

    [AvaloniaFact]
    public void CheckBox_indeterminate_uses_filled_box_and_dash_mark()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var checkBox = new Loam.Controls.CheckBox
        {
            Color = LoamColor.Primary,
            IsThreeState = true,
            IsChecked = null,
        };
        Show(checkBox);

        var box = Part(checkBox, "PART_Box");
        ((ISolidColorBrush)box.Background!).Color.ShouldBe(Color.Parse("#6750A4"));
        box.BorderThickness.ShouldBe(default);

        var mark = checkBox.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>().First();
        mark.IsVisible.ShouldBeTrue();
        mark.Margin.ShouldBe(new Thickness(4, 0));
    }

    [AvaloniaFact]
    public void CheckBox_state_layer_wraps_mark_and_tracks_focus()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var checkBox = new Loam.Controls.CheckBox
        {
            Content = "Remember me",
            Color = LoamColor.Primary,
            IsChecked = true,
        };
        Show(checkBox);

        var stateLayer = Part(checkBox, "PART_StateLayer");
        stateLayer.Width.ShouldBe(40);
        stateLayer.Height.ShouldBe(40);
        stateLayer.Background.ShouldBe(Brushes.Transparent);

        checkBox.Focus();
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldNotBe(Brushes.Transparent);

        checkBox.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void CheckBox_exposes_content_as_automation_name()
    {
        var checkBox = new Loam.Controls.CheckBox { Content = "Receive updates" };
        Show(checkBox);

        AutomationProperties.GetName(checkBox).ShouldBe("Receive updates");
        AutomationProperties.GetHelpText(checkBox).ShouldBe("Checkbox");

        checkBox.Content = "Receive weekly updates";
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(checkBox).ShouldBe("Receive weekly updates");
    }

    [AvaloniaFact]
    public void Switch_thumb_slides_with_checked_state()
    {
        var on = new Switch { IsChecked = true };
        Show(on);
        Part(on, "PART_Thumb").HorizontalAlignment.ShouldBe(HorizontalAlignment.Right);

        var off = new Switch { IsChecked = false };
        Show(off);
        Part(off, "PART_Thumb").HorizontalAlignment.ShouldBe(HorizontalAlignment.Left);
    }

    [AvaloniaFact]
    public void Switch_size_changes_track_and_thumb_geometry()
    {
        var extraSmall = new Switch { Size = LoamSize.ExtraSmall };
        Show(extraSmall);
        Part(extraSmall, "PART_Track").Width.ShouldBe(26d);
        Part(extraSmall, "PART_Track").Height.ShouldBe(10d);
        Part(extraSmall, "PART_Thumb").Width.ShouldBe(16d);
        Part(extraSmall, "PART_Thumb").Height.ShouldBe(16d);

        var small = new Switch { Size = LoamSize.Small };
        Show(small);
        Part(small, "PART_Track").Width.ShouldBe(30d);
        Part(small, "PART_Track").Height.ShouldBe(12d);
        Part(small, "PART_Thumb").Width.ShouldBe(18d);
        Part(small, "PART_Thumb").Height.ShouldBe(18d);

        var large = new Switch { Size = LoamSize.Large };
        Show(large);
        Part(large, "PART_Track").Width.ShouldBe(48d);
        Part(large, "PART_Track").Height.ShouldBe(18d);
        Part(large, "PART_Thumb").Width.ShouldBe(28d);
        Part(large, "PART_Thumb").Height.ShouldBe(28d);

        var extraLarge = new Switch { Size = LoamSize.ExtraLarge };
        Show(extraLarge);
        Part(extraLarge, "PART_Track").Width.ShouldBe(58d);
        Part(extraLarge, "PART_Track").Height.ShouldBe(22d);
        Part(extraLarge, "PART_Thumb").Width.ShouldBe(32d);
        Part(extraLarge, "PART_Thumb").Height.ShouldBe(32d);
    }

    [AvaloniaFact]
    public void Switch_state_layer_follows_thumb_and_tracks_focus()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var toggle = new Switch
        {
            Content = "Notifications",
            Color = LoamColor.Primary,
            IsChecked = true,
        };
        Show(toggle);

        var stateLayer = Part(toggle, "PART_StateLayer");
        stateLayer.Width.ShouldBe(40);
        stateLayer.Height.ShouldBe(40);
        stateLayer.HorizontalAlignment.ShouldBe(HorizontalAlignment.Right);
        stateLayer.Background.ShouldBe(Brushes.Transparent);

        toggle.Focus();
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldNotBe(Brushes.Transparent);

        toggle.IsChecked = false;
        Dispatcher.UIThread.RunJobs();

        stateLayer.HorizontalAlignment.ShouldBe(HorizontalAlignment.Left);

        toggle.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void Switch_exposes_content_and_state_as_automation_text()
    {
        var toggle = new Switch { Content = "Notifications", IsChecked = true };
        Show(toggle);

        AutomationProperties.GetName(toggle).ShouldBe("Notifications");
        AutomationProperties.GetHelpText(toggle).ShouldBe("Switch on");

        toggle.IsChecked = false;
        toggle.Content = "Marketing";
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(toggle).ShouldBe("Marketing");
        AutomationProperties.GetHelpText(toggle).ShouldBe("Switch off");
    }

    [AvaloniaFact]
    public void TextField_binds_text_two_way()
    {
        var field = new TextField();
        Show(field);
        field.ApplyTemplate();
        var box = field.GetVisualDescendants().OfType<TextBox>().First();

        field.Text = "hello";
        Dispatcher.UIThread.RunJobs();
        box.Text.ShouldBe("hello");

        box.Text = "world";
        Dispatcher.UIThread.RunJobs();
        field.Text.ShouldBe("world");
    }

    [AvaloniaFact]
    public void TextField_outlined_resting_label_sits_inside_when_empty()
    {
        var field = new TextField { Label = "Name", Variant = Variant.Outlined };
        Show(field);
        field.ApplyTemplate();

        Part(field, "PART_InputBorder").BorderThickness.ShouldBe(new Thickness(1));
        Part(field, "PART_InputBorder").Margin.ShouldBe(default);
        Part(field, "PART_LabelHost").IsVisible.ShouldBeFalse();
        var label = field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Label");
        label.Text.ShouldBe("Name");
        label.IsVisible.ShouldBeFalse();

        var restingLabel = field.GetVisualDescendants().OfType<Loam.Controls.Text>()
            .First(t => t.Name == "PART_RestingLabel");
        restingLabel.Text.ShouldBe("Name");
        restingLabel.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void TextField_outlined_floats_label_when_focused_or_filled()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var field = new TextField { Label = "Name", Variant = Variant.Outlined };
        Show(field);
        field.ApplyTemplate();
        var box = field.GetVisualDescendants().OfType<TextBox>().First();

        box.Focus();
        Dispatcher.UIThread.RunJobs();

        var inputBorder = Part(field, "PART_InputBorder");
        inputBorder.Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
        inputBorder.MinHeight.ShouldBe(56);
        var labelHost = Part(field, "PART_LabelHost");
        labelHost.IsVisible.ShouldBeTrue();
        labelHost.Margin.Top.ShouldBe(0);
        ((ISolidColorBrush)labelHost.Background!).Color.ShouldBe(Color.Parse("#F3EDF7"));
        var label = field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Label");
        label.Text.ShouldBe("Name");
        label.IsVisible.ShouldBeTrue();
        field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_RestingLabel")
            .IsVisible.ShouldBeFalse();

        box.Focusable.ShouldBeTrue();

        var filled = new TextField { Label = "Email", Text = "a@b.com", Variant = Variant.Outlined };
        Show(filled);
        filled.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Part(filled, "PART_LabelHost").IsVisible.ShouldBeTrue();
        filled.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_RestingLabel")
            .IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Filled_and_text_field_labels_do_not_draw_notch_background()
    {
        var filled = new TextField { Label = "Price", Text = "9.50", Variant = Variant.Filled };
        Show(filled);
        filled.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var filledLabelHost = Part(filled, "PART_LabelHost");
        filledLabelHost.IsVisible.ShouldBeTrue();
        filledLabelHost.Margin.Top.ShouldBe(0);
        ((ISolidColorBrush)filledLabelHost.Background!).Color.A.ShouldBe((byte)0);
        filledLabelHost.Padding.ShouldBe(default);

        var text = new TextField { Label = "Search", Text = "audit", Variant = Variant.Text };
        Show(text);
        text.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var textLabelHost = Part(text, "PART_LabelHost");
        textLabelHost.IsVisible.ShouldBeTrue();
        textLabelHost.Margin.Top.ShouldBe(0);
        ((ISolidColorBrush)textLabelHost.Background!).Color.A.ShouldBe((byte)0);
        textLabelHost.Padding.ShouldBe(default);
    }

    [AvaloniaFact]
    public void NumericField_filled_label_does_not_draw_notch_background()
    {
        var field = new NumericField { Label = "Price", Variant = Variant.Filled, Value = 9.5 };
        Show(field);
        field.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var labelHost = Part(field, "PART_LabelHost");
        labelHost.IsVisible.ShouldBeTrue();
        ((ISolidColorBrush)labelHost.Background!).Color.A.ShouldBe((byte)0);
        labelHost.Padding.ShouldBe(default);
    }

    [AvaloniaFact]
    public void TextField_shrink_label_keeps_label_notched_when_empty()
    {
        var field = new TextField { Label = "Name", ShrinkLabel = true };
        Show(field);
        field.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Part(field, "PART_LabelHost").IsVisible.ShouldBeTrue();
        field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Label")
            .IsVisible.ShouldBeTrue();
        field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_RestingLabel")
            .IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void TextField_supports_adornments_and_floating_label()
    {
        var start = new TextBlock { Text = "$" };
        var end = new TextBlock { Text = "USD" };
        var field = new TextField
        {
            Label = "Amount",
            FloatingLabel = true,
            StartAdornment = start,
            EndAdornment = end,
        };
        Show(field);
        field.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var label = field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Label");
        label.IsVisible.ShouldBeFalse();

        var startPresenter = field.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_StartAdornment");
        startPresenter.IsVisible.ShouldBeTrue();
        startPresenter.Content.ShouldBeSameAs(start);

        var endPresenter = field.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_EndAdornment");
        endPresenter.IsVisible.ShouldBeTrue();
        endPresenter.Content.ShouldBeSameAs(end);

        field.Text = "20";
        Dispatcher.UIThread.RunJobs();
        label.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void TextField_inner_text_box_is_borderless_and_transparent()
    {
        var field = new TextField { Label = "Phone", Text = "(555) 123-4567" };
        Show(field);
        field.ApplyTemplate();

        var box = field.GetVisualDescendants().OfType<TextBox>().First();
        box.BorderThickness.ShouldBe(default);
        box.FocusAdorner.ShouldBeNull();
        ((ISolidColorBrush)box.BorderBrush!).Color.A.ShouldBe((byte)0);
        ((ISolidColorBrush)box.Background!).Color.A.ShouldBe((byte)0);
        box.Padding.ShouldBe(default);
        var innerBorder = box.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_BorderElement");
        innerBorder.BorderThickness.ShouldBe(default);
        ((ISolidColorBrush)innerBorder.BorderBrush!).Color.A.ShouldBe((byte)0);

        box.Focus();
        Dispatcher.UIThread.RunJobs();

        box.BorderThickness.ShouldBe(default);
        box.FocusAdorner.ShouldBeNull();
        innerBorder.BorderThickness.ShouldBe(default);
    }

    [AvaloniaFact]
    public void FieldEditor_make_chromeless_resets_plain_textbox_chrome()
    {
        var raw = new TextBox { Text = "555" };
        var textBox = FieldEditor.MakeChromeless(raw);
        textBox.ShouldBeSameAs(raw);
        Show(textBox);
        textBox.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        textBox.BorderThickness.ShouldBe(default);
        textBox.FocusAdorner.ShouldBeNull();
        ((ISolidColorBrush)textBox.BorderBrush!).Color.A.ShouldBe((byte)0);
        ((ISolidColorBrush)textBox.Background!).Color.A.ShouldBe((byte)0);
        textBox.Padding.ShouldBe(default);

        var innerBorder = textBox.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_BorderElement");
        innerBorder.BorderThickness.ShouldBe(default);
        ((ISolidColorBrush)innerBorder.BorderBrush!).Color.A.ShouldBe((byte)0);
    }

    [AvaloniaFact]
    public void TextField_inner_text_box_stays_transparent_on_hover_and_focus()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var field = new TextField { Label = "Project", Text = "Component audit", Width = 360 };
        var window = new Window { Width = 500, Height = 220, Content = field };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        field.ApplyTemplate();

        var box = field.GetVisualDescendants().OfType<TextBox>().First();
        var innerBorder = box.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_BorderElement");
        var point = box.TranslatePoint(new Point(box.Bounds.Width / 2, box.Bounds.Height / 2), window);
        point.ShouldNotBeNull();

        window.MouseMove(point.Value);
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        ((ISolidColorBrush)box.Background!).Color.A.ShouldBe((byte)0);
        ((ISolidColorBrush)innerBorder.Background!).Color.A.ShouldBe((byte)0);

        box.Focus();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        ((ISolidColorBrush)box.Background!).Color.A.ShouldBe((byte)0);
        ((ISolidColorBrush)innerBorder.Background!).Color.A.ShouldBe((byte)0);
    }

    [AvaloniaFact]
    public void TextField_error_colors_border_and_shows_error_text()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var field = new TextField { Error = true, ErrorText = "Required", HelperText = "Hint" };
        Show(field);
        field.ApplyTemplate();

        ((ISolidColorBrush)Part(field, "PART_InputBorder").BorderBrush!).Color.ShouldBe(Color.Parse("#B3261E"));
        field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Required");
    }

    [Fact]
    public void Slider_fraction_clamps_to_unit_range()
    {
        Loam.Controls.Slider.Fraction(50, 0, 100).ShouldBe(0.5);
        Loam.Controls.Slider.Fraction(-10, 0, 100).ShouldBe(0);
        Loam.Controls.Slider.Fraction(200, 0, 100).ShouldBe(1);
    }

    [AvaloniaFact]
    public void Slider_is_focusable_named_and_keyboard_adjustable()
    {
        var slider = new Loam.Controls.Slider { Minimum = 0, Maximum = 10, Value = 5 };
        Show(slider);
        slider.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        slider.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(slider).ShouldBe("Slider");
        AutomationProperties.GetHelpText(slider).ShouldBe("Value 5 from 0 to 10");
        var area = slider.GetVisualDescendants().OfType<Panel>().First(p => p.Name == "PART_Area");
        area.Focusable.ShouldBeTrue();
        area.MinHeight.ShouldBeGreaterThanOrEqualTo(40);

        var right = KeyArgs(Key.Right);
        slider.RaiseEvent(right);
        right.Handled.ShouldBeTrue();
        slider.Value.ShouldBe(6);

        var home = KeyArgs(Key.Home);
        slider.RaiseEvent(home);
        home.Handled.ShouldBeTrue();
        slider.Value.ShouldBe(0);

        var end = KeyArgs(Key.End);
        slider.RaiseEvent(end);
        end.Handled.ShouldBeTrue();
        slider.Value.ShouldBe(10);
        AutomationProperties.GetHelpText(slider).ShouldBe("Value 10 from 0 to 10");
    }

    [AvaloniaFact]
    public void Slider_state_layer_follows_thumb_and_tracks_focus()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var slider = new Loam.Controls.Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Width = 200,
            Color = LoamColor.Primary,
        };
        Show(slider);
        slider.ApplyTemplate();
        slider.Measure(new Size(200, 48));
        slider.Arrange(new Rect(0, 0, 200, 48));
        Dispatcher.UIThread.RunJobs();

        var stateLayer = Part(slider, "PART_StateLayer");
        stateLayer.Width.ShouldBe(40);
        stateLayer.Height.ShouldBe(40);
        stateLayer.Background.ShouldBe(Brushes.Transparent);

        slider.Focus();
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldNotBe(Brushes.Transparent);

        slider.Value = 100;
        Dispatcher.UIThread.RunJobs();

        stateLayer.Margin.Left.ShouldBeGreaterThan(0);

        slider.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void Slider_disabled_suppresses_keyboard_adjustment()
    {
        var slider = new Loam.Controls.Slider
        {
            Minimum = 0,
            Maximum = 10,
            Value = 5,
            IsEnabled = false,
        };
        Show(slider);

        var right = KeyArgs(Key.Right);
        slider.RaiseEvent(right);

        right.Handled.ShouldBeFalse();
        slider.Value.ShouldBe(5);
    }

    [Fact]
    public void TextField_required_and_custom_validation()
    {
        var required = new TextField { Required = true };
        required.Validate().ShouldBe("Required");
        required.Error.ShouldBeTrue();
        required.Text = "x";
        required.Validate().ShouldBeNull();
        required.Error.ShouldBeFalse();

        var custom = new TextField { Validation = v => v == "ok" ? null : "bad" };
        custom.Text = "no";
        custom.Validate().ShouldBe("bad");
        custom.Text = "ok";
        custom.Validate().ShouldBeNull();
    }

    [AvaloniaFact]
    public void Field_renders_custom_content_label_helper_and_adornments()
    {
        var start = new TextBlock { Text = "$" };
        var end = new TextBlock { Text = "USD" };
        var content = new TextBlock { Text = "Custom amount editor" };
        var field = new Field
        {
            Label = "Amount",
            HelperText = "Before tax",
            StartAdornment = start,
            EndAdornment = end,
            Content = content,
        };
        Show(field);
        field.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var inputBorder = Part(field, "PART_InputBorder");
        inputBorder.BorderThickness.ShouldBe(new Thickness(1));
        inputBorder.Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
        var labelHost = Part(field, "PART_LabelHost");
        labelHost.IsVisible.ShouldBeTrue();
        labelHost.Margin.Top.ShouldBe(0);
        var label = field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Label");
        label.Text.ShouldBe("Amount");
        label.IsVisible.ShouldBeTrue();
        field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_HelperText").Text
            .ShouldBe("Before tax");

        var startPresenter = field.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_StartAdornment");
        startPresenter.IsVisible.ShouldBeTrue();
        startPresenter.Content.ShouldBeSameAs(start);

        var endPresenter = field.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_EndAdornment");
        endPresenter.IsVisible.ShouldBeTrue();
        endPresenter.Content.ShouldBeSameAs(end);

        field.GetVisualDescendants().OfType<TextBlock>().ShouldContain(content);
        AutomationProperties.GetName(field).ShouldBe("Amount");
    }

    [AvaloniaFact]
    public void Field_error_state_and_inner_padding_are_applied()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var field = new Field
        {
            Label = "Custom",
            Content = new TextBlock { Text = "Body" },
            Error = true,
            ErrorText = "Required",
            HelperText = "Hint",
            InnerPadding = false,
        };
        Show(field);
        field.ApplyTemplate();

        var border = Part(field, "PART_InputBorder");
        border.Padding.ShouldBe(default);
        ((ISolidColorBrush)border.BorderBrush!).Color.ShouldBe(Color.Parse("#B3261E"));
        field.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_HelperText").Text
            .ShouldBe("Required");
    }

    [AvaloniaFact]
    public void Form_validate_aggregates_field_results()
    {
        var required = new TextField { Required = true };
        var form = new Form { Child = new StackPanel { Children = { required } } };
        Show(form);

        form.Validate().ShouldBeFalse();
        form.IsValid.ShouldBeFalse();

        required.Text = "filled";
        form.Validate().ShouldBeTrue();
        form.IsValid.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Form_generated_layout_validates_and_resets_actions()
    {
        var name = new TextField { Required = true };
        var email = new TextField
        {
            Required = true,
            Validation = value => value?.Contains('@', StringComparison.Ordinal) == true ? null : "Enter an email",
        };
        var cancel = new Loam.Controls.Button { Content = "Cancel" };
        var submittedValid = false;
        var reset = false;
        var form = new Form
        {
            Title = "Project access",
            Subtitle = "Validate required fields before inviting a collaborator.",
            HelperText = "Fill the required fields and validate.",
            SuccessText = "Ready to submit.",
            ErrorText = "Review the highlighted fields.",
            FieldWidth = 320,
            SubmitText = "Validate",
            ResetText = "Reset",
            SubmitIcon = Icons.Material.Filled.Check,
            ResetIcon = Icons.Material.Filled.Close,
            ActionSize = LoamSize.Small,
            ResetVariant = Variant.Outlined,
            ResetColor = LoamColor.Secondary,
            ActionsHorizontalAlignment = HorizontalAlignment.Right,
            Children = { name, email },
            Actions = { cancel },
        };
        form.SubmitAction = submittedForm => submittedValid = submittedForm.IsValid;
        form.ResetAction = _ => reset = true;
        Show(form);
        Dispatcher.UIThread.RunJobs();

        form.Child.ShouldBeOfType<StackPanel>();
        name.Width.ShouldBe(320);
        email.Width.ShouldBe(320);
        AutomationProperties.GetName(form).ShouldBe("Project access");
        AutomationProperties.GetHelpText(form).ShouldBe("Validate required fields before inviting a collaborator.");
        form.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Project access").ShouldBeTrue();
        form.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Fill the required fields and validate.").ShouldBeTrue();

        var buttons = form.GetVisualDescendants().OfType<Loam.Controls.Button>().ToList();
        buttons.Select(button => button.Content?.ToString()).ShouldContain("Cancel");
        var validate = buttons.First(button => button.Content?.ToString() == "Validate");
        var resetButton = buttons.First(button => button.Content?.ToString() == "Reset");
        form.ActionsHorizontalAlignment.ShouldBe(HorizontalAlignment.Right);
        validate.Size.ShouldBe(LoamSize.Small);
        validate.Variant.ShouldBe(Variant.Filled);
        validate.Color.ShouldBe(LoamColor.Primary);
        validate.StartIcon.ShouldBe(Icons.Material.Filled.Check);
        resetButton.Size.ShouldBe(LoamSize.Small);
        resetButton.Variant.ShouldBe(Variant.Outlined);
        resetButton.Color.ShouldBe(LoamColor.Secondary);
        resetButton.StartIcon.ShouldBe(Icons.Material.Filled.Close);

        validate.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        form.IsValid.ShouldBeFalse();
        submittedValid.ShouldBeFalse();
        form.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Review the highlighted fields.").ShouldBeTrue();

        name.Text = "Ada";
        email.Text = "ada@example.com";
        validate.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        form.IsValid.ShouldBeTrue();
        submittedValid.ShouldBeTrue();
        form.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Ready to submit.").ShouldBeTrue();

        resetButton.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        reset.ShouldBeTrue();
        name.Text.ShouldBeNull();
        email.Text.ShouldBeNull();
        name.Error.ShouldBeFalse();
        email.Error.ShouldBeFalse();
        form.IsValid.ShouldBeTrue();
        form.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Fill the required fields and validate.").ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Form_disabled_suppresses_generated_submit_and_reset_actions()
    {
        var name = new TextField { Required = true, Text = "Ada" };
        var submitted = false;
        var reset = false;
        var form = new Form
        {
            IsEnabled = false,
            SubmitText = "Validate",
            ResetText = "Reset",
            Children = { name },
        };
        form.Submitted += (_, _) => submitted = true;
        form.Reset += (_, _) => reset = true;
        Show(form);
        Dispatcher.UIThread.RunJobs();

        var submit = form.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => button.Name == "PART_Submit");
        var resetButton = form.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => button.Name == "PART_Reset");
        submit.IsEnabled.ShouldBeFalse();
        resetButton.IsEnabled.ShouldBeFalse();

        submit.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        resetButton.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        submitted.ShouldBeFalse();
        reset.ShouldBeFalse();
        name.Text.ShouldBe("Ada");
        form.IsValid.ShouldBeTrue();

        form.IsEnabled = true;
        Dispatcher.UIThread.RunJobs();

        submit = form.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => button.Name == "PART_Submit");
        submit.IsEnabled.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void RadioGroup_selects_radio_by_value()
    {
        var a = new Radio { Value = "a", Content = "A" };
        var b = new Radio { Value = "b", Content = "B" };
        var group = new RadioGroup { Child = new StackPanel { Children = { a, b } } };
        Show(group);

        group.Value = "b";
        Dispatcher.UIThread.RunJobs();

        b.IsChecked.ShouldBe(true);
        a.IsChecked.ShouldBe(false);
    }

    [AvaloniaFact]
    public void Radio_size_changes_ring_and_dot_geometry()
    {
        var extraSmall = new Radio { Size = LoamSize.ExtraSmall, IsChecked = true };
        Show(extraSmall);
        Part(extraSmall, "PART_Ring").Width.ShouldBe(16d);
        Part(extraSmall, "PART_Ring").Height.ShouldBe(16d);
        Part(extraSmall, "PART_Dot").Width.ShouldBe(8d);
        Part(extraSmall, "PART_Dot").Height.ShouldBe(8d);

        var large = new Radio { Size = LoamSize.Large, IsChecked = true };
        Show(large);
        Part(large, "PART_Ring").Width.ShouldBe(24d);
        Part(large, "PART_Ring").Height.ShouldBe(24d);
        Part(large, "PART_Dot").Width.ShouldBe(12d);
        Part(large, "PART_Dot").Height.ShouldBe(12d);

        var extraLarge = new Radio { Size = LoamSize.ExtraLarge, IsChecked = true };
        Show(extraLarge);
        Part(extraLarge, "PART_Ring").Width.ShouldBe(28d);
        Part(extraLarge, "PART_Ring").Height.ShouldBe(28d);
        Part(extraLarge, "PART_Dot").Width.ShouldBe(14d);
        Part(extraLarge, "PART_Dot").Height.ShouldBe(14d);
    }

    [AvaloniaFact]
    public void Radio_state_layer_wraps_mark_and_tracks_focus()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var radio = new Radio
        {
            Content = "Express",
            Color = LoamColor.Primary,
            IsChecked = true,
        };
        Show(radio);

        var stateLayer = Part(radio, "PART_StateLayer");
        stateLayer.Width.ShouldBe(40);
        stateLayer.Height.ShouldBe(40);
        stateLayer.Background.ShouldBe(Brushes.Transparent);

        radio.Focus();
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldNotBe(Brushes.Transparent);

        radio.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        stateLayer.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void Radio_exposes_content_and_state_as_automation_text()
    {
        var radio = new Radio { Content = "Express", IsChecked = true };
        Show(radio);

        AutomationProperties.GetName(radio).ShouldBe("Express");
        AutomationProperties.GetHelpText(radio).ShouldBe("Radio selected");

        radio.IsChecked = false;
        radio.Content = "Standard";
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(radio).ShouldBe("Standard");
        AutomationProperties.GetHelpText(radio).ShouldBe("Radio unselected");
    }

    [AvaloniaFact]
    public void RadioGroup_updates_value_from_checked_radio_and_refreshes_child_content()
    {
        var a = new Radio { Value = "a", Content = "A" };
        var b = new Radio { Value = "b", Content = "B" };
        var group = new RadioGroup { Child = new StackPanel { Children = { a, b } } };
        Show(group);

        b.IsChecked = true;
        Dispatcher.UIThread.RunJobs();

        group.Value.ShouldBe("b");
        group.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(group).ShouldBe("Radio group");
        AutomationProperties.GetHelpText(group).ShouldBe("Single-choice group selected b");

        var c = new Radio { Value = "c", Content = "C" };
        group.Child = new StackPanel { Children = { c } };
        group.Value = "c";
        Dispatcher.UIThread.RunJobs();

        (c.IsChecked == true).ShouldBeTrue();
        AutomationProperties.GetHelpText(group).ShouldBe("Single-choice group selected c");
    }

    [AvaloniaFact]
    public void Select_display_shows_placeholder_then_selected_text()
    {
        var select = new Select { Placeholder = "Pick one" };
        select.Items.Add(new SelectItem("One", 1));
        select.Items.Add(new SelectItem("Two", 2));
        Show(select);
        select.ApplyTemplate();

        var display = select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Display");
        display.Text.ShouldBe("Pick one");

        select.Value = 2;
        Dispatcher.UIThread.RunJobs();
        display.Text.ShouldBe("Two");
    }

    [AvaloniaFact]
    public void Select_label_rests_inside_until_active_or_selected()
    {
        var select = new Select { Label = "Priority", Placeholder = "Pick one" };
        select.Items.Add(new SelectItem("High", "high"));
        Show(select);
        select.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Part(select, "PART_LabelHost").IsVisible.ShouldBeFalse();
        select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_RestingLabel")
            .IsVisible.ShouldBeTrue();
        select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Display")
            .IsVisible.ShouldBeFalse();

        select.Value = "high";
        Dispatcher.UIThread.RunJobs();

        Part(select, "PART_LabelHost").IsVisible.ShouldBeTrue();
        select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_RestingLabel")
            .IsVisible.ShouldBeFalse();
        select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Display")
            .IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Select_error_state_uses_error_text_and_active_border()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var select = new Select
        {
            Label = "Priority",
            Error = true,
            ErrorText = "Choose one",
            HelperText = "Shown when valid",
        };
        Show(select);
        select.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var border = Part(select, "PART_Box");
        border.BorderThickness.ShouldBe(new Thickness(2));
        ((ISolidColorBrush)border.BorderBrush!).Color.ShouldBe(Color.Parse("#B3261E"));
        select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_HelperText").Text
            .ShouldBe("Choose one");
    }

    [AvaloniaFact]
    public void Select_multiselect_display_uses_selected_values()
    {
        var select = new Select { MultiSelect = true, Placeholder = "Pick many" };
        select.Items.Add(new SelectItem("One", 1));
        select.Items.Add(new SelectItem("Two", 2));
        select.Items.Add(new SelectItem("Three", 3));
        select.SelectedValues.Add(1);
        select.SelectedValues.Add(3);
        Show(select);
        select.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var display = select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Display");
        display.Text.ShouldBe("One, Three");
    }

    [AvaloniaFact]
    public void Select_display_formatter_overrides_item_text()
    {
        var select = new Select { DisplayTextFunc = item => $"#{item.Value}" };
        select.Items.Add(new SelectItem("One", 1));
        select.Items.Add(new SelectItem("Two", 2));
        select.Value = 2;
        Show(select);
        select.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var display = select.GetVisualDescendants().OfType<Loam.Controls.Text>().First(t => t.Name == "PART_Display");
        display.Text.ShouldBe("#2");
    }

    [AvaloniaFact]
    public void Select_is_focusable_named_and_keyboard_openable()
    {
        var select = new Select { Label = "Fruit", Placeholder = "Pick one" };
        Show(select);
        select.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        select.Focusable.ShouldBeTrue();
        var box = Part(select, "PART_Box");
        box.Focusable.ShouldBeTrue();
        ((ISolidColorBrush)box.Background!).Color.A.ShouldBeLessThanOrEqualTo((byte)1);
        AutomationProperties.GetName(select).ShouldBe("Fruit");

        var popup = select.GetVisualDescendants().OfType<Popup>().First(p => p.Name == "PART_Popup");
        var open = KeyArgs(Key.Enter);
        select.RaiseEvent(open);
        open.Handled.ShouldBeTrue();
        popup.IsOpen.ShouldBeTrue();
        popup.Child.ShouldBeOfType<Paper>();
        popup.Child.ShouldBeOfType<Paper>().Padding.ShouldBe(new Thickness(0));
        box.BorderThickness.ShouldBe(new Thickness(2));

        var close = KeyArgs(Key.Escape);
        select.RaiseEvent(close);
        close.Handled.ShouldBeTrue();
        popup.IsOpen.ShouldBeFalse();
        box.BorderThickness.ShouldBe(new Thickness(1));
    }

    [AvaloniaFact]
    public void Select_opens_from_empty_box_surface()
    {
        var select = new Select { Label = "Priority", Width = 360, Value = "high" };
        select.Items.Add(new SelectItem("Normal", "normal"));
        select.Items.Add(new SelectItem("High", "high"));
        select.Items.Add(new SelectItem("Urgent", "urgent"));
        var window = new Window { Width = 500, Height = 220, Content = select };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        select.ApplyTemplate();

        var box = Part(select, "PART_Box");
        var popup = select.GetVisualDescendants().OfType<Popup>().First(p => p.Name == "PART_Popup");
        var point = box.TranslatePoint(new Point(box.Bounds.Width / 2, box.Bounds.Height / 2), window);
        point.ShouldNotBeNull();

        window.MouseDown(point.Value, MouseButton.Left);
        window.MouseUp(point.Value, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        popup.IsOpen.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Select_item_activation_updates_value_and_closes_popup()
    {
        var select = new Select { Label = "Priority", Width = 360 };
        select.Items.Add(new SelectItem("Normal", "normal"));
        select.Items.Add(new SelectItem("High", "high"));
        select.Items.Add(new SelectItem("Urgent", "urgent"));
        Show(select);
        select.ApplyTemplate();

        var popup = select.GetVisualDescendants().OfType<Popup>().First(p => p.Name == "PART_Popup");
        select.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        popup.IsOpen.ShouldBeTrue();
        var paper = popup.Child.ShouldBeOfType<Paper>();
        var list = paper.Content.ShouldBeOfType<StackPanel>();
        var high = list.Children.OfType<ListItem>().ElementAt(1);
        high.RaiseEvent(new RoutedEventArgs(ListItem.ActivatedEvent));
        Dispatcher.UIThread.RunJobs();

        select.Value.ShouldBe("high");
        popup.IsOpen.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Select_popup_marks_selected_item_with_check_icon()
    {
        var select = new Select { Label = "Priority", Width = 360, Value = "high" };
        select.Items.Add(new SelectItem("Normal", "normal"));
        select.Items.Add(new SelectItem("High", "high"));
        select.Items.Add(new SelectItem("Urgent", "urgent"));
        Show(select);
        select.ApplyTemplate();

        var popup = select.GetVisualDescendants().OfType<Popup>().First(p => p.Name == "PART_Popup");
        select.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        var paper = popup.Child.ShouldBeOfType<Paper>();
        var list = paper.Content.ShouldBeOfType<StackPanel>();
        var high = list.Children.OfType<ListItem>().ElementAt(1);

        high.Content.ShouldBe("High");
        high.Icon.ShouldBe(Icons.Material.Filled.Check);
        high.IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void NumericField_clamp_bounds_value()
    {
        NumericField.Clamp(5, 0, 10).ShouldBe(5);
        NumericField.Clamp(-3, 0, 10).ShouldBe(0);
        NumericField.Clamp(99, 0, 10).ShouldBe(10);
    }

    [AvaloniaFact]
    public void NumericField_value_clamps_and_text_reflects()
    {
        var field = new NumericField { Minimum = 0, Maximum = 10, Value = 3 };
        Show(field);
        field.ApplyTemplate();
        var box = field.GetVisualDescendants().OfType<TextBox>().First();
        box.Text.ShouldBe("3");

        field.Value = 50;
        Dispatcher.UIThread.RunJobs();
        field.Value.ShouldBe(10);
        box.Text.ShouldBe("10");
    }

    [AvaloniaFact]
    public void NumericField_parses_text_into_value()
    {
        var field = new NumericField { Minimum = 0, Maximum = 100 };
        Show(field);
        field.ApplyTemplate();
        var box = field.GetVisualDescendants().OfType<TextBox>().First();

        box.Text = "42";
        Dispatcher.UIThread.RunJobs();
        field.Value.ShouldBe(42);
    }

    [AvaloniaFact]
    public void NumericField_keyboard_spinners_and_automation_are_accessible()
    {
        var field = new NumericField { Label = "Quantity", Minimum = 0, Maximum = 10, Step = 0.5, Value = 3 };
        Show(field);
        field.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(field).ShouldBe("Quantity");
        AutomationProperties.GetHelpText(field).ShouldBe("Value 3");

        var up = Part(field, "PART_Up");
        var down = Part(field, "PART_Down");
        AutomationProperties.GetName(up).ShouldBe("Increase value");
        AutomationProperties.GetName(down).ShouldBe("Decrease value");

        var box = field.GetVisualDescendants().OfType<TextBox>().First();
        var increment = KeyArgs(Key.Up);
        box.RaiseEvent(increment);
        increment.Handled.ShouldBeTrue();
        field.Value.ShouldBe(3.5);

        var decrement = KeyArgs(Key.Down);
        box.RaiseEvent(decrement);
        decrement.Handled.ShouldBeTrue();
        field.Value.ShouldBe(3);

        field.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        up.IsEnabled.ShouldBeFalse();
        down.IsEnabled.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Rating_fills_stars_up_to_selected_value()
    {
        var rating = new Rating { MaxValue = 5, SelectedValue = 3 };
        Show(rating);
        rating.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var stars = rating.GetVisualDescendants().OfType<Icon>().ToList();
        stars.Count.ShouldBe(5);
        stars[2].Color.ShouldBe(LoamColor.Warning);
        stars[3].Color.ShouldBe(LoamColor.Default);

        rating.SelectedValue = 1;
        Dispatcher.UIThread.RunJobs();
        stars[0].Color.ShouldBe(LoamColor.Warning);
        stars[1].Color.ShouldBe(LoamColor.Default);
    }

    [AvaloniaFact]
    public void Rating_is_focusable_named_and_keyboard_adjustable()
    {
        var rating = new Rating { MaxValue = 5, SelectedValue = 2 };
        Show(rating);
        rating.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        rating.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(rating).ShouldBe("Rating");
        AutomationProperties.GetHelpText(rating).ShouldBe("Rating 2 of 5");
        rating.GetVisualDescendants().OfType<StackPanel>().First(p => p.Name == "PART_Stars")
            .Focusable.ShouldBeTrue();

        var right = KeyArgs(Key.Right);
        rating.RaiseEvent(right);
        right.Handled.ShouldBeTrue();
        rating.SelectedValue.ShouldBe(3);

        var left = KeyArgs(Key.Left);
        rating.RaiseEvent(left);
        left.Handled.ShouldBeTrue();
        rating.SelectedValue.ShouldBe(2);

        rating.RaiseEvent(KeyArgs(Key.Home));
        rating.SelectedValue.ShouldBe(0);

        rating.RaiseEvent(KeyArgs(Key.End));
        rating.SelectedValue.ShouldBe(5);
        AutomationProperties.GetHelpText(rating).ShouldBe("Rating 5 of 5");

        rating.ReadOnly = true;
        rating.RaiseEvent(KeyArgs(Key.Left));
        rating.SelectedValue.ShouldBe(5);
    }

    [AvaloniaFact]
    public void Rating_state_layers_wrap_stars_and_track_focus()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var rating = new Rating
        {
            MaxValue = 5,
            SelectedValue = 3,
            Color = LoamColor.Warning,
        };
        Show(rating);
        rating.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var stateLayers = rating.GetVisualDescendants().OfType<Border>()
            .Where(border => border.Name == "PART_StateLayer")
            .ToList();
        stateLayers.Count.ShouldBe(5);
        stateLayers[0].Width.ShouldBe(40);
        stateLayers[0].Height.ShouldBe(40);
        stateLayers.All(layer => layer.Background == Brushes.Transparent).ShouldBeTrue();

        rating.Focus();
        Dispatcher.UIThread.RunJobs();

        stateLayers[2].Background.ShouldNotBe(Brushes.Transparent);

        rating.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        stateLayers.All(layer => layer.Background == Brushes.Transparent).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Rating_disabled_and_read_only_suppress_keyboard_changes()
    {
        var rating = new Rating
        {
            MaxValue = 5,
            SelectedValue = 2,
            IsEnabled = false,
        };
        Show(rating);
        rating.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var right = KeyArgs(Key.Right);
        rating.RaiseEvent(right);

        right.Handled.ShouldBeFalse();
        rating.SelectedValue.ShouldBe(2);

        rating.IsEnabled = true;
        rating.ReadOnly = true;
        Dispatcher.UIThread.RunJobs();

        var left = KeyArgs(Key.Left);
        rating.RaiseEvent(left);

        rating.SelectedValue.ShouldBe(2);
    }

    [Fact]
    public void Autocomplete_filter_matches_case_insensitive_contains_and_caps()
    {
        var items = new[] { "Apple", "Banana", "Grape", "Pineapple" };
        var expected = new[] { "Apple", "Grape", "Pineapple" };
        Autocomplete.Filter(items, "ap", 10).ShouldBe(expected);
        Autocomplete.Filter(items, "", 2).Count.ShouldBe(2);
        Autocomplete.Filter(items, "xyz", 10).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void Autocomplete_value_fills_inner_field()
    {
        var ac = new Autocomplete { Label = "Fruit" };
        ac.Items.Add("Apple");
        ac.Items.Add("Banana");
        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        ac.Value = "Banana";
        Dispatcher.UIThread.RunJobs();
        field.Text.ShouldBe("Banana");
    }

    [AvaloniaFact]
    public void Autocomplete_typing_updates_value_without_recursive_field_sync()
    {
        var ac = new Autocomplete { Label = "Fruit", MaxItems = 5, Width = 320 };
        ac.Items.Add("Cherry");
        ac.Items.Add("Citrus");
        ac.Items.Add("Orange");
        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.Text = "Cherr";
        Dispatcher.UIThread.RunJobs();

        ac.Value.ShouldBe("Cherr");
        field.Text.ShouldBe("Cherr");

        var rows = AutocompleteRows(ac);
        rows.Count.ShouldBe(1);
        rows[0].Height.ShouldBe(48);
        AutocompleteRowText(rows[0]).ShouldBe("Cherry");

        var popup = AutocompletePopup(ac);
        popup.IsOpen.ShouldBeTrue();
        var paper = popup.Child.ShouldBeOfType<Paper>();
        paper.Width.ShouldBe(320);
        paper.Height.ShouldBe(48);
        paper.ClipToBounds.ShouldBeTrue();
        var scroller = paper.Content.ShouldBeOfType<ScrollViewer>();
        scroller.Width.ShouldBe(320);
        scroller.Height.ShouldBe(48);
        scroller.ClipToBounds.ShouldBeTrue();
        rows[0].Width.ShouldBe(320);
        rows[0].Height.ShouldBe(48);
    }

    [AvaloniaFact]
    public void Autocomplete_selecting_item_completes_text_and_moves_caret_to_end()
    {
        var ac = new Autocomplete { Label = "Fruit" };
        ac.Items.Add("Cherry");
        ac.Items.Add("Citrus");
        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.Text = "Cherr";
        Dispatcher.UIThread.RunJobs();

        AutocompleteRows(ac)[0].RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        ac.Value.ShouldBe("Cherry");
        field.Text.ShouldBe("Cherry");
        AutocompletePopup(ac).IsOpen.ShouldBeFalse();

        field.ApplyTemplate();
        var textBox = field.GetVisualDescendants().OfType<TextBox>().First();
        textBox.CaretIndex.ShouldBe("Cherry".Length);
        textBox.SelectionStart.ShouldBe("Cherry".Length);
        textBox.SelectionEnd.ShouldBe("Cherry".Length);
    }

    [AvaloniaFact]
    public void Autocomplete_empty_focus_does_not_open_suggestions()
    {
        var ac = new Autocomplete { Label = "Fruit", Width = 320 };
        ac.Items.Add("Apple");
        ac.Items.Add("Apricot");
        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.GetVisualDescendants().OfType<TextBox>().First().Focus();
        Dispatcher.UIThread.RunJobs();

        AutocompletePopup(ac).IsOpen.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Autocomplete_popup_closes_when_field_loses_focus()
    {
        var ac = new Autocomplete { Label = "Fruit", Width = 320 };
        ac.Items.Add("Blueberry");
        ac.Items.Add("Blackberry");
        var next = new Avalonia.Controls.Button { Content = "Next" };
        Show(new StackPanel { Children = { ac, next } });
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.ApplyTemplate();
        var textBox = field.GetVisualDescendants().OfType<TextBox>().First();
        textBox.Focus();
        field.Text = "Blue";
        Dispatcher.UIThread.RunJobs();

        AutocompletePopup(ac).IsOpen.ShouldBeTrue();

        next.Focus();
        Dispatcher.UIThread.RunJobs();

        AutocompletePopup(ac).IsOpen.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Autocomplete_popup_clamps_many_rows_to_scroll_surface()
    {
        var ac = new Autocomplete { Label = "Fruit", Width = 320, MaxItems = 10 };
        foreach (var name in new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pineapple" })
        {
            ac.Items.Add(name);
        }

        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.ApplyTemplate();
        field.GetVisualDescendants().OfType<TextBox>().First().Focus();
        field.Text = "a";
        Dispatcher.UIThread.RunJobs();

        var rows = AutocompleteRows(ac);
        rows.Count.ShouldBe(8);
        rows.All(row => row.Height == 48).ShouldBeTrue();

        var paper = AutocompletePopup(ac).Child.ShouldBeOfType<Paper>();
        paper.Height.ShouldBe(320);
        paper.ClipToBounds.ShouldBeTrue();
        var scroller = paper.Content.ShouldBeOfType<ScrollViewer>();
        scroller.Height.ShouldBe(320);
        scroller.MaxHeight.ShouldBe(320);
        scroller.ClipToBounds.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Autocomplete_open_popup_raises_control_above_stacked_siblings()
    {
        var fruit = new Autocomplete { Label = "Fruit", Width = 320 };
        fruit.Items.Add("Grape");
        fruit.Items.Add("Grapefruit");

        var country = new Autocomplete { Label = "Country", Width = 320, Value = "Sweden" };
        country.Items.Add("Sweden");

        Show(new StackPanel { Children = { fruit, country } });
        fruit.ApplyTemplate();
        country.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = fruit.GetVisualDescendants().OfType<TextField>().First();
        field.GetVisualDescendants().OfType<TextBox>().First().Focus();
        field.Text = "Gra";
        Dispatcher.UIThread.RunJobs();

        AutocompletePopup(fruit).IsOpen.ShouldBeTrue();
        fruit.ZIndex.ShouldBe(LoamZIndex.Default.Popover);
        country.ZIndex.ShouldBe(0);
        AutocompletePopup(fruit).Child.ShouldBeOfType<Paper>().ZIndex.ShouldBe(LoamZIndex.Default.Popover);

        field.Text = "Grapefruit";
        Dispatcher.UIThread.RunJobs();

        AutocompletePopup(fruit).IsOpen.ShouldBeFalse();
        fruit.ZIndex.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Autocomplete_popup_rows_are_opaque_over_stacked_siblings()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var fruit = new Autocomplete { Label = "Fruit", Width = 320 };
        fruit.Items.Add("Apple");
        fruit.Items.Add("Pineapple");

        var country = new Autocomplete { Label = "Country", Width = 320, Value = "Sweden" };
        country.Items.Add("Sweden");

        Show(new StackPanel { Children = { fruit, country } });
        fruit.ApplyTemplate();
        country.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = fruit.GetVisualDescendants().OfType<TextField>().First();
        field.GetVisualDescendants().OfType<TextBox>().First().Focus();
        field.Text = "App";
        Dispatcher.UIThread.RunJobs();

        var rows = AutocompleteRows(fruit);
        rows.Count.ShouldBe(2);
        AutocompleteRowText(rows[0]).ShouldBe("Apple");
        AutocompleteRowText(rows[1]).ShouldBe("Pineapple");
        foreach (var row in rows)
        {
            var background = row.Background.ShouldBeAssignableTo<ISolidColorBrush>();
            background.ShouldNotBeNull();
            background!.Color.A.ShouldBe(byte.MaxValue);
            row.Child.ShouldBeOfType<Avalonia.Controls.Grid>().Children.OfType<Border>().First()
                .Background.ShouldBe(Brushes.Transparent);
        }
    }

    [AvaloniaFact]
    public void Autocomplete_filters_against_current_inner_editor_text()
    {
        var ac = new Autocomplete { Label = "Fruit", Width = 320, MaxItems = 10 };
        foreach (var name in new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pineapple" })
        {
            ac.Items.Add(name);
        }

        ac.SearchFunc = text => ac.Items.Where(item => item.Contains(text ?? "", StringComparison.OrdinalIgnoreCase));
        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.ApplyTemplate();
        var textBox = field.GetVisualDescendants().OfType<TextBox>().First();
        textBox.Focus();
        textBox.Text = "a";
        Dispatcher.UIThread.RunJobs();

        textBox.Text = "ap";
        Dispatcher.UIThread.RunJobs();

        var expected = new[] { "Apple", "Apricot", "Grape", "Pineapple" };
        ac.Value.ShouldBe("ap");
        field.Text.ShouldBe("ap");
        AutocompleteRows(ac).Select(AutocompleteRowText).ShouldBe(expected);
    }

    [AvaloniaFact]
    public void Autocomplete_filters_keyboard_typing_against_latest_text()
    {
        var ac = new Autocomplete { Label = "Fruit", Width = 320, MaxItems = 10 };
        foreach (var name in new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pineapple" })
        {
            ac.Items.Add(name);
        }

        ac.SearchFunc = text => ac.Items.Where(item => item.Contains(text ?? "", StringComparison.OrdinalIgnoreCase));
        var window = new Window { Content = ac };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.ApplyTemplate();
        var textBox = field.GetVisualDescendants().OfType<TextBox>().First();
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyTextInput("a");
        Dispatcher.UIThread.RunJobs();
        AutocompleteRows(ac).Count.ShouldBe(8);
        var firstPaper = AutocompletePopup(ac).Child.ShouldBeOfType<Paper>();
        firstPaper.Height.ShouldBe(320);
        var firstScroller = firstPaper.Content.ShouldBeOfType<ScrollViewer>();

        window.KeyTextInput("p");
        window.KeyTextInput("p");
        Dispatcher.UIThread.RunJobs();

        var expected = new[] { "Apple", "Pineapple" };
        ac.Value.ShouldBe("app");
        field.Text.ShouldBe("app");
        var narrowedPaper = AutocompletePopup(ac).Child.ShouldBeOfType<Paper>();
        narrowedPaper.ShouldBeSameAs(firstPaper);
        narrowedPaper.Height.ShouldBe(320);
        narrowedPaper.Content.ShouldBeSameAs(firstScroller);
        AutocompleteRows(ac).Select(AutocompleteRowText).ShouldBe(expected);
    }

    [AvaloniaFact]
    public void Autocomplete_normalizes_simple_template_text_typography()
    {
        var ac = new Autocomplete
        {
            Label = "Fruit",
            Width = 320,
            ItemTemplate = name => new Text { Text = name, Typo = Typo.Body2 },
        };
        ac.Items.Add("Apple");
        ac.Items.Add("Apricot");

        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.ApplyTemplate();
        field.GetVisualDescendants().OfType<TextBox>().First().Focus();
        field.Text = "Ap";
        Dispatcher.UIThread.RunJobs();

        var rows = AutocompleteRows(ac);
        rows.Count.ShouldBe(2);
        foreach (var rowText in rows.Select(AutocompleteRowTextBlock))
        {
            rowText.FontSize.ShouldBe(16);
            rowText.LineHeight.ShouldBe(24);
            rowText.FontWeight.ShouldBe(FontWeight.Normal);
            rowText.Margin.ShouldBe(default);
        }
    }

    [AvaloniaFact]
    public void Autocomplete_forwards_field_state_properties()
    {
        var ac = new Autocomplete
        {
            Label = "Fruit",
            ShrinkLabel = true,
            Error = true,
            ErrorText = "Required",
            HelperText = "Pick one",
        };
        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.ShrinkLabel.ShouldBeTrue();
        field.Error.ShouldBeTrue();
        field.ErrorText.ShouldBe("Required");
        field.HelperText.ShouldBe("Pick one");
    }

    [AvaloniaFact]
    public void Autocomplete_exposes_root_and_suggestion_automation()
    {
        var ac = new Autocomplete
        {
            Label = "Fruit",
            HelperText = "Pick one",
            Width = 320,
        };
        ac.Items.Add("Apple");
        ac.Items.Add("Apricot");
        ac.Items.Add("Banana");
        Show(ac);
        ac.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(ac).ShouldBe("Fruit");
        AutomationProperties.GetHelpText(ac).ShouldBe("Pick one");

        var field = ac.GetVisualDescendants().OfType<TextField>().First();
        field.Text = "Ap";
        Dispatcher.UIThread.RunJobs();

        var rows = AutocompleteRows(ac);
        rows.Count.ShouldBe(2);
        AutomationProperties.GetName(rows[0]).ShouldBe("Apple");
        AutomationProperties.GetHelpText(rows[0]).ShouldBe("Autocomplete suggestion");

        ac.Error = true;
        ac.ErrorText = "Required";
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetHelpText(ac).ShouldBe("Required");
    }

    private static List<Border> AutocompleteRows(Autocomplete autocomplete)
    {
        var popup = AutocompletePopup(autocomplete);
        var paper = popup.Child.ShouldBeOfType<Paper>();
        var scroller = paper.Content.ShouldBeOfType<ScrollViewer>();
        var list = scroller.Content.ShouldBeOfType<StackPanel>();
        return list.Children.OfType<Border>().ToList();
    }

    private static string? AutocompleteRowText(Border row)
    {
        return AutocompleteRowTextBlock(row).Text;
    }

    private static TextBlock AutocompleteRowTextBlock(Border row) =>
        AutocompleteRowContent(row).Children.OfType<TextBlock>().First();

    private static Avalonia.Controls.Grid AutocompleteRowContent(Border row) =>
        row.Child.ShouldBeOfType<Avalonia.Controls.Grid>();

    private static Popup AutocompletePopup(Autocomplete autocomplete)
    {
        var popupField = typeof(Autocomplete).GetField("_popup",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        popupField.ShouldNotBeNull();
        var popup = popupField!.GetValue(autocomplete) as Popup;
        popup.ShouldNotBeNull();
        return popup!;
    }

    [Fact]
    public void Mask_apply_formats_digits_letters_and_literals()
    {
        Mask.Apply("1234567890", "(###) ###-####").ShouldBe("(123) 456-7890");
        Mask.Apply("12", "(###) ###-####").ShouldBe("(12");
        Mask.Apply("abc123", "AAA-###").ShouldBe("abc-123");
        Mask.Apply("", "(###)").ShouldBe("");
    }

    [AvaloniaFact]
    public void MaskedTextField_reformats_text_on_input()
    {
        var field = new MaskedTextField { Pattern = "(###) ###-####" };
        Show(field);
        field.ApplyTemplate();

        field.Text = "1234567890";
        Dispatcher.UIThread.RunJobs();
        field.Text.ShouldBe("(123) 456-7890");
    }

    [AvaloniaFact]
    public void MaskedTextField_masks_live_editor_input_and_keeps_caret_at_end()
    {
        var field = new MaskedTextField
        {
            Label = "Phone",
            Pattern = "(###) ###-####",
        };
        Show(field);
        field.ApplyTemplate();
        var box = field.GetVisualDescendants().OfType<TextBox>().First();

        box.Focus();
        box.Text = "0709801046";
        Dispatcher.UIThread.RunJobs();

        const string expected = "(070) 980-1046";
        field.Text.ShouldBe(expected);
        box.Text.ShouldBe(expected);
        box.CaretIndex.ShouldBe(expected.Length);
        box.SelectionStart.ShouldBe(expected.Length);
        box.SelectionEnd.ShouldBe(expected.Length);
    }

    [AvaloniaFact]
    public void MaskedTextField_keyboard_typing_preserves_append_caret_through_literals()
    {
        var field = new MaskedTextField
        {
            Label = "Phone",
            Pattern = "(###) ###-####",
        };
        var window = new Window { Content = field };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        field.ApplyTemplate();
        var box = field.GetVisualDescendants().OfType<TextBox>().First();

        box.Focus();
        Dispatcher.UIThread.RunJobs();
        foreach (var digit in "0709801046")
        {
            window.KeyTextInput(digit.ToString());
            Dispatcher.UIThread.RunJobs();
            box.CaretIndex.ShouldBe(box.Text?.Length ?? 0);
        }

        const string expected = "(070) 980-1046";
        field.Text.ShouldBe(expected);
        box.Text.ShouldBe(expected);
        box.CaretIndex.ShouldBe(expected.Length);
    }

    [AvaloniaFact]
    public void MaskedTextField_exposes_pattern_state_and_field_chrome()
    {
        var field = new MaskedTextField
        {
            Label = "Phone",
            Pattern = "(###) ###-####",
            Text = "5551234567",
            HelperText = "Digits only.",
        };
        Show(field);
        field.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        field.Text.ShouldBe("(555) 123-4567");
        AutomationProperties.GetName(field).ShouldBe("Phone");
        AutomationProperties.GetHelpText(field).ShouldBe("Pattern (###) ###-####. Digits only.");

        var inputBorder = Part(field, "PART_InputBorder");
        var labelHost = Part(field, "PART_LabelHost");
        inputBorder.Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
        labelHost.Margin.Top.ShouldBe(0);
        labelHost.IsVisible.ShouldBeTrue();

        field.Error = true;
        field.ErrorText = "Complete the pattern.";
        Dispatcher.UIThread.RunJobs();
        AutomationProperties.GetHelpText(field).ShouldBe("Pattern (###) ###-####. Complete the pattern.");
    }

    [AvaloniaFact]
    public void FileUpload_shows_selected_names_as_chips()
    {
        var upload = new FileUpload { ButtonText = "Attach" };
        Show(upload);
        upload.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var filesPanel = upload.GetVisualDescendants().OfType<WrapPanel>().First(p => p.Name == "PART_Files");
        filesPanel.IsVisible.ShouldBeFalse();

        var names = new[] { "report.pdf", "data.csv" };
        upload.ShowSelection(names);
        Dispatcher.UIThread.RunJobs();

        filesPanel.IsVisible.ShouldBeTrue();
        var chips = upload.GetVisualDescendants().OfType<Chip>().Select(c => c.Text).ToList();
        chips.ShouldContain("report.pdf");
        chips.ShouldContain("data.csv");
    }

    [AvaloniaFact]
    public void FileUpload_renders_generated_label_helper_and_selection_status()
    {
        var upload = new FileUpload
        {
            Label = "Evidence",
            HelperText = "Attach documents for review.",
            EmptyText = "No evidence attached",
            SelectedTextFormat = "{0} selected: {1}",
            ButtonText = "Attach",
        };
        Show(upload);
        upload.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(upload).ShouldBe("Evidence");
        AutomationProperties.GetHelpText(upload).ShouldBe("Attach documents for review.");
        upload.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Evidence").ShouldBeTrue();
        upload.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Attach documents for review.").ShouldBeTrue();
        upload.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "No evidence attached").ShouldBeTrue();

        upload.ShowSelection(["brief.md", "metrics.csv"]);
        Dispatcher.UIThread.RunJobs();

        upload.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "2 selected: brief.md, metrics.csv").ShouldBeTrue();
    }

    [AvaloniaFact]
    public void FileUpload_applies_button_visual_properties()
    {
        var upload = new FileUpload
        {
            ButtonText = "Attach evidence",
            ButtonIcon = Icons.Material.Filled.Article,
            Variant = Variant.Filled,
            Color = LoamColor.Secondary,
            Size = LoamSize.Large,
        };
        Show(upload);
        upload.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var button = upload.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => button.Name == "PART_Button");
        button.Content.ShouldBe("Attach evidence");
        button.Variant.ShouldBe(Variant.Filled);
        button.Color.ShouldBe(LoamColor.Secondary);
        button.Size.ShouldBe(LoamSize.Large);
        button.StartIcon.ShouldBe(Icons.Material.Filled.Article);

        upload.Variant = Variant.Text;
        upload.Color = LoamColor.Error;
        upload.Size = LoamSize.Small;
        upload.ButtonIcon = Icons.Material.Filled.CloudUpload;
        Dispatcher.UIThread.RunJobs();

        button.Variant.ShouldBe(Variant.Text);
        button.Color.ShouldBe(LoamColor.Error);
        button.Size.ShouldBe(LoamSize.Small);
        button.StartIcon.ShouldBe(Icons.Material.Filled.CloudUpload);
    }

    [AvaloniaFact]
    public void FileUpload_removes_and_clears_selected_names()
    {
        var removed = string.Empty;
        var cleared = false;
        var upload = new FileUpload
        {
            ButtonText = "Attach",
            ShowRemoveButtons = true,
            ShowClearButton = true,
        };
        upload.FileRemoved += name => removed = name;
        upload.SelectionCleared += (_, _) => cleared = true;
        Show(upload);
        upload.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        upload.ShowSelection(["report.pdf", "data.csv"]);
        Dispatcher.UIThread.RunJobs();

        var report = upload.GetVisualDescendants().OfType<Chip>().First(chip => chip.Text == "report.pdf");
        report.Closeable.ShouldBeTrue();
        report.RaiseEvent(KeyArgs(Key.Space));
        Dispatcher.UIThread.RunJobs();

        removed.ShouldBe("report.pdf");
        upload.FileNames.ShouldNotContain("report.pdf");
        upload.FileNames.ShouldContain("data.csv");

        var clear = upload.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Clear");
        clear.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        cleared.ShouldBeTrue();
        upload.FileNames.ShouldBeEmpty();
        upload.GetVisualDescendants().OfType<WrapPanel>().First(p => p.Name == "PART_Files").IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void FileUpload_size_flows_to_generated_selection_actions()
    {
        var upload = new FileUpload
        {
            Size = LoamSize.ExtraLarge,
            ShowRemoveButtons = true,
            ShowClearButton = true,
        };
        Show(upload);
        upload.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        upload.ShowSelection(["contract.pdf"]);
        Dispatcher.UIThread.RunJobs();

        upload.GetVisualDescendants().OfType<Chip>().Single().Size.ShouldBe(LoamSize.ExtraLarge);
        upload.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Clear")
            .Size.ShouldBe(LoamSize.ExtraLarge);

        upload.Size = LoamSize.ExtraSmall;
        Dispatcher.UIThread.RunJobs();

        upload.GetVisualDescendants().OfType<Chip>().Single().Size.ShouldBe(LoamSize.ExtraSmall);
        upload.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Clear")
            .Size.ShouldBe(LoamSize.ExtraSmall);
    }

    [AvaloniaFact]
    public void FileUpload_disabled_suppresses_generated_selection_actions()
    {
        var removed = false;
        var cleared = false;
        var upload = new FileUpload
        {
            IsEnabled = false,
            ShowRemoveButtons = true,
            ShowClearButton = true,
        };
        upload.FileRemoved += _ => removed = true;
        upload.SelectionCleared += (_, _) => cleared = true;
        Show(upload);
        upload.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        upload.ShowSelection(["report.pdf", "data.csv"]);
        Dispatcher.UIThread.RunJobs();

        upload.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => button.Name == "PART_Button")
            .IsEnabled.ShouldBeFalse();
        upload.GetVisualDescendants().OfType<Chip>().All(chip => !chip.IsEnabled).ShouldBeTrue();
        upload.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Clear")
            .IsEnabled.ShouldBeFalse();

        var report = upload.GetVisualDescendants().OfType<Chip>().First(chip => chip.Text == "report.pdf");
        report.RaiseEvent(KeyArgs(Key.Space));

        var clear = upload.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Clear");
        clear.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        removed.ShouldBeFalse();
        cleared.ShouldBeFalse();
        upload.FileNames.ShouldContain("report.pdf");
        upload.FileNames.ShouldContain("data.csv");
    }

    [AvaloniaFact]
    public void ToggleGroup_fills_selected_segment_with_color()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var group = new ToggleGroup { Color = LoamColor.Primary };
        group.Items.Add(new ToggleItem("Day", "day"));
        group.Items.Add(new ToggleItem("Week", "week"));
        group.Items.Add(new ToggleItem("Month", "month"));
        group.SelectedValue = "week";
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var segments = ToggleSegments(group);
        segments.Count.ShouldBe(3);

        var week = ToggleSegment(group, "Week");
        week.Variant.ShouldBe(Variant.Filled);
        week.Color.ShouldBe(LoamColor.Primary);
        ((ISolidColorBrush)ButtonRoot(week).Background!).Color.ShouldBe(Color.Parse("#6750A4"));

        group.SelectedValue = "day";
        Dispatcher.UIThread.RunJobs();
        week.Variant.ShouldBe(Variant.Outlined);
        var day = ToggleSegment(group, "Day");
        day.Variant.ShouldBe(Variant.Filled);
        ((ISolidColorBrush)ButtonRoot(day).Background!).Color.ShouldBe(Color.Parse("#6750A4"));
    }

    [AvaloniaFact]
    public void ToggleGroup_segments_are_focusable_named_and_keyboard_selectable()
    {
        var group = new ToggleGroup();
        group.Items.Add(new ToggleItem("Day", "day"));
        group.Items.Add(new ToggleItem("Week", "week"));
        group.Items.Add(new ToggleItem("Month", "month"));
        group.SelectedValue = "day";
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        group.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(group).ShouldBe("Toggle group");
        AutomationProperties.GetHelpText(group).ShouldBe("Selected Day");
        var segments = ToggleSegments(group);
        segments.Count.ShouldBe(3);
        segments.All(segment => segment.Focusable).ShouldBeTrue();
        segments.All(segment => segment.MinHeight >= 40).ShouldBeTrue();

        segments.All(segment => segment.Bounds.Width > 56).ShouldBeTrue();

        var week = ToggleSegment(group, "Week");
        AutomationProperties.GetName(week).ShouldBe("Week");
        AutomationProperties.GetHelpText(week).ShouldBe("Toggle option");
        var activate = KeyArgs(Key.Space);
        week.RaiseEvent(activate);
        activate.Handled.ShouldBeTrue();
        group.SelectedValue.ShouldBe("week");
        AutomationProperties.GetHelpText(group).ShouldBe("Selected Week");
        AutomationProperties.GetHelpText(week).ShouldBe("Toggle option selected");

        var next = KeyArgs(Key.Right);
        week.RaiseEvent(next);
        next.Handled.ShouldBeTrue();
        group.SelectedValue.ShouldBe("month");
    }

    [AvaloniaFact]
    public void ToggleGroup_uses_button_segments_for_state_and_disabled_behavior()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var group = new ToggleGroup { Color = LoamColor.Primary, SelectedValue = "day" };
        group.Items.Add(new ToggleItem("Day", "day"));
        group.Items.Add(new ToggleItem("Week", "week"));
        group.Items.Add(new ToggleItem("Month", "month"));
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var segments = ToggleSegments(group);
        segments.Count.ShouldBe(3);
        ToggleSegment(group, "Day").Variant.ShouldBe(Variant.Filled);
        ToggleSegment(group, "Week").Variant.ShouldBe(Variant.Outlined);

        group.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        segments.All(segment => !segment.IsEnabled).ShouldBeTrue();
        var activate = KeyArgs(Key.Space);
        ToggleSegment(group, "Week").RaiseEvent(activate);
        activate.Handled.ShouldBeFalse();
        group.SelectedValue.ShouldBe("day");
    }

    [AvaloniaFact]
    public void ToggleGroup_size_changes_segment_density_and_typography()
    {
        var extraSmall = new ToggleGroup { Size = LoamSize.ExtraSmall };
        extraSmall.Items.Add(new ToggleItem("Day", "day"));
        Show(extraSmall);
        extraSmall.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var extraSmallSegment = ToggleSegment(extraSmall, "Day");
        extraSmallSegment.Size.ShouldBe(LoamSize.ExtraSmall);
        extraSmallSegment.MinHeight.ShouldBe(32);
        extraSmallSegment.FontSize.ShouldBe(11);

        var extraLarge = new ToggleGroup { Size = LoamSize.ExtraLarge };
        extraLarge.Items.Add(new ToggleItem("Month", "month"));
        Show(extraLarge);
        extraLarge.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var extraLargeSegment = ToggleSegment(extraLarge, "Month");
        extraLargeSegment.Size.ShouldBe(LoamSize.ExtraLarge);
        extraLargeSegment.MinHeight.ShouldBe(64);
        extraLargeSegment.FontSize.ShouldBe(16);
    }

    [AvaloniaFact]
    public void ToggleGroup_keeps_segment_labels_unclipped()
    {
        var group = new ToggleGroup { SelectedValue = "review" };
        group.Items.Add(new ToggleItem("Draft", "draft"));
        group.Items.Add(new ToggleItem("Review", "review"));
        group.Items.Add(new ToggleItem("Urgent", "urgent"));
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var segments = ToggleSegments(group);
        segments.Count.ShouldBe(3);
        segments.All(segment => segment.Bounds.Width > 0).ShouldBeTrue();
        segments.All(segment => segment.DesiredSize.Width > 0).ShouldBeTrue();

        var compact = new ToggleGroup { Size = LoamSize.ExtraSmall, SelectedValue = "month" };
        compact.Items.Add(new ToggleItem("Day", "day"));
        compact.Items.Add(new ToggleItem("Week", "week"));
        compact.Items.Add(new ToggleItem("Month", "month"));
        Show(compact);
        compact.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var month = ToggleSegment(compact, "Month");
        month.Bounds.Width.ShouldBeGreaterThan(0);

        foreach (var size in new[] { LoamSize.ExtraSmall, LoamSize.Small, LoamSize.Medium, LoamSize.Large, LoamSize.ExtraLarge })
        {
            var sized = new ToggleGroup { Size = size, SelectedValue = "week" };
            sized.Items.Add(new ToggleItem("Day", "day"));
            sized.Items.Add(new ToggleItem("Week", "week"));
            sized.Items.Add(new ToggleItem("Month", "month"));
            Show(sized);
            sized.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var sizedSegments = ToggleSegments(sized);
            sizedSegments.Count.ShouldBe(3);
            foreach (var segment in sizedSegments)
            {
                segment.Size.ShouldBe(size);
                segment.Bounds.Width.ShouldBeGreaterThan(0);
                ButtonRoot(segment).Bounds.Width.ShouldBeGreaterThan(0);
            }
        }

        var constrained = new ToggleGroup { Size = LoamSize.ExtraLarge, SelectedValue = "week" };
        constrained.Items.Add(new ToggleItem("Day", "day"));
        constrained.Items.Add(new ToggleItem("Week", "week"));
        constrained.Items.Add(new ToggleItem("Month", "month"));
        var constrainedHost = new Avalonia.Controls.Grid { ColumnDefinitions = new ColumnDefinitions("260") };
        constrainedHost.Children.Add(constrained);
        Show(constrainedHost);
        constrained.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var constrainedStrip = constrained.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Name == "PART_Items");
        constrainedStrip.Bounds.Width.ShouldBeGreaterThan(260);
        ToggleSegments(constrained).Select(segment => ButtonRoot(segment).Bounds.Width).Sum().ShouldBeGreaterThan(260);
        ToggleSegment(constrained, "Month").Bounds.Width.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void ToggleGroup_disabled_suppresses_keyboard_selection()
    {
        var group = new ToggleGroup { IsEnabled = false, SelectedValue = "day" };
        group.Items.Add(new ToggleItem("Day", "day"));
        group.Items.Add(new ToggleItem("Week", "week"));
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var week = ToggleSegment(group, "Week");
        week.IsEnabled.ShouldBeFalse();
        var activate = KeyArgs(Key.Space);
        week.RaiseEvent(activate);

        activate.Handled.ShouldBeFalse();
        group.SelectedValue.ShouldBe("day");
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
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

        Part(field, "PART_InputBorder").Margin.Top.ShouldBe(7);
        var labelHost = Part(field, "PART_LabelHost");
        labelHost.IsVisible.ShouldBeTrue();
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

        Part(field, "PART_InputBorder").BorderThickness.ShouldBe(new Thickness(1));
        Part(field, "PART_LabelHost").IsVisible.ShouldBeTrue();
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

        rating.ReadOnly = true;
        rating.RaiseEvent(KeyArgs(Key.Left));
        rating.SelectedValue.ShouldBe(5);
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

        var segments = group.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Child is Loam.Controls.Text).ToList();
        segments.Count.ShouldBe(3);

        var week = segments.First(s => ((Loam.Controls.Text)s.Child!).Text == "Week");
        ((ISolidColorBrush)week.Background!).Color.ShouldBe(Color.Parse("#6750A4"));

        group.SelectedValue = "day";
        Dispatcher.UIThread.RunJobs();
        ((ISolidColorBrush)week.Background!).Color.A.ShouldBe((byte)0);
        var day = segments.First(s => ((Loam.Controls.Text)s.Child!).Text == "Day");
        ((ISolidColorBrush)day.Background!).Color.ShouldBe(Color.Parse("#6750A4"));
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
        var segments = group.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Child is Loam.Controls.Text).ToList();
        segments.Count.ShouldBe(3);
        segments.All(segment => segment.Focusable).ShouldBeTrue();
        segments.All(segment => segment.MinHeight >= 40).ShouldBeTrue();

        var week = segments.First(segment => ((Loam.Controls.Text)segment.Child!).Text == "Week");
        AutomationProperties.GetName(week).ShouldBe("Week");
        var activate = KeyArgs(Key.Space);
        week.RaiseEvent(activate);
        activate.Handled.ShouldBeTrue();
        group.SelectedValue.ShouldBe("week");

        var next = KeyArgs(Key.Right);
        week.RaiseEvent(next);
        next.Handled.ShouldBeTrue();
        group.SelectedValue.ShouldBe("month");
    }
}

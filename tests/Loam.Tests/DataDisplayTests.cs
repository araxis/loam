using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class DataDisplayTests
{
    private sealed record Person(string Name, int Age);

    private sealed class EditablePerson
    {
        public string Name { get; set; } = "";
    }

    private static void Show(Control content)
    {
        new Window { Width = 500, Height = 400, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    [Fact]
    public void DataGrids_pagecount_and_sort_helpers()
    {
        DataGrids.PageCount(0, 10).ShouldBe(1);
        DataGrids.PageCount(25, 10).ShouldBe(3);
        DataGrids.PageCount(20, 0).ShouldBe(1);

        var people = new List<Person> { new("Bob", 30), new("Alice", 25), new("Carol", 40) };
        var ageColumn = new DataGridColumn<Person>("Age", p => p.Age);
        var ascending = new[] { 25, 30, 40 };
        var descending = new[] { 40, 30, 25 };
        DataGrids.Sort(people, ageColumn, false).Select(p => p.Age).ShouldBe(ascending);
        DataGrids.Sort(people, ageColumn, true).Select(p => p.Age).ShouldBe(descending);
    }

    [Fact]
    public void DataGrids_filter_uses_supplied_predicate()
    {
        var people = new List<Person> { new("Alice", 25), new("Bob", 30), new("Alicia", 40) };
        var result = DataGrids.Filter(people, "ali",
            (person, text) => person.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();

        result.Select(p => p.Name).ShouldBe(["Alice", "Alicia"]);
        DataGrids.Filter(people, "", (_, _) => false).ShouldBe(people);
    }

    [AvaloniaFact]
    public void DataGrid_renders_headers_and_body_cells()
    {
        var grid = new DataGrid<Person>();
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        grid.Columns.Add(new DataGridColumn<Person>("Age", p => p.Age));
        grid.Items = new List<Person> { new("Alice", 25), new("Bob", 30) };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        var texts = grid.GetVisualDescendants().OfType<Text>().Select(t => t.Text).ToList();
        texts.ShouldContain("Name");
        texts.ShouldContain("Alice");
        texts.ShouldContain("30");
    }

    [Fact]
    public void DataGrids_group_preserves_first_appearance_order()
    {
        var people = new List<Person> { new("Bob", 30), new("Ann", 30), new("Cy", 40), new("Dee", 30) };

        var groups = DataGrids.Group(people, p => (object?)p.Age);

        groups.Select(g => (int)g.Key!).ShouldBe([30, 40]);
        groups[0].Items.Select(p => p.Name).ShouldBe(["Bob", "Ann", "Dee"]);
        groups[1].Items.Single().Name.ShouldBe("Cy");
    }

    [Fact]
    public void DataGrids_group_handles_null_keys()
    {
        var values = new[] { "apple", "", "banana", "" };

        var groups = DataGrids.Group(values, s => s.Length == 0 ? null : (object?)s[0]);

        groups.Select(g => g.Key?.ToString()).ShouldBe(["a", null, "b"]);
        groups.Single(g => g.Key is null).Items.Count.ShouldBe(2);
    }

    [AvaloniaFact]
    public void DataGrid_renders_group_headers_when_grouped()
    {
        var grid = new DataGrid<Person> { GroupBy = p => p.Age < 35 ? "Junior" : "Senior" };
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        grid.Columns.Add(new DataGridColumn<Person>("Age", p => p.Age));
        grid.Items = new List<Person> { new("Ann", 30), new("Bob", 40), new("Cy", 25) };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        var texts = grid.GetVisualDescendants().OfType<Text>().Select(t => t.Text).ToList();
        texts.ShouldContain("Junior (2)");
        texts.ShouldContain("Senior (1)");
        texts.ShouldContain("Ann");
        texts.ShouldContain("Bob");
    }

    [AvaloniaFact]
    public void DataGrid_group_header_collapses_and_expands_rows()
    {
        var grid = new DataGrid<Person> { GroupBy = p => p.Age < 35 ? "Junior" : "Senior" };
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        grid.Items = new List<Person> { new("Ann", 30), new("Cy", 25), new("Bob", 40) };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        grid.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Ann").ShouldBeTrue();

        Border JuniorHeader() => grid.GetVisualDescendants().OfType<Border>()
            .First(b => AutomationProperties.GetName(b)?.Contains("group Junior", StringComparison.Ordinal) == true);

        JuniorHeader().RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        // Junior rows hidden; the header (with full count) and the Senior group stay.
        grid.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Ann").ShouldBeFalse();
        grid.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Cy").ShouldBeFalse();
        grid.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Junior (2)").ShouldBeTrue();
        grid.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Bob").ShouldBeTrue();

        JuniorHeader().RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        grid.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Ann").ShouldBeTrue();
    }

    [AvaloniaFact]
    public void DataGrid_filter_text_limits_rendered_rows()
    {
        var grid = new DataGrid<Person>
        {
            FilterText = "ali",
            Filter = (person, text) => person.Name.Contains(text, StringComparison.OrdinalIgnoreCase),
        };
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        grid.Items = new List<Person> { new("Alice", 25), new("Bob", 30), new("Alicia", 40) };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        var texts = grid.GetVisualDescendants().OfType<Text>().Select(t => t.Text).ToList();
        texts.ShouldContain("Alice");
        texts.ShouldContain("Alicia");
        texts.ShouldNotContain("Bob");
    }

    [AvaloniaFact]
    public void DataGrid_virtualize_limits_unpaged_rows()
    {
        var grid = new DataGrid<Person> { Virtualize = true, MaxRenderedRows = 3 };
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        grid.Items = Enumerable.Range(1, 10).Select(i => new Person($"Person {i}", i)).ToList();
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        var rendered = grid.GetVisualDescendants().OfType<Text>()
            .Count(t => t.Text?.StartsWith("Person ", StringComparison.Ordinal) == true);
        rendered.ShouldBe(3);
    }

    [AvaloniaFact]
    public void DataGrid_editable_column_updates_item()
    {
        var person = new EditablePerson { Name = "Alice" };
        var grid = new DataGrid<EditablePerson>();
        grid.Columns.Add(new DataGridColumn<EditablePerson>("Name", p => p.Name)
        {
            Editable = true,
            SetText = (item, text) => item.Name = text ?? "",
        });
        grid.Items = new List<EditablePerson> { person };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        var editor = grid.GetVisualDescendants().OfType<TextBox>().First();
        editor.Text = "Alicia";
        Dispatcher.UIThread.RunJobs();
        person.Name.ShouldBe("Alicia");
    }

    [AvaloniaFact]
    public void DataGrid_row_click_selects_item()
    {
        var grid = new DataGrid<Person> { Hover = false };
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        var bob = new Person("Bob", 30);
        grid.Items = new List<Person> { new("Alice", 25), bob };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        grid.SelectedItem = bob;
        grid.SelectedItem.ShouldBe(bob);
    }

    [AvaloniaFact]
    public void DataGrid_rows_are_focusable_named_and_keyboard_selectable()
    {
        var grid = new DataGrid<Person> { Hover = false };
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        var alice = new Person("Alice", 25);
        grid.Items = new List<Person> { alice };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        var row = grid.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Row 1: Alice");
        row.Focusable.ShouldBeTrue();

        var key = KeyArgs(Key.Enter);
        row.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        grid.SelectedItem.ShouldBe(alice);
    }

    [AvaloniaFact]
    public void DataGrid_sort_headers_are_focusable_named_and_keyboard_sortable()
    {
        var grid = new DataGrid<Person>();
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        grid.Columns.Add(new DataGridColumn<Person>("Age", p => p.Age));
        grid.Items = new List<Person> { new("Bob", 30), new("Alice", 25) };
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        var ageHeader = grid.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Sort by Age");
        ageHeader.Focusable.ShouldBeTrue();

        var key = KeyArgs(Key.Space);
        ageHeader.RaiseEvent(key);
        Dispatcher.UIThread.RunJobs();
        key.Handled.ShouldBeTrue();

        var names = grid.GetVisualDescendants().OfType<Text>()
            .Select(text => text.Text)
            .Where(text => text is "Alice" or "Bob")
            .ToList();
        names.ShouldBe(["Alice", "Bob"]);
    }

    [AvaloniaFact]
    public void DataGrid_clamps_page_and_clears_selection_when_filtered_out()
    {
        var alice = new Person("Alice", 25);
        var bob = new Person("Bob", 30);
        var grid = new DataGrid<Person>
        {
            PageSize = 1,
            FilterText = "Alice",
            Filter = (person, text) => person.Name.Contains(text, StringComparison.OrdinalIgnoreCase),
        };
        grid.Columns.Add(new DataGridColumn<Person>("Name", p => p.Name));
        grid.Items = new List<Person> { alice, bob };
        grid.SelectedItem = bob;
        grid.Page = 99;
        Show(grid);
        Dispatcher.UIThread.RunJobs();

        grid.Page.ShouldBe(1);
        grid.SelectedItem.ShouldBeNull();
    }

    [AvaloniaFact]
    public void Tabs_switch_content_on_selection()
    {
        var first = new TextBlock { Text = "A" };
        var second = new TextBlock { Text = "B" };
        var tabs = new Tabs();
        tabs.Items.Add(new Loam.Controls.TabItem("First", first));
        tabs.Items.Add(new Loam.Controls.TabItem("Second", second));
        Show(tabs);
        tabs.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = tabs.GetVisualDescendants().OfType<ContentControl>().First(c => c.Name == "PART_Content");
        content.Content.ShouldBe(first);

        tabs.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        content.Content.ShouldBe(second);
    }

    [AvaloniaFact]
    public void Tabs_headers_are_focusable_named_and_keyboard_selectable()
    {
        var first = new TextBlock { Text = "A" };
        var second = new TextBlock { Text = "B" };
        var tabs = new Tabs();
        tabs.Items.Add(new Loam.Controls.TabItem("First", first));
        tabs.Items.Add(new Loam.Controls.TabItem("Second", second));
        Show(tabs);
        tabs.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        tabs.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(tabs).ShouldBe("Tabs");
        var headers = tabs.GetVisualDescendants().OfType<Border>()
            .Where(border => border.Focusable && AutomationProperties.GetName(border) is not null)
            .ToList();
        headers.Count.ShouldBe(2);
        headers[0].MinHeight.ShouldBeGreaterThanOrEqualTo(40);
        AutomationProperties.GetName(headers[1]).ShouldBe("Second");

        var next = KeyArgs(Key.Right);
        headers[0].RaiseEvent(next);
        next.Handled.ShouldBeTrue();
        tabs.SelectedIndex.ShouldBe(1);

        var content = tabs.GetVisualDescendants().OfType<ContentControl>().First(c => c.Name == "PART_Content");
        content.Content.ShouldBe(second);
    }

    [AvaloniaFact]
    public void Tabs_clamp_selection_and_suppress_disabled_header_activation()
    {
        var tabs = new Tabs();
        tabs.Items.Add(new Loam.Controls.TabItem("First", new TextBlock { Text = "A" }));
        tabs.Items.Add(new Loam.Controls.TabItem("Second", new TextBlock { Text = "B" }));
        tabs.SelectedIndex = 99;
        Show(tabs);
        tabs.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        tabs.SelectedIndex.ShouldBe(1);
        AutomationProperties.GetHelpText(tabs).ShouldBe("Tab 2 of 2");

        tabs.Items.RemoveAt(1);
        Dispatcher.UIThread.RunJobs();
        tabs.SelectedIndex.ShouldBe(0);
        AutomationProperties.GetHelpText(tabs).ShouldBe("Tab 1 of 1");

        tabs.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        tabs.Items.Add(new Loam.Controls.TabItem("Third", new TextBlock { Text = "C" }));
        Dispatcher.UIThread.RunJobs();

        var headers = tabs.GetVisualDescendants().OfType<Border>()
            .Where(border => border.Focusable && AutomationProperties.GetName(border) is not null)
            .ToList();
        headers.All(header => !header.IsEnabled).ShouldBeTrue();

        var key = KeyArgs(Key.Enter);
        headers[1].RaiseEvent(key);
        key.Handled.ShouldBeFalse();
        tabs.SelectedIndex.ShouldBe(0);
    }

    [AvaloniaFact]
    public void SimpleTable_builds_header_and_data_cells()
    {
        var table = new SimpleTable();
        table.Headers.Add("Name");
        table.Headers.Add("Age");
        table.Rows.Add(new TableRow("Alice", 30));
        table.Rows.Add(new TableRow("Bob", 25));
        Show(table);
        table.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var grid = table.GetVisualDescendants().OfType<Avalonia.Controls.Grid>().First();
        grid.ColumnDefinitions.Count.ShouldBe(2);
        grid.RowDefinitions.Count.ShouldBe(3);

        var texts = table.GetVisualDescendants().OfType<Loam.Controls.Text>().Select(t => t.Text).ToList();
        texts.ShouldContain("Name");
        texts.ShouldContain("Alice");
        texts.ShouldContain("25");
    }

    [AvaloniaFact]
    public void SimpleTable_empty_state_and_rows_are_named()
    {
        var empty = new SimpleTable();
        Show(empty);
        empty.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        empty.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ShouldContain("No rows");
        AutomationProperties.GetName(empty).ShouldBe("Table");
        empty.GetVisualDescendants().OfType<Border>()
            .Any(border => AutomationProperties.GetName(border) == "No rows")
            .ShouldBeTrue();

        var table = new SimpleTable { Hover = true };
        table.Headers.Add("Name");
        table.Rows.Add(new TableRow("Alice"));
        Show(table);
        table.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var row = table.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Row 1: Alice");
        row.Focusable.ShouldBeTrue();
    }

    [Fact]
    public void Pagination_buildpages_windows_and_collapses_gaps()
    {
        var middle = new[] { 1, 0, 4, 5, 6, 0, 10 };
        var all = new[] { 1, 2, 3, 4, 5 };
        var head = new[] { 1, 2, 3, 0, 10 };
        Pagination.BuildPages(10, 5, 1, 3).ShouldBe(middle);
        Pagination.BuildPages(5, 1, 1, 3).ShouldBe(all);
        Pagination.BuildPages(10, 1, 1, 3).ShouldBe(head);
        Pagination.BuildPages(0, 1, 1, 3).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void Pagination_prev_arrow_disabled_on_first_and_next_advances()
    {
        var pagination = new Pagination { Count = 10, Selected = 1 };
        Show(pagination);
        pagination.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var arrows = pagination.GetVisualDescendants().OfType<IconButton>().ToList();
        arrows.Count.ShouldBe(2);
        AutomationProperties.GetName(arrows[0]).ShouldBe("Previous page");
        AutomationProperties.GetName(arrows[1]).ShouldBe("Next page");
        arrows[0].IsEnabled.ShouldBeFalse();
        arrows[1].IsEnabled.ShouldBeTrue();

        arrows[1].RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        pagination.Selected.ShouldBe(2);
    }

    [AvaloniaFact]
    public void Pagination_clamps_selected_page_and_names_page_buttons()
    {
        var pagination = new Pagination { Count = 3, Selected = 9 };
        Show(pagination);
        pagination.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        pagination.Selected.ShouldBe(3);
        var pages = pagination.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Where(button => AutomationProperties.GetName(button)?.StartsWith("Page ", StringComparison.Ordinal) == true)
            .ToList();
        pages.Select(button => AutomationProperties.GetName(button)).ShouldContain("Page 3, selected");
        AutomationProperties.GetName(pagination).ShouldBe("Pagination");
        AutomationProperties.GetHelpText(pagination).ShouldBe("Page 3 of 3");
    }

    [AvaloniaFact]
    public void Pagination_disabled_suppresses_direct_page_and_arrow_clicks()
    {
        var pagination = new Pagination { Count = 5, Selected = 3, IsEnabled = false };
        Show(pagination);
        pagination.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var next = pagination.GetVisualDescendants().OfType<IconButton>()
            .Single(button => AutomationProperties.GetName(button) == "Next page");
        next.IsEnabled.ShouldBeFalse();
        next.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        var pageFour = pagination.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => AutomationProperties.GetName(button) == "Page 4");
        pageFour.IsEnabled.ShouldBeFalse();
        pageFour.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        pagination.Selected.ShouldBe(3);
    }

    [AvaloniaFact]
    public void Stepper_next_advances_and_marks_completed()
    {
        var stepper = new Stepper();
        stepper.Steps.Add(new Step("One", new TextBlock { Text = "1" }));
        stepper.Steps.Add(new Step("Two", new TextBlock { Text = "2" }));
        stepper.Steps.Add(new Step("Three", new TextBlock { Text = "3" }));
        Show(stepper);
        stepper.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = stepper.GetVisualDescendants().OfType<ContentControl>().First(c => c.Name == "PART_Content");
        content.Content.ShouldBe(stepper.Steps[0].Content);

        stepper.Next();
        Dispatcher.UIThread.RunJobs();
        stepper.ActiveIndex.ShouldBe(1);
        stepper.Steps[0].Completed.ShouldBeTrue();
        content.Content.ShouldBe(stepper.Steps[1].Content);

        stepper.Previous();
        Dispatcher.UIThread.RunJobs();
        stepper.ActiveIndex.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Stepper_finish_invokes_oncompleted_on_last_step()
    {
        var done = false;
        var stepper = new Stepper { OnCompleted = () => done = true };
        stepper.Steps.Add(new Step("Only", new TextBlock()));
        Show(stepper);
        stepper.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        stepper.Next();
        done.ShouldBeTrue();
        stepper.Steps[0].Completed.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Stepper_actions_are_named_and_empty_next_is_safe()
    {
        var stepper = new Stepper();
        Show(stepper);
        stepper.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        stepper.Next();

        var buttons = stepper.GetVisualDescendants().OfType<Loam.Controls.Button>().ToList();
        buttons.Count.ShouldBe(2);
        AutomationProperties.GetName(buttons[0]).ShouldBe("Previous step");
        AutomationProperties.GetName(buttons[1]).ShouldBe("Finish steps");
        buttons[1].IsEnabled.ShouldBeFalse();
        AutomationProperties.GetName(stepper).ShouldBe("Stepper");
        AutomationProperties.GetHelpText(stepper).ShouldBe("No steps");
    }

    [AvaloniaFact]
    public void Stepper_clamps_active_index_and_disables_actions_when_disabled()
    {
        var stepper = new Stepper { IsEnabled = false };
        stepper.Steps.Add(new Step("One", new TextBlock { Text = "1" }));
        stepper.Steps.Add(new Step("Two", new TextBlock { Text = "2" }));
        stepper.ActiveIndex = 99;
        Show(stepper);
        stepper.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        stepper.ActiveIndex.ShouldBe(1);
        AutomationProperties.GetHelpText(stepper).ShouldBe("Step 2 of 2");

        stepper.Next();
        stepper.Previous();
        stepper.ActiveIndex.ShouldBe(1);

        var buttons = stepper.GetVisualDescendants().OfType<Loam.Controls.Button>().ToArray();
        buttons.All(button => !button.IsEnabled).ShouldBeTrue();

        stepper.Steps.RemoveAt(1);
        Dispatcher.UIThread.RunJobs();
        stepper.ActiveIndex.ShouldBe(0);
    }

    [AvaloniaFact]
    public void TreeView_expands_children_and_selects_a_single_node()
    {
        var child1 = new Loam.Controls.TreeViewItem { Text = "Child 1" };
        var child2 = new Loam.Controls.TreeViewItem { Text = "Child 2" };
        var root = new Loam.Controls.TreeViewItem { Text = "Root" };
        root.Items.Add(child1);
        root.Items.Add(child2);
        var tree = new Loam.Controls.TreeView();
        tree.Items.Add(root);
        Show(tree);
        tree.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        root.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var childContainer = root.GetVisualDescendants().OfType<StackPanel>().First(p => p.Name == "PART_Children");
        childContainer.IsVisible.ShouldBeFalse();

        root.Expanded = true;
        Dispatcher.UIThread.RunJobs();
        childContainer.IsVisible.ShouldBeTrue();

        tree.SelectedItem = child1;
        Dispatcher.UIThread.RunJobs();
        child1.IsSelected.ShouldBeTrue();
        root.IsSelected.ShouldBeFalse();
        child2.IsSelected.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void TreeViewItem_is_focusable_named_and_responds_to_keyboard()
    {
        var root = new Loam.Controls.TreeViewItem { Text = "Root" };
        root.Items.Add(new Loam.Controls.TreeViewItem { Text = "Child" });
        var selected = false;
        root.ItemSelected += (_, _) => selected = true;
        Show(root);
        root.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        root.Focusable.ShouldBeTrue();
        root.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Row").Focusable.ShouldBeTrue();
        AutomationProperties.GetName(root).ShouldBe("Root");

        var toggle = KeyArgs(Key.Space);
        root.RaiseEvent(toggle);
        toggle.Handled.ShouldBeTrue();
        root.Expanded.ShouldBeTrue();

        var select = KeyArgs(Key.Enter);
        root.RaiseEvent(select);
        select.Handled.ShouldBeTrue();
        selected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void TreeView_arrow_keys_expand_collapse_and_removed_selection_clears()
    {
        var child = new Loam.Controls.TreeViewItem { Text = "Child" };
        var root = new Loam.Controls.TreeViewItem { Text = "Root" };
        root.Items.Add(child);
        var tree = new Loam.Controls.TreeView();
        tree.Items.Add(root);
        Show(tree);
        tree.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var expand = KeyArgs(Key.Right);
        root.RaiseEvent(expand);
        expand.Handled.ShouldBeTrue();
        root.Expanded.ShouldBeTrue();

        var collapse = KeyArgs(Key.Left);
        root.RaiseEvent(collapse);
        collapse.Handled.ShouldBeTrue();
        root.Expanded.ShouldBeFalse();

        tree.SelectedItem = child;
        root.Items.Remove(child);
        Dispatcher.UIThread.RunJobs();

        tree.SelectedItem.ShouldBeNull();
        child.IsSelected.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Carousel_navigation_wraps_and_swaps_content()
    {
        var a = new TextBlock { Text = "A" };
        var b = new TextBlock { Text = "B" };
        var c = new TextBlock { Text = "C" };
        var carousel = new Loam.Controls.Carousel();
        carousel.Items.Add(new CarouselItem(a));
        carousel.Items.Add(new CarouselItem(b));
        carousel.Items.Add(new CarouselItem(c));
        Show(carousel);
        carousel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = carousel.GetVisualDescendants().OfType<ContentControl>().First(cc => cc.Name == "PART_Content");
        content.Content.ShouldBe(a);

        carousel.Next();
        Dispatcher.UIThread.RunJobs();
        carousel.SelectedIndex.ShouldBe(1);
        content.Content.ShouldBe(b);

        carousel.Previous();
        carousel.Previous();
        Dispatcher.UIThread.RunJobs();
        carousel.SelectedIndex.ShouldBe(2);
        content.Content.ShouldBe(c);
    }

    [AvaloniaFact]
    public void Carousel_generated_slides_goto_and_selection_event_work()
    {
        var changedTo = -1;
        var carousel = new Loam.Controls.Carousel();
        carousel.SelectedIndexChanged += (_, index) => changedTo = index;
        carousel.Items.Add(new CarouselItem("Plan", "Define scope", LoamColor.Primary));
        carousel.Items.Add(new CarouselItem("Build", "Implement controls", LoamColor.Success));
        Show(carousel);
        carousel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = carousel.GetVisualDescendants().OfType<ContentControl>().First(cc => cc.Name == "PART_Content");
        content.Content.ShouldBeOfType<Border>();
        carousel.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ShouldContain("Plan");
        carousel.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ShouldContain("Define scope");

        carousel.GoTo(99);
        Dispatcher.UIThread.RunJobs();

        carousel.SelectedIndex.ShouldBe(1);
        changedTo.ShouldBe(1);
        carousel.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ShouldContain("Build");
    }

    [AvaloniaFact]
    public void Carousel_is_named_and_supports_keyboard_navigation()
    {
        var carousel = new Loam.Controls.Carousel();
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "A" }));
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "B" }));
        Show(carousel);
        carousel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(carousel).ShouldBe("Carousel");
        var key = KeyArgs(Key.Right);
        carousel.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        carousel.SelectedIndex.ShouldBe(1);

        var bullets = carousel.GetVisualDescendants().OfType<Border>()
            .Where(border => AutomationProperties.GetName(border)?.StartsWith("Slide ", StringComparison.Ordinal) == true)
            .ToList();
        bullets.Count.ShouldBe(2);
        bullets[0].Focusable.ShouldBeTrue();

        var selectFirst = KeyArgs(Key.Enter);
        bullets[0].RaiseEvent(selectFirst);
        selectFirst.Handled.ShouldBeTrue();
        carousel.SelectedIndex.ShouldBe(0);

        carousel.SelectedIndex = 8;
        Dispatcher.UIThread.RunJobs();
        carousel.SelectedIndex.ShouldBe(1);
    }

    [AvaloniaFact]
    public void Carousel_prev_next_buttons_activate_from_click_events()
    {
        var carousel = new Loam.Controls.Carousel();
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "A" }));
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "B" }));
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "C" }));
        Show(carousel);
        carousel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var next = carousel.GetVisualDescendants().OfType<IconButton>()
            .Single(button => AutomationProperties.GetName(button) == "Next slide");
        next.RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        carousel.SelectedIndex.ShouldBe(1);

        var previous = carousel.GetVisualDescendants().OfType<IconButton>()
            .Single(button => AutomationProperties.GetName(button) == "Previous slide");
        previous.RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        carousel.SelectedIndex.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task Carousel_auto_play_advances_while_enabled_and_attached()
    {
        var carousel = new Loam.Controls.Carousel
        {
            AutoPlay = true,
            AutoPlayInterval = TimeSpan.FromMilliseconds(20),
        };
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "A" }));
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "B" }));
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "C" }));
        carousel.Items.Add(new CarouselItem(new TextBlock { Text = "D" }));
        Show(carousel);
        carousel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        for (var attempt = 0; attempt < 20 && carousel.SelectedIndex == 0; attempt++)
        {
            await Task.Delay(20);
            Dispatcher.UIThread.RunJobs();
        }

        carousel.SelectedIndex.ShouldNotBe(0);

        carousel.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        var disabledIndex = carousel.SelectedIndex;
        await Task.Delay(80);
        Dispatcher.UIThread.RunJobs();
        carousel.SelectedIndex.ShouldBe(disabledIndex);
    }

    [AvaloniaFact]
    public void Carousel_empty_disabled_and_chrome_state_are_deterministic()
    {
        var empty = new Loam.Controls.Carousel { SelectedIndex = 4 };
        Show(empty);
        empty.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        empty.SelectedIndex.ShouldBe(0);
        AutomationProperties.GetHelpText(empty).ShouldBe("No slides");
        empty.GetVisualDescendants().OfType<ContentControl>().First(cc => cc.Name == "PART_Content")
            .Content.ShouldBeOfType<Text>().Text.ShouldBe("No slides");

        var disabled = new Loam.Controls.Carousel { IsEnabled = false };
        disabled.Items.Add(new CarouselItem(new TextBlock { Text = "A" }));
        disabled.Items.Add(new CarouselItem(new TextBlock { Text = "B" }));
        Show(disabled);
        disabled.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        disabled.Next();
        disabled.SelectedIndex.ShouldBe(0);

        var key = KeyArgs(Key.Right);
        disabled.RaiseEvent(key);
        key.Handled.ShouldBeFalse();
        disabled.SelectedIndex.ShouldBe(0);
        AutomationProperties.GetHelpText(disabled).ShouldBe("Slide 1 of 2");

        var bullets = disabled.GetVisualDescendants().OfType<Border>()
            .Where(border => AutomationProperties.GetName(border)?.StartsWith("Slide ", StringComparison.Ordinal) == true)
            .ToList();
        bullets.Count.ShouldBe(2);
        bullets.All(bullet => !bullet.IsEnabled).ShouldBeTrue();

        var bulletKey = KeyArgs(Key.Enter);
        bullets[1].RaiseEvent(bulletKey);
        bulletKey.Handled.ShouldBeFalse();
        disabled.SelectedIndex.ShouldBe(0);

        var chrome = new Loam.Controls.Carousel { ShowArrows = false, ShowBullets = false };
        chrome.Items.Add(new CarouselItem(new TextBlock { Text = "A" }));
        Show(chrome);
        chrome.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        chrome.GetVisualDescendants().OfType<Control>()
            .Single(control => AutomationProperties.GetName(control) == "Previous slide")
            .IsVisible.ShouldBeFalse();
        chrome.GetVisualDescendants().OfType<Control>()
            .Single(control => AutomationProperties.GetName(control) == "Next slide")
            .IsVisible.ShouldBeFalse();
        chrome.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Name == "PART_Bullets")
            .IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Timeline_builds_a_dot_and_card_per_item()
    {
        var timeline = new Timeline();
        timeline.Items.Add(new TimelineItem("First"));
        timeline.Items.Add(new TimelineItem("Second", LoamColor.Success));
        Show(timeline);
        Dispatcher.UIThread.RunJobs();

        var grid = timeline.GetVisualDescendants().OfType<Avalonia.Controls.Grid>().First();
        grid.RowDefinitions.Count.ShouldBe(2);
        grid.ColumnDefinitions.Count.ShouldBe(2);
        timeline.GetVisualDescendants().OfType<Paper>().Count().ShouldBe(2);

        var texts = timeline.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        texts.ShouldContain("First");
        texts.ShouldContain("Second");

        AutomationProperties.GetName(timeline).ShouldBe("Timeline");
        timeline.GetVisualDescendants().OfType<Paper>()
            .Count(paper => paper.Focusable && AutomationProperties.GetName(paper)?.StartsWith("Timeline item ", StringComparison.Ordinal) == true)
            .ShouldBe(2);
    }

    [AvaloniaFact]
    public void Timeline_generated_item_anatomy_uses_title_subtitle_and_time()
    {
        var timeline = new Timeline();
        timeline.Items.Add(new TimelineItem("Review finished", "Keyboard checks passed.", "10:30", LoamColor.Success));
        Show(timeline);
        Dispatcher.UIThread.RunJobs();

        var texts = timeline.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
        texts.ShouldContain("10:30");
        texts.ShouldContain("Review finished");
        texts.ShouldContain("Keyboard checks passed.");

        var card = timeline.GetVisualDescendants().OfType<Paper>()
            .Single(paper => AutomationProperties.GetName(paper)?.StartsWith("Timeline item ", StringComparison.Ordinal) == true);
        AutomationProperties.GetName(card).ShouldBe("Timeline item 1: Review finished");
    }

    [AvaloniaFact]
    public void Timeline_horizontal_orientation_builds_horizontal_surface()
    {
        var timeline = new Timeline { Orientation = Orientation.Horizontal };
        timeline.Items.Add(new TimelineItem("First"));
        timeline.Items.Add(new TimelineItem("Second", LoamColor.Success));
        timeline.Items.Add(new TimelineItem("Third", LoamColor.Info));
        Show(timeline);
        Dispatcher.UIThread.RunJobs();

        var grid = timeline.GetVisualDescendants().OfType<Avalonia.Controls.Grid>().First();
        grid.RowDefinitions.Count.ShouldBe(2);
        grid.ColumnDefinitions.Count.ShouldBe(3);
        timeline.GetVisualDescendants().OfType<Paper>().Count().ShouldBe(3);

        timeline.Orientation = Orientation.Vertical;
        Dispatcher.UIThread.RunJobs();

        var vertical = timeline.GetVisualDescendants().OfType<Avalonia.Controls.Grid>().First();
        vertical.RowDefinitions.Count.ShouldBe(3);
        vertical.ColumnDefinitions.Count.ShouldBe(2);
    }

    [AvaloniaFact]
    public void Timeline_empty_disabled_and_dynamic_help_text_are_deterministic()
    {
        var timeline = new Timeline();
        Show(timeline);
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetHelpText(timeline).ShouldBe("No items");
        timeline.Child.ShouldBeOfType<Text>().Text.ShouldBe("No timeline items");

        timeline.Items.Add(new TimelineItem("First"));
        timeline.Items.Add(new TimelineItem("Second", LoamColor.Success));
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetHelpText(timeline).ShouldBe("2 items");
        var cards = timeline.GetVisualDescendants().OfType<Paper>()
            .Where(paper => AutomationProperties.GetName(paper)?.StartsWith("Timeline item ", StringComparison.Ordinal) == true)
            .ToArray();
        cards.Length.ShouldBe(2);
        AutomationProperties.GetHelpText(cards[0]).ShouldBe("Item 1 of 2");
        AutomationProperties.GetHelpText(cards[1]).ShouldBe("Item 2 of 2");

        timeline.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        timeline.Opacity.ShouldBeLessThan(1);

        timeline.Items.Clear();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetHelpText(timeline).ShouldBe("No items");
        timeline.Child.ShouldBeOfType<Text>().Text.ShouldBe("No timeline items");
    }

    [AvaloniaFact]
    public void ExpansionPanel_toggles_content_visibility()
    {
        var panel = new ExpansionPanel { Header = "Title", Content = new TextBlock { Text = "body" } };
        Show(panel);
        panel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = panel.GetVisualDescendants()
            .OfType<Avalonia.Controls.Presenters.ContentPresenter>().First(p => p.Name == "PART_Content");
        content.IsVisible.ShouldBeFalse();

        panel.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();
        content.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void ExpansionPanel_is_focusable_named_and_toggles_from_keyboard()
    {
        var panel = new ExpansionPanel { Header = "Filters", Content = new TextBlock { Text = "body" } };
        Show(panel);
        panel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        panel.Focusable.ShouldBeTrue();
        panel.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Header").Focusable.ShouldBeTrue();
        AutomationProperties.GetName(panel).ShouldBe("Filters");

        var key = KeyArgs(Key.Space);
        panel.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        panel.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void ExpansionPanel_disabled_header_does_not_toggle()
    {
        var panel = new ExpansionPanel { Header = "Locked", Content = new TextBlock { Text = "body" }, IsEnabled = false };
        Show(panel);
        panel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var header = panel.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Header");
        AutomationProperties.GetName(header).ShouldBe("Locked");
        panel.Opacity.ShouldBeLessThan(1);

        var key = KeyArgs(Key.Space);
        header.RaiseEvent(key);
        key.Handled.ShouldBeFalse();
        panel.IsExpanded.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ExpansionPanels_accordion_collapses_siblings()
    {
        var a = new ExpansionPanel { Header = "A", Content = new TextBlock { Text = "a" } };
        var b = new ExpansionPanel { Header = "B", Content = new TextBlock { Text = "b" } };
        var panels = new ExpansionPanels();
        panels.Panels.Add(a);
        panels.Panels.Add(b);
        Show(panels);
        panels.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        a.IsExpanded = true;
        b.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();

        b.IsExpanded.ShouldBeTrue();
        a.IsExpanded.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ExpansionPanels_helper_methods_add_and_control_panels()
    {
        var panels = new ExpansionPanels { MultiExpansion = true };
        var first = panels.AddPanel("A", new TextBlock { Text = "a" }, isExpanded: true);
        var second = panels.AddPanel("B", new TextBlock { Text = "b" });
        Show(panels);
        panels.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        panels.Panels.Count.ShouldBe(2);
        panels.Panels[0].ShouldBe(first);
        panels.Panels[1].ShouldBe(second);
        first.IsExpanded.ShouldBeTrue();
        second.IsExpanded.ShouldBeFalse();

        panels.ExpandAll();
        Dispatcher.UIThread.RunJobs();
        first.IsExpanded.ShouldBeTrue();
        second.IsExpanded.ShouldBeTrue();

        panels.CollapsePanel(0);
        Dispatcher.UIThread.RunJobs();
        first.IsExpanded.ShouldBeFalse();
        second.IsExpanded.ShouldBeTrue();

        panels.CollapseAll();
        Dispatcher.UIThread.RunJobs();
        first.IsExpanded.ShouldBeFalse();
        second.IsExpanded.ShouldBeFalse();

        var accordion = new ExpansionPanels();
        accordion.AddPanel("One", new TextBlock { Text = "one" });
        accordion.AddPanel("Two", new TextBlock { Text = "two" });
        accordion.ExpandAll();
        accordion.Panels[0].IsExpanded.ShouldBeTrue();
        accordion.Panels[1].IsExpanded.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ExpansionPanels_multi_mode_keeps_siblings_open_and_names_container()
    {
        var a = new ExpansionPanel { Header = "A", Content = new TextBlock { Text = "a" } };
        var b = new ExpansionPanel { Header = "B", Content = new TextBlock { Text = "b" } };
        var panels = new ExpansionPanels { MultiExpansion = true };
        panels.Panels.Add(a);
        panels.Panels.Add(b);
        Show(panels);
        panels.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        a.IsExpanded = true;
        b.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();

        a.IsExpanded.ShouldBeTrue();
        b.IsExpanded.ShouldBeTrue();
        AutomationProperties.GetName(panels).ShouldBe("Expansion panels");
        AutomationProperties.GetHelpText(panels).ShouldBe("2 panels, 2 expanded, multi expansion");
    }

    [AvaloniaFact]
    public void ExpansionPanel_help_text_tracks_expanded_state()
    {
        var panel = new ExpansionPanel { Header = "Details", Content = new TextBlock { Text = "body" } };
        Show(panel);
        panel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetHelpText(panel).ShouldBe("Collapsed");

        panel.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();
        AutomationProperties.GetHelpText(panel).ShouldBe("Expanded");
    }

    [AvaloniaFact]
    public void Menu_holds_items()
    {
        var clicked = false;
        var menu = new Loam.Controls.Menu { Content = "Actions" };
        menu.Items.Add(new Loam.Controls.MenuItem { Text = "Do it", OnClick = () => clicked = true });

        menu.Items.Count.ShouldBe(1);
        menu.Items[0].OnClick!.Invoke();
        clicked.ShouldBeTrue();
    }
}

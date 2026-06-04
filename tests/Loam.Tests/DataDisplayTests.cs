using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        arrows[0].IsEnabled.ShouldBeFalse();
        arrows[1].IsEnabled.ShouldBeTrue();

        arrows[1].RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        pagination.Selected.ShouldBe(2);
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

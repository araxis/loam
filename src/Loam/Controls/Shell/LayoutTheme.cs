using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Layout"/> theme: a dock panel with the app bar on top, the drawer on the left, and the content filling the rest.</summary>
internal static class LayoutTheme
{
    public static ControlTheme Create() =>
        new(typeof(Layout))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Layout> BuildTemplate() =>
        new((layout, scope) =>
        {
            var appBar = new ContentPresenter().Named("PART_AppBar", scope);
            DockPanel.SetDock(appBar, Dock.Top);
            appBar.Bind(ContentPresenter.ContentProperty, layout.GetObservable(Layout.AppBarProperty));

            var drawer = new ContentPresenter().Named("PART_Drawer", scope);
            drawer.Bind(ContentPresenter.ContentProperty, layout.GetObservable(Layout.DrawerProperty));

            var content = new ContentPresenter().Named("PART_ContentPresenter", scope);
            content.Bind(ContentPresenter.ContentProperty, layout.GetObservable(ContentControl.ContentProperty));
            content.Bind(ContentPresenter.ContentTemplateProperty, layout.GetObservable(ContentControl.ContentTemplateProperty));

            var scrim = new Border
            {
                Background = new ImmutableSolidColorBrush(Color.FromArgb(0x66, 0, 0, 0)),
                IsVisible = false,
            }.Named("PART_DrawerScrim", scope);
            scrim.PointerPressed += (_, _) =>
            {
                if (drawer.Content is Drawer { Mode: DrawerMode.Temporary, CloseOnScrimClick: true } current)
                {
                    current.Open = false;
                }
            };

            var body = new DrawerLayoutPanel
            {
                Children = { content, scrim, drawer },
            };

            var dock = new DockPanel
            {
                LastChildFill = true,
                Children = { appBar, body },
            };

            var root = new Border { Child = dock }.Named("PART_Root", scope);
            root.Bind(Border.BackgroundProperty, layout.GetResourceObservable(LoamTokens.Background));
            return root;
        });

    private sealed class DrawerLayoutPanel : Panel
    {
        private Drawer? _observedDrawer;

        protected override Size MeasureOverride(Size availableSize)
        {
            var content = Content;
            var scrim = Scrim;
            var drawer = DrawerPresenter;
            Observe(drawer?.Content as Drawer);

            drawer?.Measure(availableSize);
            var drawerWidth = DockedDrawerWidth(drawer);
            var contentSize = new Size(AvailableWidth(availableSize.Width, drawerWidth), availableSize.Height);
            content?.Measure(contentSize);
            scrim?.Measure(availableSize);

            var desiredWidth = double.IsInfinity(availableSize.Width)
                ? (content?.DesiredSize.Width ?? 0d) + drawerWidth
                : availableSize.Width;
            var desiredHeight = double.IsInfinity(availableSize.Height)
                ? Math.Max(content?.DesiredSize.Height ?? 0d, drawer?.DesiredSize.Height ?? 0d)
                : availableSize.Height;
            return new Size(desiredWidth, desiredHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var content = Content;
            var scrim = Scrim;
            var drawer = DrawerPresenter;
            var currentDrawer = drawer?.Content as Drawer;
            Observe(currentDrawer);
            var temporary = currentDrawer?.Mode == DrawerMode.Temporary;
            var open = currentDrawer?.Open == true;
            var drawerWidth = Math.Max(0d, drawer?.DesiredSize.Width ?? currentDrawer?.Width ?? 0d);
            var dockedWidth = temporary ? 0d : drawerWidth;

            content?.Arrange(new Rect(dockedWidth, 0, AvailableWidth(finalSize.Width, dockedWidth), finalSize.Height));

            if (scrim is not null)
            {
                scrim.IsVisible = temporary && open && currentDrawer?.ShowScrim == true;
                scrim.Arrange(scrim.IsVisible ? new Rect(finalSize) : new Rect());
            }

            drawer?.Arrange(new Rect(0, 0, temporary && !open ? 0d : drawerWidth, finalSize.Height));
            return finalSize;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            InvalidateArrange();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            Observe(null);
            base.OnDetachedFromVisualTree(e);
        }

        private Control? Content => Children.Count > 0 ? Children[0] : null;

        private Border? Scrim => Children.Count > 1 ? Children[1] as Border : null;

        private ContentPresenter? DrawerPresenter => Children.Count > 2 ? Children[2] as ContentPresenter : null;

        private static double DockedDrawerWidth(ContentPresenter? drawer)
        {
            if (drawer?.Content is not Drawer { Mode: DrawerMode.Docked })
            {
                return 0d;
            }

            return Math.Max(0d, drawer.DesiredSize.Width);
        }

        private static double AvailableWidth(double width, double used) =>
            double.IsInfinity(width) ? double.PositiveInfinity : Math.Max(0d, width - used);

        private void Observe(Drawer? drawer)
        {
            if (ReferenceEquals(_observedDrawer, drawer))
            {
                return;
            }

            if (_observedDrawer is not null)
            {
                _observedDrawer.PropertyChanged -= OnDrawerPropertyChanged;
            }

            _observedDrawer = drawer;
            if (_observedDrawer is not null)
            {
                _observedDrawer.PropertyChanged += OnDrawerPropertyChanged;
            }
        }

        private void OnDrawerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Drawer.OpenProperty || e.Property == Drawer.MiniProperty ||
                e.Property == Drawer.DrawerWidthProperty || e.Property == Drawer.MiniWidthProperty ||
                e.Property == Drawer.ModeProperty || e.Property == Drawer.ShowScrimProperty)
            {
                InvalidateMeasure();
                InvalidateArrange();
            }
        }
    }
}

namespace Loam;

/// <summary>
/// A curated set of icon path strings (24×24 view box), mirroring the reference API's
/// <c>Icons.Material.Filled.*</c> structure (ADR-0006). This core set covers common needs; a full
/// generated icon pack ships later as the separate <c>Loam.Icons</c> package.
/// </summary>
public static class Icons
{
    /// <summary>Built-in icon catalog.</summary>
    public static class Material
    {
        /// <summary>Filled (solid) icons.</summary>
        public static class Filled
        {
            /// <summary>Home.</summary>
            public const string Home = "M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z";

            /// <summary>Hamburger menu.</summary>
            public const string Menu = "M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z";

            /// <summary>Search.</summary>
            public const string Search = "M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z";

            /// <summary>Close / X.</summary>
            public const string Close = "M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z";

            /// <summary>Check mark.</summary>
            public const string Check = "M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z";

            /// <summary>Add / plus.</summary>
            public const string Add = "M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z";

            /// <summary>Delete / trash.</summary>
            public const string Delete = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z";

            /// <summary>Favorite / heart.</summary>
            public const string Favorite = "M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z";

            /// <summary>Favorite outline (unfilled heart).</summary>
            public const string FavoriteBorder = "M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z";

            /// <summary>Star.</summary>
            public const string Star = "M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z";

            /// <summary>Dashboard tiles.</summary>
            public const string Dashboard = "M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z";

            /// <summary>Typography scale.</summary>
            public const string FormatSize = "M9 4v3h5v12h3V7h5V4H9zM3 12h3v7h3v-7h3V9H3v3z";

            /// <summary>Icon/widget grid.</summary>
            public const string Widgets = "M13 13h8v8h-8v-8zM3 13h8v8H3v-8zM3 3h8v8H3V3zm10 0h8v8h-8V3z";

            /// <summary>Horizontal divider.</summary>
            public const string HorizontalRule = "M4 11h16v2H4z";

            /// <summary>Label/tag.</summary>
            public const string Label = "M17.63 5.84C17.27 5.33 16.67 5 16 5L5 5.01C3.9 5.01 3 5.9 3 7v10c0 1.1.9 1.99 2 1.99L16 19c.67 0 1.27-.33 1.63-.84L22 12l-4.37-6.16z";

            /// <summary>Notification badge.</summary>
            public const string Notifications = "M12 22c1.1 0 2-.9 2-2h-4c0 1.1.89 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5S10.5 3.17 10.5 4v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z";

            /// <summary>People group.</summary>
            public const string Groups = "M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5s-3 1.34-3 3 1.34 3 3 3zM8 11c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5C15 14.17 10.33 13 8 13zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z";

            /// <summary>Pointer tap.</summary>
            public const string TouchApp = "M9 11.24V7.5C9 6.12 10.12 5 11.5 5S14 6.12 14 7.5v3.74l1.84-.97c.31-.16.68-.16.99.02l2.91 1.69c.39.23.58.68.47 1.12l-1.38 5.52C18.6 19.47 17.85 20 17 20h-5.07c-.62 0-1.2-.29-1.58-.79L7.1 14.9c-.6-.8-.52-1.92.19-2.62.5-.51 1.25-.7 1.95-.51l.76.23z";

            /// <summary>Toggle switch on.</summary>
            public const string ToggleOn = "M17 7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h10c2.76 0 5-2.24 5-5s-2.24-5-5-5zm0 8c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3z";

            /// <summary>Segmented columns.</summary>
            public const string ViewWeek = "M5 4h14c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H5c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2zm0 2v12h4V6H5zm6 0v12h3V6h-3zm5 0v12h3V6h-3z";

            /// <summary>Checkbox checked.</summary>
            public const string CheckBox = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-9 14l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z";

            /// <summary>Radio button checked.</summary>
            public const string RadioButtonChecked = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 15c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5z";

            /// <summary>Tune sliders.</summary>
            public const string Tune = "M3 17v2h6v-2H3zM3 5v2h10V5H3zm10 16v-2h8v-2h-8v-2h-2v6h2zM7 9v2H3v2h4v2h2V9H7zm14 4v-2H11v2h10zm-6-4h2V7h4V5h-4V3h-2v6z";

            /// <summary>Document/article.</summary>
            public const string Article = "M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zM13 9V3.5L18.5 9H13zM8 13h8v2H8v-2zm0 4h8v2H8v-2zm0-8h4v2H8V9z";

            /// <summary>Color palette.</summary>
            public const string Palette = "M12 3C7.03 3 3 6.58 3 11c0 3.31 2.69 6 6 6h1.5c.83 0 1.5.67 1.5 1.5S12.67 20 13.5 20H15c4.42 0 8-3.58 8-8.5C23 6.81 18.97 3 12 3zM6.5 11C5.67 11 5 10.33 5 9.5S5.67 8 6.5 8 8 8.67 8 9.5 7.33 11 6.5 11zM9.5 7C8.67 7 8 6.33 8 5.5S8.67 4 9.5 4 11 4.67 11 5.5 10.33 7 9.5 7zm5 0c-.83 0-1.5-.67-1.5-1.5S13.67 4 14.5 4 16 4.67 16 5.5 15.33 7 14.5 7zm3 4c-.83 0-1.5-.67-1.5-1.5S16.67 8 17.5 8 19 8.67 19 9.5 18.33 11 17.5 11z";

            /// <summary>Information circle.</summary>
            public const string Info = "M11 17h2v-6h-2v6zm1-14C6.48 3 2 7.48 2 13s4.48 10 10 10 10-4.48 10-10S17.52 3 12 3zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm-1-10h2V7h-2v4z";

            /// <summary>Progress activity.</summary>
            public const string ProgressActivity = "M12 2v4c3.31 0 6 2.69 6 6h4c0-5.52-4.48-10-10-10zm0 18c-4.41 0-8-3.59-8-8S7.59 4 12 4V2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10h-4c0 4.41-3.59 8-8 8z";

            /// <summary>Content placeholders.</summary>
            public const string ViewHeadline = "M4 5h16v4H4V5zm0 5h16v4H4v-4zm0 5h12v4H4v-4z";

            /// <summary>Layer stack.</summary>
            public const string Layers = "M11.99 18.54l-7.37-5.73L3 14.07l9 7 9-7-1.63-1.27-7.38 5.74zM12 16l7.36-5.73L21 9l-9-7-9 7 1.63 1.27L12 16z";

            /// <summary>Message bubble.</summary>
            public const string Chat = "M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z";

            /// <summary>Table grid.</summary>
            public const string Table = "M3 3h18v18H3V3zm2 2v4h16V5H5zm0 6v8h5v-8H5zm7 0v8h9v-8h-9z";

            /// <summary>Grid view.</summary>
            public const string GridView = "M3 3h8v8H3V3zm10 0h8v8h-8V3zM3 13h8v8H3v-8zm10 0h8v8h-8v-8z";

            /// <summary>Tree structure.</summary>
            public const string AccountTree = "M22 11V3h-8v3H8V3H0v8h8V8h6v13h8v-8h-6v-2h6z";

            /// <summary>Tabs.</summary>
            public const string Tabs = "M3 3h10v6H3V3zm0 8h18v10H3V11zm12-8h6v6h-6V3z";

            /// <summary>Timeline.</summary>
            public const string Timeline = "M12 2c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 8c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 8c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zM11 6h2v4h-2V6zm0 8h2v4h-2v-4z";

            /// <summary>Carousel frames.</summary>
            public const string ViewCarousel = "M19 5h-1V4c0-.55-.45-1-1-1H7c-.55 0-1 .45-1 1v1H5c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h1v1c0 .55.45 1 1 1h10c.55 0 1-.45 1-1v-1h1c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zM6 17H5V7h1v10zm12 0H8V5h10v12zm1 0h-1V7h1v10z";

            /// <summary>Checklist.</summary>
            public const string Checklist = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-10 14l-4-4 1.41-1.41L9 14.17l7.59-7.59L18 8l-9 9z";

            /// <summary>More options.</summary>
            public const string MoreHoriz = "M6 10c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm6 0c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm6 0c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z";

            /// <summary>Route/breadcrumb path.</summary>
            public const string AltRoute = "M9.78 11.16l-1.42 1.42L5 9.83V20H3V9.83L.64 12.58-.78 11.16 4 6.34l4.78 4.82zM14 4h7v7h-2V7.41l-9.83 9.83-1.41-1.41L17.59 6H14V4z";

            /// <summary>Open external link.</summary>
            public const string OpenInNew = "M14 3v2h3.59l-9.83 9.83 1.41 1.41L19 6.41V10h2V3h-7zM5 5h5V3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2v-5h-2v5H5V5z";

            /// <summary>Horizontal spacing.</summary>
            public const string SwapHoriz = "M6.99 11L3 7l3.99-4v3H14v2H6.99v3zM17.01 13H10v2h7.01v3L21 14l-3.99-4v3z";

            /// <summary>Visibility hidden.</summary>
            public const string VisibilityOff = "M12 6.5c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92C21.07 14.98 22 13.33 22 11.5 20.27 7.39 16 4.5 12 4.5c-1.27 0-2.49.24-3.62.67l2.16 2.16c.47-.2.98-.33 1.46-.33zM2.71 3.16L1.39 4.48l2.55 2.55C2.38 8.25 1.27 9.94 1 11.5c1.73 4.11 6 7 11 7 1.55 0 3.03-.3 4.38-.84l3.17 3.17 1.32-1.32L2.71 3.16z";

            /// <summary>App window.</summary>
            public const string WebAsset = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM5 8h14v11H5V8z";

            /// <summary>Pie chart.</summary>
            public const string PieChart = "M11 2v20c-5.05-.5-9-4.76-9-10s3.95-9.5 9-10zm2 0v9h9c-.47-4.84-4.16-8.53-9-9zm0 11v9c4.84-.47 8.53-4.16 9-9h-9z";

            /// <summary>Bar chart.</summary>
            public const string BarChart = "M5 19h3V9H5v10zm6 0h3V5h-3v14zm6 0h3v-7h-3v7z";

            /// <summary>Line chart.</summary>
            public const string ShowChart = "M3.5 18.49l6-6.01 4 4L22 6.92 20.59 5.5l-7.09 7.97-4-4L2 17l1.5 1.49z";

            /// <summary>Settings / gear.</summary>
            public const string Settings = "M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58c.18-.14.23-.41.12-.61l-1.92-3.32c-.12-.22-.37-.29-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94l-.36-2.54c-.04-.24-.24-.41-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87c-.12.21-.08.47.12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58c-.18.14-.23.41-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32c.12-.22.07-.47-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z";

            /// <summary>Arrow back.</summary>
            public const string ArrowBack = "M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z";

            /// <summary>Arrow forward.</summary>
            public const string ArrowForward = "M12 4l-1.41 1.41L16.17 11H4v2h12.17l-5.58 5.59L12 20l8-8z";

            /// <summary>Edit / pencil.</summary>
            public const string Edit = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z";

            /// <summary>Person / account.</summary>
            public const string Person = "M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z";

            /// <summary>Expand more (chevron down).</summary>
            public const string ExpandMore = "M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z";

            /// <summary>Expand less (chevron up).</summary>
            public const string ExpandLess = "M12 8l-6 6 1.41 1.41L12 10.83l4.59 4.58L18 14z";

            /// <summary>Calendar (today).</summary>
            public const string CalendarToday = "M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z";

            /// <summary>Clock / schedule.</summary>
            public const string Schedule = "M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z";

            /// <summary>Cloud upload.</summary>
            public const string CloudUpload = "M19.35 10.04C18.67 6.59 15.64 4 12 4 9.11 4 6.6 5.64 5.35 8.04 2.34 8.36 0 10.91 0 14c0 3.31 2.69 6 6 6h13c2.76 0 5-2.24 5-5 0-2.64-2.05-4.78-4.65-4.96zM14 13v4h-4v-4H7l5-5 5 5h-3z";
        }
    }
}

# Findings — MudBlazor theme defaults (sourced 2026-06-02)

Values read from MudBlazor **v9.5.0** source via the GitHub API
(`src/MudBlazor/Themes/Models/`). These are the faithful defaults Loam mirrors in
`LoamPalette`, `LoamTypography`, `LoamShadows`, `LoamLayout`, `LoamZIndex`.

## Palette — light (base `Palette.cs`)

Primary `#594AE2` · Secondary `#FF4081` (Pink A200) · Tertiary `#1EC8A5` · Info `#2196F3` (Blue 500)
· Success `#00C853` (Green A700) · Warning `#FF9800` (Orange 500) · Error `#F44336` (Red 500) ·
Dark `#424242` (Gray 800). All ContrastText = white.
TextPrimary `#424242` · TextSecondary `rgba(0,0,0,.54)` · TextDisabled `.38`.
Action: Default `.54` · Disabled `.26` · DisabledBackground `.12` (black alphas).
Background `#FFF` · BackgroundGray `#F5F5F5` · Surface `#FFF` · DrawerBackground `#FFF` ·
DrawerText `#424242` · DrawerIcon `#616161` · AppbarBackground `#594AE2` · AppbarText `#FFF`.
LinesDefault `rgba(0,0,0,.12)` · LinesInputs `#BDBDBD` · TableLines `#E0E0E0` ·
TableStriped `.02` · TableHover `.04` · Divider `#E0E0E0` · DividerLight `rgba(0,0,0,.8)` ·
Skeleton `.11`. Grays: Default `#9E9E9E`, Light `#BDBDBD`, Lighter `#E0E0E0`, Dark `#757575`,
Darker `#616161`. OverlayDark `rgba(#212121,.5)` · OverlayLight `rgba(#fff,.5)`.
HoverOpacity `0.06` · RippleOpacity `0.1`.

## Palette — dark (`PaletteDark.cs` overrides)

Black/Dark/BackgroundGray/DrawerBackground/AppbarBackground `#27272F` · Primary `#776BE7` ·
Info `#3299FF` · Success `#0BBA83` · Warning `#FFA800` · Error `#F64E62` · Background `#32333D` ·
Surface `#373740` · ActionDefault `#ADADB1`. Text/lines/etc. are white alphas:
TextPrimary `.70` · TextSecondary/Drawer `.50` · TextDisabled `.20` · ActionDisabled `.26` ·
ActionDisabledBackground/LinesDefault/TableLines/Divider `.12` · LinesInputs `.30` ·
TableStriped `.20` · DividerLight `.06` · Skeleton `.11` · AppbarText `.70`.
(Secondary, Tertiary, grays, overlays inherit light.)

## Typography (`Typography.cs`) — FontFamily `["Roboto","Helvetica","Arial","sans-serif"]`

| Style | size(rem→px) | weight | lineHeight | letterSpacing(em) |
| --- | --- | --- | --- | --- |
| Default | .875→14 | 400 | 1.43 | .01071 |
| H1 | 6→96 | 300 | 1.167 | -.01562 |
| H2 | 3.75→60 | 300 | 1.2 | -.00833 |
| H3 | 3→48 | 400 | 1.167 | 0 |
| H4 | 2.125→34 | 400 | 1.235 | .00735 |
| H5 | 1.5→24 | 400 | 1.334 | 0 |
| H6 | 1.25→20 | 500 | 1.6 | .0075 |
| Subtitle1 | 1→16 | 400 | 1.75 | .00938 |
| Subtitle2 | .875→14 | 500 | 1.57 | .00714 |
| Body1 | 1→16 | 400 | 1.5 | .00938 |
| Body2 | .875→14 | 400 | 1.43 | .01071 |
| Button† | .875→14 | 500 | 1.75 | .02857 (uppercase) |
| Caption† | .75→12 | 400 | 1.66 | .03333 |
| Overline† | .75→12 | 400 | 2.66 | .08333 (uppercase) |

† Button/Caption/Overline rows were below the scrape window; filled from the standard MUI defaults
MudBlazor uses. **TODO:** re-verify these three against source when convenient.

## Shadows (`Shadow.cs`)

26 levels (0 = none; 1–24 = MUI elevation set; 25 = a custom soft shadow). Each is a 3-layer CSS
box-shadow (`rgba(0,0,0,.2)` umbra / `.14` penumbra / `.12` ambient). Loam stores the exact strings
and converts via `LoamShadows.ParseCss` → Avalonia `BoxShadows`.

## Layout (`LayoutProperties.cs`) / Z-index (`Z-Index.cs`)

Radius `4px` · DrawerWidth(L/R) `240px` · DrawerMiniWidth `56px` · AppbarHeight `64px`.
Drawer `1100` · Popover `1200` · AppBar `1300` · Dialog `1400` · Snackbar `1500` · Tooltip `1600`.

## Conversion notes (rgba → ARGB used in Loam)

`.54→0x8A · .50→0x80 · .38→0x61 · .30→0x4D · .26→0x42 · .20→0x33 · .14→0x24 · .12→0x1F ·
.11→0x1C · .70→0xB3 · .06→0x0F · .04→0x0A · .02→0x05` (alpha = round(opacity×255)).

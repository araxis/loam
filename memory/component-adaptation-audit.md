# Component Adaptation Audit

This tracker records the full-library pass for Loam 1.3.0. Each component should be checked for
rest, hover, focus, pressed, disabled, error, selected, open, loading, density, shape, typography,
color roles, elevation, motion, keyboard, and automation behavior.

| Area | Components | Status | Notes |
| --- | --- | --- | --- |
| Theme tokens | Theme data, tokens, projection | In progress | Additive shape, state, motion, and field metric tokens. |
| Field surfaces | Field, TextField, NumericField, Select, Autocomplete, pickers | In progress | Shared label, outline, focus, popup, and automation behavior. |
| Actions | Button, IconButton, ToggleIconButton, Fab, ButtonGroup | Pending | State layer, focus, touch target, disabled contrast review. |
| Selection | CheckBox, Switch, Radio, Slider, Rating, ToggleGroup, Chip, ChipSet | Pending | Selected, hover, focus, keyboard, and hit target review. |
| Surfaces and feedback | Paper, Card, Dialog, Menu, Popover, Snackbar, Tooltip, Overlay, Alert, progress, Skeleton, Collapse | Pending | Elevation, shape, scrim, placement, keyboard close, motion review. |
| Navigation | AppBar, Drawer, NavLink, NavGroup, Breadcrumbs, Link, Tabs, Stepper, Pagination | Pending | Active, selected, focus, density, reveal, and keyboard review. |
| Data display | List, Table, DataGrid, TreeView, ExpansionPanel, Timeline, Carousel | Pending | Row states, dividers, density, keyboard, selected state review. |
| Charts | Pie, Bar, Line | Pending | Theme color, typography, surface, and legend/tooltip readiness review only. |
| Gallery and docs | Gallery, docs site | Pending | Component pages with real samples, state coverage, and code snippets. |

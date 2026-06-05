# Component Adaptation Audit

This tracker records the full-library pass for Loam 2.0.0. Each component should be checked for
rest, hover, focus, pressed, disabled, error, selected, open, loading, density, shape, typography,
color roles, elevation, motion, keyboard, and automation behavior.

| Area | Components | Status | Notes |
| --- | --- | --- | --- |
| Theme tokens | Theme data, tokens, projection | Done | Role color schemes, compatibility palettes, typography roles, shape, spacing, stroke, density, elevation, state, motion, and field metrics project through tokens. |
| Field surfaces | Field, TextField, NumericField, Select, Autocomplete, pickers | In progress | Shared label, outline, focus, popup, and automation behavior; field metrics now align with the new baseline. |
| Actions | Button, IconButton, ToggleIconButton, Fab, ButtonGroup | In progress | Button/Fab/IconButton consume role colors, density padding, state layers, focus stroke, and tokenized ripple. |
| Selection | CheckBox, Switch, Radio, Slider, Rating, ToggleGroup, Chip, ChipSet | Pending | Selected, hover, focus, keyboard, and hit target review. |
| Surfaces and feedback | Paper, Card, Dialog, Menu, Popover, Snackbar, Tooltip, Overlay, Alert, progress, Skeleton, Collapse | In progress | Paper now resolves tonal elevation plus shadows; scrim/motion queue behavior still needs deeper pass. |
| Navigation | AppBar, Drawer, NavLink, NavGroup, Breadcrumbs, Link, Tabs, Stepper, Pagination | Pending | Active, selected, focus, density, reveal, and keyboard review. |
| Data display | List, Table, DataGrid, TreeView, ExpansionPanel, Timeline, Carousel | In progress | Table/DataGrid density padding is tokenized; remaining row/keyboard/selected audits continue. |
| Charts | Pie, Bar, Line | Pending | Theme color, typography, surface, and legend/tooltip readiness review only. |
| Gallery and docs | Gallery, docs site | In progress | Design-system page now shows role colors, typography roles, shape, spacing, motion, and tonal elevation. |

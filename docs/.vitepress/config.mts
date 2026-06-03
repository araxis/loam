import { defineConfig } from 'vitepress'

// NOTE: `base` must match the GitHub Pages path. For a project site published at
// https://<user>.github.io/<repo>/ set base to '/<repo>/'. This repo's folder is
// "MudAvalonia"; change it (or set to '/' for a user/org root site) if your repo differs.
const base = process.env.DOCS_BASE ?? '/MudAvalonia/'

// Set this to your repository URL (no git remote was configured when these docs were generated).
const repo = process.env.DOCS_REPO ?? 'https://github.com/your-org/MudAvalonia'

export default defineConfig({
  base,
  lang: 'en-US',
  title: 'Loam',
  description: 'Material-Design Avalonia controls with a MudBlazor-flavored API — authored in pure C#.',
  cleanUrls: true,
  lastUpdated: true,
  ignoreDeadLinks: true,

  head: [
    ['meta', { name: 'theme-color', content: '#594AE2' }],
    ['meta', { name: 'keywords', content: 'avalonia, mudblazor, material design, ui, controls, csharp, dotnet' }],
  ],

  markdown: {
    lineNumbers: true,
  },

  themeConfig: {
    siteTitle: 'Loam',

    nav: [
      { text: 'Guide', link: '/guide/introduction', activeMatch: '/guide/' },
      { text: 'Components', link: '/components/overview', activeMatch: '/components/' },
      {
        text: 'v1',
        items: [
          { text: 'Development Plan', link: `${repo}/blob/main/DEVELOPMENT_PLAN.md` },
          { text: 'Progress log', link: `${repo}/blob/main/memory/progress/progress-log.md` },
          { text: 'Component tracker', link: `${repo}/blob/main/memory/component-inventory.md` },
        ],
      },
    ],

    sidebar: {
      '/guide/': [
        {
          text: 'Guide',
          items: [
            { text: 'Introduction', link: '/guide/introduction' },
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Theming', link: '/guide/theming' },
            { text: 'Authoring UI in C#', link: '/guide/csharp-ui' },
          ],
        },
      ],
      '/components/': [
        {
          text: 'Components',
          items: [
            { text: 'Overview', link: '/components/overview' },
            { text: 'Display primitives', link: '/components/display' },
            { text: 'Buttons & menus', link: '/components/buttons' },
            { text: 'Surfaces & layout', link: '/components/layout' },
            { text: 'Form inputs', link: '/components/inputs' },
            { text: 'Pickers', link: '/components/pickers' },
            { text: 'Data display', link: '/components/data-display' },
            { text: 'Navigation', link: '/components/navigation' },
            { text: 'Overlays & feedback', link: '/components/overlays' },
            { text: 'Charts & effects', link: '/components/charts' },
          ],
        },
      ],
    },

    socialLinks: [
      { icon: 'github', link: repo },
    ],

    search: {
      provider: 'local',
    },

    outline: { level: [2, 3], label: 'On this page' },

    footer: {
      message: 'MIT Licensed · An independent, MudBlazor-inspired library (not affiliated with MudBlazor).',
      copyright: 'Built with Avalonia in pure C#.',
    },
  },
})

import { defineConfig } from 'vitepress'

const base = process.env.DOCS_BASE ?? '/loam/'

const repo = process.env.DOCS_REPO ?? 'https://github.com/araxis/loam'

export default defineConfig({
  base,
  lang: 'en-US',
  title: 'Loam',
  description: 'Pure C# themed controls for Avalonia applications.',
  cleanUrls: true,
  lastUpdated: true,
  ignoreDeadLinks: true,

  head: [
    ['link', { rel: 'icon', type: 'image/png', href: `${base}favicon.png` }],
    ['meta', { name: 'theme-color', content: '#594AE2' }],
    ['meta', { name: 'keywords', content: 'avalonia, ui, controls, theme, csharp, dotnet' }],
  ],

  markdown: {
    lineNumbers: true,
  },

  themeConfig: {
    logo: '/loam-icon-128.png',
    siteTitle: 'Loam',

    nav: [
      { text: 'Guide', link: '/guide/introduction', activeMatch: '/guide/' },
      { text: 'Components', link: '/components/overview', activeMatch: '/components/' },
      { text: 'Migrate', link: '/migration/v3-to-v3.1', activeMatch: '/migration/' },
      {
        text: 'Project',
        items: [
          { text: 'v3 plan', link: `${repo}/blob/main/PLAN.md` },
          { text: 'v3 review', link: `${repo}/blob/main/REVIEW.md` },
          { text: 'v2 development plan', link: `${repo}/blob/main/DEVELOPMENT_PLAN.md` },
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
            { text: 'Why Loam vs plain Avalonia', link: '/guide/why-loam' },
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Theming', link: '/guide/theming' },
            { text: 'Authoring UI in C#', link: '/guide/csharp-ui' },
            { text: 'Changelog', link: '/guide/changelog' },
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
      '/migration/': [
        {
          text: 'Migration',
          items: [
            { text: 'v3.0 → v3.1', link: '/migration/v3-to-v3.1' },
            { text: 'v2 → v3', link: '/migration/v2-to-v3' },
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
      message: 'MIT Licensed · Independent Avalonia controls.',
      copyright: 'Built with Avalonia in pure C#.',
    },
  },
})

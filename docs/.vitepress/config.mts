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
      {
        text: 'v2',
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
      message: 'MIT Licensed · Independent Avalonia controls.',
      copyright: 'Built with Avalonia in pure C#.',
    },
  },
})

import process from 'node:process'
import { defineUserConfig } from '@vuepress/cli'
import { getDirname, path } from '@vuepress/utils'
import { navbar } from './theme/navbar'
import { bccCodeTheme } from './theme/theme'

const __dirname = getDirname(import.meta.url)
const isProd = process.env.NODE_ENV === 'production'

export default defineUserConfig({
  base: '/',

  title: 'Design System',

  head: [['link', { rel: 'icon', href: '/favicon.ico' }]],

  theme: bccCodeTheme({
    logoDark: 'logoWhite.png',
    logo: 'logo.png',
    repo: 'bcc-code',
    docsRepo: 'bcc-code/bcc-design', // TODO replace with dynamic value
    docsDir: 'docs', // TODO replace with dynamic value?

    navbar,

    // sidebar
    sidebar: {
        '/': [
            {
              text: 'Design System',
              children: [
                '/README.md',
                '/tokens.md',
              ],
            },
            {
              text: 'Components',
              children: [
                '/components/README.md',
              ],
            },
          ],
    },

    editLinkText: 'Edit this page on GitHub',

    themePlugins: {
      git: isProd,
      prismjs: !isProd,
    },
  }),

  markdown: {
    importCode: {
      handleImportPath: (str) =>
        str.replace(/^@vuepress/, path.resolve(__dirname, '../../ecosystem')),
    },
  },

  plugins: [
    // only enable shiki plugin in production mode
    // isProd ? shikiPlugin({ theme: 'dark-plus' }) : [],
  ],
})

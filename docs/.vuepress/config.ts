import process from 'node:process'
import { defineUserConfig } from '@vuepress/cli'
import { defaultTheme } from '@vuepress/theme-default'
import { getDirname, path } from '@vuepress/utils'
import { navbar } from './theme/navbar'

const __dirname = getDirname(import.meta.url)
const isProd = process.env.NODE_ENV === 'production'

export default defineUserConfig({
  // set site base to default value
  base: '/',

  // configure default theme
  theme: defaultTheme({
    logoDark: "logoWhite.png",
    logo: "logo.png",
    repo: 'bcc-code',
    docsRepo: 'bcc-code/bcc-design', // TODO replace with dynamic value
    docsDir: 'docs', // TODO replace with dynamic value?

    navbar,

    // sidebar
    sidebar: {
        '/': [
            {
              text: 'BCC Design',
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
    // page meta
    editLinkText: 'Edit this page on GitHub',

    themePlugins: {
      // only enable git plugin in production mode
      git: isProd,
      // use shiki plugin in production mode instead
      prismjs: !isProd,
    },
  }),

  // configure markdown
  markdown: {
    importCode: {
      handleImportPath: (str) =>
        str.replace(/^@vuepress/, path.resolve(__dirname, '../../ecosystem')),
    },
  },

  // use plugins
  plugins: [
    // only enable shiki plugin in production mode
    // isProd ? shikiPlugin({ theme: 'dark-plus' }) : [],
  ],
})

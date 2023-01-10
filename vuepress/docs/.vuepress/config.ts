import process from 'node:process'
import { defineUserConfig } from '@vuepress/cli'
import { shikiPlugin } from '@vuepress/plugin-shiki'
import { navbar } from './theme/navbar'
import { bccCodeTheme } from './theme/theme'
import * as data from "./data.json";
import { generateSidebar } from './theme/helpers/generateSidebar'

const isProd = process.env.NODE_ENV === 'production'

export default defineUserConfig({
  base: data.base,

  title: data.title,

  head: [
    ['link', { rel: 'icon', href: `${data.base}favicon.ico` }]
  ],

  theme: bccCodeTheme({
    logoDark: 'logoWhite.png',
    logo: 'logo.png',
    repo: 'bcc-code',
    docsRepo: data.docsRepo,
    docsDir: data.docsDir,

    navbar,

    sidebar: generateSidebar(),

    editLinkText: 'Edit this page on GitHub',

    themePlugins: {
      git: isProd,
      prismjs: false,
    },
  }),

  plugins: [
    shikiPlugin({ theme: 'github-dark' }),
  ],
})
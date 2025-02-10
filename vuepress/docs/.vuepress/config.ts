import process from 'node:process'
import { defineUserConfig } from '@vuepress/cli'
import { shikiPlugin } from '@vuepress/plugin-shiki'
import { registerComponentsPlugin } from '@vuepress/plugin-register-components'
import { navbar } from './theme/navbar'
import { bccCodeTheme } from './theme/theme'
import * as data from "./data.json";
import { generateSidebar } from './theme/helpers/generateSidebar';
import { getDirname, path } from '@vuepress/utils'
import { mermaidWrapperPlugin } from 'vuepress-plugin-mermaid-wrapper'
import { viteBundler } from '@vuepress/bundler-vite'

const __dirname = import.meta.dirname || getDirname(import.meta.url)

const isProd = process.env.NODE_ENV === 'production';

const sidebar = generateSidebar();

export default defineUserConfig({
  base: data.base,

  title: data.title,
  description: data.description,

  head: [
    ['link', { rel: 'icon', href: `${data.base}favicon.ico` }]
  ],

  theme: bccCodeTheme({
    logoDark: 'logoWhite.png',
    logo: 'logo.png',
    repo: 'bcc-code',
    docsRepo: data.docsRepo,
    docsDir: data.docsDir,
    docsBranch: data.docsBranch,

    navbar,

    sidebar,

    editLinkText: 'Edit this page on GitHub',

    themePlugins: {
      git: isProd,
      prismjs: false,
    },
  }),
  
  alias: {
    '@theme/VPHomeFeatures.vue': path.resolve(__dirname, './theme/components/VPHomeFeatures.vue'),
    '@theme/VPNavbarBrand.vue': path.resolve(__dirname, './theme/components/VPNavbarBrand.vue'),
  },

  bundler: viteBundler({
    viteOptions: {},
    vuePluginOptions: {},
  }),

  plugins: [
    shikiPlugin({ theme: 'github-dark' }),
      data.autoRegisterComponents ? registerComponentsPlugin({ componentsDir: path.resolve(__dirname, `./auto-register-components`) }) : [],
      mermaidWrapperPlugin({}),
  ],
})

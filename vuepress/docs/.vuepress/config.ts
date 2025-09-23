import process from "node:process";
import { defineUserConfig } from "@vuepress/cli";
import { registerComponentsPlugin } from "@vuepress/plugin-register-components";

import theme from "./theme/theme";
import * as data from "./data.json";

import { getDirname, path } from "@vuepress/utils";
import { viteBundler } from "@vuepress/bundler-vite";
import Components from "unplugin-vue-components/vite";
import { PrimeVueResolver } from "unplugin-vue-components/resolvers";

const __dirname = getDirname(import.meta.url);

const isProd = process.env.NODE_ENV === "production";

export default defineUserConfig({
  base: data.base,

  title: data.title,
  description: data.description,

  head: [["link", { rel: "icon", href: `${data.base}favicon.ico` }]],

  theme,

  bundler: viteBundler({
    viteOptions: {
      plugins: [
        Components({
          // Auto-import PrimeVue components
          resolvers: [PrimeVueResolver()],
          // Optionally enable directives, if you want vRipple, etc.
          directives: true,
        }),
      ],
    },
    vuePluginOptions: {},
  }),

  alias: {
    "@theme-hope/HomeFeatures.vue": path.resolve(
      __dirname,
      "./components/HomeFeatures.vue"
    ),
    "@theme-hope/NavbarBrand.vue": path.resolve(
      __dirname,
      "./components/NavbarBrand.vue"
    ),
  },

  plugins: [
    data.autoRegisterComponents
      ? registerComponentsPlugin({
          componentsDir: path.resolve(__dirname, `./auto-register-components`),
        })
      : [],
  ],
});

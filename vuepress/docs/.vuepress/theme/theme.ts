import type { Theme } from '@vuepress/core'
import { defaultTheme, type DefaultThemeOptions } from '@vuepress/theme-default'
import { getDirname, path } from '@vuepress/utils';

const __dirname = getDirname(import.meta.url);

export const bccCodeTheme = (options: DefaultThemeOptions): Theme => {
  return {
    name: 'vuepress-theme-bcc-code',

    extends: defaultTheme(options),

    clientConfigFile: path.resolve(__dirname, './client.ts'),

    alias: {
      '@theme/VPHomeFeatures.vue': path.resolve(__dirname, './components/VPHomeFeatures.vue'),
      '@theme/VPNavbarBrand.vue': path.resolve(__dirname, './components/VPNavbarBrand.vue'),
    },
  }
};

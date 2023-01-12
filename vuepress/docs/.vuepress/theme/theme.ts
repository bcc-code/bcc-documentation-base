import type { Theme } from '@vuepress/core'
import { defaultTheme, type DefaultThemeOptions } from '@vuepress/theme-default'
import { getDirname, path } from '@vuepress/utils';

const __dirname = getDirname(import.meta.url);

export const bccCodeTheme = (options: DefaultThemeOptions): Theme => {
  return {
    name: 'vuepress-theme-bcc-code',
    extends: defaultTheme(options),

    alias: {
      '@theme/HomeFeatures.vue': path.resolve(__dirname, './components/HomeFeatures.vue'),
    },
  }
};

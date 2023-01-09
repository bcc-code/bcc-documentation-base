import type { Theme } from '@vuepress/core'
import { defaultTheme, type DefaultThemeOptions } from '@vuepress/theme-default'

export const bccCodeTheme = (options: DefaultThemeOptions): Theme => {
  return {
    name: 'vuepress-theme-bcc-code',
    extends: defaultTheme(options),
  }
};

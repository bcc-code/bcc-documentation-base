<script setup lang="ts">
import { ClientOnly, withBase } from '@vuepress/client'
import { computed, h } from 'vue'
import type { FunctionalComponent } from 'vue'
import { useDarkMode, useThemeLocaleData } from '@vuepress/theme-default/lib/client/composables/index'

const themeLocale = useThemeLocaleData()
const isDarkMode = useDarkMode()

const navbarBrandLogo = computed(() => {
  if (isDarkMode.value && themeLocale.value.logoDark !== undefined) {
    return themeLocale.value.logoDark
  }
  return themeLocale.value.logo
})
const NavbarBrandLogo: FunctionalComponent = () => {
  if (!navbarBrandLogo.value) return null
  const img = h('img', {
    class: 'logo',
    src: withBase(navbarBrandLogo.value),
    alt: "BCC",
  })
  if (themeLocale.value.logoDark === undefined) {
    return img
  }
  // wrap brand logo with <ClientOnly> to avoid ssr-mismatch
  // when using a different brand logo in dark mode
  return h(ClientOnly, () => img)
}
</script>

<template>
  <a href="https://developer.bcc.no">
    <NavbarBrandLogo />
  </a>
</template>

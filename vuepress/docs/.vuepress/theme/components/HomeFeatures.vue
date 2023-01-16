<script setup lang="ts">
import { usePageFrontmatter } from '@vuepress/client'
import { isArray } from '@vuepress/shared'
import { computed } from 'vue'
import type { DefaultThemeHomePageFrontmatter } from '@vuepress/shared/dist/index'

const frontmatter = usePageFrontmatter<DefaultThemeHomePageFrontmatter>()
const features = computed(() => {
  if (isArray(frontmatter.value.features)) {
    return frontmatter.value.features
  }
  return []
})
</script>

<template>
  <div v-if="features.length" class="bcc-features">
    <div v-for="feature in features" :key="feature.title" class="bcc-feature">
      <a :href="feature.link" v-if="feature.link.startsWith('http')" target="_blank" rel="noopener"><h2>{{ feature.title }}</h2></a>
      <router-link :to="feature.link ?? '/'" v-else><h2>{{ feature.title }}</h2></router-link>
      <p>{{ feature.details }}</p>
      <router-link :to="feature.link ?? '/'">Read More →</router-link>
    </div>
  </div>
</template>

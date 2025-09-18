import { defineClientConfig } from "@vuepress/client";
import NotFound from "./theme/layouts/NotFound.vue";
import PrimeVue from "primevue/config";
import Aura from "@primeuix/themes/aura";

export default defineClientConfig({
  layouts: {
    NotFound,
  },
  enhance({ app, router, siteData }) {
    app.use(PrimeVue, {
      ripple: false,
      theme: {
        preset: Aura,
        options: {
          prefix: "prime",
          darkModeSelector: "html[data-theme=dark]",
          cssLayer: false,
        },
      },
    });
  },
});

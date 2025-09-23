import { hopeTheme } from "vuepress-theme-hope";
import { getDirname, path } from "@vuepress/utils";
import navbar from "./navbar.js";
import sidebar from "./sidebar.js";
import * as data from "../data.json";
import process from "node:process";

const __dirname = getDirname(import.meta.url);
const isProd = process.env.NODE_ENV === "production";

export default hopeTheme({
  name: "vuepress-theme-bcc-code",

  home: "/",

  hostname: "https://developer.bcc.no",

  author: {
    name: "BCC",
    url: "https://developer.bcc.no",
  },

  logo: "logo.png",
  logoDark: "logoWhite.png",

  repo: "bcc-code",
  docsRepo: data.docsRepo,
  docsDir: data.docsDir,
  docsBranch: data.docsBranch,

  // navbar
  navbar,

  navbarLayout: {
    // Items on the left side (e.g., your logo or brand)
    start: ["Brand"],
    // Items in the center (e.g., navigation links)
    center: [],
    // Items on the right (e.g., search, repository link, language switcher, etc.)
    end: ["Search", "Links", "Repo", "Darkmode", "Outlook"],
  },

  navbarTitle: "",

  // sidebar
  sidebar,

  footer: "BCC Developer Portal",

  displayFooter: true,

  clientConfigFile: path.resolve(__dirname, "./client.ts"),

  encrypt: {
    config: {
      "/demo/encrypt.html": {
        hint: "Password: 1234",
        password: "1234",
      },
    },
  },

  metaLocales: {
    editLink: "Edit this page on GitHub",
  },

  // These features are enabled for demo, only preserve features you need here
  markdown: {
    align: true,
    attrs: true,
    codeTabs: true,
    component: true,
    demo: true,
    figure: true,
    gfm: true,
    imgLazyload: true,
    imgSize: true,
    include: true,
    mark: true,
    plantuml: true,
    spoiler: true,
    stylize: [
      {
        matcher: "Recommended",
        replacer: ({ tag }) => {
          if (tag === "em")
            return {
              tag: "Badge",
              attrs: { type: "tip" },
              content: "Recommended",
            };
        },
      },
    ],
    sub: true,
    sup: true,
    tabs: true,
    tasklist: true,
    vPre: true,
    git: isProd,
    prismjs: true,

    highlighter: {
      type: "shiki",
    },

    // uncomment these if you need TeX support
    // math: {
    //   // install katex before enabling it
    //   type: "katex",
    //   // or install mathjax-full before enabling it
    //   type: "mathjax",
    // },

    // install chart.js before enabling it
    // chartjs: true,

    // install echarts before enabling it
    // echarts: true,

    // install flowchart.ts before enabling it
    // flowchart: true,

    // install mermaid before enabling it
    mermaid: true,

    // playground: {
    //   presets: ["ts", "vue"],
    // },

    // install @vue/repl before enabling it
    // vuePlayground: true,

    // install sandpack-vue3 before enabling it
    // sandpack: true,

    // install @vuepress/plugin-revealjs and uncomment these if you need slides
    // revealjs: {
    //   plugins: ["highlight", "math", "search", "notes", "zoom"],
    // },
  },

  plugins: {
    // // Note: This is for testing ONLY!
    // // You MUST generate and use your own comment service in production.
    // comment: {
    //   provider: "Giscus",
    //   repo: "vuepress-theme-hope/giscus-discussions",
    //   repoId: "R_kgDOG_Pt2A",
    //   category: "Announcements",
    //   categoryId: "DIC_kwDOG_Pt2M4COD69",
    // },

    components: {
      components: ["Badge", "VPCard"],
    },

    icon: {
      prefix: "fa6-solid:",
    },

    search: {
      locales: {
        "/": {
          placeholder: "Search",
        },
      },

      getExtraFields: (page) => {
        const extraFields = [];

        if (page.content) {
          extraFields.push(page.content);
        }

        return extraFields;
      },
    },

    redirect: {
      config: (app) => {
        const redirects: Record<string, string> = {};

        app.pages.forEach((page) => {
          if (page.path.endsWith("/") && page.path !== "/") {
            const pathWithoutSlash = page.path.slice(0, -1);
            redirects[pathWithoutSlash] = page.path;
          }
        });

        return redirects;
      },
    },

    // install @vuepress/plugin-pwa and uncomment these if you want a PWA
    // pwa: {
    //   favicon: "/favicon.ico",
    //   cacheHTML: true,
    //   cacheImage: true,
    //   appendBase: true,
    //   apple: {
    //     icon: "/assets/icon/apple-icon-152.png",
    //     statusBarColor: "black",
    //   },
    //   msTile: {
    //     image: "/assets/icon/ms-icon-144.png",
    //     color: "#ffffff",
    //   },
    //   manifest: {
    //     icons: [
    //       {
    //         src: "/assets/icon/chrome-mask-512.png",
    //         sizes: "512x512",
    //         purpose: "maskable",
    //         type: "image/png",
    //       },
    //       {
    //         src: "/assets/icon/chrome-mask-192.png",
    //         sizes: "192x192",
    //         purpose: "maskable",
    //         type: "image/png",
    //       },
    //       {
    //         src: "/assets/icon/chrome-512.png",
    //         sizes: "512x512",
    //         type: "image/png",
    //       },
    //       {
    //         src: "/assets/icon/chrome-192.png",
    //         sizes: "192x192",
    //         type: "image/png",
    //       },
    //     ],
    //     shortcuts: [
    //       {
    //         name: "Demo",
    //         short_name: "Demo",
    //         url: "/demo/",
    //         icons: [
    //           {
    //             src: "/assets/icon/guide-maskable.png",
    //             sizes: "192x192",
    //             purpose: "maskable",
    //             type: "image/png",
    //           },
    //         ],
    //       },
    //     ],
    //   },
    // },
  },
});

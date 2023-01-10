---
order: 20
---

# Extra available frontmatter

To make it easy to control the order of pages in the sidebar, you can optionally add an `order` property to the page's [frontmatter](https://v2.vuepress.vuejs.org/guide/page.html#frontmatter).

```md
---
order: 1
---
```

By default, any `README.md` or `index.md` file will get an order of `-1`, while files without an order will be sorted alphabetically.

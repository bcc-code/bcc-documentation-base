---
order: 10
---
# VuePress

The documentation sites (like this one) use a custom [VuePress 2](https://v2.vuepress.vuejs.org/guide/) theme, based on the default theme. This means that any [Markdown plugins](https://v2.vuepress.vuejs.org/guide/markdown.html) used by VuePress and its default theme can also be used in your documentation.

## Sidebar navigation
This theme will build a sidebar from the folder structure in your `docs` folder, with titles derived from the `title` property in the frontmatter or the primary heading in each Markdown file.

To make it easy to control the order of pages in the sidebar, you can optionally add an `order` property to the page's [frontmatter](https://v2.vuepress.vuejs.org/guide/page.html#frontmatter).

```md
---
order: 1
---
```

By default, any `README.md` or `index.md` file will get an order of `-1`, while files without an order will be sorted alphabetically.

::: tip
Increment the order by tens, so from 10 to 20 to 30, to enable you to easily insert another file in the middle later without having to update all the other files.
:::

Any subfolders will be added as a separate heading to the sidebar, with the name of the subfolder rendered in Title Case, and without any dashes `-` or underscores `_`.

## Writing Markdown
Be sure to check the VuePress [documentation on Markdown](https://v2.vuepress.vuejs.org/guide/markdown.html) to view all syntax extensions on normal markdown available. 

### Custom containers
A powerful feature in the VuePress theme are the [custom containers](https://v2.vuepress.vuejs.org/reference/default-theme/markdown.html), which enable you to render blocks like this:

```md
::: tip
This is a tip
:::
```

::: tip
This is a tip
:::

Besides `tip`, `warning` and `danger` can also be used.

Furthermore, a `details` block allows you to create a collapsable block. This can be helpful to not clutter the page when having a large amount of code for example. In addition, by adding a text after the type of the custom container you can customize the title of it:

```md
::: details Custom title
Collapsable content.

`With code`
:::
```

::: details Custom title
Collapsable content.

```
With code
```
:::

### Badges
The default theme also includes a [Badge component](https://v2.vuepress.vuejs.org/reference/default-theme/components.html) by default.

```md
- VuePress - <Badge type="tip" text="v2" vertical="top" />
- VuePress - <Badge type="warning" text="v2" vertical="middle" />
- VuePress - <Badge type="danger" text="v2" vertical="bottom" />
```

- VuePress - <Badge type="tip" text="v2" vertical="top" />
- VuePress - <Badge type="warning" text="v2" vertical="middle" />
- VuePress - <Badge type="danger" text="v2" vertical="bottom" />

---
order: 5
---

# Action Reference
The `bcc-code/bcc-documentation-base` GitHub Action used for building the VuePress theme accepts the following inputs:

## Basic
### title <Badge type="tip" text="required" vertical="middle" />
Title of the generated website

Type: `string`

Default: `${{ github.event.repository.name }}`

### description <Badge type="tip" text="required" vertical="middle" />
Description of the generated website, used for the head meta tag

Type: `string`

Default: `${{ github.event.repository.description }}`

### docs-dir
Directory where the docs are located

Type: `string`

Default: `docs`
  
### branch
Which branch to use for "Edit this page on GitHub" links

Type: `string`

Default: `main`

## Advanced
### base
The base url for the website

Caution: You should normally not have to set this option, as GitHub Pages deploys to the repository url by default

Type: `string`

Default: `/${{ github.event.repository.name }}/`

### collapse-sidebar
Whether to collapse sidebar sections by default

This can be useful when your documentation has a lot of different sidebar sections

Type: `boolean`

Default: `false`

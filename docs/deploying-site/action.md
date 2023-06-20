---
order: 20
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
  
### branch
Which branch to use for "Edit this page on GitHub" links

Type: `string`

Default: `main`

## Advanced
### docs-dir
Directory where the docs are located

Type: `string`

Default: `docs`

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

### auto-register-components
Whether to automatically register Vue components

Type: `boolean`

Default: `false`

### components-dir
The directory from where Vue components should automatically be registered

Type: `string`

Default: `src/components`

### public <Badge type="warning" text="v3 or later" vertical="middle" />
Whether the documentation will be [publicly available](./README.md#make-documentation-publicly-available) or not (works only for private repositories)

Type: `boolean`

Default: `false`

### authentication <Badge type="warning" text="v5 or later" vertical="middle" />
Which provider to use for the [login flow](./README.md#Change-authentication-method-for-the-documentation) when accessing the documentation.  
Possible values are: `'github'`, `'azuread'` or `'portal'`.  

Type: `string`

Default: `github`
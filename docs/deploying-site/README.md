# Get started

### Folder structure
We recommend to use the following conventions when writing documentation:
- Use a `docs` folder for all public documentation about your project.
- Use the readme in the main folder only for information that is needed to run that project locally. Generally this kind of information is not interesting for people reading documentation about an API for example. If this information does need to be in the public documentation, link to the readme in the `docs` folder from the main readme.

### Publishing documentation
Documentation can be published to [developer.bcc.no](https://developer.bcc.no) with VuePress. For this we have created a [custom theme](./vuepress/). To convert a `docs` folder into a VuePress site, follow these steps.

::: tip HEADS UP
Ensure your folder has an `index.md` or `README.md` file in it, otherwise there will be no `index.html` located at the root of your documentation site.
:::

1. Create a new file in `.github/workflows` named `build-and-deploy-documentation.yml`, and copy the following contents to it:

::: details build-and-deploy-documentation.yml
```yml
name: Build and Deploy Documentation Site

on:
  # Runs on pushes to the docs folder targeting the default branch
  push:
    branches: ["master","main"]
    paths:
      - docs/**

  # Allows you to run this workflow manually from the Actions tab
  workflow_dispatch:

# Sets permissions of the GITHUB_TOKEN to allow deployment to GitHub Pages
permissions:
  contents: read
  id-token: write

# Allow one concurrent deployment
concurrency:
  group: "pages"
  cancel-in-progress: true

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Build documentation site
        uses: bcc-code/bcc-documentation-base@v5
        with:
          title: FILL IN
          description: FILL IN
```
:::

Be sure to set the name and description of the repository, the name will be used for the main section in the sidebar and in the `title` tag, the description will be set for the `meta` description tag.

2. Push this to the main branch of the repository and your documentation site should be deployed to developer.bcc.no/repository-name/ 🎉

#### Using a different folder for documentation
It is possible to use a different folder than `docs` for documentation. This can be configured by setting the `docs-dir` option in the action:
```yml
steps:
  - name: Build documentation site
    uses: bcc-code/bcc-documentation-base@v5
    with:
      ...
      docs-dir: documentation
```

Remember to update the `paths` in the Action file as well so the documentation is built whenever files are changed in that folder (or omit `paths` to always run this action for every commit).
```yml
on:
  push:
    branches: ["master","main"]
    paths:
      - documentation/**
```

#### Make documentation publicly available
For private repositories by default, the documentation is only visible to github members who are apart of the bcc-code organization or the collaborators of the repository.  
But if you want your documentation to be accessiable to **anyone** with a github account, than it can be configured using the `public` option in the action:
```yml
steps:
  - name: Build documentation site
    uses: bcc-code/bcc-documentation-base@v5
    with:
      ...
      public: true
```  
::: warning <del></del>
Be sure you are using **v3** or later of the action.
:::

#### Change authentication method for the documentation
By default, the documentation is only accessible by logging in using a github account.  
But if you want your documentation to be accessible using a different provider, than it can be configured using the `authentication` option in the action:
```yml
steps:
  - name: Build documentation site
    uses: bcc-code/bcc-documentation-base@v5
    with:
      ...
      authentication: 'azuread'
```  
- Currently the only available providers are: **Github**, **AzureAD** and **BCC Portal**.  
::: warning <span></span>
Be sure you are using **v5** or later of the action.
:::
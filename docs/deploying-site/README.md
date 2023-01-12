# Get started

### Folder structure
We recommend to use the following conventions when writing documentation:
- Use a `docs` folder for all public documentation about your project.
- Use the readme in the main folder only for information that is needed to run that project locally. Generally this kind of information is not interesting for people reading documentation about an API for example. If this information does need to be in the public documentation, link to the readme in the `docs` folder from the main readme.

### Publishing documentation
Documentation can be published to GitHub Pages under [developer.bcc.no](https://developer.bcc.no) with VuePress. For this we have created a [custom theme](./vuepress/). To convert a `docs` folder into a VuePress site, follow these steps:

1. Enable GitHub Pages in the settings of the repository. Be sure to select **GitHub Actions** as the `Source`, and check `Enforce HTTPS`. Then your settings should look like this:

![GitHub Pages part of repository settings](./enable-github-pages.png)

2. Create a new file in `.github/workflows` named `build-and-deploy-documentation.yml`, and copy the following contents to it:

::: details build-and-deploy-documentation.yml
```yml
name: Build and Deploy Documentation Site

on:
  # Runs on pushes targeting the default branch
  push:
    branches: ["master","main"]

  # Allows you to run this workflow manually from the Actions tab
  workflow_dispatch:

# Sets permissions of the GITHUB_TOKEN to allow deployment to GitHub Pages
permissions:
  contents: read
  pages: write
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
        uses: bcc-code/bcc-documentation-base@v1
        with:
          title: FILL IN
          description: FILL IN
        
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    needs: build
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v1
```
:::

Be sure to set the name and description of the repository, the name will be used for the main section in the sidebar and in the `title` tag, the description will be set for the `meta` description tag.

3. Push this to the main branch of the repository and your documentation site should be deployed to developer.bcc.no/repository-name/ 🎉

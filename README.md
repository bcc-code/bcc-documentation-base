# BCC documentation base template

This repository holds the base for documentation of all BCC projects. It consists of these parts:
- `vuepress` - This is a VuePress base template including a theme, to be used by different documentation sites. Read more in [its readme](./vuepress/README.md).
- `docs` - Public documentation for this project.
- `action.yml` - A GitHub Action to be used by other repositories to build the documentation.
- `auth-proxy` - A proxy that is used in Azure to require GitHub login for all documentation sites.

## Updating the GitHub Action
The `action.yml` file is a [composite](https://docs.github.com/en/actions/creating-actions/creating-a-composite-action) GitHub Action. It is used by workflows in other repositories like this:

```yml
steps:
    - name: Build documentation site
        uses: bcc-code/bcc-documentation-base@v1
        with:
            title: Documentation Guide
            description: Information on how to set up documentation for BCC projects
```

You can see a full workflow in action [in this repository](./.github/workflows/build-and-deploy-documentation.yml), as it is used here as well to build the documentation.

The "version" of the Action (the `@v1` part) is just a tag that is added to a commit on this repository. When updating the `action.yml` file, there are two ways to propagate this update to all the other repositories.

### 1. Create a new tag
This is the easiest approach. Use this if there are any breaking changes to the action, such as renaming an argument. Create a new tag with a comment like this:
```sh
git tag -a -m "Action: Add argument" v2
```
This creates a `v2` tag with a comment of `Action: Add argument`.

Then push the tag to GitHub (and any non-pushed commits) by appending the `follow-tags` flag to `git push`:
```sh
git push --follow-tags
```

After doing this, all the workflows in this and other repositories need to be updated to use that `v2` tag. This enables gradual adoption, but the downside is of course a potential burden of having to upgrade a lot of repositories.

### 2. Republish an existing tag
By deleting and republishing the last tag, any future workflow will use the updated version without having to update all the other workflows. This is a **dangerous** action though, as an error in the configuration can lead to all repositories breaking, and you're deleting the tag from the server forever. **Only use this for backwards compatible changes**.

First delete the tag locally:
```sh
git tag -d v1
```

Then delete it on GitHub:
```sh
git push --delete origin v1
```

The run the two commands under option 1 to create and push this tag. Note that any workflows running between deleting the old tag and pushing the new tag will fail.

## License
Everything in this repository is licensed under the [Apache 2.0 license](./LICENSE).

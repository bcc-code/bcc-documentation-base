# BCC VuePress theme

This folder holds a base VuePress 2 installation and a custom BCC theme, based on the default theme.

## Notes
This is a VuePress 2 setup, so be sure to refer to the [VuePress v2 documentation](https://v2.vuepress.vuejs.org/).

## Structure
The source code is in the `docs` folder, which does not have any files apart from the `.vuepress` folder holding the theme configuration. The intended usage of this setup is to copy any documentation files into the `docs` folder, after which a VuePress site can be built without the need to configure anything. These folders are named after VuePress conventions.

## Running locally
First install dependencies:
```sh
npm ci
```

Then run a dev server with
```sh
npm start
```

This will also copy all files from the parent `docs` folder over to this `docs` folder to have some content to test the theme with. Any files in `docs` outside of `.vuepress` are ignored by Git.

A production version of the site can be built with
```sh
npm run build
```
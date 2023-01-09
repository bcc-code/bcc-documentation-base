# Tokens

After the tokens have been deployed to the tokens folder, they need to be included into the package theme.
Unfortunately, raw tokens data cannot be read by the tailwind theme naming convention right away.
To parse the token to be an appropriate JSON file we use style-dictionary and sd-tailwindcss-transformer

Source: https://github.com/nado1001/style-dictionary-tailwindcss-transformer#creating-each-theme-file

To use it properly, remember that you have to provide a styles token to the package config. Otherwise it may return an orange colored error that tailwind.config.js has no properties.

To transform the conventions properly, remember that the token should wrap the contents of styles inside objects named as below. Otherwise the styles may not  work.
https://night-tailwindcss.vercel.app/docs/theme

## Flow
When you have provided token, you can transform it by using command: "yarn build:tailwind". The command will auto update the tailwind.config.js file.
There may be a problem, because it does not recognise the flowbite plugin in the configuration. I have not fully tested it but at the moment, it seems that the configuration works flawless.

## Current state
To finish the flow we should update the tokens in a way that the colors object wraps all the colors inside.
There may be other object naming problems with other objects - to fix those update tokens in a way that they match the tailwind supported naming convention from the source: https://night-tailwindcss.vercel.app/docs/theme

Items that will be not recognised and require renaming: "sizing".

## Automatization
I have not recognised the topic yet, but in my opinion adding a script :  "yarn build:tailwind" to the CI/CD to the package build process will automatically transform the given tokens values.
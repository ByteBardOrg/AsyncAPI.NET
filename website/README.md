# AsyncAPI.NET documentation site

The documentation site is built with Docusaurus 3. From this directory:

```sh
npm install
npm start
```

Open the local address printed by Docusaurus (normally http://localhost:3000/AsyncAPI.NET/). To create and preview the production site, run `npm run build` and then `npm run serve`.

The deployed site is configured for GitHub Pages at https://bytebard.org/AsyncAPI.NET/.

Documentation lives in two directories:

- `docs/guides/` contains the **Use the library** guides (getting started, reading, extensions, references, schemas, and version conversion) and the **Extend the library** guides (custom extension parsers, bindings, schema parsers, and validation rules). Both sections share a sidebar, arranged in `sidebars.js`.
- `docs/bindings/` contains protocol-specific binding pages. Its sidebar is generated automatically from the files in that directory.

Add a guide or binding page to its matching directory. Update `sidebars.js` when adding guides so it appears in the appropriate section.

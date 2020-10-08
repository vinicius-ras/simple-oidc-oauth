# About the app

This application is a project which has been bootstrapped using the [Create React App](https://github.com/facebook/create-react-app) tool. Thus, the app is compatible with most of the standards adopted by a [Create React App](https://github.com/facebook/create-react-app) project.

This project's main goal is to provide a web app to enable the user to communicate with the *OpenID Connect / OAuth 2.0 server*. Most users will simply be using it to perform login and consent clients to access protected resources. Users with the necessary claims will also be able to manage and provide configuration for clients and protected resources themselves.

**Attention:** The web application requires the *OpenID Connect / OAuth 2.0 server* to be running. If the auth server is not running, the web app will be stuck in a loading screen, making new attempts until it is able to communicate with the auth server.

# Scripts

## Running  the project

The app can be run just like a standard [Create React App](https://github.com/facebook/create-react-app) project:

```
cd <SPA_project_folder>
npm start
```

If not configured otherwise, the project will be accessible through a web browser under the default address [http://localhost:3000](http://localhost:3000).

Editing the project's files while it's running will automatically update it, and changes will be reflected in the browser's open pages.

## Building the project

Once the project is ready to be deployed to a production server, it must be built. The build process will bundle the project, optiize it (e.g., compile TypeScript files, minify generated output, etc), and set it up for production mode.

Again, the project complies with [Create React App](https://github.com/facebook/create-react-app) standards and can be build the following:

```
cd <SPA_project_folder>
npm run build
```

For more information, consult the [Create React App documentation on deployment](https://create-react-app.dev/docs/deployment/).

## Generating code documentation

Most of the project's code entities (classes, functions, methods, properties, etc) are documented using [TSDoc](https://github.com/microsoft/tsdoc): a proposal to standardize code documentation.

There are many tools which can parse the project's code, find [TSDoc](https://github.com/microsoft/tsdoc) documentation tags, and output this in a more human-readable format. From all of the options, this project mainly uses [TypeDoc](https://github.com/TypeStrong/typedoc) as the documentation generator.

There are many ways to run this tool. One of the easiest ones is to run it through NPX in the app's folder:

```
cd <SPA_project_folder>
npx typedoc --out ./docs
```

This will generate the documentation in the project's `docs/` folder - just open the generated `docs/index.html` file to view it.

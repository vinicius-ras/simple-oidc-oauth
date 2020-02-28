# Description

This project is an implementation of an OpenID Connect (OIDC) and OAuth 2.0 provider.

It is based on IdentityServer4 - a well known and certified OIDC/OAuth provider implementation - that can be used by developers to both abstract the implementation of a future login system (based on the OIDC/OAuth 2.0 protocols) for their projects, or even tweaked to suit their specific needs and to be used in production.

# Project status

The project is currently in its alpha version, having the OIDC/OAuth 2.0 protocols implemented, but needing to be configured manually. An SQLite database is being used to hold configurations, such as: protected API resources, identity resources, known clients, access tokens, refresh tokens, identity tokens, etc.

Manual configurations need to be performed in the database itself for now. Future versions are planned to have a web app where the user can easily configure the provider through its UI.
// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace SimpleOidcOauth
{
    /// <summary>Configuration values to be applied to IdentityServer4.</summary>
    public static class Config
    {
        /// <summary>Resources available for the Identity Tokens emitted by the server.</summary>
        /// <value>A collection of resources to be made available in the Identity Tokens emitted by the server.</value>
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        /// <summary>Resources available for the Access Tokens emitted by the server.</summary>
        /// <value>A collection of resources to be made available in the Access Tokens emitted by the server.</value>
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
            };

        /// <summary>Client applications known to the server.</summary>
        /// <value>A collection of objects describing the clients known to the server.</value>
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
            };
    }
}
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity
{
    /// <summary>
    ///     Creates a ClaimsIdentity from a User
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class ClaimsIdentityFactory<TUser> : IClaimsIdentityFactory<TUser>
        where TUser : class
    {
        /// <summary>
        ///     ClaimType used for the security stamp by default
        /// </summary>
        public const string DefaultSecurityStampClaimType = "AspNet.Identity.SecurityStamp";

        /// <summary>
        ///     Constructor
        /// </summary>
        public ClaimsIdentityFactory()
        {
            RoleClaimType = ClaimsIdentity.DefaultRoleClaimType;
            UserIdClaimType = ClaimTypes.NameIdentifier;
            UserNameClaimType = ClaimsIdentity.DefaultNameClaimType;
            SecurityStampClaimType = DefaultSecurityStampClaimType;
        }

        /// <summary>
        ///     Claim type used for role claims
        /// </summary>
        public string RoleClaimType { get; set; }

        /// <summary>
        ///     Claim type used for the user name
        /// </summary>
        public string UserNameClaimType { get; set; }

        /// <summary>
        ///     Claim type used for the user id
        /// </summary>
        public string UserIdClaimType { get; set; }

        /// <summary>
        ///     Claim type used for the user security stamp
        /// </summary>
        public string SecurityStampClaimType { get; set; }

        /// <summary>
        ///     CreateAsync a ClaimsIdentity from a user
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <param name="authenticationType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<ClaimsIdentity> CreateAsync(UserManager<TUser> manager, TUser user,
            string authenticationType, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            var userId = await manager.GetUserIdAsync(user, cancellationToken);
            var userName = await manager.GetUserNameAsync(user, cancellationToken);
            var id = new ClaimsIdentity(authenticationType, UserNameClaimType, RoleClaimType);
            id.AddClaim(new Claim(UserIdClaimType, userId));
            id.AddClaim(new Claim(UserNameClaimType, userName, ClaimValueTypes.String));
            if (manager.SupportsUserSecurityStamp)
            {
                id.AddClaim(new Claim(SecurityStampClaimType, await manager.GetSecurityStampAsync(user, cancellationToken)));
            }
            if (manager.SupportsUserRole)
            {
                var roles = await manager.GetRolesAsync(user, cancellationToken);
                foreach (var roleName in roles)
                {
                    id.AddClaim(new Claim(RoleClaimType, roleName, ClaimValueTypes.String));
                }
            }
            if (manager.SupportsUserClaim)
            {
                id.AddClaims(await manager.GetClaimsAsync(user, cancellationToken));
            }
            return id;
        }
    }
}
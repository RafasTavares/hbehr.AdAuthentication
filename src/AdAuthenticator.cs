﻿/*
 * The MIT License (MIT)
 * Copyright (c) 2014 Henrique Borba Behr

 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web;

namespace AdAuthentication
{
    public class AdAuthenticator
    {
        internal string LdapDomain { get; set; }
        internal string LdapPath { get; set; }

        public AdAuthenticator ConfigureLdapDomain(string ldapDomain)
        {
            LdapDomain = ldapDomain;
            return this;
        }

        public AdAuthenticator ConfigureSetLdapPath(string ldapPath)
        {
            LdapPath = ldapPath;
            return this;
        }

        public AdUser SearchUserBy(string login, string password)
        {
            new Validator(this)
                .ValidateConfiguration()
                .ValidateParameters(login, password)
                .ValidateUserPasswordAtAd(login, password);

            return GetUserFromAdBy(login);
        }

        public AdUser GetUserFromAd()
        {
            new Validator(this).ValidateConfiguration();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                throw new AdException(AdError.UserNotFound, "User not authenticated, use Windows Authentication");
            }

            string login = HttpContext.Current.User.Identity.Name;
            return GetUserFromAdBy(login);
        }

        public AdGroup GetAdGroups()
        {
            new Validator(this).ValidateConfiguration();
        }

        private AdUser GetUserFromAdBy(string login)
        {
            PrincipalContext principalContext = GetPrincipalContext();

            Principal principal = Principal.FindByIdentity(principalContext, login);

            if (principal == null)
            {
                throw new AdException(AdError.UserNotFound, "User not found");
            }
            return new AdUser(principal, principal.GetGroups(principalContext));
        }

        
        private PrincipalContext GetPrincipalContext()
        {
            PrincipalContext principalContext;
            try
            {
                principalContext = new PrincipalContext(ContextType.Domain, LdapDomain);
            }
            catch (PrincipalServerDownException)
            {
                throw new AdException(AdError.InvalidLdapDomain, "Ldap Domain not found");
            }
            catch (Exception e)
            {
                throw new AdException(e);
            }
            return principalContext;
        }
    }
}

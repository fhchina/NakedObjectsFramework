﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Component;
using NakedObjects.Reflect.Spec;
using NakedObjects.Security;

namespace NakedObjects.Reflect.Authorization {
    public class CustomAuthorizerInstaller : IAuthorizerInstaller {
        private readonly CustomAuthorizationManager authManager;

        public CustomAuthorizerInstaller(ITypeAuthorizer<object> defaultAuthorizer) {
            authManager = new CustomAuthorizationManager(defaultAuthorizer);
        }

        /// <summary>
        /// </summary>
        /// <param name="defaultAuthorizer">This will be used unless the object type exactly matches one of the typeauthorizers</param>
        /// <param name="typeAuthorizers">Each authorizer must implement ITypeAuthorizer for a concrete domain type</param>
        public CustomAuthorizerInstaller(ITypeAuthorizer<object> defaultAuthorizer, params object[] typeAuthorizers) {
            authManager = new CustomAuthorizationManager(defaultAuthorizer, typeAuthorizers);
        }

        /// <summary>
        /// </summary>
        /// <param name="defaultAuthorizer">This will be used unless the object is recognised by one of the namespaceAuthorizers</param>
        public CustomAuthorizerInstaller(ITypeAuthorizer<object> defaultAuthorizer, params INamespaceAuthorizer[] namespaceAuthorizers) {
            authManager = new CustomAuthorizationManager(defaultAuthorizer, namespaceAuthorizers);
        }

        #region IAuthorizerInstaller Members

        public string Name {
            get { return "TypeAuthorizerInstaller"; }
        }

        public IFacetDecorator[] CreateDecorators(IReflector reflector) {
            throw new NotImplementedException();
        }

        #endregion

        public IFacetDecorator[] CreateDecorators(IMetamodel metamodel) {
            authManager.Metamodel = metamodel;
            return new IFacetDecorator[] {new AuthorizationFacetDecorator(authManager)};
        }
    }
}
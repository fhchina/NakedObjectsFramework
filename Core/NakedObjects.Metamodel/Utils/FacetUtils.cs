// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Util;

namespace NakedObjects.Meta.Utils {
    public static class FacetUtils {
        public static string[] SplitOnComma(string toSplit) {
            if (string.IsNullOrEmpty(toSplit)) {
                return new string[] {};
            }
            return toSplit.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }

        public static bool IsAllowed(ISession session, string[] roles, string[] users) {
            if (roles.Any(role => session.Principal.IsInRole(role))) {
                return true;
            }

            if (users.Any(user => session.Principal.Identity.Name == user)) {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Attaches the <see cref="IFacet" /> to its <see cref="IFacet.Specification" />
        /// </summary>
        /// <returns>
        ///     <c>true</c> if a non-<c>null</c> facet was added, <c>false</c> otherwise.
        /// </returns>
        public static void AddFacet(IFacet facet) {
            if (facet != null) {
                ((ISpecificationBuilder) facet.Specification).AddFacet(facet);
            }
        }

        /// <summary>
        ///     Attaches each <see cref="IFacet" /> to its <see cref="IFacet.Specification" />
        /// </summary>
        /// <returns>
        ///     <c>true</c> if any facets were added, <c>false</c> otherwise.
        /// </returns>
        public static void AddFacets(IEnumerable<IFacet> facetList) {
            facetList.ForEach(AddFacet);
        }

        public static INakedObject[] MatchParameters(string[] parameterNames, IDictionary<string, INakedObject> parameterNameValues) {
            var parmValues = new List<INakedObject>();

            foreach (string name in parameterNames) {
                if (parameterNameValues != null && parameterNameValues.ContainsKey(name)) {
                    parmValues.Add(parameterNameValues[name]);
                }
                else {
                    parmValues.Add(null);
                }
            }

            return parmValues.ToArray();
        }

        public static bool IsNotANoopFacet(IFacet facet) {
            return facet != null && !facet.IsNoOp;
        }
    }
}
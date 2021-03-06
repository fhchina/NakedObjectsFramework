// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Meta.Facet;

namespace NakedObjects.Reflect.FacetFactory {
    /// <summary>
    ///     Sets up all the <see cref="IFacet" />s for an action in a single shot
    /// </summary>
    public class ComplementaryMethodsFilteringFacetFactory : FacetFactoryAbstract, IMethodFilteringFacetFactory {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ComplementaryMethodsFilteringFacetFactory));

        private static readonly string[] PropertyPrefixes = {
            PrefixesAndRecognisedMethods.AutoCompletePrefix,
            PrefixesAndRecognisedMethods.ModifyPrefix,
            PrefixesAndRecognisedMethods.ClearPrefix,
            PrefixesAndRecognisedMethods.ChoicesPrefix,
            PrefixesAndRecognisedMethods.DefaultPrefix,
            PrefixesAndRecognisedMethods.ValidatePrefix,
            PrefixesAndRecognisedMethods.HidePrefix,
            PrefixesAndRecognisedMethods.DisablePrefix
        };

        private static readonly string[] ActionPrefixes = {
            PrefixesAndRecognisedMethods.ValidatePrefix,
            PrefixesAndRecognisedMethods.HidePrefix,
            PrefixesAndRecognisedMethods.DisablePrefix
        };

        private static readonly string[] ParameterPrefixes = {
            PrefixesAndRecognisedMethods.AutoCompletePrefix,
            PrefixesAndRecognisedMethods.ParameterChoicesPrefix,
            PrefixesAndRecognisedMethods.ParameterDefaultPrefix,
            PrefixesAndRecognisedMethods.ValidatePrefix
        };

        /// <summary>
        ///     The <see cref="IFacet" />s registered are the generic ones from no-architecture (where they exist)
        /// </summary>
        /// <param name="numericOrder"></param>
        public ComplementaryMethodsFilteringFacetFactory(int numericOrder)
            : base(numericOrder, FeatureType.Action) {}

        #region IMethodFilteringFacetFactory Members

        public bool Filters(MethodInfo method, IClassStrategy classStrategy) {
            return IsComplementaryMethod(method);
        }

        #endregion

        private static bool IsComplementaryMethod(MethodInfo actionMethod) {
            return PropertyPrefixes.Any(prefix => IsComplementaryPropertyMethod(actionMethod, prefix)) ||
                   ActionPrefixes.Any(prefix => IsComplementaryActionMethod(actionMethod, prefix)) ||
                   ParameterPrefixes.Any(prefix => IsComplementaryParameterMethod(actionMethod, prefix));
        }

        private static bool IsComplementaryPropertyMethod(MethodInfo actionMethod, string prefix) {
            string propertyName;
            if (MatchesPrefix(actionMethod, prefix, out propertyName)) {
                Type declaringType = actionMethod.DeclaringType;
                Trace.Assert(declaringType != null, "declaringType != null");
                Type baseType = declaringType.BaseType;
                Trace.Assert(baseType != null, "baseType != null");

                if (InheritsProperty(baseType, propertyName)) {
                    Log.InfoFormat("Filtering method {0} because of property {1} on {2}", actionMethod.Name, propertyName, baseType.FullName);
                    return true;
                }
            }
            return false;
        }

        private static bool IsComplementaryActionMethod(MethodInfo actionMethod, string prefix) {
            string propertyName;
            if (MatchesPrefix(actionMethod, prefix, out propertyName)) {
                Type declaringType = actionMethod.DeclaringType;
                Debug.Assert(declaringType != null, "declaringType != null");
                if (InheritsMethod(declaringType.BaseType, propertyName)) {
                    string baseTypeName = declaringType.BaseType == null ? "Unknown type" : declaringType.BaseType.FullName;
                    Log.InfoFormat("Filtering method {0} because of action {1} on {2}", actionMethod.Name, propertyName, baseTypeName);
                    return true;
                }
            }
            return false;
        }

        private static bool IsComplementaryParameterMethod(MethodInfo actionMethod, string prefix) {
            string propertyName;
            if (MatchesPrefix(actionMethod, prefix, out propertyName)) {
                propertyName = TrimDigits(propertyName);
                Type declaringType = actionMethod.DeclaringType;
                Debug.Assert(declaringType != null, "declaringType != null");
                if (InheritsMethod(declaringType.BaseType, propertyName)) {
                    string baseTypeName = declaringType.BaseType == null ? "Unknown type" : declaringType.BaseType.FullName;
                    Log.InfoFormat("Filtering method {0} because of action {1} on {2}", actionMethod.Name, propertyName, baseTypeName);
                    return true;
                }
            }
            return false;
        }

        private static string TrimDigits(string toTrim) {
            while (toTrim.Length > 0 && char.IsDigit(toTrim, 0)) {
                toTrim = toTrim.Substring(1);
            }

            return toTrim;
        }

        private static bool MatchesPrefix(MethodInfo actionMethod, string prefix, out string propertyName) {
            if (actionMethod.Name.StartsWith(prefix)) {
                propertyName = actionMethod.Name.Remove(0, prefix.Length);
                return true;
            }
            propertyName = null;
            return false;
        }

        private static bool InheritsProperty(Type typeToCheck, string name) {
            if (typeToCheck == null) {
                return false;
            }

            return typeToCheck.GetProperty(name) != null || InheritsProperty(typeToCheck.BaseType, name);
        }

        private static bool InheritsMethod(Type typeToCheck, string name) {
            if (typeToCheck == null) {
                return false;
            }

            return typeToCheck.GetMethod(name) != null || InheritsMethod(typeToCheck.BaseType, name);
        }
    }
}
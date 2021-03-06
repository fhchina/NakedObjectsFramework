﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.Utils;

namespace NakedObjects.Reflect.FacetFactory {
    public class RemoveDynamicProxyMethodsFacetFactory : FacetFactoryAbstract {
        private static readonly string[] MethodsToRemove = {"GetBasePropertyValue", "SetBasePropertyValue", "SetChangeTracker"};

        public RemoveDynamicProxyMethodsFacetFactory(int numericOrder)
            : base(numericOrder, FeatureType.ObjectsAndProperties) {}

        private static bool IsDynamicProxyType(Type type) {
            return type.FullName.StartsWith("System.Data.Entity.DynamicProxies");
        }

        public override void Process(IReflector reflector, Type type, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            if (IsDynamicProxyType(type)) {
                foreach (MethodInfo method in type.GetMethods().Join(MethodsToRemove, mi => mi.Name, s => s, (mi, s) => mi)) {
                    if (methodRemover != null && method != null) {
                        methodRemover.RemoveMethod(method);
                    }
                }
            }
        }

        public override void Process(IReflector reflector, PropertyInfo property, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            if (IsDynamicProxyType(property.DeclaringType) && property.Name == "RelationshipManager") {
                FacetUtils.AddFacet(new HiddenFacet(WhenTo.Always, specification));
            }
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
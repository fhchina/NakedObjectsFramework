// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.Utils;

namespace NakedObjects.Reflect.FacetFactory {
    public class TypeMarkerFacetFactory : AnnotationBasedFacetFactoryAbstract {
        public TypeMarkerFacetFactory(int numericOrder)
            : base(numericOrder, FeatureType.Objects) {}

        public override void Process(IReflector reflector, Type type, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            var facets = new List<IFacet>();

            if (IsAbstract(type)) {
                facets.Add(new TypeIsAbstractFacet(specification));
            }

            if (IsInterface(type)) {
                facets.Add(new TypeIsInterfaceFacet(specification));
            }

            if (IsSealed(type)) {
                facets.Add(new TypeIsSealedFacet(specification));
            }

            if (IsVoid(type)) {
                facets.Add(new TypeIsVoidFacet(specification));
            }

            FacetUtils.AddFacets(facets);
        }

        private static bool IsVoid(Type type) {
            return type == typeof (void);
        }

        private static bool IsSealed(Type type) {
            return type.IsSealed;
        }

        private static bool IsInterface(Type type) {
            return type.IsInterface;
        }

        private static bool IsAbstract(Type type) {
            return type.IsAbstract;
        }
    }
}
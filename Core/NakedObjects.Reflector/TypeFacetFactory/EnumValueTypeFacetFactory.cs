// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.SemanticsProvider;
using NakedObjects.Meta.Utils;

namespace NakedObjects.Reflect.TypeFacetFactory {
    public class EnumValueTypeFacetFactory : FacetFactoryAbstract {
        public EnumValueTypeFacetFactory(IReflector reflector)
            : base(reflector, FeatureType.Objects) {}

        public override bool Process(Type type, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            if (typeof (Enum).IsAssignableFrom(type)) {
                Type semanticsProviderType = typeof (EnumValueSemanticsProvider<>).MakeGenericType(type);
                var spec = Reflector.LoadSpecification(type);

                object semanticsProvider = Activator.CreateInstance(semanticsProviderType, spec, specification);
                Type facetType = typeof (ValueFacetUsingSemanticsProvider<>).MakeGenericType(type);
                var facet = (IFacet) Activator.CreateInstance(facetType, semanticsProvider, semanticsProvider);
                FacetUtils.AddFacet(facet);
                return true;
            }
            return false;
        }
    }
}
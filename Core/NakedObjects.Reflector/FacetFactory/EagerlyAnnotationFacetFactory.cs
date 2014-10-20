﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Reflection;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Metamodel.Facet;
using NakedObjects.Metamodel.Utils;

using NakedObjects.Util;

namespace NakedObjects.Reflector.FacetFactory {
    public class EagerlyAnnotationFacetFactory : FacetFactoryAbstract {
        public EagerlyAnnotationFacetFactory(IReflector reflector)
            : base(reflector, FeatureType.EverythingButParameters) {}

        public override bool Process(Type type, IMethodRemover methodRemover, ISpecification specification) {
            var attribute = type.GetCustomAttributeByReflection<EagerlyAttribute>();
            return FacetUtils.AddFacet(Create(attribute, specification));
        }

        public override bool Process(PropertyInfo property, IMethodRemover methodRemover, ISpecification specification) {
            var attribute = AttributeUtils.GetCustomAttribute<EagerlyAttribute>(property);
            return FacetUtils.AddFacet(Create(attribute, specification));
        }

        public override bool Process(MethodInfo method, IMethodRemover methodRemover, ISpecification specification) {
            var attribute = AttributeUtils.GetCustomAttribute<EagerlyAttribute>(method);
            return FacetUtils.AddFacet(Create(attribute, specification));
        }

        private static IEagerlyFacet Create(EagerlyAttribute attribute, ISpecification holder) {
            return attribute == null ? null : new EagerlyFacet(EagerlyAttribute.Do.Rendering, holder);
        }
    }
}
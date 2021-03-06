// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.Utils;

namespace NakedObjects.Reflect.FacetFactory {
    public class RangeAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        private static readonly ILog Log = LogManager.GetLogger(typeof (RangeAnnotationFacetFactory));

        public RangeAnnotationFacetFactory(int numericOrder)
            : base(numericOrder, FeatureType.PropertiesAndParameters) {}

        private static void Process(MemberInfo member, bool isDate, ISpecification specification) {
            var attribute = member.GetCustomAttribute<RangeAttribute>();
            FacetUtils.AddFacet(Create(attribute, isDate, specification));
        }

        public override void Process(IReflector reflector, PropertyInfo property, IMethodRemover methodRemover, ISpecificationBuilder specification) {
            bool isDate = property.PropertyType.IsAssignableFrom(typeof (DateTime));
            Process(property, isDate, specification);
        }

        public override void ProcessParams(IReflector reflector, MethodInfo method, int paramNum, ISpecificationBuilder holder) {
            ParameterInfo parameter = method.GetParameters()[paramNum];
            bool isDate = parameter.ParameterType.IsAssignableFrom(typeof (DateTime));
            var range = parameter.GetCustomAttribute<RangeAttribute>();
            FacetUtils.AddFacet(Create(range, isDate, holder));
        }

        private static IRangeFacet Create(RangeAttribute attribute, bool isDate, ISpecification holder) {
            if (attribute == null) {
                return null;
            }

            if (attribute.OperandType != typeof (int) && attribute.OperandType != typeof (double)) {
                Log.WarnFormat("Unsupported use of range attribute with explicit type on {0}", holder);
                return null;
            }

            var min = attribute.Minimum as IConvertible;
            var max = attribute.Maximum as IConvertible;

            if (min == null || max == null) {
                Log.WarnFormat("Min Max values must be IConvertible for Range on {0}", holder);
                return null;
            }

            return new RangeFacet(min, max, isDate, holder);
        }
    }
}
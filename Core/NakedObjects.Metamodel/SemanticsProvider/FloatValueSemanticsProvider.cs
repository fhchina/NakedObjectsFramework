// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Globalization;
using NakedObjects.Architecture;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Core.Util;

namespace NakedObjects.Meta.SemanticsProvider {
    [Serializable]
    public class FloatValueSemanticsProvider : ValueSemanticsProviderAbstract<float>, IFloatingPointValueFacet {
        private const float DefaultValueConst = 0;
        private const bool EqualByContent = true;
        private const bool Immutable = true;
        private const int TypicalLengthConst = 12;

        public FloatValueSemanticsProvider(IObjectSpecImmutable spec, ISpecification holder)
            : base(Type, holder, AdaptedType, TypicalLengthConst, Immutable, EqualByContent, DefaultValueConst, spec) {}

        public static Type Type {
            get { return typeof (IFloatingPointValueFacet); }
        }

        public static Type AdaptedType {
            get { return typeof (float); }
        }

        #region IFloatingPointValueFacet Members

        public float FloatValue(INakedObject nakedObject) {
            return nakedObject.GetDomainObject<float>();
        }

        #endregion

        public static bool IsAdaptedType(Type type) {
            return type == typeof (float);
        }

        protected override float DoParse(string entry) {
            try {
                return float.Parse(entry);
            }
            catch (FormatException) {
                throw new InvalidEntryException(FormatMessage(entry));
            }
            catch (OverflowException) {
                throw new InvalidEntryException(OutOfRangeMessage(entry, float.MinValue, float.MaxValue));
            }
        }

        protected override float DoParseInvariant(string entry) {
            return float.Parse(entry, CultureInfo.InvariantCulture);
        }

        protected override string GetInvariantString(float obj) {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        protected override string TitleStringWithMask(string mask, float value) {
            return value.ToString(mask);
        }

        protected override string DoEncode(float obj) {
            return obj.ToString("G", CultureInfo.InvariantCulture);
        }

        protected override float DoRestore(string data) {
            return float.Parse(data, CultureInfo.InvariantCulture);
        }

        public override string ToString() {
            return "FloatAdapter: ";
        }
    }
}
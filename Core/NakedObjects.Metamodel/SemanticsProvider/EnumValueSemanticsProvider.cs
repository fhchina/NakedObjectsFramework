// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Util;

namespace NakedObjects.Meta.SemanticsProvider {
    [Serializable]
    public class EnumValueSemanticsProvider<T> : ValueSemanticsProviderAbstract<T>, IEnumValueFacet {
        private const bool EqualBycontent = true;
        private const bool Immutable = true;
        private const int TypicalLengthConst = 11;

        public EnumValueSemanticsProvider(IObjectSpecImmutable spec, ISpecification holder)
            : base(Type, holder, AdaptedType, TypicalLengthConst, Immutable, EqualBycontent, default(T), spec) {}

        public static Type Type {
            get { return typeof (IEnumValueFacet); }
        }

        public static Type AdaptedType {
            get { return typeof (T); }
        }

        #region IEnumValueFacet Members

        public string IntegralValue(INakedObject nakedObject) {
            if (nakedObject.Object is T || TypeUtils.IsIntegralValueForEnum(nakedObject.Object)) {
                return Convert.ChangeType(nakedObject.Object, Enum.GetUnderlyingType(typeof (T))).ToString();
            }
            return null;
        }

        #endregion

        public static bool IsAdaptedType(Type type) {
            return type == AdaptedType;
        }

        protected override T DoParse(string entry) {
            try {
                return (T) Enum.Parse(typeof (T), entry);
            }
            catch (ArgumentException) {
                throw new InvalidEntryException(FormatMessage(entry));
            }
            catch (OverflowException oe) {
                throw new InvalidEntryException(oe.Message);
            }
        }

        protected override T DoParseInvariant(string entry) {
            return (T) Enum.Parse(typeof (T), entry);
        }

        protected override string GetInvariantString(T obj) {
            return obj.ToString();
        }

        protected override string TitleString(T obj) {
            return NameUtils.NaturalName(obj.ToString());
        }

        protected override string TitleStringWithMask(string mask, T value) {
            return TitleString(value);
        }

        protected override string DoEncode(T obj) {
            return obj.GetType().FullName + ":" + obj;
        }

        protected override T DoRestore(string data) {
            string[] typeAndValue = data.Split(':');
            return (T) Enum.Parse(TypeUtils.GetType(typeAndValue[0]), typeAndValue[1]);
        }

        public override string ToString() {
            return "EnumAdapter: ";
        }
    }
}
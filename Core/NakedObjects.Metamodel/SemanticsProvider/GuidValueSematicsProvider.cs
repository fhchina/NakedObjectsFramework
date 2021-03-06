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
using NakedObjects.Core.Util;

namespace NakedObjects.Meta.SemanticsProvider {
    [Serializable]
    public class GuidValueSemanticsProvider : ValueSemanticsProviderAbstract<Guid>, IGuidValueFacet {
        private const bool EqualByContent = true;
        private const bool Immutable = true;
        private const int TypicalLengthConst = 36;
        private static readonly Guid DefaultValueConst = Guid.Empty;

        public GuidValueSemanticsProvider(IObjectSpecImmutable spec, ISpecification holder)
            : base(Type, holder, AdaptedType, TypicalLengthConst, Immutable, EqualByContent, DefaultValueConst, spec) {}

        public static Type Type {
            get { return typeof (IGuidValueFacet); }
        }

        public static Type AdaptedType {
            get { return typeof (Guid); }
        }

        #region IGuidValueFacet Members

        public Guid GuidValue(INakedObject nakedObject) {
            return nakedObject.GetDomainObject<Guid>();
        }

        #endregion

        public static bool IsAdaptedType(Type type) {
            return type == typeof (Guid);
        }

        protected override Guid DoParse(string entry) {
            try {
                return new Guid(entry);
            }
            catch (FormatException) {
                throw new InvalidEntryException(FormatMessage(entry));
            }
            catch (OverflowException) {
                throw new InvalidEntryException(FormatMessage(entry));
            }
        }

        protected override Guid DoParseInvariant(string entry) {
            return Guid.Parse(entry);
        }

        protected override string GetInvariantString(Guid obj) {
            return obj.ToString();
        }

        protected override string TitleStringWithMask(string mask, Guid value) {
            return value.ToString(mask);
        }

        protected override string DoEncode(Guid obj) {
            return obj.ToString();
        }

        protected override Guid DoRestore(string data) {
            return new Guid(data);
        }

        public override string ToString() {
            return "GuidAdapter: ";
        }
    }
}
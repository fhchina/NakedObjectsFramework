﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
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
    public class UIntValueSemanticsProvider : ValueSemanticsProviderAbstract<uint>, IUnsignedIntegerValueFacet {
        private const uint DefaultValueConst = 0;
        private const bool EqualByContent = true;
        private const bool Immutable = true;
        private const int TypicalLengthConst = 10;

        public UIntValueSemanticsProvider(IObjectSpecImmutable spec, ISpecification holder)
            : base(Type, holder, AdaptedType, TypicalLengthConst, Immutable, EqualByContent, DefaultValueConst, spec) {}

        public static Type Type {
            get { return typeof (IUnsignedIntegerValueFacet); }
        }

        public static Type AdaptedType {
            get { return typeof (uint); }
        }

        #region IUnsignedIntegerValueFacet Members

        public uint UnsignedIntegerValue(INakedObject nakedObject) {
            return nakedObject.GetDomainObject<uint>();
        }

        #endregion

        public object GetDefault(INakedObject inObject) {
            return DefaultValueConst;
        }

        public static bool IsAdaptedType(Type type) {
            return type == typeof (uint);
        }

        protected override uint DoParse(string entry) {
            try {
                return uint.Parse(entry, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands);
            }
            catch (FormatException) {
                throw new InvalidEntryException(FormatMessage(entry));
            }
            catch (OverflowException) {
                throw new InvalidEntryException(OutOfRangeMessage(entry, uint.MinValue, uint.MaxValue));
            }
        }

        protected override uint DoParseInvariant(string entry) {
            return uint.Parse(entry, CultureInfo.InvariantCulture);
        }

        protected override string GetInvariantString(uint obj) {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        protected override string TitleStringWithMask(string mask, uint value) {
            return value.ToString(mask);
        }

        protected override string DoEncode(uint obj) {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        protected override uint DoRestore(string data) {
            return uint.Parse(data, CultureInfo.InvariantCulture);
        }

        public override string ToString() {
            return "UIntAdapter: ";
        }
    }
}
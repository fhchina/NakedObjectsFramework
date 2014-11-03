// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Capabilities;
using NakedObjects.Meta.Facet;

namespace NakedObjects.Meta.SemanticsProvider {
    [Serializable]
    public abstract class ValueSemanticsProviderAbstract<T> : FacetAbstract, IValueSemanticsProvider<T>, IEncoderDecoder<T>, IParser<T>, IDefaultsProvider<T> {
        private readonly Type adaptedType;
        private readonly T defaultValue;

        private readonly bool equalByContent;
        private readonly bool immutable;
        private readonly IObjectSpecImmutable specImmutable;
        private readonly int typicalLength;

        /// <summary>
        ///     Lazily looked up per <see cref="SpecImmutable" />
        /// </summary>
        protected ValueSemanticsProviderAbstract(Type adapterFacetType,
                                                 ISpecification holder,
                                                 Type adaptedType,
                                                 int typicalLength,
                                                 bool immutable,
                                                 bool equalByContent,
                                                 T defaultValue,
                                                 IObjectSpecImmutable specImmutable)
            : base(adapterFacetType, holder) {
            this.adaptedType = adaptedType;
            this.typicalLength = typicalLength;
            this.immutable = immutable;
            this.equalByContent = equalByContent;
            this.defaultValue = defaultValue;
            this.specImmutable = specImmutable;
        }

        public IObjectSpecImmutable SpecImmutable {
            get { return specImmutable; }
        }

        /// <summary>
        ///     We don't replace any (non- no-op) facets.
        /// </summary>
        /// <para>
        ///     For example, if there is already a <see cref="IPropertyDefaultFacet" /> then we shouldn't replace it.
        /// </para>
        public override bool CanAlwaysReplace {
            get { return false; }
        }

        #region IDefaultsProvider<T> Members

        public T DefaultValue {
            get { return defaultValue; }
        }

        #endregion

        #region IEncoderDecoder<T> Members

        public string ToEncodedString(T obj) {
            return DoEncode(obj);
        }

        public T FromEncodedString(string data) {
            return DoRestore(data);
        }

        #endregion

        #region IParser<T> Members

        public virtual object ParseTextEntry(string entry) {
            if (entry == null) {
                throw new ArgumentException();
            }
            if (entry.Trim().Equals("")) {
                return null;
            }
            return DoParse(entry);
        }

        public object ParseInvariant(string entry) {
            return DoParseInvariant(entry);
        }

        public string InvariantString(T obj) {
            return GetInvariantString(obj);
        }

        public string EditableTitleOf(T existing) {
            return DisplayTitleOf(existing);
        }

        public string DisplayTitleOf(T obj) {
            return TitleString(obj);
        }

        public string TitleWithMaskOf(string mask, T obj) {
            return TitleStringWithMask(mask, obj);
        }

        public int TypicalLength {
            get { return typicalLength; }
        }

        #endregion

        #region IValueSemanticsProvider<T> Members

        public IEncoderDecoder<T> EncoderDecoder {
            get { return this; }
        }

        public virtual IParser<T> Parser {
            get { return this; }
        }

        public IDefaultsProvider<T> DefaultsProvider {
            get { return this; }
        }

        public virtual IFromStream FromStream {
            get { return null; }
        }

        public bool IsEqualByContent {
            get { return equalByContent; }
        }

        public bool IsImmutable {
            get { return immutable; }
        }

        #endregion

        public Type GetAdaptedClass() {
            return adaptedType;
        }


        protected abstract T DoParse(string entry);

        protected abstract T DoParseInvariant(string entry);

        protected abstract string GetInvariantString(T obj);

        protected virtual string TitleString(T obj) {
            return obj.ToString();
        }

        protected virtual string TitleStringWithMask(string mask, T obj) {
            return obj.ToString();
        }


        protected abstract string DoEncode(T obj);


        protected abstract T DoRestore(string data);


        protected string OutOfRangeMessage(string entry, T minValue, T maxValue) {
            return string.Format(Resources.NakedObjects.OutOfRange, entry, minValue, maxValue);
        }

        protected static string FormatMessage(string entry) {
            return string.Format(Resources.NakedObjects.CannotFormat, entry, typeof (T).Name);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Util;

namespace NakedObjects.Core.Adapter {
    public class AggregateOid : IOid, IEncodedToStrings {
        private readonly string fieldName;
        private readonly IMetamodelManager metamodel;
        private readonly IOid parentOid;
        private readonly string typeName;

        public AggregateOid(IMetamodelManager metamodel, IOid oid, string id, string typeName) {
            Assert.AssertNotNull(metamodel);
            Assert.AssertNotNull(oid);
            Assert.AssertNotNull(id);
            Assert.AssertNotNull(typeName);

            this.metamodel = metamodel;
            parentOid = oid;
            fieldName = id;
            this.typeName = typeName;
        }

        public AggregateOid(IMetamodelManager metamodel, string[] strings) {
            Assert.AssertNotNull(metamodel);
            this.metamodel = metamodel;
            var helper = new StringDecoderHelper(metamodel, strings);
            typeName = helper.GetNextString();
            fieldName = helper.GetNextString();
            if (helper.HasNext) {
                parentOid = (IOid) helper.GetNextEncodedToStrings();
            }
        }

        public virtual IOid ParentOid {
            get { return parentOid; }
        }

        public virtual string FieldName {
            get { return fieldName; }
        }

        #region IEncodedToStrings Members

        public string[] ToEncodedStrings() {
            var helper = new StringEncoderHelper();
            helper.Add(typeName);
            helper.Add(fieldName);
            if (parentOid != null) {
                helper.Add(parentOid as IEncodedToStrings);
            }
            return helper.ToArray();
        }

        public string[] ToShortEncodedStrings() {
            return ToEncodedStrings();
        }

        #endregion

        #region IOid Members

        public virtual IOid Previous {
            get { return null; }
        }

        public virtual bool IsTransient {
            get { return parentOid.IsTransient; }
        }

        public virtual void CopyFrom(IOid oid) {
            throw new NotImplementedException();
        }

        public ITypeSpec Spec {
            get { return metamodel.GetSpecification(typeName); }
        }

        public virtual bool HasPrevious {
            get { return false; }
        }

        #endregion

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            var otherOid = obj as AggregateOid;
            return otherOid != null && Equals(otherOid);
        }

        private bool Equals(AggregateOid otherOid) {
            return otherOid.parentOid.Equals(parentOid) &&
                   otherOid.fieldName.Equals(fieldName) &&
                   otherOid.typeName.Equals(typeName);
        }

        public override int GetHashCode() {
            int hashCode = 17;
            hashCode = 37*hashCode + parentOid.GetHashCode();
            hashCode = 37*hashCode + fieldName.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            return "AOID[" + parentOid + "," + fieldName + "]";
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
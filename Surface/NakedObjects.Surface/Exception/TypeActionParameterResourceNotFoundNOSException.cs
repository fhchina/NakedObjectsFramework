// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;

namespace NakedObjects.Surface {
    public class TypeActionParameterResourceNotFoundNOSException : ResourceNotFoundNOSException {
        public TypeActionParameterResourceNotFoundNOSException(string resourceId, string domainId, string parmId) : base(resourceId) {
            DomainId = domainId;
            ParmId = parmId;
        }

        public string DomainId { get; private set; }

        public string ParmId { get; private set; }

        public override string Message {
            get { return String.Format("No such parameter name RId:{0} DId {1} ", ResourceId, DomainId); }
        }
    }
}
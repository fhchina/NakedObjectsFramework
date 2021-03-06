// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using NakedObjects.Surface;
using RestfulObjects.Snapshot.Constants;
using RestfulObjects.Snapshot.Strategies;
using RestfulObjects.Snapshot.Utility;

namespace RestfulObjects.Snapshot.Representations {
    [DataContract]
    public class InlinePropertyRepresentation : InlineMemberAbstractRepresentation {
        protected InlinePropertyRepresentation(PropertyRepresentationStrategy strategy) : base(strategy.GetFlags()) {
            MemberType = MemberTypes.Property;
            Id = strategy.GetId();
            HasChoices = strategy.GetHasChoices();
            Links = strategy.GetLinks(true);
            Extensions = strategy.GetExtensions();
            SetHeader(strategy.GetTarget());
        }

        [DataMember(Name = JsonPropertyNames.HasChoices)]
        public bool HasChoices { get; set; }

        public static InlinePropertyRepresentation Create(HttpRequestMessage req, PropertyContextSurface propertyContext, IList<OptionalProperty> optionals, RestControlFlags flags) {
            if (!RestUtils.IsBlobOrClob(propertyContext.Specification) && !RestUtils.IsAttachment(propertyContext.Specification)) {
                optionals.Add(new OptionalProperty(JsonPropertyNames.Value, GetPropertyValue(req, propertyContext.Property, propertyContext.Target, flags)));
            }
            RestUtils.AddChoices(req, propertyContext, optionals, flags);
            return CreateWithOptionals<InlinePropertyRepresentation>(new object[] {new PropertyRepresentationStrategy(req, propertyContext, flags)}, optionals);
        }
    }
}
// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using NakedObjects.Surface;
using NakedObjects.Surface.Utility;
using RestfulObjects.Snapshot.Constants;
using RestfulObjects.Snapshot.Strategies;
using RestfulObjects.Snapshot.Utility;

namespace RestfulObjects.Snapshot.Representations {
    [DataContract]
    public class InlineCollectionRepresentation : InlineMemberAbstractRepresentation {
        protected InlineCollectionRepresentation(CollectionRepresentationStrategy strategy) : base(strategy.GetFlags()) {
            MemberType = MemberTypes.Collection;
            Id = strategy.GetId();
            Size = strategy.GetSize();
            Links = strategy.GetLinks(true);
            Extensions = strategy.GetExtensions();
            SetHeader(strategy.GetTarget());
        }

        [DataMember(Name = JsonPropertyNames.Size)]
        public int Size { get; set; }

        public static InlineCollectionRepresentation Create(HttpRequestMessage req, PropertyContextSurface propertyContext, IList<OptionalProperty> optionals, RestControlFlags flags) {
            if (propertyContext.Property.IsEager(propertyContext.Target) && !propertyContext.Target.IsTransient()) {
                IEnumerable<INakedObjectSurface> collectionItems = propertyContext.Property.GetNakedObject(propertyContext.Target).ToEnumerable();
                IEnumerable<LinkRepresentation> items = collectionItems.Select(i => LinkRepresentation.Create(new ValueRelType(propertyContext.Property, new UriMtHelper(req, i)), flags, new OptionalProperty(JsonPropertyNames.Title, RestUtils.SafeGetTitle(i))));
                optionals.Add(new OptionalProperty(JsonPropertyNames.Value, items.ToArray()));
            }

            var collectionRepresentationStrategy = new CollectionRepresentationStrategy(req, propertyContext, flags);
            if (optionals.Count == 0) {
                return new InlineCollectionRepresentation(collectionRepresentationStrategy);
            }
            return CreateWithOptionals<InlineCollectionRepresentation>(new object[] {collectionRepresentationStrategy}, optionals);
        }
    }
}
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
using RestfulObjects.Snapshot.Constants;
using RestfulObjects.Snapshot.Utility;

namespace RestfulObjects.Snapshot.Representations {
    [DataContract]
    public class VersionRepresentation : Representation {
        private VersionRepresentation(HttpRequestMessage req, IDictionary<string, string> capabilitiesMap, RestControlFlags flags) : base(flags) {
            SelfRelType = new VersionRelType(RelValues.Self, new UriMtHelper(req));
            SetScalars();
            SetLinks(new HomePageRelType(RelValues.Up, new UriMtHelper(req)));
            SetOptionalCapabilities(capabilitiesMap);
            SetExtensions();
            SetHeader();
        }

        [DataMember(Name = JsonPropertyNames.SpecVersion)]
        public string SpecVersion { get; set; }

        [DataMember(Name = JsonPropertyNames.ImplVersion)]
        public string ImplVersion { get; set; }

        [DataMember(Name = JsonPropertyNames.Links)]
        public LinkRepresentation[] Links { get; set; }

        [DataMember(Name = JsonPropertyNames.Extensions)]
        public MapRepresentation Extensions { get; set; }

        [DataMember(Name = JsonPropertyNames.OptionalCapabilities)]
        public MapRepresentation OptionalCapabilities { get; set; }

        private void SetHeader() {
            caching = CacheType.NonExpiring;
        }

        private void SetScalars() {
            SpecVersion = "1.1";
            ImplVersion = "1.5.0";
        }

        private void SetOptionalCapabilities(IDictionary<string, string> capabilitiesMap) {
            OptionalProperty[] properties = capabilitiesMap.Select(kvp => new OptionalProperty(kvp.Key, kvp.Value)).ToArray();
            OptionalCapabilities = MapRepresentation.Create(properties);
        }

        private void SetExtensions() {
            Extensions = new MapRepresentation();
        }

        private void SetLinks(HomePageRelType homePageRelType) {
            Links = new[] {LinkRepresentation.Create(SelfRelType, Flags), LinkRepresentation.Create(homePageRelType, Flags)};
        }

        public static VersionRepresentation Create(HttpRequestMessage req, IDictionary<string, string> capabilities, RestControlFlags flags) {
            return new VersionRepresentation(req, capabilities, flags);
        }
    }
}
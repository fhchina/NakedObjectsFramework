// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using NakedObjects.Surface;
using NakedObjects.Surface.Utility;
using RestfulObjects.Snapshot.Constants;
using RestfulObjects.Snapshot.Representations;
using RestfulObjects.Snapshot.Utility;

namespace RestfulObjects.Snapshot.Strategies {
    [DataContract]
    public class PropertyRepresentationStrategy : MemberRepresentationStrategy {
        public PropertyRepresentationStrategy(HttpRequestMessage req, PropertyContextSurface propertyContext, RestControlFlags flags) :
            base(req, propertyContext, flags) {}

        private void AddPrompt(List<LinkRepresentation> links) {
            if (propertyContext.Property.IsAutoCompleteEnabled || propertyContext.Property.GetChoicesParameters().Any()) {
                links.Add(CreatePromptLink());
            }
        }


        private LinkRepresentation CreatePromptLink() {
            var opts = new List<OptionalProperty>();

            if (propertyContext.Property.IsAutoCompleteEnabled) {
                var arguments = new OptionalProperty(JsonPropertyNames.Arguments, MapRepresentation.Create(new OptionalProperty(JsonPropertyNames.XRoSearchTerm, MapRepresentation.Create(new OptionalProperty(JsonPropertyNames.Value, null, typeof (object))))));
                var extensions = new OptionalProperty(JsonPropertyNames.Extensions, MapRepresentation.Create(new OptionalProperty(JsonPropertyNames.MinLength, propertyContext.Property.AutoCompleteMinLength())));

                opts.Add(arguments);
                opts.Add(extensions);
            }
            else {
                Tuple<string, INakedObjectSpecificationSurface>[] parms = propertyContext.Property.GetChoicesParameters();
                OptionalProperty[] args = parms.Select(pnt => RestUtils.CreateArgumentProperty(req, pnt, Flags)).ToArray();
                var arguments = new OptionalProperty(JsonPropertyNames.Arguments, MapRepresentation.Create(args));
                opts.Add(arguments);
            }

            return LinkRepresentation.Create(new PromptRelType(new UriMtHelper(req, propertyContext)), Flags, opts.ToArray());
        }

        private void AddMutatorLinks(List<LinkRepresentation> links) {
            if (propertyContext.Property.IsUsable(propertyContext.Target).IsAllowed) {
                links.Add(CreateModifyLink());

                if (!propertyContext.Property.IsMandatory()) {
                    links.Add(CreateClearLink());
                }
            }
        }

        private LinkRepresentation CreateClearLink() {
            return LinkRepresentation.Create(new MemberRelType(RelValues.Clear, new UriMtHelper(req, propertyContext)) {Method = RelMethod.Delete}, Flags);
        }

        private LinkRepresentation CreateModifyLink() {
            return LinkRepresentation.Create(new MemberRelType(RelValues.Modify, new UriMtHelper(req, propertyContext)) {Method = RelMethod.Put}, Flags,
                new OptionalProperty(JsonPropertyNames.Arguments, MapRepresentation.Create(new OptionalProperty(JsonPropertyNames.Value, null, typeof (object)))));
        }


        public new LinkRepresentation[] GetLinks(bool inline) {
            var links = new List<LinkRepresentation>(base.GetLinks(inline));

            if (!propertyContext.Target.IsTransient()) {
                AddMutatorLinks(links);
                AddPrompt(links);
            }

            return links.ToArray();
        }


        private IDictionary<string, object> GetCustomPropertyExtensions() {
            IDictionary<string, object> custom = propertyContext.Property.ExtensionData();

            if (IsUnconditionalChoices()) {
                Tuple<INakedObjectSurface, string>[] choices = propertyContext.Property.GetChoicesAndTitles(propertyContext.Target, null);
                Tuple<object, string>[] choicesArray = choices.Select(tuple => new Tuple<object, string>(RestUtils.GetChoiceValue(req, tuple.Item1, propertyContext.Property, Flags), tuple.Item2)).ToArray();

                OptionalProperty[] op = choicesArray.Select(tuple => new OptionalProperty(tuple.Item2, tuple.Item1)).ToArray();
                MapRepresentation map = MapRepresentation.Create(op);

                custom = custom ?? new Dictionary<string, object>();
                custom[JsonPropertyNames.CustomChoices] = map;
            }

            string mask = propertyContext.Property.Mask();

            if (!string.IsNullOrWhiteSpace(mask)) {
                custom = custom ?? new Dictionary<string, object>();
                custom[JsonPropertyNames.CustomMask] = mask;
            }

            return custom;
        }

        private bool IsUnconditionalChoices() {
            return propertyContext.Property.IsChoicesEnabled &&
                   (propertyContext.Property.Specification.IsParseable() || (propertyContext.Property.Specification.IsCollection() && propertyContext.Property.ElementSpecification.IsParseable())) &&
                   !propertyContext.Property.GetChoicesParameters().Any();
        }

        protected override MapRepresentation GetExtensionsForSimple() {
            return RestUtils.GetExtensions(friendlyname: propertyContext.Property.Name(),
                description: propertyContext.Property.Description(),
                pluralName: null,
                domainType: null,
                isService: null,
                hasParams: null,
                optional: !propertyContext.Property.IsMandatory(),
                maxLength: propertyContext.Property.MaxLength(),
                pattern: propertyContext.Property.Pattern(),
                memberOrder: propertyContext.Property.MemberOrder(),
                customExtensions: GetCustomPropertyExtensions(),
                returnType: propertyContext.Specification,
                elementType: propertyContext.ElementSpecification );
        }

        public bool GetHasChoices() {
            return propertyContext.Property.IsChoicesEnabled && !propertyContext.Property.GetChoicesParameters().Any();
        }
    }
}
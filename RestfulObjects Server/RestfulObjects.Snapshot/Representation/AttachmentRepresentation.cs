﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using NakedObjects.Surface;
using NakedObjects.Surface.Interface;
using RestfulObjects.Snapshot.Utility;

namespace RestfulObjects.Snapshot.Representations {
    public class AttachmentRepresentation : Representation {
        private MediaTypeHeaderValue contentType;

        public AttachmentRepresentation(HttpRequestMessage req, PropertyContextSurface propertyContext, RestControlFlags flags)
            : base(flags) {
            SetContentType(propertyContext);
            SetContentDisposition(propertyContext);
            SetStream(propertyContext);
            SetHeader(propertyContext.Target);
        }

        public ContentDispositionHeaderValue ContentDisposition { get; set; }

        public Stream AsStream { get; set; }

        public override HttpResponseMessage GetAsMessage(MediaTypeFormatter formatter, Tuple<int, int, int> cacheSettings) {
            var content = new StreamContent(AsStream);
            var msg = new HttpResponseMessage {Content = content};
            msg.Content.Headers.ContentDisposition = ContentDisposition;

            msg.Content.Headers.ContentType = GetContentType();

            SetCaching(msg, cacheSettings);
            return msg;
        }

        public override MediaTypeHeaderValue GetContentType() {
            return contentType;
        }

        private void SetHeader(INakedObjectSurface target) {
            caching = CacheType.Transactional;
            SetEtag(target);
        }

        private void SetContentType(PropertyContextSurface context) {
            INakedObjectSurface no = context.Property.GetNakedObject(context.Target);
            string mtv = no != null ? no.GetAttachment().MimeType : AttachmentContext.DefaultMimeType;
            contentType = new MediaTypeHeaderValue(mtv);
        }

        private void SetContentDisposition(PropertyContextSurface context) {
            INakedObjectSurface no = context.Property.GetNakedObject(context.Target);
            string cd = no != null ? no.GetAttachment().ContentDisposition : AttachmentContext.DefaultContentDisposition;
            string fn = no != null ? no.GetAttachment().FileName : AttachmentContext.DefaultFileName;
            ContentDisposition = new ContentDispositionHeaderValue(cd) {FileName = fn};
        }

        private void SetStream(PropertyContextSurface context) {
            INakedObjectSurface no = context.Property.GetNakedObject(context.Target);
            AsStream = no != null ? no.GetAttachment().Content : new MemoryStream();
        }


        public static Representation Create(HttpRequestMessage req, PropertyContextSurface propertyContext, RestControlFlags flags) {
            return new AttachmentRepresentation(req, propertyContext, flags);
        }
    }
}
﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Linq;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Spec;
using NakedObjects.Surface.Context;
using NakedObjects.Surface.Nof4.Wrapper;

namespace NakedObjects.Surface.Nof4.Context {
    public class ListContext {
        public INakedObject[] List { get; set; }
        public IObjectSpec ElementType { get; set; }
        public bool IsListOfServices { get; set; }

        public ListContextSurface ToListContextSurface(INakedObjectsSurface surface, INakedObjectsFramework framework) {
            return new ListContextSurface {
                ElementType = new NakedObjectSpecificationWrapper(ElementType, surface, framework),
                List = List.Select(no => NakedObjectWrapper.Wrap(no, surface, framework)).Cast<INakedObjectSurface>().ToArray(),
                IsListOfServices = IsListOfServices
            };
        }
    }
}
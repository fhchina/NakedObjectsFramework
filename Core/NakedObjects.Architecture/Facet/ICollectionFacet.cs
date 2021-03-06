// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Linq;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;

namespace NakedObjects.Architecture.Facet {
    public interface ICollectionFacet : IFacet {
        bool IsASet { get; }
        bool IsQueryable { get; }
        bool Contains(INakedObject collection, INakedObject element);
        INakedObject Page(int page, int size, INakedObject collection, INakedObjectManager manager, bool forceEnumerable);
        IEnumerable<INakedObject> AsEnumerable(INakedObject collection, INakedObjectManager manager);
        IQueryable AsQueryable(INakedObject objectRepresentingCollection);
        void Init(INakedObject collection, INakedObject[] initData);
    }

    // Copyright (c) Naked Objects Group Ltd.
}
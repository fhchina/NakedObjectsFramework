// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Meta.Facet {
    /// <summary>
    ///     Adapter superclass for <see cref="IFacet" />s for <see cref="ICallbackFacet" />
    /// </summary>
    /// 
    [Serializable]
    public abstract class CallbackFacetAbstract : FacetAbstract, ICallbackFacet {
        protected CallbackFacetAbstract(Type facetType, ISpecification holder)
            : base(facetType, holder) {}

        #region ICallbackFacet Members

        public abstract void Invoke(INakedObject nakedObject, ISession session, ILifecycleManager lifecycleManager, IMetamodelManager metamodelManager);

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}
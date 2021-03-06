// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using NakedObjects.Architecture;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;

namespace NakedObjects.Core.Spec {
    internal class ObjectSpec : TypeSpec, IObjectSpec {
        private IActionSpec[] collectionContributedActions;
        private IActionSpec[] combinedActions;
        private IActionSpec[] contributedActions;
        private IActionSpec[] finderActions;
        private IAssociationSpec[] objectFields;

        public ObjectSpec(SpecFactory memberFactory, IMetamodelManager metamodelManager, INakedObjectManager nakedObjectManager, IObjectSpecImmutable innerSpec) :
            base(memberFactory, metamodelManager, nakedObjectManager, innerSpec) {}

        private IActionSpec[] ContributedActions {
            get { return contributedActions ?? (contributedActions = MemberFactory.CreateActionSpecs(InnerSpec.ContributedActions)); }
        }

        #region IObjectSpec Members

        public virtual IAssociationSpec[] Properties {
            get { return objectFields ?? (objectFields = InnerSpec.Fields.Select(element => MemberFactory.CreateAssociationSpec(element)).ToArray()); }
        }

        public IAssociationSpec GetProperty(string id) {
            try {
                return Properties.First(f => f.Id.Equals(id));
            }
            catch (InvalidOperationException) {
                throw new ReflectionException(string.Format("No field called '{0}' in '{1}'", id, SingularName));
            }
        }

        public override IActionSpec[] GetObjectActions() {
            if (combinedActions == null) {
                var ca = new List<IActionSpec>();
                ca.AddRange(ObjectActions);
                ca.AddRange(ContributedActions);
                combinedActions = ca.ToArray();
            }
            return combinedActions;
        }

        public IActionSpec[] GetCollectionContributedActions() {
            return collectionContributedActions ?? (collectionContributedActions = MemberFactory.CreateActionSpecs(InnerSpec.CollectionContributedActions));
        }

        public IActionSpec[] GetFinderActions() {
            return finderActions ?? (finderActions = MemberFactory.CreateActionSpecs(InnerSpec.FinderActions));
        }

        #endregion

        protected override PersistableType GetPersistable() {
            if (InnerSpec.ContainsFacet<INotPersistedFacet>()) {
                return PersistableType.Transient;
            }
            if (InnerSpec.ContainsFacet<IProgramPersistableOnlyFacet>()) {
                return PersistableType.ProgramPersistable;
            }
            return PersistableType.UserPersistable;
        }
    }
}

// Copyright (c) Naked Objects Group Ltd.
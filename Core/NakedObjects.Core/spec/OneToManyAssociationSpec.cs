// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Core.Resolve;
using NakedObjects.Core.Util;

namespace NakedObjects.Core.Spec {
    internal class OneToManyAssociationSpec : AssociationSpecAbstract, IOneToManyAssociationSpec {
        private readonly IObjectSpec elementSpec;
        private readonly bool isASet;
        private readonly IObjectPersistor persistor;

        public OneToManyAssociationSpec(IMetamodelManager metamodel, IOneToManyAssociationSpecImmutable association, ISession session, ILifecycleManager lifecycleManager, INakedObjectManager manager, IObjectPersistor persistor)
            : base(metamodel, association, session, lifecycleManager, manager) {
            this.persistor = persistor;
            isASet = association.ContainsFacet<IIsASetFacet>();

            elementSpec = (IObjectSpec) MetamodelManager.GetSpecification(association.ElementSpec);
        }

        public override bool IsChoicesEnabled {
            get { return false; }
        }

        public override bool IsAutoCompleteEnabled {
            get { return false; }
        }

        #region IOneToManyAssociationSpec Members

        public override INakedObject GetNakedObject(INakedObject inObject) {
            return GetCollection(inObject);
        }

        public override IObjectSpec ElementSpec {
            get { return elementSpec; }
        }

        public override bool IsASet {
            get { return isASet; }
        }

        public override bool IsEmpty(INakedObject inObject) {
            return Count(inObject) == 0;
        }

        public virtual int Count(INakedObject inObject) {
            return persistor.CountField(inObject, Id);
        }

        public override bool IsInline {
            get { return false; }
        }

        public override bool IsMandatory {
            get { return false; }
        }

        public override INakedObject GetDefault(INakedObject nakedObject) {
            return null;
        }

        public override TypeOfDefaultValue GetDefaultType(INakedObject nakedObject) {
            return TypeOfDefaultValue.Implicit;
        }

        public override void ToDefault(INakedObject target) {}

        #endregion

        public override INakedObject[] GetChoices(INakedObject nakedObject, IDictionary<string, INakedObject> parameterNameValues) {
            return new INakedObject[0];
        }

        public override Tuple<string, IObjectSpec>[] GetChoicesParameters() {
            return new Tuple<string, IObjectSpec>[0];
        }

        public override INakedObject[] GetCompletions(INakedObject nakedObject, string autoCompleteParm) {
            return new INakedObject[0];
        }

        private INakedObject GetCollection(INakedObject inObject) {
            object collection = GetFacet<IPropertyAccessorFacet>().GetProperty(inObject);
            if (collection == null) {
                return null;
            }
            INakedObject adapterFor = Manager.CreateAggregatedAdapter(inObject, ((IAssociationSpec) this).Id, collection);
            adapterFor.TypeOfFacet = GetFacet<ITypeOfFacet>();
            SetResolveStateForDerivedCollections(adapterFor);
            return adapterFor;
        }

        private void SetResolveStateForDerivedCollections(INakedObject adapterFor) {
            bool isDerived = !IsPersisted;
            if (isDerived && !adapterFor.ResolveState.IsResolved()) {
                if (adapterFor.GetAsEnumerable(Manager).Any()) {
                    adapterFor.ResolveState.Handle(Events.StartResolvingEvent);
                    adapterFor.ResolveState.Handle(Events.EndResolvingEvent);
                }
            }
        }

        public override string ToString() {
            var str = new AsString(this);
            str.Append(base.ToString());
            str.Append(",");
            str.Append("persisted", IsPersisted);
            str.Append("type", ReturnSpec == null ? "unknown" : ReturnSpec.ShortName);
            return str.ToString();
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
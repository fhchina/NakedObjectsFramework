// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets.Objects.Aggregated;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Resolve;
using NakedObjects.Architecture.Security;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.Util;
using NakedObjects.Core.Context;
using NakedObjects.Core.Persist;
using NakedObjects.Core.Util;
using NakedObjects.Persistor.Objectstore;
using NakedObjects.Persistor.Transaction;
using NakedObjects.Util;

namespace NakedObjects.Managers {
    public class ObjectPersistor : IObjectPersistor {
        private static readonly ILog Log;
        private readonly INakedObjectManager manager;
        private readonly INakedObjectStore objectStore;
        private readonly ISession session;
        private readonly INakedObjectTransactionManager transactionManager;
        private readonly IUpdateNotifier updateNotifier;

        static ObjectPersistor() {
            Log = LogManager.GetLogger(typeof (ObjectPersistor));
        }

        public ObjectPersistor(INakedObjectStore objectStore,
                               INakedObjectTransactionManager transactionManager,
                               ISession session,
                               INakedObjectManager manager,
                               IUpdateNotifier updateNotifier) {
            this.objectStore = objectStore;
            this.transactionManager = transactionManager;
            this.session = session;
            this.manager = manager;
            this.updateNotifier = updateNotifier;
        }

        #region IObjectPersistor Members

        public virtual IQueryable<T> Instances<T>() where T : class {
            Log.DebugFormat("Instances<T> of: {0}", typeof (T));
            return GetInstances<T>();
        }

        public virtual IQueryable Instances(Type type) {
            Log.DebugFormat("Instances of: {0}", type);
            return GetInstances(type);
        }

        public virtual IQueryable Instances(INakedObjectSpecification specification) {
            Log.DebugFormat("Instances of: {0}", specification);
            return GetInstances(specification);
        }

        public INakedObject LoadObject(IOid oid, INakedObjectSpecification specification) {
            Log.DebugFormat("LoadObject oid: {0} specification: {1}", oid, specification);

            Assert.AssertNotNull("needs an OID", oid);
            Assert.AssertNotNull("needs a specification", specification);

            return objectStore.GetObject(oid, specification);
        }

        public void AddPersistedObject(INakedObject nakedObject) {
            if (nakedObject.Specification.ContainsFacet(typeof (IComplexTypeFacet))) {
                return;
            }
            ICreateObjectCommand createObjectCommand = objectStore.CreateCreateObjectCommand(nakedObject, session);
            transactionManager.AddCommand(createObjectCommand);
        }

        public void Reload(INakedObject nakedObject) {
            Log.DebugFormat("Reload nakedObject: {0}", nakedObject);
            objectStore.Reload(nakedObject);
        }

        public void ResolveField(INakedObject nakedObject, INakedObjectAssociation field) {
            Log.DebugFormat("ResolveField nakedObject: {0} field: {1}", nakedObject, field);
            if (field.Specification.HasNoIdentity) {
                return;
            }
            INakedObject reference = field.GetNakedObject(nakedObject);
            if (reference == null || reference.ResolveState.IsResolved()) {
                return;
            }
            if (!reference.ResolveState.IsPersistent()) {
                return;
            }
            if (Log.IsInfoEnabled) {
                // don't log object - its ToString() may use the unresolved field or unresolved collection
                Log.Info("resolve field " + nakedObject.Specification.ShortName + "." + field.Id + ": " + reference.Specification.ShortName + " " + reference.ResolveState.CurrentState.Code + " " + reference.Oid);
            }
            objectStore.ResolveField(nakedObject, field);
        }

        public void LoadField(INakedObject nakedObject, string field) {
            Log.DebugFormat("LoadField nakedObject: {0} field: {1}", nakedObject, field);
            INakedObjectAssociation association = nakedObject.Specification.Properties.Single(x => x.Id == field);
            ResolveField(nakedObject, association);
        }

        public int CountField(INakedObject nakedObject, string field) {
            Log.DebugFormat("CountField nakedObject: {0} field: {1}", nakedObject, field);

            INakedObjectAssociation association = nakedObject.Specification.Properties.Single(x => x.Id == field);

            if (nakedObject.Specification.IsViewModel) {
                INakedObject collection = association.GetNakedObject(nakedObject);
                return collection.GetCollectionFacetFromSpec().AsEnumerable(collection, manager).Count();
            }

            return objectStore.CountField(nakedObject, association);
        }

        public PropertyInfo[] GetKeys(Type type) {
            Log.Debug("GetKeys of: " + type);
            return objectStore.GetKeys(type);
        }

        public INakedObject FindByKeys(Type type, object[] keys) {
            Log.Debug("FindByKeys");
            return objectStore.FindByKeys(type, keys);
        }

        public void Refresh(INakedObject nakedObject) {
            Log.DebugFormat("Refresh nakedObject: {0}", nakedObject);
            objectStore.Refresh(nakedObject);
        }

        public void ResolveImmediately(INakedObject nakedObject) {
            Log.DebugFormat("ResolveImmediately nakedObject: {0}", nakedObject);
            if (nakedObject.ResolveState.IsResolvable()) {
                Assert.AssertFalse("only resolve object that is not yet resolved", nakedObject, nakedObject.ResolveState.IsResolved());
                Assert.AssertTrue("only resolve object that is persistent", nakedObject, nakedObject.ResolveState.IsPersistent());
                if (nakedObject.Oid is AggregateOid) {
                    return;
                }
                if (Log.IsInfoEnabled) {
                    // don't log object - it's ToString() may use the unresolved field, or unresolved collection
                    Log.Info("resolve immediately: " + nakedObject.Specification.ShortName + " " + nakedObject.ResolveState.CurrentState.Code + " " + nakedObject.Oid);
                }
                objectStore.ResolveImmediately(nakedObject);
            }
        }

        public void ObjectChanged(INakedObject nakedObject) {
            Log.DebugFormat("ObjectChanged nakedObject: {0}", nakedObject);
            if (nakedObject.ResolveState.RespondToChangesInPersistentObjects()) {
                if (nakedObject.Specification.ContainsFacet(typeof (IComplexTypeFacet))) {
                    nakedObject.Updating(session);
                    nakedObject.Updated(session);
                    updateNotifier.AddChangedObject(nakedObject);
                }
                else {
                    INakedObjectSpecification specification = nakedObject.Specification;
                    if (specification.IsAlwaysImmutable() || (specification.IsImmutableOncePersisted() && nakedObject.ResolveState.IsPersistent())) {
                        throw new NotPersistableException("cannot change immutable object");
                    }
                    nakedObject.Updating(session);
                    ISaveObjectCommand saveObjectCommand = objectStore.CreateSaveObjectCommand(nakedObject, session);
                    transactionManager.AddCommand(saveObjectCommand);
                    nakedObject.Updated(session);
                    updateNotifier.AddChangedObject(nakedObject);
                }
            }

            if (nakedObject.ResolveState.RespondToChangesInPersistentObjects() ||
                nakedObject.ResolveState.IsTransient()) {
                updateNotifier.AddChangedObject(nakedObject);
            }
        }

        public void DestroyObject(INakedObject nakedObject) {
            Log.DebugFormat("DestroyObject nakedObject: {0}", nakedObject);

            nakedObject.Deleting(session);
            IDestroyObjectCommand command = objectStore.CreateDestroyObjectCommand(nakedObject);
            transactionManager.AddCommand(command);
            nakedObject.ResolveState.Handle(Events.DestroyEvent);
            nakedObject.Deleted(session);
        }

        public object CreateObject(INakedObjectSpecification specification) {
            Log.DebugFormat("CreateObject: " + specification);

            Type type = TypeUtils.GetType(specification.FullName);
            return objectStore.CreateInstance(type);
        }

        #endregion

        protected IQueryable<T> GetInstances<T>() where T : class {
            Log.Debug("GetInstances<T> of: " + typeof (T));
            return objectStore.GetInstances<T>();
        }

        protected IQueryable GetInstances(Type type) {
            Log.Debug("GetInstances of: " + type);
            return objectStore.GetInstances(type);
        }

        protected IQueryable GetInstances(INakedObjectSpecification specification) {
            Log.Debug("GetInstances<T> of: " + specification);
            return objectStore.GetInstances(specification);
        }

        private static IEnumerable<INakedObjectSpecification> GetLeafNodes(INakedObjectSpecification spec) {
            if ((spec.IsInterface || spec.IsAbstract)) {
                return spec.Subclasses.SelectMany(GetLeafNodes);
            }
            return new[] { spec };
        }

        public IEnumerable GetBoundedSet(INakedObjectSpecification spec) {
            if (spec.IsBoundedSet()) {
                if (spec.IsInterface) {
                    IList<object> instances = new List<object>();
                    foreach (INakedObjectSpecification subSpec in GetLeafNodes(spec)) {
                        foreach (object instance in Instances(subSpec)) {
                            instances.Add(instance);
                        }
                    }
                    return instances;
                }
                return Instances(spec);
            }
            return new object[] { };
        }

    }
}
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
using System.Security.Principal;
using Common.Logging;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Objects.Bounded;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Security;
using NakedObjects.Architecture.Spec;
using NakedObjects.Boot;
using NakedObjects.Core.Adapter.Map;
using NakedObjects.Core.Context;
using NakedObjects.Core.NakedObjectsSystem;
using NakedObjects.Core.Reflect;
using NakedObjects.EntityObjectStore;
using NakedObjects.Objects;
using NakedObjects.Persistor;
using NakedObjects.Persistor.Objectstore;
using NakedObjects.Reflector.DotNet;
using NakedObjects.Reflector.DotNet.Facets;
using NakedObjects.Reflector.DotNet.Reflect;
using NakedObjects.Reflector.DotNet.Reflect.Strategy;
using NakedObjects.Service;

namespace NakedObjects.Xat {
    public abstract class AcceptanceTestCase {
        private static readonly ILog Log;
        private readonly Lazy<IUnityContainer> unityContainer;
        private INakedObjectsFramework nakedObjectsFramework;
        private IDictionary<string, ITestService> servicesCache = new Dictionary<string, ITestService>();
        private ITestObjectFactory testObjectFactory;
        private IPrincipal testPrincipal;
        private ISession testSession;

        static AcceptanceTestCase() {
            Log = LogManager.GetLogger(typeof (AcceptanceTestCase));
        }

        protected AcceptanceTestCase(string name) {
            Name = name;

            unityContainer = new Lazy<IUnityContainer>(() => {
                var c = new UnityContainer();
                RegisterTypes(c);
                return c;
            });
        }

        protected AcceptanceTestCase() : this("Unnamed") {}

        protected string Name { set; get; }

        protected virtual ITestObjectFactory TestObjectFactoryClass {
            get {
                if (testObjectFactory == null) {
                    testObjectFactory = new TestObjectFactory(NakedObjectsFramework.Metamodel, NakedObjectsFramework.Session, NakedObjectsFramework.LifecycleManager);
                }
                return testObjectFactory;
            }
        }

        protected virtual ISession TestSession {
            get {
                if (testSession == null) {
                    testSession = new TestSession(TestPrincipal);
                }
                return testSession;
            }
            set { testSession = value; }
        }

        protected virtual IPrincipal TestPrincipal {
            get {
                if (testPrincipal == null) {
                    testPrincipal = CreatePrincipal("Test", new string[] {});
                }
                return testPrincipal;
            }
            set { testPrincipal = value; }
        }

        [Obsolete("Use NakedObjectsFramework")]
        protected INakedObjectsFramework NakedObjectsContext {
            get { return nakedObjectsFramework; }
        }

        protected INakedObjectsFramework NakedObjectsFramework {
            get { return nakedObjectsFramework; }
        }

        protected virtual IFixturesInstaller Fixtures {
            get { return new FixturesInstaller(new object[] {}); }
        }

        protected virtual IServicesInstaller MenuServices {
            get { return new ServicesInstaller(new object[] {}); }
        }

        protected virtual IServicesInstaller ContributedActions {
            get { return new ServicesInstaller(new object[] {}); }
        }

        protected virtual IServicesInstaller SystemServices {
            get { return new ServicesInstaller(new object[] {}); }
        }

        protected virtual IObjectPersistorInstaller Persistor {
            get { return null; }
        }

        protected void StartTest() {
            nakedObjectsFramework = GetConfiguredContainer().Resolve<INakedObjectsFramework>();
        }

        protected void RunFixtures() {
            if (nakedObjectsFramework == null) {
                 nakedObjectsFramework = GetConfiguredContainer().Resolve<INakedObjectsFramework>();
            }
            Fixtures.InstallFixtures(nakedObjectsFramework.LifecycleManager, nakedObjectsFramework.Injector); 
        }

        protected ITestService GetTestService(Type type) {
            return NakedObjectsFramework.LifecycleManager.GetServices().
                Where(no => type.IsAssignableFrom(no.Object.GetType())).
                Select(no => TestObjectFactoryClass.CreateTestService(no.Object)).
                FirstOrDefault();
        }

        protected ITestService GetTestService(string serviceName) {
            if (!servicesCache.ContainsKey(serviceName.ToLower())) {
                foreach (INakedObject service in NakedObjectsFramework.LifecycleManager.GetServices()) {
                    if (service.TitleString().Equals(serviceName, StringComparison.CurrentCultureIgnoreCase)) {
                        ITestService testService = TestObjectFactoryClass.CreateTestService(service.Object);
                        if (testService == null) {
                            Assert.Fail("Invalid service name " + serviceName);
                        }
                        servicesCache[serviceName.ToLower()] = testService;
                        return testService;
                    }
                }
                Assert.Fail("No such service: " + serviceName);
            }
            return servicesCache[serviceName.ToLower()];
        }

        protected ITestObject GetBoundedInstance<T>(string title) {
            return GetBoundedInstance(typeof (T), title);
        }

        protected ITestObject GetBoundedInstance(Type type, string title) {
            INakedObjectSpecification spec = NakedObjectsFramework.Metamodel.GetSpecification(type);
            return GetBoundedInstance(title, spec);
        }

        protected ITestObject GetBoundedInstance(string classname, string title) {
            INakedObjectSpecification spec = NakedObjectsFramework.Metamodel.GetSpecification(classname);
            return GetBoundedInstance(title, spec);
        }

        private ITestObject GetBoundedInstance(string title, INakedObjectSpecification spec) {
            if (spec.GetFacet<IBoundedFacet>() == null) {
                Assert.Fail(spec.SingularName + " is not a Bounded type");
            }
            IEnumerable allInstances = NakedObjectsFramework.LifecycleManager.Instances(spec);
            object inst = allInstances.Cast<object>().Single(o => NakedObjectsFramework.LifecycleManager.CreateAdapter(o, null, null).TitleString() == title);
            return TestObjectFactoryClass.CreateTestObject(NakedObjectsFramework.LifecycleManager.CreateAdapter(inst, null, null));
        }

        private static IPrincipal CreatePrincipal(string name, string[] roles) {
            return new GenericPrincipal(new GenericIdentity(name), roles);
        }

        protected void SetUser(string username, params string[] roles) {
            testPrincipal = CreatePrincipal(username, roles);
            var ts = TestSession as TestSession;
            if (ts != null) {
                ts.ReplacePrincipal(testPrincipal);
            }
        }

        protected void SetUser(string username) {
            SetUser(username, new string[] {});
        }

        protected static void InitializeNakedObjectsFramework(AcceptanceTestCase tc) {
            Log.Info("test initialize " + tc.Name);
            tc.servicesCache = new Dictionary<string, ITestService>();

            var reflector = tc.GetConfiguredContainer().Resolve<INakedObjectReflector>();

            List<Type> s1 = tc.MenuServices.GetServices().Select(s => s.GetType()).ToList();
            List<Type> s2 = tc.ContributedActions.GetServices().Select(s => s.GetType()).ToList();
            List<Type> s3 = tc.SystemServices.GetServices().Select(s => s.GetType()).ToList();
            Type[] services = s1.Union(s2).Union(s3).ToArray();

            reflector.InstallServiceSpecifications(services);
            reflector.PopulateContributedActions(s1.Union(s2).ToArray());
        }

        protected static void CleanupNakedObjectsFramework(AcceptanceTestCase tc) {
            Log.Info("test cleanup " + tc.Name);
            Log.Info("cleanup " + tc.Name);

            tc.servicesCache.Clear();
            tc.servicesCache = null;
            tc.testObjectFactory = null;
        }


        protected virtual void RegisterTypes(IUnityContainer container) {
            container.RegisterType<IClassStrategy, DefaultClassStrategy>();
            container.RegisterType<IFacetFactorySet, FacetFactorySetImpl>();

            container.RegisterType<INakedObjectReflector, DotNetReflector>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMetamodel, DotNetReflector>(new ContainerControlledLifetimeManager());

            container.RegisterType<IPrincipal>(new InjectionFactory(c => TestPrincipal));

            var config = new EntityObjectStoreConfiguration();

            //config.UsingEdmxContext("Model").AssociateTypes(AdventureWorksTypes);
            //config.SpecifyTypesNotAssociatedWithAnyContext(() => new[] { typeof(AWDomainObject) });

            container.RegisterInstance<IEntityObjectStoreConfiguration>(config, (new ContainerControlledLifetimeManager()));

            var serviceConfig = new ServicesConfiguration();

            serviceConfig.AddMenuServices(MenuServices.GetServices());
            serviceConfig.AddContributedActions(ContributedActions.GetServices());
            serviceConfig.AddSystemServices(SystemServices.GetServices());

            container.RegisterInstance <IServicesConfiguration> (serviceConfig, new ContainerControlledLifetimeManager());

            container.RegisterType<NakedObjectFactory, NakedObjectFactory>(new PerResolveLifetimeManager());
            container.RegisterType<IPocoAdapterMap, PocoAdapterHashMap>(new PerResolveLifetimeManager(), new InjectionConstructor(10));
            container.RegisterType<IIdentityAdapterMap, IdentityAdapterHashMap>(new PerResolveLifetimeManager(), new InjectionConstructor(10));

            container.RegisterType<IContainerInjector, DotNetDomainObjectContainerInjector>(new PerResolveLifetimeManager());

            container.RegisterType<IOidGenerator, EntityOidGenerator>(new PerResolveLifetimeManager());
            
            container.RegisterType<INakedObjectStore, EntityObjectStore.EntityObjectStore>(new PerResolveLifetimeManager());
            container.RegisterType<IIdentityMap, EntityIdentityMapImpl>(new PerResolveLifetimeManager());

            container.RegisterType<INakedObjectTransactionManager, ObjectStoreTransactionManager>(new PerResolveLifetimeManager());
            container.RegisterType<INakedObjectManager, NakedObjectManager>(new PerResolveLifetimeManager());
            container.RegisterType<IObjectPersistor, ObjectPersistor>(new PerResolveLifetimeManager());
            container.RegisterType<IServicesManager, ServicesManager>(new PerResolveLifetimeManager());
            container.RegisterType<IPersistAlgorithm, EntityPersistAlgorithm>(new PerResolveLifetimeManager());

            container.RegisterType<IAuthorizationManager, NullAuthorizationManager>(new PerResolveLifetimeManager());
            container.RegisterType<ILifecycleManager, LifeCycleManager>(new PerResolveLifetimeManager());

            container.RegisterType<ISession>(new PerResolveLifetimeManager(), new InjectionFactory(c => TestSession));

            container.RegisterType<IUpdateNotifier, SimpleUpdateNotifier>(new PerResolveLifetimeManager());
            container.RegisterType<IMessageBroker, SimpleMessageBroker>(new PerResolveLifetimeManager());

            container.RegisterType<INakedObjectsFramework, NakedObjectsFramework>();
        }

        /// <summary>
        ///     Gets the configured Unity unityContainer.
        /// </summary>
        protected IUnityContainer GetConfiguredContainer() {
            return unityContainer.Value;
        }
    }

    // Copyright (c) INakedObject Objects Group Ltd.
}
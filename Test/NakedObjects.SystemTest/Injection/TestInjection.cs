﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Core.Container;
using NakedObjects.Services;

namespace NakedObjects.SystemTest.Injection {
    [TestClass]
    public class TestInjection : AbstractSystemTest<InjectionDbContext> {
        #region Setup/Teardown

        

        [ClassCleanup]
        public static void ClassCleanup() {
            CleanupNakedObjectsFramework(new TestInjection());
            Database.Delete(InjectionDbContext.DatabaseName);
        }

        [TestInitialize()]
        public void TestInitialize() {
            InitializeNakedObjectsFrameworkOnce();
            StartTest();
        }


        #endregion

        protected override Type[] Types {
            get { return new Type[] { typeof(Object1), typeof(Object2), typeof(Service1), typeof(Service2) ,typeof(Service3)}; }
        }

        protected override object[] MenuServices {
            get {
                return (new object[] {
                    new SimpleRepository<Object1>(),
                    new SimpleRepository<Object2>(),
                    new Service1(),
                    new ServiceImplementation()
                });
            }
        }

        [TestMethod]
        public void InjectContainer() {
            var testObject = (Object1) NewTestObject<Object1>().GetDomainObject();
            Assert.IsNotNull(testObject.Container);
            Assert.IsInstanceOfType(testObject.Container, typeof (IDomainObjectContainer));
        }

        [TestMethod]
        public void InjectService() {
            var testObject = (Object2) NewTestObject<Object2>().GetDomainObject();
            Assert.IsNotNull(testObject.GetService1());
            Assert.IsInstanceOfType(testObject.GetService1(), typeof (Service1));
        }

        [TestMethod]
        public void InjectServiceDefinedByInterface() {
            var testObject = (Object2) NewTestObject<Object2>().GetDomainObject();
            Assert.IsNotNull(testObject.GetService2());
            Assert.IsInstanceOfType(testObject.GetService2(), typeof (ServiceImplementation));
            Assert.IsNotNull(testObject.GetService3());
            Assert.IsInstanceOfType(testObject.GetService3(), typeof (ServiceImplementation));
        }

        [TestMethod]
        public void InjectedPropertiesAreHidden() {
            var obj = NewTestObject<Object2>();
            try {
                obj.GetPropertyByName("My Service1");
                Assert.Fail();
            }
            catch (Exception e) {
                Assert.IsNotNull(e);
            }

            var prop = obj.GetPropertyByName("Id");
            prop.AssertIsVisible();
        }
    }

    #region Domain classes used by tests

    public class InjectionDbContext : DbContext {
        public const string DatabaseName = "TestInjection";
        public InjectionDbContext() : base(DatabaseName) {}

        public DbSet<Object1> Object1 { get; set; }
        public DbSet<Object2> Object2 { get; set; }
        public DbSet<Object3> Object3 { get; set; }
    }

    public class Object1 {
        public IDomainObjectContainer Container { get; set; }

        public virtual int Id { get; set; }
    }

    public class Object2 {
        public Service1 MyService1 { protected get; set; }
        public Service2 MyService2 { protected get; set; }
        public Service3 MyService3 { protected get; set; }

        public virtual int Id { get; set; }

        public object GetService1() {
            return MyService1;
        }

        public object GetService2() {
            return MyService2;
        }

        public object GetService3() {
            return MyService3;
        }

    }

    public class Object3 {
        public Service1 MyService1 { protected get; set; }
        public NotRegisteredService MyService2 { protected get; set; }

        public virtual int Id { get; set; }
    }

    public class Service1 {}

    public interface Service2 {}

    public interface Service3 {}

    public class NotRegisteredService {}

    public class ServiceImplementation : Service2, Service3 {}

    #endregion
}
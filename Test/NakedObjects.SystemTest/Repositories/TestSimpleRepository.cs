﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Services;
using NakedObjects.Xat;

namespace NakedObjects.SystemTest.Repositories {
    [TestClass]
    public class TestSimpleRepository : AbstractSystemTest<SimpleRepositoryDbContext> {
        #region Setup/Teardown

        private Customer cust1;
        private Customer cust2;

        [ClassCleanup]
        public static void ClassCleanup() {
            CleanupNakedObjectsFramework(new TestSimpleRepository());
            Database.Delete(SimpleRepositoryDbContext.DatabaseName);
        }

        [TestInitialize()]
        public void TestInitialize() {
            InitializeNakedObjectsFrameworkOnce();
            StartTest();
            ITestObject cust1To = NewTestObject<Customer>();
            cust1 = (Customer) cust1To.GetDomainObject();
            cust1.Id = 1;
            cust1To.Save();

            ITestObject cust2To = NewTestObject<Customer>();
            cust2 = (Customer) cust2To.GetDomainObject();
            cust2.Id = 2;
            cust2To.Save();
        }

        protected override string[] Namespaces {
            get { return new[] { typeof(Customer).Namespace }; }
        }

        [TestCleanup()]
        public void TestCleanup() {}

        #endregion

        protected override object[] MenuServices {
            get {
                return (new object[] {
                    new SimpleRepository<Customer>()
                });
            }
        }

        [TestMethod]
        public void FindByKey() {
            var find = GetTestService("Customers").GetAction("Find By Key");
            var result = find.InvokeReturnObject(1);
            result.GetPropertyByName("Id").AssertValueIsEqual("1");
            result = find.InvokeReturnObject(2);
            result.GetPropertyByName("Id").AssertValueIsEqual("2");
        }

        [TestMethod]
        public void KeyValueDoesNotExist() {
            var find = GetTestService("Customers").GetAction("Find By Key");
            var result = find.InvokeReturnObject(1000);
            Assert.IsNull(result);
        }
    }

    #region Classes used in tests

    public class SimpleRepositoryDbContext : DbContext {
        public const string DatabaseName = "TestSimpleRepository";
        public SimpleRepositoryDbContext() : base(DatabaseName) {}

        public DbSet<Customer> Customer { get; set; }
    }

    public class Customer {
        public virtual int Id { get; set; }
    }

    #endregion
}
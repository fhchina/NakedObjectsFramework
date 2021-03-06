﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Boot;
using NakedObjects.Core.NakedObjectsSystem;
using NakedObjects.EntityObjectStore;
using NakedObjects.Helpers.Test.ViewModel;
using NakedObjects.Services;
using NakedObjects.Xat;
using NakedObjects.Helpers.Test;

namespace NakedObjects.SystemTest.ObjectFinderCompoundKeys {

    [TestClass]
    public class TestObjectFinderWithCompoundKeys : TestObjectFinderWithCompoundKeysAbstract {
      

        protected override IServicesInstaller MenuServices {
            get {
                return new ServicesInstaller(new object[] {
                    new ObjectFinder(),
                    new SimpleRepository<Payment>(),
                    new SimpleRepository<CustomerOne>(),
                    new SimpleRepository<CustomerTwo>(),
                    new SimpleRepository<CustomerThree>(),
                    new SimpleRepository<Supplier>(),
                    new SimpleRepository<Employee>()
                });
            }
        }

        [ClassInitialize]
        public  static void SetupTestFixture(TestContext tc) {
            InitializeNakedObjectsFramework(new TestObjectFinderWithCompoundKeys());
        }

        [ClassCleanup]
        public  static void TearDownTest() {
            CleanupNakedObjectsFramework(new TestObjectFinderWithCompoundKeys());
            Database.Delete(PaymentContext.DatabaseName);
        }


               [TestMethod]
        public virtual void SetAssociatedObject() {
            payee1.SetObject(customer2a);
            key1.AssertValueIsEqual("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo|1|1001");

            payee1.SetObject(customer2b);
            Assert.AreEqual(payee1.ContentAsObject, customer2b);

            key1.AssertValueIsEqual("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo|2|1002");
        }

        [TestMethod]
        public virtual void WorksWithASingleIntegerKey() {
            payee1.SetObject(customer1);
            key1.AssertValueIsEqual("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerOne|1");
            payee1.ClearObject();

            key1.SetValue("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerOne|1");
            payee1.AssertIsNotEmpty();
            payee1.AssertObjectIsEqual(customer1);
        }

        [TestMethod]
        public virtual void WorksWithTripleIntegerKey()
        {
            payee1.SetObject(customer3);
            key1.AssertValueIsEqual("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerThree|1|1001|2001");
            payee1.ClearObject();

            key1.SetValue("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerThree|1|1001|2001");
            payee1.AssertIsNotEmpty();
            payee1.AssertObjectIsEqual(customer3);
        }

        [TestMethod]
        public virtual void FailsIfTypeNameIsEmpty()
        {
            key1.SetValue("|1|1001|2001");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Compound key: |1|1001|2001 does not contain an object type", ex.Message);
            }
        }

        [TestMethod]
        public virtual void FailsIfTypeNameIsWrong()
        {
            key1.SetValue("CustomerThree|1|1001|2001");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Type: CustomerThree cannot be found", ex.Message);
            }
        }


        [TestMethod]
        public virtual void FailsIfTooFewKeysSupplied()
        {
            key1.SetValue("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerThree|1|1001");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Number of keys provided does not match the number of keys specified for type: NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerThree", ex.Message);
            }
        }


        [TestMethod]
        public virtual void FailsIfTooManyKeysSupplied()
        {
            key1.SetValue("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo|1|1001|2001");
            try {
                payee1.AssertIsNotEmpty();
                throw new AssertFailedException("Exception should have been thrown");
            }
            catch (Exception ex) {
                Assert.AreEqual("Number of keys provided does not match the number of keys specified for type: NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo", ex.Message);
            }
        }


        [TestMethod]
        public virtual void ChangeAssociatedObjectType()
        {
            payee1.SetObject(customer2a);
            key1.AssertValueIsEqual("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo|1|1001");
            payee1.SetObject(supplier1);
            Assert.AreEqual(payee1.ContentAsObject, supplier1);

            key1.AssertValueIsEqual("NakedObjects.SystemTest.ObjectFinderCompoundKeys.Supplier|1|2001");
        }


        [TestMethod]
        public virtual void ClearAssociatedObject()
        {
            payee1.SetObject(customer2a);
            payee1.ClearObject();
            key1.AssertIsEmpty();
        }


        [TestMethod]
        public virtual void GetAssociatedObject()
        {
            key1.SetValue("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo|1|1001");
            payee1.AssertIsNotEmpty();
            payee1.ContentAsObject.GetPropertyByName("Id").AssertValueIsEqual("1");

            payee1.ClearObject();

            key1.SetValue("NakedObjects.SystemTest.ObjectFinderCompoundKeys.CustomerTwo|2|1002");
            payee1.AssertIsNotEmpty();
            payee1.ContentAsObject.GetPropertyByName("Id").AssertValueIsEqual("2");
        }

        [TestMethod]
        public virtual void NoAssociatedObject()
        {
            key1.AssertIsEmpty();
        }
    }

}
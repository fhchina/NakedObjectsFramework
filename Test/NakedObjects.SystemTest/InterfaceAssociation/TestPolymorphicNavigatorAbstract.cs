﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Data.Entity.SqlServer;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.SystemTest.PolymorphicAssociations;
using NakedObjects.Xat;
using NakedObjects.Services;

namespace NakedObjects.SystemTest.PolymorphicNavigator {
    [TestClass]
    public abstract class TestPolymorphicNavigatorAbstract : AbstractSystemTest<PolymorphicNavigationContext> {
                #region Run configuration
        protected override object[] MenuServices {
            get {
                return (new object[] {
                    new SimpleRepository<PolymorphicPayment>(),
                    new SimpleRepository<CustomerAsPayee>(),
                    new SimpleRepository<SupplierAsPayee>(),
                    new SimpleRepository<InvoiceAsPayableItem>(),
                    new SimpleRepository<ExpenseClaimAsPayableItem>()
                });
            }
        }

        protected override object[] SystemServices {
            get { return new object[] {new Services.PolymorphicNavigator()}; }
        }

        #endregion
        
        #region Setup/Teardown

        // to get EF SqlServer Dll in memory
        public SqlProviderServices instance = SqlProviderServices.Instance;



        #endregion

        public virtual void SetPolymorphicPropertyOnTransientObject(string roleObjectType) {
            ITestObject transPayment = GetTestService("Polymorphic Payments").GetAction("New Instance").InvokeReturnObject();

            ITestObject customer1 = GetTestService("Customer As Payees").GetAction("New Instance").InvokeReturnObject().Save();
            string cusId = customer1.GetPropertyByName("Id").Title;

            ITestProperty payeeProp = transPayment.GetPropertyByName("Payee");
            ITestProperty payeeLinkProp = transPayment.GetPropertyByName("Payee Link").AssertIsUnmodifiable().AssertIsEmpty();
            payeeProp.SetObject(customer1);
            transPayment.Save();
            ITestObject payeeLink = payeeLinkProp.AssertIsNotEmpty().ContentAsObject;
            ITestProperty associatedType = payeeLink.GetPropertyByName("Associated Role Object Type").AssertIsUnmodifiable();
            associatedType.AssertValueIsEqual(roleObjectType);
            ITestProperty associatedId = payeeLink.GetPropertyByName("Associated Role Object Id").AssertIsUnmodifiable();
            associatedId.AssertValueIsEqual(cusId);
        }

        public virtual void AttemptSetPolymorphicPropertyWithATransientAssociatedObject() {
            ITestObject transPayment = GetTestService("Polymorphic Payments").GetAction("New Instance").InvokeReturnObject().Save();

            ITestObject customer1 = GetTestService("Customer As Payees").GetAction("New Instance").InvokeReturnObject();
            string cusId = customer1.GetPropertyByName("Id").Title;

            ITestProperty payeeProp = transPayment.GetPropertyByName("Payee");
            ITestProperty payeeLinkProp = transPayment.GetPropertyByName("Payee Link").AssertIsUnmodifiable().AssertIsEmpty();
            try {
                payeeProp.SetObject(customer1);
                Assert.Fail("Should not get to here");
            }
            catch (Exception e) {
                Assert.IsTrue(e.Message.Contains("Can't set field of persistent with a transient reference"));
            }
        }

        public virtual void SetPolymorphicPropertyOnPersistentObject(string roleObjectType) {
            var payment1 = FindById<PolymorphicPayment>(1);
            var customer1 = FindById<CustomerAsPayee>(1);
            string cusId = customer1.GetPropertyByName("Id").Title;

            ITestProperty payeeProp = payment1.GetPropertyByName("Payee").AssertIsEmpty();
            ITestProperty payeeLinkProp = payment1.GetPropertyByName("Payee Link").AssertIsUnmodifiable().AssertIsEmpty();
            payeeProp.SetObject(customer1);

            ITestObject payeeLink = payment1.GetPropertyByName("Payee Link").AssertIsNotEmpty().ContentAsObject;
            ITestProperty associatedType = payeeLink.GetPropertyByName("Associated Role Object Type").AssertIsUnmodifiable();
            associatedType.AssertValueIsEqual(roleObjectType);
            ITestProperty associatedId = payeeLink.GetPropertyByName("Associated Role Object Id").AssertIsUnmodifiable();
            associatedId.AssertValueIsEqual(cusId);
        }

        public virtual void ChangePolymorphicPropertyOnPersistentObject(string customerType, string supplierType) {
            var payment2 = FindById<PolymorphicPayment>(3);
            var customer1 = FindById<CustomerAsPayee>(1);
            string cusId = customer1.GetPropertyByName("Id").Title;

            ITestProperty payeeProp = payment2.GetPropertyByName("Payee");
            ITestProperty payeeLinkProp = payment2.GetPropertyByName("Payee Link").AssertIsUnmodifiable();
            payeeProp.SetObject(customer1);

            ITestObject payeeLink = payeeLinkProp.AssertIsNotEmpty().ContentAsObject;
            ITestProperty associatedType = payeeLink.GetPropertyByName("Associated Role Object Type").AssertIsUnmodifiable();
            associatedType.AssertValueIsEqual(customerType);
            ITestProperty associatedId = payeeLink.GetPropertyByName("Associated Role Object Id").AssertIsUnmodifiable();
            associatedId.AssertValueIsEqual(cusId);

            ITestObject sup1 = GetTestService("Supplier As Payees").GetAction("New Instance").InvokeReturnObject().Save();
            string supId = sup1.GetPropertyByName("Id").Title;

            payeeProp.SetObject(sup1);
            associatedType.AssertValueIsEqual(supplierType);
            associatedId.AssertValueIsEqual(supId);

            payeeProp.ClearObject();
            payeeLinkProp.AssertIsEmpty();
            payeeProp.AssertIsEmpty();
        }

        public virtual void ClearPolymorphicProperty() {
            var payment3 = FindById<PolymorphicPayment>(3);

            var payeeProp = payment3.GetPropertyByName("Payee");
            var payeeLinkProp = payment3.GetPropertyByName("Payee Link");

            payeeLinkProp.AssertIsNotEmpty();
            payeeProp.AssertIsNotEmpty();

            payeeProp.ClearObject();
            payeeLinkProp.AssertIsEmpty();
            payeeProp.AssertIsEmpty();
        }

        public virtual void PolymorphicCollectionAddMutlipleItemsOfOneType(string roleObjectType) {
            var payment = FindById<PolymorphicPayment>(4);
            var inv = FindById<InvoiceAsPayableItem>(1);

            string invId = inv.GetPropertyByName("Id").Title;
            ITestObject inv2 = GetTestService("Invoice As Payable Items").GetAction("New Instance").InvokeReturnObject().Save();
            string inv2Id = inv2.GetPropertyByName("Id").Title;


            ITestCollection links = payment.GetPropertyByName("Payable Item Links").ContentAsCollection;
            ITestCollection items = payment.GetPropertyByName("Payable Items").ContentAsCollection;

            links.AssertCountIs(0);
            items.AssertCountIs(0);

            //Add an Invoice
            payment.GetAction("Add Payable Item").InvokeReturnObject(inv);
            links.AssertCountIs(1);
            ITestObject link1 = links.ElementAt(0);
            link1.GetPropertyByName("Associated Role Object Type").AssertValueIsEqual(roleObjectType);
            link1.GetPropertyByName("Associated Role Object Id").AssertValueIsEqual(invId);
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            ITestObject item = items.AssertCountIs(1).ElementAt(0);
            item.AssertIsType(typeof (InvoiceAsPayableItem));

            //Add an expense claim
            payment.GetAction("Add Payable Item").InvokeReturnObject(inv2);
            links.AssertCountIs(2);
            ITestObject link2 = links.ElementAt(1);
            link2.GetPropertyByName("Associated Role Object Type").AssertValueIsEqual(roleObjectType);
            link2.GetPropertyByName("Associated Role Object Id").AssertValueIsEqual(inv2Id);
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            item = items.AssertCountIs(2).ElementAt(1);
            item.AssertIsType(typeof (InvoiceAsPayableItem));
        }

        public virtual void PolymorphicCollectionAddDifferentItems(string roleType1, string roleType2) {
            var payment = FindById<PolymorphicPayment>(5);
            var inv = FindById<InvoiceAsPayableItem>(1);

            string invId = inv.GetPropertyByName("Id").Title;
            ITestObject exp = GetTestService("Expense Claim As Payable Items").GetAction("New Instance").InvokeReturnObject().Save();
            string expId = exp.GetPropertyByName("Id").Title;

            ITestCollection links = payment.GetPropertyByName("Payable Item Links").ContentAsCollection;
            ITestCollection items = payment.GetPropertyByName("Payable Items").ContentAsCollection;

            links.AssertCountIs(0);
            items.AssertCountIs(0);

            //Add an Invoice
            payment.GetAction("Add Payable Item").InvokeReturnObject(inv);
            links.AssertCountIs(1);
            ITestObject link1 = links.ElementAt(0);
            link1.GetPropertyByName("Associated Role Object Type").AssertValueIsEqual(roleType1);
            link1.GetPropertyByName("Associated Role Object Id").AssertValueIsEqual(invId);
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            ITestObject item = items.AssertCountIs(1).ElementAt(0);
            item.AssertIsType(typeof (InvoiceAsPayableItem));

            //Add an expense claim
            payment.GetAction("Add Payable Item").InvokeReturnObject(exp);
            links.AssertCountIs(2);
            ITestObject link2 = links.ElementAt(1);
            link2.GetPropertyByName("Associated Role Object Type").AssertValueIsEqual(roleType2);
            link2.GetPropertyByName("Associated Role Object Id").AssertValueIsEqual(expId);
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            item = items.AssertCountIs(2).ElementAt(1);
            item.AssertIsType(typeof (ExpenseClaimAsPayableItem));
        }

        public virtual void AttemptToAddSameItemTwice() {
            var payment = FindById<PolymorphicPayment>(6);
            var inv = FindById<InvoiceAsPayableItem>(1);
            string invId = inv.GetPropertyByName("Id").Title;

            ITestCollection links = payment.GetPropertyByName("Payable Item Links").ContentAsCollection;
            ITestCollection items = payment.GetPropertyByName("Payable Items").ContentAsCollection;


            links.AssertCountIs(1);
            items.AssertCountIs(1);
            Assert.AreEqual(inv, items.ElementAt(0));

            //Try adding same expense claim again
            payment.GetAction("Add Payable Item").InvokeReturnObject(inv);
            links.AssertCountIs(1); //Should still be 1
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            items.AssertCountIs(1);
        }

        public virtual void RemoveItem() {
            var payment = FindById<PolymorphicPayment>(7);
            var inv = FindById<InvoiceAsPayableItem>(1);

            ITestCollection links = payment.GetPropertyByName("Payable Item Links").ContentAsCollection;
            ITestCollection items = payment.GetPropertyByName("Payable Items").ContentAsCollection;

            links.AssertCountIs(1);
            items.AssertCountIs(1);
            Assert.AreEqual(inv, items.ElementAt(0));

            //Now remove the invoice
            payment.GetAction("Remove Payable Item").InvokeReturnObject(inv);
            links.AssertCountIs(0);
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            items.AssertCountIs(0);
        }

        public virtual void AttemptToRemoveNonExistentItem() {
            var payment = FindById<PolymorphicPayment>(8);
            var inv = FindById<InvoiceAsPayableItem>(1);
            var inv2 = FindById<InvoiceAsPayableItem>(2);

            string invId = inv.GetPropertyByName("Id").Title;

            ITestCollection links = payment.GetPropertyByName("Payable Item Links").ContentAsCollection;
            ITestCollection items = payment.GetPropertyByName("Payable Items").ContentAsCollection;

            links.AssertCountIs(0);
            items.AssertCountIs(0);

            //Add Invoice 1
            payment.GetAction("Add Payable Item").InvokeReturnObject(inv);
            links.AssertCountIs(1);
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            ITestObject item = items.AssertCountIs(1).ElementAt(0);
            item.AssertIsType(typeof (InvoiceAsPayableItem));

            //Now attempt to remove invoice 2
            payment.GetAction("Remove Payable Item").InvokeReturnObject(inv2);
            links.AssertCountIs(1);
            items = payment.GetPropertyByName("Payable Items").ContentAsCollection;
            item = items.AssertCountIs(1).ElementAt(0);
        }

        public virtual void FindOwnersForObject() {
            var payment9 = FindById<PolymorphicPayment>(9);
            var payment10 = FindById<PolymorphicPayment>(10);
            var payment11 = FindById<PolymorphicPayment>(11);

            var cus = FindById<CustomerAsPayee>(2);
            var inv = FindById<InvoiceAsPayableItem>(3);

            var payeeProp = payment9.GetPropertyByName("Payee");
            payeeProp.SetObject(cus);
            payeeProp = payment11.GetPropertyByName("Payee");
            payeeProp.SetObject(cus);

            var results = cus.GetAction("Payments To This Payee").InvokeReturnCollection();
            results.AssertCountIs(2);
            Assert.IsTrue(results.Contains(payment9));
            Assert.IsTrue(results.Contains(payment11));

            results = inv.GetAction("Payments Containing This").InvokeReturnCollection();
            results.AssertCountIs(2);
            Assert.IsTrue(results.Contains(payment9));
            Assert.IsTrue(results.Contains(payment10));
        }
    }
}
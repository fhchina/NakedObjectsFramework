// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Meta.Facet;
using NakedObjects.Reflect.FacetFactory;

namespace NakedObjects.Reflect.Test.FacetFactory {
    [TestClass]
    public class ViewModelFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();

            facetFactory = new ViewModelFacetFactory(0);
        }

        [TestCleanup]
        public new void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private ViewModelFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (IViewModelFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        private class Class1 : IViewModel {
            public string Value1 { get; set; }
            public string Value2 { get; set; }

            #region IViewModel Members

            public string[] DeriveKeys() {
                return new[] {Value1, Value2};
            }

            public void PopulateUsingKeys(string[] instanceId) {
                Value1 = instanceId[0];
                Value2 = instanceId[1];
            }

            #endregion
        }

        private class Class2 {
            // ReSharper disable once UnusedMember.Local
            public string[] DeriveKeys() {
                throw new NotImplementedException();
            }

            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            public void PopulateUsingKeys(string[] instanceId) {
                throw new NotImplementedException();
            }
        }

        private class Class3 : IViewModelEdit {
            private string Value1 { get; set; }
            private string Value2 { get; set; }

            #region IViewModel Members

            public string[] DeriveKeys() {
                return new[] { Value1, Value2 };
            }

            public void PopulateUsingKeys(string[] instanceId) {
                Value1 = instanceId[0];
                Value2 = instanceId[1];
            }

            #endregion
        }

        private class Class4 : IViewModelSwitchable {
            private string Value1 { get; set; }
            private string Value2 { get; set; }

            #region IViewModel Members

            public string[] DeriveKeys() {
                return new[] { Value1, Value2 };
            }

            public void PopulateUsingKeys(string[] instanceId) {
                Value1 = instanceId[0];
                Value2 = instanceId[1];
            }

            public bool IsEditView() {
                throw new NotImplementedException();
            }

            #endregion
        }


        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Property));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Action));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.ActionParameter));
        }

        [TestMethod]
        public void TestViewModelDerive() {
            facetFactory.Process(Reflector, typeof (Class1), MethodRemover, Specification);
            var facet = Specification.GetFacet<IViewModelFacet>();
            Assert.IsNotNull(facet);

            var testClass = new Class1 {Value1 = "testValue1", Value2 = "testValue2"};
            var mock = new Mock<INakedObject>();
            INakedObject value = mock.Object;
            mock.Setup(no => no.Object).Returns(testClass);

            string[] key = facet.Derive(value);

            Assert.AreEqual(2, key.Length);
            Assert.AreEqual(testClass.Value1, key[0]);
            Assert.AreEqual(testClass.Value2, key[1]);
        }

        [TestMethod]
        public void TestViewModelNotPickedUp() {
            facetFactory.Process(Reflector, typeof (Class2), MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IViewModelFacet));
            Assert.IsNull(facet);
        }

        [TestMethod]
        public void TestViewModelPickedUp() {
            var class1Type = typeof (Class1);
            facetFactory.Process(Reflector, class1Type, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IViewModelFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ViewModelFacetConvention);

            MethodInfo m1 = class1Type.GetMethod("DeriveKeys");
            MethodInfo m2 = class1Type.GetMethod("PopulateUsingKeys");

            AssertMethodsRemoved(new[] {m1, m2});
        }

        [TestMethod]
        public void TestViewModelEditPickedUp() {
            var class3Type = typeof(Class3);
            facetFactory.Process(Reflector, class3Type, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof(IViewModelFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ViewModelEditFacetConvention);

            MethodInfo m1 = class3Type.GetMethod("DeriveKeys");
            MethodInfo m2 = class3Type.GetMethod("PopulateUsingKeys");

            AssertMethodsRemoved(new[] { m1, m2 });
        }

        [TestMethod]
        public void TestViewModelSwitchablePickedUp() {
            var class4Type = typeof(Class4);
            facetFactory.Process(Reflector, class4Type, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof(IViewModelFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ViewModelSwitchableFacetConvention);

            MethodInfo m1 = class4Type.GetMethod("DeriveKeys");
            MethodInfo m2 = class4Type.GetMethod("PopulateUsingKeys");
            MethodInfo m3 = class4Type.GetMethod("IsEditView");

            AssertMethodsRemoved(new[] { m1, m2, m3 });
        }


        [TestMethod]
        public void TestViewModelPopulate() {
            facetFactory.Process(Reflector, typeof (Class1), MethodRemover, Specification);
            var facet = Specification.GetFacet<IViewModelFacet>();
            Assert.IsNotNull(facet);

            var testClass = new Class1();
            var keys = new[] {"testValue1", "testValue2"};
            var mock = new Mock<INakedObject>();
            INakedObject value = mock.Object;
            mock.Setup(no => no.Object).Returns(testClass);

            facet.Populate(keys, value);

            Assert.AreEqual(keys[0], testClass.Value1);
            Assert.AreEqual(keys[1], testClass.Value2);
        }
    }
}
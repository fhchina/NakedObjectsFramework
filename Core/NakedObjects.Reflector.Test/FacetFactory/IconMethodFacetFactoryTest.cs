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
using NakedObjects.Core.Adapter;
using NakedObjects.Meta.Facet;
using NakedObjects.Reflect.FacetFactory;

namespace NakedObjects.Reflect.Test.FacetFactory {
    [TestClass]
    public class IconMethodFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new IconMethodFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private IconMethodFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (IIconFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        private INakedObject AdapterFor(object obj) {
            ILifecycleManager lifecycleManager = new Mock<ILifecycleManager>().Object;
            IObjectPersistor persistor = new Mock<IObjectPersistor>().Object;
            ISession session = new Mock<ISession>().Object;
            INakedObjectManager manager = new Mock<INakedObjectManager>().Object;
            return new PocoAdapter(Metamodel, session, persistor, lifecycleManager, manager, obj, null);
        }

        private class Customer {
// ReSharper disable once UnusedMember.Local
            public string IconName() {
                return "TestName";
            }
        }

        [IconName("AttributeName")]
        private class Customer1 {}

        [IconName("AttributeName")]
        private class Customer2 {
// ReSharper disable once UnusedMember.Local
            public string IconName() {
                return "TestName";
            }
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
        public void TestIconNameFromAttribute() {
            facetFactory.Process(Reflector, typeof (Customer1), MethodRemover, Specification);
            var facet = Specification.GetFacet<IIconFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is IconFacetAnnotation);
            Assert.AreEqual("AttributeName", facet.GetIconName());
            INakedObject no = AdapterFor(new Customer1());
            Assert.AreEqual("AttributeName", facet.GetIconName(no));
        }

        [TestMethod]
        public void TestIconNameFromMethod() {
            facetFactory.Process(Reflector, typeof (Customer), MethodRemover, Specification);
            var facet = Specification.GetFacet<IIconFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is IconFacetViaMethod);
            Assert.IsNull(facet.GetIconName());
            INakedObject no = AdapterFor(new Customer());
            Assert.AreEqual("TestName", facet.GetIconName(no));
        }

        [TestMethod]
        public void TestIconNameMethodPickedUpOnClassAndMethodRemoved() {
            MethodInfo iconNameMethod = FindMethod(typeof (Customer), "IconName");
            facetFactory.Process(Reflector, typeof (Customer), MethodRemover, Specification);
            var facet = Specification.GetFacet<IIconFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is IconFacetViaMethod);
            AssertMethodRemoved(iconNameMethod);
        }

        [TestMethod]
        public void TestIconNameWithFallbackAttribute() {
            facetFactory.Process(Reflector, typeof (Customer2), MethodRemover, Specification);
            var facet = Specification.GetFacet<IIconFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is IconFacetViaMethod);
            Assert.AreEqual("AttributeName", facet.GetIconName());
            INakedObject no = AdapterFor(new Customer2());
            Assert.AreEqual("TestName", facet.GetIconName(no));
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.ComponentModel.DataAnnotations;
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
    public class EnumFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new EnumFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private EnumFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (IEnumFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        private static void CheckChoices(IFacet facet) {
            var facetAsEnumFacet = facet as IEnumFacet;
            Assert.IsNotNull(facetAsEnumFacet);
            Assert.AreEqual(3, facetAsEnumFacet.GetChoices(null).Length);
            Assert.AreEqual(Cities.London, facetAsEnumFacet.GetChoices(null)[0]);
            Assert.AreEqual(Cities.NewYork, facetAsEnumFacet.GetChoices(null)[1]);
            Assert.AreEqual(Cities.Paris, facetAsEnumFacet.GetChoices(null)[2]);

            Assert.AreEqual(1, facetAsEnumFacet.GetChoices(null, new object[] {Cities.NewYork}).Length);
            Assert.AreEqual(Cities.NewYork, facetAsEnumFacet.GetChoices(null, new object[] {Cities.NewYork})[0]);

            var mock = new Mock<INakedObject>();
            INakedObject nakedObject = mock.Object;
            mock.Setup(no => no.Object).Returns(Cities.NewYork);

            Assert.AreEqual("New York", facetAsEnumFacet.GetTitle(nakedObject));
        }

        private enum Cities {
            London,
            Paris,
            NewYork
        }

        private class Customer1 {
            [EnumDataType(typeof (Cities))]
// ReSharper disable UnusedMember.Local
            public int City { get; set; }
        }

        private class Customer2 {
// ReSharper disable UnusedParameter.Local
            public void SomeAction([EnumDataType(typeof (Cities))] int city) {}
        }

        private class Customer3 {
            public Cities City { get; set; }
        }

        private class Customer4 {
            public void SomeAction(Cities city) {}
        }

        private class Customer5 {
            public Cities? City { get; set; }
        }

        private class Customer6 {
            public void SomeAction(Cities? city) {}
        }

        [TestMethod]
        public void TestEnumAnnotationPickedUpOnActionParameter() {
            MethodInfo method = FindMethod(typeof (Customer2), "SomeAction", new[] {typeof (int)});
            facetFactory.ProcessParams(Reflector, method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (IEnumFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is EnumFacet);
            CheckChoices(facet);
        }

        [TestMethod]
        public void TestEnumAnnotationPickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer1), "City");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IEnumFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is EnumFacet);
            CheckChoices(facet);
        }

        [TestMethod]
        public void TestEnumTypePickedUpOnActionParameter() {
            MethodInfo method = FindMethod(typeof (Customer4), "SomeAction", new[] {typeof (Cities)});
            facetFactory.ProcessParams(Reflector, method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (IEnumFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is EnumFacet);
            CheckChoices(facet);
        }

        [TestMethod]
        public void TestEnumTypePickedUpOnNullableActionParameter() {
            MethodInfo method = FindMethod(typeof (Customer6), "SomeAction", new[] {typeof (Cities?)});
            facetFactory.ProcessParams(Reflector, method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (IEnumFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is EnumFacet);
            CheckChoices(facet);
        }

        [TestMethod]
        public void TestEnumTypePickedUpOnNullableProperty() {
            PropertyInfo property = FindProperty(typeof (Customer5), "City");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IEnumFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is EnumFacet);
            CheckChoices(facet);
        }

        [TestMethod]
        public void TestEnumTypePickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer3), "City");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IEnumFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is EnumFacet);
            CheckChoices(facet);
        }

        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Property));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Action));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.ActionParameter));
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
    // ReSharper restore UnusedMember.Local
    // ReSharper restore UnusedParameter.Local
}
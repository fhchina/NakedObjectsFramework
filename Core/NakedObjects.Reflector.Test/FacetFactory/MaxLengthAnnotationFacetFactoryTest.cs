// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Meta.Facet;
using NakedObjects.Reflect.FacetFactory;
using NUnit.Framework;

namespace NakedObjects.Reflect.Test.FacetFactory {
    [TestFixture]
    public class MaxLengthAnnotationFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [SetUp]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new MaxLengthAnnotationFacetFactory(Reflector);
        }

        [TearDown]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private MaxLengthAnnotationFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (IMaxLengthFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }


        private class Customer {}

        private class Customer1 {
            [MaxLength(30)]
            public string FirstName {
                get { return null; }
            }
        }

        private class Customer2 {
            public void someAction([MaxLength(20)] string foo) {}
        }

        private class Customer4 {
            [StringLength(30)]
            public string FirstName {
                get { return null; }
            }
        }

        private class Customer5 {
            public void someAction([StringLength(20)] string foo) {}
        }

        private class Customer7 {
            [MaxLength(30)]
            public string FirstName {
                get { return null; }
            }
        }

        private class Customer8 {
            public void someAction([MaxLength(20)] string foo) {}
        }

        [Test]
        public void TestCMMaxLengthAnnotationPickedUpOnActionParameter() {
            MethodInfo method = FindMethod(typeof (Customer8), "someAction", new[] {typeof (string)});
            facetFactory.ProcessParams(method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (IMaxLengthFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MaxLengthFacetAnnotation);
            var maxLengthFacetAnnotation = (MaxLengthFacetAnnotation) facet;
            Assert.AreEqual(20, maxLengthFacetAnnotation.Value);
        }

        [Test]
        public void TestCMMaxLengthAnnotationPickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer7), "FirstName");
            facetFactory.Process(property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IMaxLengthFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MaxLengthFacetAnnotation);
            var maxLengthFacetAnnotation = (MaxLengthFacetAnnotation) facet;
            Assert.AreEqual(30, maxLengthFacetAnnotation.Value);
        }

        [Test]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Property));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Action));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.ActionParameter));
        }

        [Test]
        public void TestNOFMaxLengthAnnotationPickedUpOnActionParameter() {
            MethodInfo method = FindMethod(typeof (Customer2), "someAction", new[] {typeof (string)});
            facetFactory.ProcessParams(method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (IMaxLengthFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MaxLengthFacetAnnotation);
            var maxLengthFacetAnnotation = (MaxLengthFacetAnnotation) facet;
            Assert.AreEqual(20, maxLengthFacetAnnotation.Value);
        }

        //[Test]
        //public void TestNOFMaxLengthAnnotationPickedUpOnClass() {
        //    facetFactory.Process(typeof (Customer), methodRemover, specification);
        //    IFacet facet = specification.GetFacet(typeof (IMaxLengthFacet));
        //    Assert.IsNotNull(facet);
        //    Assert.IsTrue(facet is MaxLengthFacetAnnotation);
        //    var maxLengthFacetAnnotation = (MaxLengthFacetAnnotation) facet;
        //    Assert.AreEqual(16, maxLengthFacetAnnotation.Value);
        //}

        [Test]
        public void TestNOFMaxLengthAnnotationPickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer1), "FirstName");
            facetFactory.Process(property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IMaxLengthFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MaxLengthFacetAnnotation);
            var maxLengthFacetAnnotation = (MaxLengthFacetAnnotation) facet;
            Assert.AreEqual(30, maxLengthFacetAnnotation.Value);
        }

        [Test]
        public void TestStringLengthAnnotationPickedUpOnActionParameter() {
            MethodInfo method = FindMethod(typeof (Customer5), "someAction", new[] {typeof (string)});
            facetFactory.ProcessParams(method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (IMaxLengthFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MaxLengthFacetAnnotation);
            var maxLengthFacetAnnotation = (MaxLengthFacetAnnotation) facet;
            Assert.AreEqual(20, maxLengthFacetAnnotation.Value);
        }

        [Test]
        public void TestStringLengthAnnotationPickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer4), "FirstName");
            facetFactory.Process(property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IMaxLengthFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is MaxLengthFacetAnnotation);
            var maxLengthFacetAnnotation = (MaxLengthFacetAnnotation) facet;
            Assert.AreEqual(30, maxLengthFacetAnnotation.Value);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
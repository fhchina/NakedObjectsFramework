// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Meta.Facet;
using NakedObjects.Reflect.FacetFactory;

namespace NakedObjects.Reflect.Test.FacetFactory {
    [TestClass]
    public class TitleMethodFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new TitleMethodFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private TitleMethodFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (ITitleFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        private class Customer {
// ReSharper disable once UnusedMember.Local
            public string Title() {
                return "Some title";
            }
        }

        private class Customer1 {
            public override string ToString() {
                return "Some title via ToString";
            }
        }

        private class Customer2 {}

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
        public void TestNoExplicitTitleOrToStringMethod() {
            facetFactory.Process(Reflector, typeof (Customer2), MethodRemover, Specification);
            Assert.IsNull(Specification.GetFacet(typeof (ITitleFacet)));
            AssertNoMethodsRemoved();
        }

        [TestMethod]
        public void TestTitleMethodPickedUpOnClassAndMethodRemoved() {
            MethodInfo titleMethod = FindMethod(typeof (Customer), "Title");
            facetFactory.Process(Reflector, typeof (Customer), MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (ITitleFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is TitleFacetViaTitleMethod);
            var titleFacetViaTitleMethod = (TitleFacetViaTitleMethod) facet;
            Assert.AreEqual(titleMethod, titleFacetViaTitleMethod.GetMethod());
            AssertMethodRemoved(titleMethod);
        }

        [TestMethod]
        public void TestToStringMethodPickedUpOnClassAndMethodRemoved() {
            MethodInfo toStringMethod = FindMethod(typeof (Customer1), "ToString");
            facetFactory.Process(Reflector, typeof (Customer1), MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (ITitleFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is TitleFacetViaToStringMethod);
            var titleFacetViaTitleMethod = (TitleFacetViaToStringMethod) facet;
            Assert.AreEqual(toStringMethod, titleFacetViaTitleMethod.GetMethod());
            AssertMethodRemoved(toStringMethod);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
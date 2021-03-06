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
    [TestClass] //TODO: Write more tests
    public class ContributedActionAnnotationFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new ContributedActionAnnotationFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private ContributedActionAnnotationFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (IContributedActionFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Property));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Action));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.ActionParameter));
        }

        //Note: The [ContributedAction] annotation is applied to the parameter,
        //but the facet is applied to the Action (if any of its params have that annotation)
        [TestMethod]
        public void TestContributedAnnotationNullByDefault() {
            MethodInfo actionMethod = FindMethod(typeof (Customer1), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet<IContributedActionFacet>();
            Assert.IsNull(facet);
            AssertNoMethodsRemoved();
        }

        [TestMethod]
        public void TestContributedAnnotationPickedUp() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            var facet = Specification.GetFacet<IContributedActionFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ContributedActionFacet);
            AssertNoMethodsRemoved();
        }

        #region Nested type: Customer

        private class Customer {
// ReSharper disable once UnusedMember.Local
// ReSharper disable once UnusedParameter.Local
            public void SomeAction([ContributedAction] Customer1 cust1) {}
        }

        #endregion

        #region Nested type: Customer1

        private class Customer1 {
// ReSharper disable once UnusedMember.Local
            public void SomeAction() {}
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}
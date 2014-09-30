// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Naming.Named;
using NakedObjects.Architecture.Reflect;
using NUnit.Framework;

namespace NakedObjects.Reflector.DotNet.Facets.Naming.Named {
    [TestFixture]
    public class MetaDataAnnotationFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [SetUp]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new NamedAnnotationFacetFactory(Reflector);
        }

        [TearDown]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private NamedAnnotationFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (INamedFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        [MetadataType(typeof (Customer1_Metadata))]
        private class Customer1 {
            public int NumberOfOrders {
                get { return 0; }
            }
        }

        private class Customer1_Metadata {
            [Named("some name")]
            public int NumberOfOrders { get; set; }
        }

        [MetadataType(typeof (Customer2_Metadata))]
        private class Customer2 {
            public IList Orders {
                get { return null; }
            }
        }

        private class Customer2_Metadata {
            [Named("some name")]
            public IList Orders { get; set; }
        }

        [Test]
        public override void TestFeatureTypes() {
            NakedObjectFeatureType[] featureTypes = facetFactory.FeatureTypes;
            Assert.IsTrue(Contains(featureTypes, NakedObjectFeatureType.Objects));
            Assert.IsTrue(Contains(featureTypes, NakedObjectFeatureType.Property));
            Assert.IsTrue(Contains(featureTypes, NakedObjectFeatureType.Collection));
            Assert.IsTrue(Contains(featureTypes, NakedObjectFeatureType.Action));
            Assert.IsTrue(Contains(featureTypes, NakedObjectFeatureType.ActionParameter));
        }

        [Test]
        public void TestNamedAnnotationPickedUpOnCollection() {
            PropertyInfo property = FindProperty(typeof (Customer2), "Orders");
            facetFactory.Process(property, MethodRemover, FacetHolder);
            IFacet facet = FacetHolder.GetFacet(typeof (INamedFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is NamedFacetAbstract);
            var namedFacetAbstract = (NamedFacetAbstract) facet;
            Assert.AreEqual("some name", namedFacetAbstract.Value);
            AssertNoMethodsRemoved();
        }

        [Test]
        public void TestNamedAnnotationPickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer1), "NumberOfOrders");
            facetFactory.Process(property, MethodRemover, FacetHolder);
            IFacet facet = FacetHolder.GetFacet(typeof (INamedFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is NamedFacetAbstract);
            var namedFacetAbstract = (NamedFacetAbstract) facet;
            Assert.AreEqual("some name", namedFacetAbstract.Value);
            AssertNoMethodsRemoved();
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
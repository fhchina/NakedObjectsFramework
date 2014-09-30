// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections.Generic;
using System.Reflection;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Reflect;
using NUnit.Framework;

namespace NakedObjects.Reflector.DotNet.Facets.Actions {
    [TestFixture]
    public class ComplementaryMethodsFilteringFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [SetUp]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new ComplementaryMethodsFilteringFacetFactory(Reflector);
        }

        [TearDown]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private ComplementaryMethodsFilteringFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new Type[] {}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        private class BasePropertyClass {
            public virtual string AProperty { get; set; }

            public virtual bool MeaninglessPrefixAProperty() {
                return false;
            }

            public virtual void ModifyAProperty(Type value) {}

            public virtual void ClearAProperty() {}

            public virtual IList<string> ChoicesAProperty() {
                return null;
            }

            public virtual string DefaultAProperty() {
                return string.Empty;
            }

            public virtual string ValidateAProperty(string prop) {
                return string.Empty;
            }

            public virtual string DisableAProperty() {
                return null;
            }

            public virtual bool HideAProperty() {
                return false;
            }
        }

        private class PropertyClass : BasePropertyClass {
            public override bool MeaninglessPrefixAProperty() {
                return false;
            }

            public override void ModifyAProperty(Type value) {}

            public override void ClearAProperty() {}

            public override IList<string> ChoicesAProperty() {
                return null;
            }

            public override string DefaultAProperty() {
                return string.Empty;
            }

            public override string ValidateAProperty(string prop) {
                return string.Empty;
            }

            public override string DisableAProperty() {
                return null;
            }

            public override bool HideAProperty() {
                return false;
            }
        }

        private class BaseNoPropertyClass {
            public virtual bool MeaninglessPrefixAProperty() {
                return false;
            }

            public virtual void ModifyAProperty(Type value) {}

            public virtual void ClearAProperty() {}

            public virtual IList<string> ChoicesAProperty() {
                return null;
            }

            public virtual string DefaultAProperty() {
                return string.Empty;
            }

            public virtual string ValidateAProperty(string prop) {
                return string.Empty;
            }

            public virtual string DisableAProperty() {
                return null;
            }

            public virtual bool HideAProperty() {
                return false;
            }
        }

        private class NoPropertyClass : BaseNoPropertyClass {
            public override bool MeaninglessPrefixAProperty() {
                return false;
            }

            public override void ModifyAProperty(Type value) {}

            public override void ClearAProperty() {}

            public override IList<string> ChoicesAProperty() {
                return null;
            }

            public override string DefaultAProperty() {
                return string.Empty;
            }

            public override string ValidateAProperty(string prop) {
                return string.Empty;
            }

            public override string DisableAProperty() {
                return null;
            }

            public override bool HideAProperty() {
                return false;
            }
        }

        private class BaseCollectionClass {
            public virtual ICollection<string> ACollection { get; set; }

            public virtual void AddToACollection(string value) {}

            public virtual void RemoveFromACollection(string value) {}
        }

        private class CollectionClass : BaseCollectionClass {
            public override void AddToACollection(string value) {}

            public override void RemoveFromACollection(string value) {}
        }

        private class BaseNoCollectionClass {
            public virtual void AddToACollection(string value) {}

            public virtual void RemoveFromACollection(string value) {}
        }

        private class NoCollectionClass : BaseNoCollectionClass {
            public override void AddToACollection(string value) {}

            public override void RemoveFromACollection(string value) {}
        }

        //

        private class BaseCollectionClass1 {
            public virtual ICollection<string> ACollection { get; set; }

            public virtual string ValidateAddToACollection(string value) {
                return null;
            }

            public virtual string ValidateRemoveFromACollection(string value) {
                return null;
            }
        }

        private class CollectionClass1 : BaseCollectionClass1 {
            public override string ValidateAddToACollection(string value) {
                return null;
            }

            public override string ValidateRemoveFromACollection(string value) {
                return null;
            }
        }

        private class BaseNoCollectionClass1 {
            public virtual string ValidateAddToACollection(string value) {
                return null;
            }

            public virtual string ValidateRemoveFromACollection(string value) {
                return null;
            }
        }

        private class NoCollectionClass1 : BaseNoCollectionClass1 {
            public override string ValidateAddToACollection(string value) {
                return null;
            }

            public override string ValidateRemoveFromACollection(string value) {
                return null;
            }
        }

        private class BaseActionClass {
            public void AnAction(string parm) {}

            public virtual string ValidateAnAction() {
                return string.Empty;
            }

            public virtual string DisableAnAction() {
                return null;
            }

            public virtual bool HideAnAction() {
                return false;
            }

            public virtual IList<string> Choices0AnAction() {
                return null;
            }

            public virtual string Default0AnAction() {
                return null;
            }

            public virtual string Validate0AnAction(string parm) {
                return null;
            }

            public virtual IList<string> ChoicesAnAction(string parm) {
                return null;
            }

            public virtual string DefaultAnAction(string parm) {
                return null;
            }

            public virtual string ValidateAnAction(string parm) {
                return null;
            }
        }

        private class ActionClass : BaseActionClass {
            public override string ValidateAnAction() {
                return string.Empty;
            }

            public override string DisableAnAction() {
                return null;
            }

            public override bool HideAnAction() {
                return false;
            }

            public override IList<string> Choices0AnAction() {
                return null;
            }

            public override string Default0AnAction() {
                return null;
            }

            public override string Validate0AnAction(string parm) {
                return null;
            }

            public override IList<string> ChoicesAnAction(string parm) {
                return null;
            }

            public override string DefaultAnAction(string parm) {
                return null;
            }

            public override string ValidateAnAction(string parm) {
                return null;
            }
        }

        private class BaseNoActionClass {
            public virtual string ValidateAnAction() {
                return string.Empty;
            }

            public virtual string DisableAnAction() {
                return null;
            }

            public virtual bool HideAnAction() {
                return false;
            }

            public virtual IList<string> Choices0AnAction() {
                return null;
            }

            public virtual string Default0AnAction() {
                return null;
            }

            public virtual string Validate0AnAction(string parm) {
                return null;
            }

            public virtual IList<string> ChoicesAnAction(string parm) {
                return null;
            }

            public virtual string DefaultAnAction(string parm) {
                return null;
            }

            public virtual string ValidateAnAction(string parm) {
                return null;
            }
        }

        private class NoActionClass : BaseNoActionClass {
            public override string ValidateAnAction() {
                return string.Empty;
            }

            public override string DisableAnAction() {
                return null;
            }

            public override bool HideAnAction() {
                return false;
            }

            public override IList<string> Choices0AnAction() {
                return null;
            }

            public override string Default0AnAction() {
                return null;
            }

            public override string Validate0AnAction(string parm) {
                return null;
            }

            public override IList<string> ChoicesAnAction(string parm) {
                return null;
            }

            public override string DefaultAnAction(string parm) {
                return null;
            }

            public override string ValidateAnAction(string parm) {
                return null;
            }
        }

        [Test]
        public override void TestFeatureTypes() {
            NakedObjectFeatureType[] featureTypes = facetFactory.FeatureTypes;
            Assert.IsFalse(Contains(featureTypes, NakedObjectFeatureType.Objects));
            Assert.IsFalse(Contains(featureTypes, NakedObjectFeatureType.Property));
            Assert.IsFalse(Contains(featureTypes, NakedObjectFeatureType.Collection));
            Assert.IsTrue(Contains(featureTypes, NakedObjectFeatureType.Action));
            Assert.IsFalse(Contains(featureTypes, NakedObjectFeatureType.ActionParameter));
        }

        [Test]
        public void TestFiltersAddTo() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (CollectionClass), "AddToACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersChoices() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "ChoicesAProperty");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersClear() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "ClearAProperty");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersDefault() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "DefaultAProperty");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersDisable() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "DisableAProperty");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersDisableAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (ActionClass), "DisableAnAction");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersHide() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "HideAProperty");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersHideAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (ActionClass), "HideAnAction");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersModify() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "ModifyAProperty");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersParameterChoices() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (ActionClass), "ChoicesAnAction");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersParameterDefault() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (ActionClass), "DefaultAnAction");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersParameterIndexChoices() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (ActionClass), "Choices0AnAction");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersParameterIndexDefault() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (ActionClass), "Default0AnAction");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersParameterIndexValidate() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (ActionClass), "Validate0AnAction");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersParameterValidate() {
            MethodInfo actionMethod = FindMethod(typeof (ActionClass), "ValidateAnAction", new[] {typeof (string)});
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersRemoveFrom() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (CollectionClass), "RemoveFromACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersValidate() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "ValidateAProperty");
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersValidateAction() {
            MethodInfo actionMethod = FindMethod(typeof (ActionClass), "ValidateAnAction", new Type[] {});
            Assert.IsTrue(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersValidateAddTo() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (CollectionClass1), "ValidateAddToACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestFiltersValidateRemoveFrom() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (CollectionClass1), "ValidateRemoveFromACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesAddToIfNoBaseCollection() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoCollectionClass), "AddToACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesChoicesIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "ChoicesAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesClearIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "ClearAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesDefaultIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "DefaultAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesDisableActionIfNoAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoActionClass), "DisableAnAction");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesDisableIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "DisableAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesHideActionIfNoAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoActionClass), "HideAnAction");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesHideIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "HideAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesMeaninglessPrefix() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (PropertyClass), "MeaninglessPrefixAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesMeaninglessPrefixIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "MeaninglessPrefixAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesModifyIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "ModifyAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesParameterChoicesIfNoAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoActionClass), "ChoicesAnAction");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesParameterDefaultIfNoAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoActionClass), "DefaultAnAction");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesParameterIndexChoicesIfNoAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoActionClass), "Choices0AnAction");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesParameterIndexDefaultIfNoAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoActionClass), "Default0AnAction");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesParameterIndexValidateIfNoAction() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoActionClass), "Validate0AnAction");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesParameterValidateIfNoAction() {
            MethodInfo actionMethod = FindMethod(typeof (NoActionClass), "ValidateAnAction", new[] {typeof (string)});
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesRemoveFromIfNoBaseCollection() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoCollectionClass), "RemoveFromACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesValidateActionIfNoAction() {
            MethodInfo actionMethod = FindMethod(typeof (NoActionClass), "ValidateAnAction", new Type[] {});
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesValidateAddToIfNoBaseCollection() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoCollectionClass1), "ValidateAddToACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesValidateIfNoBaseProperty() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoPropertyClass), "ValidateAProperty");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }

        [Test]
        public void TestLeavesValidateRemoveFromIfNoBaseCollection() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (NoCollectionClass1), "ValidateRemoveFromACollection");
            Assert.IsFalse(facetFactory.Filters(actionMethod));
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
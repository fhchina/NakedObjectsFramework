// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.SpecImmutable;
using NakedObjects.Reflect.FacetFactory;

namespace NakedObjects.Reflect.Test.FacetFactory {
    [TestClass]
    public class ActionMethodsFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new ActionMethodsFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private ActionMethodsFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get {
                return new[] {
                    typeof (INamedFacet),
                    typeof (IExecutedFacet),
                    typeof (IActionValidationFacet),
                    typeof (IActionParameterValidationFacet),
                    typeof (IActionInvocationFacet),
                    typeof (IActionDefaultsFacet),
                    typeof (IActionChoicesFacet),
                    typeof (IDescribedAsFacet),
                    typeof (IMandatoryFacet)
                };
            }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        private static ActionSpecImmutable CreateHolderWithParms() {
            var tps1 = new Mock<IObjectSpecImmutable>(); //"System.Int32"
            var tps2 = new Mock<IObjectSpecImmutable>(); //System.Int64"
            var tps3 = new Mock<IObjectSpecImmutable>(); //"System.Int64"

            var param1 = new ActionParameterSpecImmutable(tps1.Object);
            var param2 = new ActionParameterSpecImmutable(tps2.Object);
            var param3 = new ActionParameterSpecImmutable(tps3.Object);

            var parms = new IActionParameterSpecImmutable[] {param1, param2, param3};

            var tpi = new Mock<IIdentifier>(); // ""action"

            IIdentifier id = tpi.Object;
            return new ActionSpecImmutable(id, null, parms);
        }

        private void CheckDefaultFacet(MethodInfo defaultMethod, IActionParameterSpecImmutable parameter) {
            IFacet facet = parameter.GetFacet(typeof (IActionDefaultsFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionDefaultsFacetViaMethod);
            Assert.AreEqual(defaultMethod, ((ActionDefaultsFacetViaMethod) facet).GetMethod());

            AssertMethodRemoved(defaultMethod);
        }

        private void CheckValidatePrameterFacet(MethodInfo method, IActionParameterSpecImmutable parameter) {
            IFacet facet = parameter.GetFacet(typeof (IActionParameterValidationFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionParameterValidation);
            Assert.AreEqual(method, ((ActionParameterValidation) facet).GetMethod());

            AssertMethodRemoved(method);
        }

        private void CheckChoicesFacet(MethodInfo choicesMethod, IActionParameterSpecImmutable parameter) {
            IFacet facet = parameter.GetFacet(typeof (IActionChoicesFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionChoicesFacetViaMethod);
            Assert.AreEqual(choicesMethod, ((ActionChoicesFacetViaMethod) facet).GetMethod());

            AssertMethodRemoved(choicesMethod);
        }

        private void CheckAutoCompleteFacet(MethodInfo autoCompleteMethod, IActionParameterSpecImmutable parameter, int pageSize, int minLength) {
            IFacet facet = parameter.GetFacet(typeof (IAutoCompleteFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is AutoCompleteFacet);
            var acf = (AutoCompleteFacet) facet;
            Assert.AreEqual(autoCompleteMethod, acf.GetMethod());

            AssertMethodRemoved(autoCompleteMethod);

            Assert.AreEqual(pageSize, acf.PageSize);
            Assert.AreEqual(minLength, acf.MinLength);
        }

        private void CheckAutoCompleteFacetIsNull(MethodInfo autoCompleteMethod, IActionParameterSpecImmutable parameter) {
            IFacet facet = parameter.GetFacet(typeof (IAutoCompleteFacet));
            Assert.IsNull(facet);

            AssertMethodNotRemoved(autoCompleteMethod);
        }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local

        private class Customer {
            public void SomeAction() {}
        }

        private class Customer1 {
            [Named("Renamed an action with a named annotation")]
            public void AnActionWithNamedAnnotation() {}

            public void AnActionWithNullableParm(bool? parm) {}
            public void AnActionWithoutNullableParm(bool parm) {}
        }

        private class Customer11 {
            public void SomeAction(int x, long y, long z) {}

            public int Default0SomeAction() {
                return 0;
            }

            [Executed(Where.Remotely)]
            public long Default1SomeAction() {
                return 0;
            }

            [Executed(Where.Locally)]
            public long Default2SomeAction() {
                return 0;
            }
        }

        private class Customer22 {
            public void SomeAction(int x, long y, long z) {}

            public int DefaultSomeAction(int x) {
                return 0;
            }

            [Executed(Where.Remotely)]
            public long DefaultSomeAction(long y) {
                return 0;
            }

            [Executed(Where.Locally)]
            public long Default2SomeAction() {
                return 0;
            }
        }

        private class Customer13 {
            public void SomeAction(int x, long y, long z) {}

            public int[] Choices0SomeAction() {
                return new int[0];
            }

            [Executed(Where.Remotely)]
            public long[] Choices1SomeAction() {
                return new long[0];
            }

            [Executed(Where.Locally)]
            public long[] Choices2SomeAction() {
                return new long[0];
            }
        }

        private class Customer26 {
            public void SomeAction(string x, Customer26 y, long z) {}

            public IQueryable<string> AutoComplete0SomeAction(string name) {
                return new string[0].AsQueryable();
            }

            public IQueryable<Customer26> AutoComplete1SomeAction(string name) {
                return new Customer26[0].AsQueryable();
            }

            public IQueryable<long> AutoComplete2SomeAction(string name) {
                return new long[0].AsQueryable();
            }
        }

        private class Customer27 {
            public void SomeAction(string x, string y, string z) {}

            public IEnumerable<string> AutoComplete0SomeAction(string name) {
                return new string[0].AsQueryable();
            }

            public IQueryable<string> AutoComplete1SomeAction() {
                return new string[0].AsQueryable();
            }

            public IQueryable<string> AutoComplete2SomeAction(int name) {
                return new string[0].AsQueryable();
            }
        }

        private class Customer28 {
            public void SomeAction(string x, Customer26 y, long z) {}

            [PageSize(33)]
            public IQueryable<string> AutoComplete0SomeAction([MinLength(2)] string name) {
                return new string[0].AsQueryable();
            }

            [PageSize(66)]
            public IQueryable<Customer26> AutoComplete1SomeAction([MinLength(3)] string name) {
                return new Customer26[0].AsQueryable();
            }
        }

        private class Customer30 {
            public void SomeAction(int x, long y, long z) {}

            public int[] Choices0SomeAction(long y, long z) {
                return new int[0];
            }

            [Executed(Where.Remotely)]
            public long[] Choices1SomeAction(long z) {
                return new long[0];
            }

            [Executed(Where.Locally)]
            public long[] Choices2SomeAction() {
                return new long[0];
            }
        }

        private class Customer31 {
            public void SomeAction(int x, long y, long z) {}

            public int[] Choices0SomeAction(long y, long z) {
                return new int[0];
            }

            [Executed(Where.Remotely)]
            public long[] Choices0SomeAction(long z) {
                return new long[0];
            }

            [Executed(Where.Locally)]
            public long[] Choices0SomeAction() {
                return new long[0];
            }
        }

        private class Customer21 {
            public void SomeAction(int x, long y, long z) {}

            public int[] ChoicesSomeAction(int x) {
                return new int[0];
            }

            [Executed(Where.Remotely)]
            public long[] ChoicesSomeAction(long y) {
                return new long[0];
            }

            [Executed(Where.Locally)]
            public long[] Choices2SomeAction() {
                return new long[0];
            }
        }

        private class Customer14 {
            public void SomeAction() {}
        }

        private class Customer15 {
            public string SomeAction() {
                return null;
            }
        }

        private class Customer16 {
            public string SomeAction() {
                return null;
            }
        }

        private class Customer8 {
            public void SomeAction() {}

            public string ValidateSomeAction() {
                return null;
            }
        }

        private class Customer9 {
            public void SomeAction(int x, int y) {}

            public string ValidateSomeAction(int x, int y) {
                return null;
            }
        }

        private class Customer10 {
            public void SomeActionOne() {}

            public bool HideSomeActionOne() {
                return false;
            }

            public void SomeActionTwo(int x) {}

            public bool HideSomeActionTwo(int x) {
                return false;
            }

            public void SomeActionThree(int x) {}

            public bool HideSomeActionThree() {
                return false;
            }

            public void SomeActionFour(int x, int y) {}

            public bool HideSomeActionFour(int x, int y) {
                return false;
            }

            public bool HideSomeActionFour() {
                return false;
            }
        }

        private class Customer12 {
            public void SomeActionOne() {}

            public string DisableSomeActionOne() {
                return "";
            }

            public void SomeActionTwo(int x) {}

            public string DisableSomeActionTwo(int x) {
                return "";
            }

            public void SomeActionThree(int x) {}

            public string DisableSomeActionThree() {
                return "";
            }

            public void SomeActionFour(int x, int y) {}

            public string DisableSomeActionFour(int x, int y) {
                return "";
            }

            public string DisableSomeActionFour() {
                return "";
            }
        }

        private class Customer18 {
            public string DisableActionDefault() {
                return "";
            }

            public void SomeActionTwo(int x) {}

            public string DisableSomeActionTwo(int x) {
                return "";
            }

            public void SomeActionThree(int x) {}
        }

        private class Customer19 {
            public bool HideActionDefault() {
                return false;
            }

            public void SomeActionTwo(int x) {}

            public bool HideSomeActionTwo(int x) {
                return false;
            }

            public void SomeActionThree(int x) {}
        }

        public class CustomerStatic {
            public void SomeAction(int x, long y) {}

            public static bool HideSomeAction(IPrincipal principal) {
                return true;
            }

            public static string DisableSomeAction(IPrincipal principal) {
                return "disabled for this user";
            }

            public static void OtherAction(int x, long y) {}
        }

        private class Customer17 {
            public void SomeAction(int x, long y, long z) {}

            public string Validate0SomeAction(int x) {
                return "failed";
            }

            public string Validate1SomeAction(long x) {
                return null;
            }
        }

        private class Customer20 {
            public void SomeAction(int x, long y, long z) {}

            public string ValidateSomeAction(int x) {
                return "failed";
            }

            public string ValidateSomeAction(long y) {
                return null;
            }
        }

        private class Customer23 {
            public void SomeAction(int x, long y, long z) {}

            [Executed(Ajax.Enabled)]
            public string ValidateSomeAction(int x) {
                return "failed";
            }

            public string ValidateSomeAction(long y) {
                return null;
            }
        }

        private class Customer24 {
            public void SomeAction(int x, long y, long z) {}

            [Executed(Ajax.Disabled)]
            public string ValidateSomeAction(int x) {
                return "failed";
            }

            public string ValidateSomeAction(long y) {
                return null;
            }
        }

        private class Customer25 {
            public void SomeAction(int x, long y, long z) {}
        }

        public interface ICustomer {}

        private class Customer32 {
            public void SomeAction(string x, ICustomer y, long z) {}

            public IQueryable<string> AutoComplete0SomeAction(string name) {
                return new string[0].AsQueryable();
            }

            public IQueryable<ICustomer> AutoComplete1SomeAction(string name) {
                return new ICustomer[0].AsQueryable();
            }

            public IQueryable<long> AutoComplete2SomeAction(string name) {
                return new long[0].AsQueryable();
            }
        }

        private class Customer33 {
            public IQueryable<Customer33> SomeQueryableAction1() {
                return null;
            }

            [QueryOnly]
            public IEnumerable<Customer33> SomeQueryableAction2() {
                return null;
            }
        }

        // ReSharper restore UnusedMember.Local
        // ReSharper restore UnusedParameter.Local

        [TestMethod]
        public void TestActionInvocationFacetIsInstalledAndMethodRemoved() {
            MethodInfo actionMethod = FindMethod(typeof (Customer), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionInvocationFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionInvocationFacetViaMethod);
            var actionInvocationFacetViaMethod = (ActionInvocationFacetViaMethod) facet;
            Assert.AreEqual(actionMethod, actionInvocationFacetViaMethod.GetMethod());
            Assert.IsFalse(actionInvocationFacetViaMethod.IsQueryOnly);

            AssertMethodRemoved(actionMethod);
        }

        [TestMethod]
        public void TestActionInvocationFacetQueryableByType() {
            MethodInfo actionMethod = FindMethod(typeof (Customer33), "SomeQueryableAction1");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionInvocationFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionInvocationFacetViaMethod);
            var actionInvocationFacetViaMethod = (ActionInvocationFacetViaMethod) facet;
            Assert.AreEqual(actionMethod, actionInvocationFacetViaMethod.GetMethod());
            Assert.IsTrue(actionInvocationFacetViaMethod.IsQueryOnly);

            AssertMethodRemoved(actionMethod);
        }

        [TestMethod]
        public void TestActionInvocationFacetQueryableByAnnotation() {
            MethodInfo actionMethod = FindMethod(typeof (Customer33), "SomeQueryableAction2");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionInvocationFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionInvocationFacetViaMethod);
            var actionInvocationFacetViaMethod = (ActionInvocationFacetViaMethod) facet;
            Assert.AreEqual(actionMethod, actionInvocationFacetViaMethod.GetMethod());
            Assert.IsTrue(actionInvocationFacetViaMethod.IsQueryOnly);

            AssertMethodRemoved(actionMethod);
        }

        [TestMethod]
        public void TestActionOnType() {
            Type type = typeof (Customer16);
            MethodInfo actionMethod = FindMethod(type, "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionInvocationFacet));
            var actionInvocationFacetViaMethod = (ActionInvocationFacetViaMethod) facet;
            Assert.AreEqual(Reflector.LoadSpecification(type), actionInvocationFacetViaMethod.OnType);
        }

        [TestMethod]
        public void TestActionReturnTypeWhenNotVoid() {
            MethodInfo actionMethod = FindMethod(typeof (Customer15), "SomeAction");
            //   reflector.LoadSpecification(typeof(string));
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionInvocationFacet));
            var actionInvocationFacetViaMethod = (ActionInvocationFacetViaMethod) facet;
            Assert.AreEqual(Reflector.LoadSpecification(typeof (string)), actionInvocationFacetViaMethod.ReturnType);
        }

        [TestMethod]
        public void TestActionReturnTypeWhenVoid() {
            MethodInfo actionMethod = FindMethod(typeof (Customer14), "SomeAction");
            //     reflector.setLoadSpecificationClassReturn(voidNoSpec);
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionInvocationFacet));
            var actionInvocationFacetViaMethod = (ActionInvocationFacetViaMethod) facet;
            Assert.AreEqual(Reflector.LoadSpecification(typeof (void)), actionInvocationFacetViaMethod.ReturnType);
        }

        [TestMethod]
        public void TestAddsNullableFacetToParm() {
            MethodInfo method = FindMethodIgnoreParms(typeof (Customer1), "AnActionWithNullableParm");
            facetFactory.ProcessParams(Reflector, method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (INullableFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is NullableFacetAlways);
        }

        [TestMethod]
        public void TestAjaxFacetAddedIfNoValidate() {
            MethodInfo method = FindMethodIgnoreParms(typeof (Customer25), "SomeAction");
            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, method, MethodRemover, facetHolderWithParms);
            IFacet facet = facetHolderWithParms.Parameters[0].GetFacet(typeof (IAjaxFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is AjaxFacet);
        }

        [TestMethod]
        public void TestAjaxFacetFoundAndMethodRemovedDisabled() {
            MethodInfo method = FindMethodIgnoreParms(typeof (Customer24), "SomeAction");
            MethodInfo propertyValidateMethod = FindMethod(typeof (Customer24), "ValidateSomeAction", new[] {typeof (int)});
            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, method, MethodRemover, facetHolderWithParms);
            IFacet facet = facetHolderWithParms.Parameters[0].GetFacet(typeof (IAjaxFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is AjaxFacet);

            AssertMethodRemoved(propertyValidateMethod);
        }

        [TestMethod]
        public void TestAjaxFacetFoundAndMethodRemovedEnabled() {
            MethodInfo method = FindMethodIgnoreParms(typeof (Customer23), "SomeAction");
            MethodInfo propertyValidateMethod = FindMethod(typeof (Customer23), "ValidateSomeAction", new[] {typeof (int)});
            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, method, MethodRemover, facetHolderWithParms);
            IFacet facet = facetHolderWithParms.Parameters[0].GetFacet(typeof (IAjaxFacet));
            Assert.IsNull(facet);

            AssertMethodRemoved(propertyValidateMethod);
        }

        [TestMethod]
        public void TestAjaxFacetNotAddedByDefault() {
            MethodInfo method = FindMethodIgnoreParms(typeof (Customer20), "SomeAction");
            MethodInfo propertyValidateMethod = FindMethod(typeof (Customer20), "ValidateSomeAction", new[] {typeof (int)});
            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, method, MethodRemover, facetHolderWithParms);
            IFacet facet = facetHolderWithParms.Parameters[0].GetFacet(typeof (IAjaxFacet));
            Assert.IsNull(facet);

            AssertMethodRemoved(propertyValidateMethod);
        }

        [TestMethod]
        public void TestDoesntAddNullableFacetToParm() {
            MethodInfo method = FindMethodIgnoreParms(typeof (Customer1), "AnActionWithoutNullableParm");
            facetFactory.ProcessParams(Reflector, method, 0, Specification);
            IFacet facet = Specification.GetFacet(typeof (INullableFacet));
            Assert.IsNull(facet);
        }

        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Property));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Action));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.ActionParameter));
        }

        [TestMethod]
        public void TestIgnoresParameterAutoCompleteMethodByIndexNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer27), "SomeAction");
            MethodInfo autoComplete0Method = FindMethodIgnoreParms(typeof (Customer27), "AutoComplete0SomeAction");
            MethodInfo autoComplete1Method = FindMethodIgnoreParms(typeof (Customer27), "AutoComplete1SomeAction");
            MethodInfo autoComplete2Method = FindMethodIgnoreParms(typeof (Customer27), "AutoComplete2SomeAction");

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckAutoCompleteFacetIsNull(autoComplete0Method, facetHolderWithParms.Parameters[0]);
            CheckAutoCompleteFacetIsNull(autoComplete1Method, facetHolderWithParms.Parameters[1]);
            CheckAutoCompleteFacetIsNull(autoComplete2Method, facetHolderWithParms.Parameters[2]);
        }

        [TestMethod]
        public void TestInstallsDisabledForSessionFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (CustomerStatic), "SomeAction", new[] {typeof (int), typeof (long)});
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IDisableForSessionFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is DisableForSessionFacetNone);
        }

        [TestMethod]
        public void TestInstallsHiddenForSessionFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (CustomerStatic), "SomeAction", new[] {typeof (int), typeof (long)});
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHideForSessionFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HideForSessionFacetNone);
        }

        [TestMethod]
        public void TestInstallsParameterAutoCompleteMethodAttrributes() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer28), "SomeAction");
            MethodInfo autoComplete0Method = FindMethodIgnoreParms(typeof (Customer28), "AutoComplete0SomeAction");
            MethodInfo autoComplete1Method = FindMethodIgnoreParms(typeof (Customer28), "AutoComplete1SomeAction");

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckAutoCompleteFacet(autoComplete0Method, facetHolderWithParms.Parameters[0], 33, 2);
            CheckAutoCompleteFacet(autoComplete1Method, facetHolderWithParms.Parameters[1], 66, 3);
        }

        [TestMethod]
        public void TestInstallsParameterAutoCompleteMethodByIndexNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer26), "SomeAction");
            MethodInfo autoComplete0Method = FindMethodIgnoreParms(typeof (Customer26), "AutoComplete0SomeAction");
            MethodInfo autoComplete1Method = FindMethodIgnoreParms(typeof (Customer26), "AutoComplete1SomeAction");
            MethodInfo autoComplete2Method = FindMethodIgnoreParms(typeof (Customer26), "AutoComplete2SomeAction");

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckAutoCompleteFacet(autoComplete0Method, facetHolderWithParms.Parameters[0], 50, 0);
            CheckAutoCompleteFacet(autoComplete1Method, facetHolderWithParms.Parameters[1], 50, 0);
            CheckAutoCompleteFacetIsNull(autoComplete2Method, facetHolderWithParms.Parameters[2]);
        }

        [TestMethod]
        public void TestInstallsParameterAutoCompleteMethodByIndexNoArgsFacetAndRemovesMethodInterface() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer32), "SomeAction");
            MethodInfo autoComplete0Method = FindMethodIgnoreParms(typeof (Customer32), "AutoComplete0SomeAction");
            MethodInfo autoComplete1Method = FindMethodIgnoreParms(typeof (Customer32), "AutoComplete1SomeAction");
            MethodInfo autoComplete2Method = FindMethodIgnoreParms(typeof (Customer32), "AutoComplete2SomeAction");

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckAutoCompleteFacet(autoComplete0Method, facetHolderWithParms.Parameters[0], 50, 0);
            CheckAutoCompleteFacet(autoComplete1Method, facetHolderWithParms.Parameters[1], 50, 0);
            CheckAutoCompleteFacetIsNull(autoComplete2Method, facetHolderWithParms.Parameters[2]);
        }

        [TestMethod]
        public void TestInstallsParameterChoicesMethodByIndexNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer13), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo choices0Method = FindMethod(typeof (Customer13), "Choices0SomeAction", new Type[] {});
            MethodInfo choices1Method = FindMethod(typeof (Customer13), "Choices1SomeAction", new Type[] {});
            MethodInfo choices2Method = FindMethod(typeof (Customer13), "Choices2SomeAction", new Type[] {});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckChoicesFacet(choices0Method, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            CheckChoicesFacet(choices1Method, facetHolderWithParms.Parameters[1]);

            var facetExecuted1 = facetHolderWithParms.Parameters[1].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted1);

            Assert.AreEqual(facetExecuted1.ExecutedWhere(choices1Method), Where.Remotely);
            Assert.AreEqual(facetExecuted1.ExecutedWhere(choices0Method), Where.Default);

            CheckChoicesFacet(choices2Method, facetHolderWithParms.Parameters[2]);

            var facetExecuted2 = facetHolderWithParms.Parameters[2].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted2);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(choices2Method), Where.Locally);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(choices0Method), Where.Default);
        }

        [TestMethod]
        public void TestInstallsParameterChoicesMethodByIndexNoArgsFacetAndRemovesMethodDuplicate() {
            MethodInfo actionMethod = FindMethod(typeof (Customer30), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo choices0Method1 = FindMethod(typeof (Customer30), "Choices0SomeAction", new[] {typeof (long), typeof (long)});
            MethodInfo choices0Method2 = FindMethod(typeof (Customer30), "Choices0SomeAction", new[] {typeof (long)});
            MethodInfo choices0Method3 = FindMethod(typeof (Customer30), "Choices0SomeAction", new Type[] {});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckChoicesFacet(choices0Method1, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            AssertMethodNotRemoved(choices0Method2);
            AssertMethodNotRemoved(choices0Method3);
        }

        [TestMethod]
        public void TestInstallsParameterChoicesMethodByIndexNoArgsFacetAndRemovesMethodWithParms() {
            MethodInfo actionMethod = FindMethod(typeof (Customer30), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo choices0Method = FindMethod(typeof (Customer30), "Choices0SomeAction", new[] {typeof (long), typeof (long)});
            MethodInfo choices1Method = FindMethod(typeof (Customer30), "Choices1SomeAction", new[] {typeof (long)});
            MethodInfo choices2Method = FindMethod(typeof (Customer30), "Choices2SomeAction", new Type[] {});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckChoicesFacet(choices0Method, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            CheckChoicesFacet(choices1Method, facetHolderWithParms.Parameters[1]);

            var facetExecuted1 = facetHolderWithParms.Parameters[1].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted1);

            Assert.AreEqual(facetExecuted1.ExecutedWhere(choices1Method), Where.Remotely);
            Assert.AreEqual(facetExecuted1.ExecutedWhere(choices0Method), Where.Default);

            CheckChoicesFacet(choices2Method, facetHolderWithParms.Parameters[2]);

            var facetExecuted2 = facetHolderWithParms.Parameters[2].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted2);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(choices2Method), Where.Locally);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(choices0Method), Where.Default);
        }

        [TestMethod]
        public void TestInstallsParameterChoicesMethodByNameNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer21), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo choices0Method = FindMethod(typeof (Customer21), "ChoicesSomeAction", new[] {typeof (int)});
            MethodInfo choices1Method = FindMethod(typeof (Customer21), "ChoicesSomeAction", new[] {typeof (long)});
            MethodInfo choices2Method = FindMethod(typeof (Customer21), "Choices2SomeAction", new Type[] {});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();
            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckChoicesFacet(choices0Method, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            CheckChoicesFacet(choices1Method, facetHolderWithParms.Parameters[1]);

            var facetExecuted1 = facetHolderWithParms.Parameters[1].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted1);

            Assert.AreEqual(facetExecuted1.ExecutedWhere(choices1Method), Where.Remotely);
            Assert.AreEqual(facetExecuted1.ExecutedWhere(choices0Method), Where.Default);

            CheckChoicesFacet(choices2Method, facetHolderWithParms.Parameters[2]);

            var facetExecuted2 = facetHolderWithParms.Parameters[2].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted2);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(choices2Method), Where.Locally);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(choices0Method), Where.Default);
        }

        [TestMethod]
        public void TestInstallsParameterDefaultsMethodByIndexNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer11), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo default0Method = FindMethod(typeof (Customer11), "Default0SomeAction", new Type[] {});
            MethodInfo default1Method = FindMethod(typeof (Customer11), "Default1SomeAction", new Type[] {});
            MethodInfo default2Method = FindMethod(typeof (Customer11), "Default2SomeAction", new Type[] {});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();

            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckDefaultFacet(default0Method, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            CheckDefaultFacet(default1Method, facetHolderWithParms.Parameters[1]);

            var facetExecuted1 = facetHolderWithParms.Parameters[1].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted1);

            Assert.AreEqual(facetExecuted1.ExecutedWhere(default1Method), Where.Remotely);
            Assert.AreEqual(facetExecuted1.ExecutedWhere(default0Method), Where.Default);

            CheckDefaultFacet(default2Method, facetHolderWithParms.Parameters[2]);

            var facetExecuted2 = facetHolderWithParms.Parameters[2].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted2);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(default2Method), Where.Locally);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(default0Method), Where.Default);
        }

        [TestMethod]
        public void TestInstallsParameterDefaultsMethodByNameNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer22), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo default0Method = FindMethod(typeof (Customer22), "DefaultSomeAction", new[] {typeof (int)});
            MethodInfo default1Method = FindMethod(typeof (Customer22), "DefaultSomeAction", new[] {typeof (long)});
            MethodInfo default2Method = FindMethod(typeof (Customer22), "Default2SomeAction", new Type[] {});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();

            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckDefaultFacet(default0Method, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            CheckDefaultFacet(default1Method, facetHolderWithParms.Parameters[1]);

            var facetExecuted1 = facetHolderWithParms.Parameters[1].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted1);

            Assert.AreEqual(facetExecuted1.ExecutedWhere(default1Method), Where.Remotely);
            Assert.AreEqual(facetExecuted1.ExecutedWhere(default0Method), Where.Default);

            CheckDefaultFacet(default2Method, facetHolderWithParms.Parameters[2]);

            var facetExecuted2 = facetHolderWithParms.Parameters[2].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNotNull(facetExecuted2);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(default2Method), Where.Locally);
            Assert.AreEqual(facetExecuted2.ExecutedWhere(default0Method), Where.Default);
        }

        [TestMethod]
        public void TestInstallsParameterValidationMethodByIndexNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer17), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo validateParameter0Method = FindMethod(typeof (Customer17), "Validate0SomeAction", new[] {typeof (int)});
            MethodInfo validateParameter1Method = FindMethod(typeof (Customer17), "Validate1SomeAction", new[] {typeof (long)});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();

            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckValidatePrameterFacet(validateParameter0Method, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            CheckValidatePrameterFacet(validateParameter1Method, facetHolderWithParms.Parameters[1]);

            var facetExecuted1 = facetHolderWithParms.Parameters[1].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNull(facetExecuted1);
        }

        [TestMethod]
        public void TestInstallsParameterValidationMethodByNameNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer20), "SomeAction", new[] {typeof (int), typeof (long), typeof (long)});
            MethodInfo validateParameter0Method = FindMethod(typeof (Customer20), "ValidateSomeAction", new[] {typeof (int)});
            MethodInfo validateParameter1Method = FindMethod(typeof (Customer20), "ValidateSomeAction", new[] {typeof (long)});

            ActionSpecImmutable facetHolderWithParms = CreateHolderWithParms();

            facetFactory.Process(Reflector, actionMethod, MethodRemover, facetHolderWithParms);

            CheckValidatePrameterFacet(validateParameter0Method, facetHolderWithParms.Parameters[0]);

            IFacet facetExecuted0 = facetHolderWithParms.Parameters[0].GetFacet(typeof (IExecutedControlMethodFacet));
            Assert.IsNull(facetExecuted0);

            CheckValidatePrameterFacet(validateParameter1Method, facetHolderWithParms.Parameters[1]);

            var facetExecuted1 = facetHolderWithParms.Parameters[1].GetFacet<IExecutedControlMethodFacet>();
            Assert.IsNull(facetExecuted1);
        }

        [TestMethod]
        public void TestInstallsValidateMethodNoArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer8), "SomeAction");
            MethodInfo validateMethod = FindMethod(typeof (Customer8), "ValidateSomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionValidationFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionValidationFacet);
            var actionValidationFacetViaMethod = (ActionValidationFacet) facet;
            Assert.AreEqual(validateMethod, actionValidationFacetViaMethod.GetMethod());
            AssertMethodRemoved(validateMethod);
        }

        [TestMethod]
        public void TestInstallsValidateMethodSomeArgsFacetAndRemovesMethod() {
            MethodInfo actionMethod = FindMethod(typeof (Customer9), "SomeAction", new[] {typeof (int), typeof (int)});
            MethodInfo validateMethod = FindMethod(typeof (Customer9), "ValidateSomeAction", new[] {typeof (int), typeof (int)});
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IActionValidationFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is ActionValidationFacet);
            var actionValidationFacetViaMethod = (ActionValidationFacet) facet;
            Assert.AreEqual(validateMethod, actionValidationFacetViaMethod.GetMethod());
            AssertMethodRemoved(validateMethod);
        }

        [TestMethod]
        public void TestPickUpDefaultDisableMethod() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer18), "SomeActionThree");
            MethodInfo disableMethod = FindMethodIgnoreParms(typeof (Customer18), "DisableActionDefault");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IDisableForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is DisableForContextFacet);
            Assert.AreEqual(disableMethod, ((IImperativeFacet) facet).GetMethod());
            AssertMethodNotRemoved(disableMethod);
        }

        [TestMethod]
        public void TestPickUpDefaultHideMethod() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer19), "SomeActionThree");
            MethodInfo disableMethod = FindMethodIgnoreParms(typeof (Customer19), "HideActionDefault");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IHideForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HideForContextFacet);
            Assert.AreEqual(disableMethod, ((IImperativeFacet) facet).GetMethod());
            AssertMethodNotRemoved(disableMethod);
        }

        [TestMethod]
        public void TestPickUpDisableMethodDifferentSignature() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer12), "SomeActionThree");
            MethodInfo hideMethod = FindMethodIgnoreParms(typeof (Customer12), "DisableSomeActionThree");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IDisableForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is DisableForContextFacet);
            Assert.AreEqual(hideMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpDisableMethodNoParms() {
            MethodInfo actionMethod = FindMethod(typeof (Customer12), "SomeActionOne");
            MethodInfo hideMethod = FindMethod(typeof (Customer12), "DisableSomeActionOne");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IDisableForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is DisableForContextFacet);
            Assert.AreEqual(hideMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpDisableMethodOverriddingDefault() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer18), "SomeActionTwo");
            MethodInfo disableMethod = FindMethodIgnoreParms(typeof (Customer18), "DisableSomeActionTwo");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IDisableForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is DisableForContextFacet);
            Assert.AreEqual(disableMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpDisableMethodSameSignature() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer12), "SomeActionTwo");
            MethodInfo hideMethod = FindMethodIgnoreParms(typeof (Customer12), "DisableSomeActionTwo");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IDisableForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is DisableForContextFacet);
            Assert.AreEqual(hideMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpDisableMethodSignatureChoice() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer12), "SomeActionFour");
            MethodInfo hideMethodGood = FindMethod(typeof (Customer12), "DisableSomeActionFour", new[] {typeof (int), typeof (int)});
            MethodInfo hideMethodBad = FindMethod(typeof (Customer12), "DisableSomeActionFour");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IDisableForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is DisableForContextFacet);
            Assert.AreEqual(hideMethodGood, ((IImperativeFacet) facet).GetMethod());
            Assert.AreNotEqual(hideMethodBad, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpHideMethodDifferentSignature() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer10), "SomeActionThree");
            MethodInfo hideMethod = FindMethodIgnoreParms(typeof (Customer10), "HideSomeActionThree");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IHideForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HideForContextFacet);
            Assert.AreEqual(hideMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpHideMethodNoParms() {
            MethodInfo actionMethod = FindMethod(typeof (Customer10), "SomeActionOne");
            MethodInfo hideMethod = FindMethod(typeof (Customer10), "HideSomeActionOne");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IHideForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HideForContextFacet);
            Assert.AreEqual(hideMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpHideMethodOverriddingDefault() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer19), "SomeActionTwo");
            MethodInfo hideMethod = FindMethodIgnoreParms(typeof (Customer19), "HideSomeActionTwo");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IHideForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HideForContextFacet);
            Assert.AreEqual(hideMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpHideMethodSameSignature() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer10), "SomeActionTwo");
            MethodInfo hideMethod = FindMethodIgnoreParms(typeof (Customer10), "HideSomeActionTwo");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IHideForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HideForContextFacet);
            Assert.AreEqual(hideMethod, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestPickUpHideMethodSignatureChoice() {
            MethodInfo actionMethod = FindMethodIgnoreParms(typeof (Customer10), "SomeActionFour");
            MethodInfo hideMethodGood = FindMethod(typeof (Customer10), "HideSomeActionFour", new[] {typeof (int), typeof (int)});
            MethodInfo hideMethodBad = FindMethod(typeof (Customer10), "HideSomeActionFour");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);

            var facet = Specification.GetFacet<IHideForContextFacet>();
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HideForContextFacet);
            Assert.AreEqual(hideMethodGood, ((IImperativeFacet) facet).GetMethod());
            Assert.AreNotEqual(hideMethodBad, ((IImperativeFacet) facet).GetMethod());
        }

        [TestMethod]
        public void TestProvidesDefaultNameForActionButIgnoresAnyNamedAnnotation() {
            MethodInfo method = FindMethod(typeof (Customer1), "AnActionWithNamedAnnotation");
            facetFactory.Process(Reflector, method, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (INamedFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is INamedFacet);
            var namedFacet = (INamedFacet) facet;
            Assert.AreEqual("An Action With Named Annotation", namedFacet.Value);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
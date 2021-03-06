﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core;
using NakedObjects.Core.Reflect;
using NakedObjects.Core.Resolve;
using NakedObjects.Core.Spec;
using NakedObjects.Core.Util;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace NakedObjects.Xat {
    public class TestProperty : ITestProperty {
        private readonly ITestObjectFactory factory;
        private readonly IAssociationSpec field;
        private readonly INakedObjectManager manager;
        private readonly ITestHasActions owningObject;
        private readonly IObjectPersistor persistor;

        public TestProperty(IObjectPersistor persistor, IAssociationSpec field, ITestHasActions owningObject, ITestObjectFactory factory, INakedObjectManager manager) {
            this.persistor = persistor;
            this.field = field;
            this.owningObject = owningObject;
            this.factory = factory;
            this.manager = manager;
        }

        #region ITestProperty Members

        public string Name {
            get { return field.Name; }
        }

        public string Id {
            get { return field.Id; }
        }

        public string Title {
            get { return field.PropertyTitle(field.GetNakedObject(owningObject.NakedObject), manager); }
        }

        public ITestNaked Content {
            get {
                INakedObject fieldValue = field.GetNakedObject(owningObject.NakedObject);
                if (fieldValue != null && fieldValue.ResolveState.IsResolvable()) {
                    persistor.ResolveImmediately(fieldValue);
                }

                return factory.CreateTestNaked(fieldValue);
            }
        }

        public ITestObject ContentAsObject {
            get { return (ITestObject) Content; }
        }

        public ITestCollection ContentAsCollection {
            get { return (ITestCollection) Content; }
        }

        public ITestNaked GetDefault() {
            INakedObject defaultValue = field.GetDefault(owningObject.NakedObject);
            return factory.CreateTestNaked(defaultValue);
        }

        public ITestNaked[] GetChoices() {
            INakedObject[] choices = ((IOneToOneAssociationSpec) field).GetChoices(owningObject.NakedObject, null);
            return choices.Select(x => factory.CreateTestNaked(x)).ToArray();
        }

        public ITestNaked[] GetCompletions(string autoCompleteParm) {
            INakedObject[] completions = ((IOneToOneAssociationSpec) field).GetCompletions(owningObject.NakedObject, autoCompleteParm);
            return completions.Select(x => factory.CreateTestNaked(x)).ToArray();
        }

        public ITestProperty SetObject(ITestObject testObject) {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            Assert.IsFalse(field.ReturnSpec.IsParseable, "Drop(..) not allowed on value target field; use SetValue(..) instead");

            INakedObject testNakedObject = testObject.NakedObject;

            Assert.IsTrue(testNakedObject.Spec.IsOfType(field.ReturnSpec), string.Format("Can't drop a {0} on to the {1} field (which accepts {2})", testObject.NakedObject.Spec.ShortName, Name, field.ReturnSpec));

            INakedObject nakedObject = owningObject.NakedObject;

            IConsent valid;
            var associationSpec = field as IOneToOneAssociationSpec;
            if (associationSpec != null) {
                valid = associationSpec.IsAssociationValid(nakedObject, testNakedObject);
            }
            else if (field is IOneToManyAssociationSpec) {
                valid = new Veto("Always disabled");
            }
            else {
                throw new UnknownTypeException(field);
            }

            LastMessage = valid.Reason;
            Assert.IsFalse(valid.IsVetoed, string.Format("Cannot SetObject {0} in the field {1} within {2}: {3}", testNakedObject, field, nakedObject, valid.Reason));

            var spec = field as IOneToOneAssociationSpec;
            if (spec != null) {
                spec.SetAssociation(nakedObject, testNakedObject);
            }

            return this;
        }

        public ITestProperty RemoveFromCollection(ITestObject testObject) {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            Assert.IsTrue(field is IOneToManyAssociationSpec, "Cannot remove from non collection");

            INakedObject testNakedObject = testObject.NakedObject;

            Assert.IsTrue(testNakedObject.Spec.IsOfType(field.ReturnSpec),
                string.Format("Can't clear a {0} from the {1} field (which accepts {2})", testObject.NakedObject.Spec.ShortName, Name, field.ReturnSpec));

            INakedObject nakedObject = owningObject.NakedObject;

            if (!(field is IOneToManyAssociationSpec)) {
                throw new UnknownTypeException(field);
            }
            IConsent valid = new Veto("Always disabled");

            Assert.IsFalse(valid.IsVetoed, string.Format("Can't remove {0} from the field {1} within {2}: {3}", testNakedObject, field, nakedObject, valid.Reason));
            return this;
        }

        public string LastMessage { get; private set; }

        /// <summary>
        ///     Removes an existing object reference from the specified field. Mirrors the 'Remove Reference' menu
        ///     option that each object field offers by default.
        /// </summary>
        public ITestProperty ClearObject() {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            Assert.IsFalse(field.ReturnSpec.IsParseable, "Clear(..) not allowed on value target field; use SetValue(..) instead");

            INakedObject nakedObject = field.GetNakedObject(owningObject.NakedObject);
            if (nakedObject != null) {
                var spec = field as IOneToOneAssociationSpec;
                if (spec != null) {
                    spec.SetAssociation(owningObject.NakedObject, null);
                }
                else {
                    Assert.Fail("Clear(..) not allowed on collection target field");
                }
            }
            return this;
        }

        public ITestProperty SetValue(string textEntry) {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            INakedObject nakedObject = owningObject.NakedObject;
            try {
                field.GetNakedObject(nakedObject);

                var parseableFacet = field.ReturnSpec.GetFacet<IParseableFacet>();

                INakedObject newValue = parseableFacet.ParseTextEntry(textEntry, manager);

                IConsent consent = ((IOneToOneAssociationSpec) field).IsAssociationValid(nakedObject, newValue);
                LastMessage = consent.Reason;

                Assert.IsFalse(consent.IsVetoed, string.Format("Content: '{0}' is not valid. Reason: {1}", textEntry, consent.Reason));

                ((IOneToOneAssociationSpec) field).SetAssociation(nakedObject, textEntry.Trim().Equals("") ? null : newValue);
            }
            catch (InvalidEntryException) {
                Assert.Fail("Entry not recognised " + textEntry);
            }
            return this;
        }

        public ITestProperty ClearValue() {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            INakedObject nakedObject = owningObject.NakedObject;
            try {
                IConsent consent = ((IOneToOneAssociationSpec) field).IsAssociationValid(nakedObject, null);
                LastMessage = consent.Reason;
                Assert.IsFalse(consent.IsVetoed, string.Format("Content: 'null' is not valid. Reason: {0}", consent.Reason));
                ((IOneToOneAssociationSpec) field).SetAssociation(nakedObject, null);
            }
            catch (InvalidEntryException) {
                Assert.Fail("Null Entry not recognised ");
            }
            return this;
        }

        public ITestProperty TestField(string setValue, string expected) {
            SetValue(setValue);
            Assert.AreEqual("Field '" + Name + "' contains unexpected value", expected, Content.Title);
            return this;
        }

        public ITestProperty TestField(ITestObject expected) {
            SetObject(expected);
            Assert.AreEqual(expected.NakedObject, Content.NakedObject);
            return this;
        }

        #endregion

        private void ResetLastMessage() {
            LastMessage = string.Empty;
        }

        private bool IsNotParseable() {
            return field.ReturnSpec.GetFacet<IParseableFacet>() == null;
        }

        #region Asserts

        public ITestProperty AssertCannotParse(string text) {
            AssertIsVisible();
            AssertIsModifiable();

            INakedObject valueObject = field.GetNakedObject(owningObject.NakedObject);

            Assert.IsNotNull(valueObject, "Field '" + Name + "' contains null, but should contain an INakedObject object");
            try {
                var parseableFacet = field.ReturnSpec.GetFacet<IParseableFacet>();
                parseableFacet.ParseTextEntry(text, manager);
                Assert.Fail("Content was unexpectedly parsed");
            }
            catch (InvalidEntryException /*expected*/) {
                // expected
            }
            return this;
        }

        public ITestProperty AssertFieldEntryInvalid(string text) {
            return IsNotParseable() ? AssertNotParseable() : AssertParseableFieldEntryInvalid(text);
        }

        public ITestProperty AssertFieldEntryIsValid(string text) {
            return IsNotParseable() ? AssertNotParseable() : AssertParseableFieldEntryIsValid(text);
        }

        public ITestProperty AssertSetObjectInvalid(ITestObject testObject) {
            try {
                AssertSetObjectIsValid(testObject);
            }
            catch (AssertFailedException) {
                // expected 
                return this;
            }
            Assert.Fail("Object {0} was allowed in field {1} : expected it to be invalid", testObject, field);
            return this;
        }

        public ITestProperty AssertSetObjectIsValid(ITestObject testObject) {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            Assert.IsFalse(field.ReturnSpec.IsParseable, "Drop(..) not allowed on value target field; use SetValue(..) instead");
            INakedObject testNakedObject = testObject.NakedObject;
            Assert.IsTrue(testNakedObject.Spec.IsOfType(field.ReturnSpec), string.Format("Can't drop a {0} on to the {1} field (which accepts {2})", testObject.NakedObject.Spec.ShortName, Name, field.ReturnSpec));
            INakedObject nakedObject = owningObject.NakedObject;
            IConsent valid;
            var spec = field as IOneToOneAssociationSpec;
            if (spec != null) {
                valid = spec.IsAssociationValid(nakedObject, testNakedObject);
            }
            else if (field is IOneToManyAssociationSpec) {
                valid = new Veto("Always disabled");
            }
            else {
                throw new UnknownTypeException(field);
            }
            LastMessage = valid.Reason;

            Assert.IsFalse(valid.IsVetoed, string.Format("Cannot SetObject {0} in the field {1} within {2}: {3}", testNakedObject, field, nakedObject, valid.Reason));
            return this;
        }

        public ITestProperty AssertIsInvisible() {
            bool canAccess = field.IsVisible(owningObject.NakedObject);
            Assert.IsFalse(canAccess, "Field '" + Name + "' is visible");
            return this;
        }

        public ITestProperty AssertIsVisible() {
            bool canAccess = field.IsVisible(owningObject.NakedObject);
            Assert.IsTrue(canAccess, "Field '" + Name + "' is invisible");
            return this;
        }

        public ITestProperty AssertIsMandatory() {
            Assert.IsTrue(field.IsMandatory, "Field '" + field.Id + "' is optional");
            return this;
        }

        public ITestProperty AssertIsOptional() {
            Assert.IsTrue(!field.IsMandatory, "Field '" + field.Id + "' is mandatory");
            return this;
        }

        public ITestProperty AssertIsDescribedAs(string expected) {
            AssertIsVisible();
            Assert.IsTrue(expected.Equals(field.Description), "Description expected: '" + expected + "' actual: '" + field.Description + "'");
            return this;
        }

        public ITestProperty AssertIsModifiable() {
            AssertIsVisible();
            ResetLastMessage();

            IConsent isUsable = field.IsUsable(owningObject.NakedObject);
            LastMessage = isUsable.Reason;

            bool canUse = isUsable.IsAllowed;
            Assert.IsTrue(canUse, "Field '" + Name + "' in " + owningObject.NakedObject + " is unmodifiable");
            return this;
        }

        public ITestProperty AssertIsUnmodifiable() {
            ResetLastMessage();
            IConsent isUsable = field.IsUsable(owningObject.NakedObject);
            LastMessage = isUsable.Reason;

            bool canUse = isUsable.IsAllowed;
            Assert.IsFalse(canUse, "Field '" + Name + "' in " + owningObject.NakedObject + " is modifiable");
            return this;
        }

        public ITestProperty AssertIsNotEmpty() {
            AssertIsVisible();
            Assert.IsFalse(field.IsEmpty(owningObject.NakedObject), "Expected" + " '" + Name + "' to contain something but it was empty");
            return this;
        }

        public ITestProperty AssertIsEmpty() {
            AssertIsVisible();
            Assert.IsTrue(field.IsEmpty(owningObject.NakedObject), "Expected" + " '" + Name + "' to be empty");
            return this;
        }

        public ITestProperty AssertValueIsEqual(string expected) {
            AssertIsVisible();
            Assert.AreEqual(expected, Content.Title);
            return this;
        }

        public ITestProperty AssertTitleIsEqual(string expected) {
            AssertIsVisible();
            Assert.AreEqual(expected, Title);
            return this;
        }

        public ITestProperty AssertObjectIsEqual(ITestNaked expected) {
            AssertIsVisible();
            Assert.AreEqual(expected, Content);
            return this;
        }

        public ITestProperty AssertIsValidToSave() {
            if (field.IsMandatory && field.IsVisible(owningObject.NakedObject) && field.IsUsable(owningObject.NakedObject).IsAllowed) {
                Assert.IsFalse(field.IsEmpty(owningObject.NakedObject), "Cannot save object as mandatory field " + " '" + Name + "' is empty");
                Assert.IsTrue(field.GetNakedObject(owningObject.NakedObject).ValidToPersist() == null);
            }
            if (field is IOneToManyAssociationSpec) {
                field.GetNakedObject(owningObject.NakedObject).GetAsEnumerable(manager).ForEach(no => Assert.AreEqual(no.ValidToPersist(), null));
            }

            return this;
        }

        public ITestProperty AssertLastMessageIs(string message) {
            Assert.IsTrue(message.Equals(LastMessage), "Last message expected: '" + message + "' actual: '" + LastMessage + "'");
            return this;
        }

        public ITestProperty AssertLastMessageContains(string message) {
            Assert.IsTrue(LastMessage.Contains(message), "Last message expected to contain: '" + message + "' actual: '" + LastMessage + "'");
            return this;
        }

        public ITestProperty AssertParseableFieldEntryInvalid(string text) {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            INakedObject nakedObject = owningObject.NakedObject;
            field.GetNakedObject(nakedObject);
            var parseableFacet = field.ReturnSpec.GetFacet<IParseableFacet>();
            try {
                INakedObject newValue = parseableFacet.ParseTextEntry(text, manager);
                IConsent isAssociationValid = ((IOneToOneAssociationSpec) field).IsAssociationValid(owningObject.NakedObject, newValue);
                LastMessage = isAssociationValid.Reason;
                Assert.IsFalse(isAssociationValid.IsAllowed, "Content was unexpectedly validated");
            }
            catch (InvalidEntryException) {
                // expected 
            }
            return this;
        }

        public ITestProperty AssertParseableFieldEntryIsValid(string text) {
            AssertIsVisible();
            AssertIsModifiable();
            ResetLastMessage();

            INakedObject nakedObject = owningObject.NakedObject;
            field.GetNakedObject(nakedObject);
            var parseableFacet = field.ReturnSpec.GetFacet<IParseableFacet>();
            INakedObject newValue = parseableFacet.ParseTextEntry(text, manager);
            IConsent isAssociationValid = ((IOneToOneAssociationSpec) field).IsAssociationValid(owningObject.NakedObject, newValue);
            LastMessage = isAssociationValid.Reason;
            Assert.IsTrue(isAssociationValid.IsAllowed, "Content was unexpectedly invalidated");
            return this;
        }

        private ITestProperty AssertNotParseable() {
            Assert.Fail("Not a parseable field");
            return this;
        }

        #endregion
    }
}
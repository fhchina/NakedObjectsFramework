﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Linq;
using System.Web.Routing;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Web.Mvc.Html {
    internal abstract class ObjectContext {
        protected ObjectContext(ObjectContext otherContext) {
            Target = otherContext.Target;
        }

        protected ObjectContext(INakedObject target) {
            Target = target;
        }

        public INakedObject Target { get; set; }
    }


    internal abstract class FeatureContext : ObjectContext {
        protected FeatureContext(ObjectContext otherContext) : base(otherContext) {}
        protected FeatureContext(INakedObject target) : base(target) {}
        public abstract IFeatureSpec Feature { get; }
    }

    internal class PropertyContext : FeatureContext {
        public PropertyContext(PropertyContext otherContext) : base(otherContext) {
            ParentContext = otherContext.ParentContext;
        }

        public PropertyContext(INakedObject target, IAssociationSpec property, bool isEdit, PropertyContext parentContext = null)
            : base(target) {
            Property = property;
            IsPropertyEdit = isEdit;
            IsEdit = isEdit;
            ParentContext = parentContext;
        }

        public INakedObject OriginalTarget { get { return ParentContext == null ? Target : ParentContext.OriginalTarget; } }

        public PropertyContext ParentContext { get; set; }

        public IAssociationSpec Property { get; set; }

        public override IFeatureSpec Feature {
            get { return Property; }
        }

        public INakedObject GetValue(INakedObjectsFramework framework) {
            return Property.GetNakedObject(Target);
        }

        public bool IsEdit { get; private set; }

        public bool IsPropertyEdit { get; set; }

        //TODO:  Make a property for consistency?
        public bool IsFindMenuEnabled() {
            if (Property is IOneToOneAssociationSpec) {
                return (Property as IOneToOneAssociationSpec).IsFindMenuEnabled;
            }
            return false;
        }

        public ObjectContext OuterContext { get { return this; } }

        public string GetFieldInputId() {
            return ParentContext == null ? IdHelper.GetFieldInputId(Target, Property) : IdHelper.GetInlineFieldInputId(ParentContext.Property , Target, Property);
        }

        public string GetAutoCompleteFieldId() {
            // append autocomplete suffix if reference field only to differentiate for hidden input field with actual value 
            return IdHelper.GetFieldAutoCompleteId(GetFieldInputId(), Target, Property);
        }

        public string GetFieldId() {
            var thisFieldId = IdHelper.GetFieldId(Target, Property);
            return ParentContext == null ? thisFieldId : IdHelper.MakeId(ParentContext.GetFieldId(), thisFieldId);
        }

        private string GetPresentationHint() {
            var facet = Property.GetFacet<IPresentationHintFacet>();
            return facet == null ? "" : " " + facet.Value;
        }

        public string GetFieldClass() {
            return IdHelper.FieldName + GetPresentationHint();
        }

        public string GetConcurrencyFieldInputId() {
            return ParentContext == null ? IdHelper.GetConcurrencyFieldInputId(Target, Property) : IdHelper.GetInlineConcurrencyFieldInputId(ParentContext.Property, Target, Property);
        }
    }


    internal class ActionContext : FeatureContext {
        public ActionContext(ActionContext otherContext)
            : base(otherContext) {
            EmbeddedInObject = otherContext.EmbeddedInObject;
            Action = otherContext.Action;
        }

        public ActionContext(INakedObject target, IActionSpec action)
            : base(target) {
            EmbeddedInObject = false;
            Action = action;
        }

        public ActionContext(bool embeddedInObject, INakedObject target, IActionSpec action)
            : base(target) {
            EmbeddedInObject = embeddedInObject;
            Action = action;
        }

        private Func<IActionParameterSpec, bool> filter;

        private ParameterContext[] parameterContexts; 

        public Func<IActionParameterSpec, bool> Filter {
            get {
                if (filter == null) {
                    return x => true;
                }
                return filter;
            }
            set { filter = value; }
        }


        public bool EmbeddedInObject { get; set; }

        public IActionSpec Action { get; set; }

        public RouteValueDictionary ParameterValues { get; set; }

        public ParameterContext[] GetParameterContexts(INakedObjectsFramework framework) {
            if (parameterContexts == null) {
                parameterContexts = Action.Parameters.Where(Filter).Select(p => new ParameterContext(EmbeddedInObject, Target, Action, p, true)).ToArray();

                if (ParameterValues != null) {
                    foreach (ParameterContext pc in parameterContexts) {
                        object value;
                        if (ParameterValues.TryGetValue(pc.Parameter.Id, out value)) {
                            pc.IsHidden = true;
                            pc.CustomValue = framework.GetNakedObject(value);
                        }
                    }
                }
            }

            return parameterContexts;
        }

        public override IFeatureSpec Feature {
            get { return Action; }
        }

        public string GetConcurrencyActionInputId(IAssociationSpec nakedObjectAssociation) {
            return IdHelper.GetConcurrencyActionInputId(Target, Action, nakedObjectAssociation);
        }

        public string GetActionId() {
            return IdHelper.GetActionId(Target, Action);
        }

        private string GetPresentationHint() {
            var facet = Action == null ? null : Action.GetFacet<IPresentationHintFacet>();
            return facet == null ? "" : " " + facet.Value;
        }

        private  bool IsFileActionNoParms(INakedObjectsFramework framework) {
            return Action != null && Action.ReturnSpec.IsFile(framework) && !Action.Parameters.Any();
        }

        public string GetActionClass(INakedObjectsFramework framework) {
            return (IsFileActionNoParms(framework) ? IdHelper.ActionNameFile : IdHelper.ActionName) + GetPresentationHint();
        }

        public string GetSubMenuId() {
            return IdHelper.GetSubMenuId(Target, Action);
        }

        public string GetActionDialogId() {
            return IdHelper.GetActionDialogId(Target, Action);
        }

        public string GetFindMenuId(string propertyName) {
            return IdHelper.GetFindMenuId(Target, Action, propertyName);
        }
    }


    internal class ParameterContext : ActionContext {
        public ParameterContext(ParameterContext otherContext) : base(otherContext) {
            Parameter = otherContext.Parameter;
        }

        public ParameterContext(bool embeddedInObject, INakedObject target, IActionSpec action, IActionParameterSpec parameter, bool isEdit)
            : base(embeddedInObject, target, action) {
            Parameter = parameter;
            IsParameterEdit = isEdit;
        }

        public bool IsHidden { get; set; }

        //TODO:  Make a property for consistency?
        public bool IsFindMenuEnabled() {
            if (Parameter is IOneToOneActionParameterSpec) {
                return (Parameter as IOneToOneActionParameterSpec).IsFindMenuEnabled;
            }
            return false;
        }

        public IActionParameterSpec Parameter { get; set; }

        public INakedObject CustomValue { get; set; }

        public override IFeatureSpec Feature {
            get { return Parameter; }
        }

        public bool IsParameterEdit { get; set; }

        public string GetParameterInputId() {
            return IdHelper.GetParameterInputId(Action, Parameter);
        }

        public string GetParameterAutoCompleteId() {
            return IdHelper.GetParameterAutoCompleteId(Action, Parameter);
        }

        public string GetParameterId() {
            return IdHelper.GetParameterId(Action, Parameter);
        }

        private string GetPresentationHint() {
            var facet = Parameter == null ? null : Parameter.GetFacet<IPresentationHintFacet>();
            return facet == null ? "" : " " + facet.Value;
        }

        public string GetParameterClass() {
            return IdHelper.ParamName + GetPresentationHint();
        }
    }
}
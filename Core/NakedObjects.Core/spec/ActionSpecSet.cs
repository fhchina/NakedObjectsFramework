// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Collections.Generic;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Interactions;
using NakedObjects.Architecture.Persist;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.Util;
using NakedObjects.Core.Util;

namespace NakedObjects.Reflector.Spec {
    public class ActionSpecSet : IActionSpec {
        private readonly IActionSpec[] actionsSpec;
        private readonly IServicesManager servicesManager;
        private readonly string id;
        private readonly string name;
        private readonly string shortId; 

        public ActionSpecSet(string id, IActionSpec[] actionsSpec, IServicesManager servicesManager1)
            : this(id, null, actionsSpec, servicesManager1) { }

        public ActionSpecSet(string id, string name, IActionSpec[] actionsSpec, IServicesManager servicesManager) {
            this.name = name;
            this.id = id;
            this.actionsSpec = actionsSpec;
            this.servicesManager = servicesManager;
            this.shortId = TypeNameUtils.GetShortName(id);
        }

        public virtual bool OnInstance {
            get { return false; }
        }

        #region INakedObjectAction Members

        public virtual string DebugData {
            get { return ""; }
        }

        public virtual IActionSpec[] Actions {
            get { return actionsSpec; }
        }

        public virtual string Description {
            get { return ""; }
        }

        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual Type[] FacetTypes {
            get { return new Type[0]; }
        }

        public virtual IIdentifier Identifier {
            get { return null; }
        }

        public virtual string Help {
            get { return ""; }
        }

        public virtual string Id {
            get { return id; }
        }

        public virtual string GetName() {
            if (name == null) {
                var service = servicesManager.GetService(shortId);
                return service.TitleString();
            }
            return name;
        }

        public virtual IObjectSpec OnType {
            get { return null; }
        }

        public virtual int ParameterCount {
            get { return 0; }
        }

        public virtual IObjectSpec ReturnType {
            get { return null; }
        }

        public virtual Where Target {
            get { return Where.Default; }
        }

        public bool IsContributedTo(IObjectSpec spec) {
            return false;
        }

        public bool IsFinderMethod {
            get { return false; }
        }

        public virtual ActionType ActionType {
            get { return ActionType.Set; }
        }

        public virtual bool IsContributedMethod {
            get { return false; }
        }

        public virtual bool PromptForParameters(INakedObject nakedObject) {
            return false;
        }

        /// <summary>
        ///     Always returns <c>null</c>
        /// </summary>
        public virtual IObjectSpec Spec {
            get { return null; }
        }

        public virtual INakedObject Execute(INakedObject target, INakedObject[] parameterSet) {
            throw new UnexpectedCallException();
        }

        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual IFacet GetFacet(Type type) {
            return null;
        }

        public virtual T GetFacet<T>() where T : IFacet {
            return default(T);
        }

        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual IEnumerable<IFacet> GetFacets() {
            return null;
        }

        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual void AddFacet(IFacet facet) {}

        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual void AddFacet(IMultiTypedFacet facet) {}


        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual void RemoveFacet(IFacet facet) {}


        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual bool ContainsFacet(Type facetType) {
            return false;
        }

        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual bool ContainsFacet<T>() where T : IFacet {
            return false;
        }


        /// <summary>
        ///     Does nothing
        /// </summary>
        public virtual void RemoveFacet(Type facetType) {}

        public virtual IActionParameterSpec[] Parameters {
            get { return new IActionParameterSpec[0]; }
        }

        public INakedObject[] RealParameters(INakedObject target, INakedObject[] parameterSet) {
            return new INakedObject[] {};
        }

        public virtual bool HasReturn() {
            return false;
        }

        public virtual IConsent IsParameterSetValid(INakedObject nakedObject, INakedObject[] parameterSet) {
            throw new UnexpectedCallException();
        }

        public virtual IConsent IsUsable(INakedObject target) {
            return Allow.Default;
        }

        public bool IsNullable {
            get { return false; }
        }

        public virtual bool IsVisible(INakedObject target) {
            return true;
        }

        public virtual INakedObject RealTarget(INakedObject target) {
            return null;
        }

        #endregion

        public virtual INakedObject[] GetDefaultParameterValues(INakedObject target) {
            throw new UnexpectedCallException();
        }

        public virtual INakedObject[][] GetChoices(INakedObject target) {
            throw new UnexpectedCallException();
        }

        public virtual IConsent IsParameterSetValidDeclaratively(INakedObject nakedObject, INakedObject[] parameters) {
            throw new UnexpectedCallException();
        }

        public virtual IConsent IsParameterSetValidImperatively(INakedObject nakedObject, INakedObject[] parameters) {
            throw new UnexpectedCallException();
        }

        public virtual bool IsVisible(InteractionContext ic) {
            return true;
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
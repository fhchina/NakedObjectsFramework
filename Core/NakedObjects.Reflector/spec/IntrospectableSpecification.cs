// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Actcoll.Typeof;
using NakedObjects.Architecture.Facets.Collections.Modify;
using NakedObjects.Architecture.Facets.Naming.Named;
using NakedObjects.Architecture.Facets.Objects.Ident.Icon;
using NakedObjects.Architecture.Facets.Objects.Ident.Plural;
using NakedObjects.Architecture.Facets.Objects.Parseable;
using NakedObjects.Architecture.Facets.Types;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Reflector.DotNet.Facets.Naming.Named;
using NakedObjects.Reflector.DotNet.Facets.Objects.Ident.Plural;
using NakedObjects.Reflector.DotNet.Facets.Ordering;
using NakedObjects.Reflector.DotNet.Reflect;
using NakedObjects.Reflector.Peer;
using NakedObjects.Util;

namespace NakedObjects.Reflector.Spec {
    public class IntrospectableSpecification : FacetHolderImpl, IIntrospectableSpecification {
        private static readonly ILog Log = LogManager.GetLogger(typeof (IntrospectableSpecification));

        private readonly IIdentifier identifier;
        private readonly INakedObjectReflector reflector;
        private DotNetIntrospector introspector;

        public IntrospectableSpecification(Type type, INakedObjectReflector reflector) {
            Subclasses = new IIntrospectableSpecification[] {};
            this.reflector = reflector;
            introspector = new DotNetIntrospector(type, this, reflector);
            identifier = new IdentifierImpl((IMetadata) reflector, type.FullName);
            Interfaces = new IIntrospectableSpecification[] {};
            Subclasses = new IIntrospectableSpecification[] {};
            ValidationMethods = new INakedObjectValidation[] {};
            //ObjectActions = new INakedObjectActionPeer[]{};
            ContributedActions = new Dictionary<string, IOrderSet<INakedObjectActionPeer>>();
            RelatedActions = new Dictionary<string, IOrderSet<INakedObjectActionPeer>>();
        }

      
        #region IIntrospectableSpecification Members

        public IIntrospectableSpecification Superclass { get; private set; }
        

        public override IIdentifier Identifier {
            get { return identifier; }
        }

        public Type Type { get; set; }

        public string FullName { get; set; }

        public string ShortName { get; set; }

        public IOrderSet<INakedObjectActionPeer> ObjectActions { get; private set; }

        public IDictionary<string, IOrderSet<INakedObjectActionPeer>> ContributedActions { get; private set; }

        public IDictionary<string, IOrderSet<INakedObjectActionPeer>> RelatedActions { get; private set; }

        public INakedObjectAssociationPeer[] Fields { get; set; }

        public IIntrospectableSpecification[] Interfaces { get; set; }

        public IIntrospectableSpecification[] Subclasses { get; set; }

        public bool Service { get; set; }

        public INakedObjectValidation[] ValidationMethods { get; set; }

        public override IFacet GetFacet(Type facetType) {
            IFacet facet = base.GetFacet(facetType);
            if (FacetUtils.IsNotANoopFacet(facet)) {
                return facet;
            }

            IFacet noopFacet = facet;

            if (Superclass != null) {
                IFacet superClassFacet = Superclass.GetFacet(facetType);
                if (FacetUtils.IsNotANoopFacet(superClassFacet)) {
                    return superClassFacet;
                }
                if (noopFacet == null) {
                    noopFacet = superClassFacet;
                }
            }
            if (Interfaces != null) {
                var interfaceSpecs = Interfaces;
                foreach (var interfaceSpec in interfaceSpecs) {
                    IFacet interfaceFacet = interfaceSpec.GetFacet(facetType);
                    if (FacetUtils.IsNotANoopFacet(interfaceFacet)) {
                        return interfaceFacet;
                    }
                    if (noopFacet == null) {
                        noopFacet = interfaceFacet;
                    }
                }
            }
            return noopFacet;
        }

        public void Introspect(IFacetDecoratorSet decorator) {
            if (introspector == null) {
                throw new ReflectionException("Introspection already taken place, cannot introspect again");
            }

            introspector.IntrospectClass();

            Type = introspector.IntrospectedType;
            FullName = introspector.FullName;
            ShortName = introspector.ShortName;
            var namedFacet = GetFacet<INamedFacet>();
            if (namedFacet == null) {
                namedFacet = new NamedFacetInferred(NameUtils.NaturalName(ShortName), this);
                AddFacet(namedFacet);
            }

            var pluralFacet = GetFacet<IPluralFacet>();
            if (pluralFacet == null) {
                pluralFacet = new PluralFacetInferred(NameUtils.PluralName(namedFacet.Value), this);
                AddFacet(pluralFacet);
            }

            // TODO can we do this in the introspector 
            if (introspector.IsAbstract) {
                AddFacet(new AbstractFacet(this));
            }

            if (introspector.IsInterface) {
                AddFacet(new InterfaceFacet(this));
            }

            if (introspector.IsSealed) {
                AddFacet(new SealedFacet(this));
            }

            if (introspector.IsVoid) {
                AddFacet(new VoidFacet(this));
            }

            string superclassName = introspector.SuperclassName;
            string[] interfaceNames = introspector.InterfacesNames;

            if (superclassName != null && !TypeUtils.IsSystem(superclassName)) {
                Superclass = reflector.LoadSpecification(superclassName);
                if (Superclass != null) {
                    Log.DebugFormat("Superclass {0}", superclassName);
                    Superclass.AddSubclass(this);
                }
            }
            else if (Type != typeof (object)) {
                // always root in object (unless this is object!) 
                Superclass = reflector.LoadSpecification(typeof(object));
                if (Superclass != null) {
                    Log.DebugFormat("Superclass {0}", typeof (object).Name);
                    Superclass.AddSubclass(this);
                }
            }

            var interfaceList = new List<IIntrospectableSpecification>();
            foreach (string interfaceName in interfaceNames) {
                var interfaceSpec = reflector.LoadSpecification(interfaceName);
                interfaceSpec.AddSubclass(this);
                interfaceList.Add(interfaceSpec);
            }

            Interfaces = interfaceList.ToArray();

            introspector.IntrospectPropertiesAndCollections();
            Fields = introspector.Fields;

            ValidationMethods = introspector.IntrospectObjectValidationMethods();

            introspector.IntrospectActions();
            ObjectActions =   introspector.ObjectActions;

            introspector = null;

            DecorateAllFacets(decorator);
        }

        public void PopulateAssociatedActions(Type[] services) {
            if (string.IsNullOrWhiteSpace(FullName)) {
                string id = (identifier != null ? identifier.ClassName : "unknown") ?? "unknown";
                Log.WarnFormat("Specification with id : {0} as has null or empty name", id);
            }

            if (TypeUtils.IsSystem(FullName) && !IsCollection) {
                return;
            }
            if (TypeUtils.IsNakedObjects(FullName)) {
                return;
            }

            PopulateContributedActions(services);
            PopulateRelatedActions(services);
        }

        public void MarkAsService() {
            if (Fields.Any(field => field.Identifier.MemberName != "Id")) {
                string fieldNames = Fields.Where(field => field.Identifier.MemberName != "Id").Aggregate("", (current, field) => current + (current.Length > 0 ? ", " : "") /*+ field.GetName(persistor)*/);
                throw new ModelException(string.Format(Resources.NakedObjects.ServiceObjectWithFieldsError, FullName, fieldNames));
            }
            Service = true;
        }

        public void AddSubclass(IIntrospectableSpecification subclass) {
            var subclassList = new List<IIntrospectableSpecification>(Subclasses) {subclass};
            Subclasses = subclassList.ToArray();
        }

        #endregion


        private void DecorateAllFacets(IFacetDecoratorSet decorator) {
            decorator.DecorateAllHoldersFacets(this);
            foreach (INakedObjectAssociationPeer field in Fields) {
                decorator.DecorateAllHoldersFacets(field);
            }
            foreach (INakedObjectActionPeer action in ObjectActions.Flattened) {
                DecorateAction(decorator, action);
            }
        }

        private static void DecorateAction(IFacetDecoratorSet decorator, INakedObjectActionPeer action) {
            decorator.DecorateAllHoldersFacets(action);
            foreach (INakedObjectActionParamPeer parm in action.Parameters) {
                decorator.DecorateAllHoldersFacets(parm);
            }
        }

        private void PopulateContributedActions(Type[] services) {
            if (!Service) {
                foreach (Type serviceType in services) {
                    if (serviceType != Type) {
                        var serviceSpecification = reflector.LoadSpecification(serviceType);

                        INakedObjectActionPeer[] matchingServiceActions = serviceSpecification.ObjectActions.Flattened.Where(serviceAction => serviceAction.IsContributedTo(this)).ToArray();

                        if (matchingServiceActions.Any()) {
                            var os = SimpleOrderSet<INakedObjectActionPeer>.CreateOrderSet("", matchingServiceActions);

                            ContributedActions.Add(serviceSpecification.Identifier.ClassName, os);
                        }
                    }
                }
            }
        }

        private void PopulateRelatedActions(Type[] services) {
            foreach (Type serviceType in services) {
                var serviceSpecification = reflector.LoadSpecification(serviceType);
                var matchingActions = new List<INakedObjectActionPeer>();

                foreach (var serviceAction in serviceSpecification.ObjectActions.Flattened.Where(a => a.IsFinderMethod)) {
                    var returnType = serviceAction.ReturnType;
                    if (returnType != null && returnType.IsCollection) {
                        var elementType = returnType.GetFacet<ITypeOfFacet>().ValueSpec;
                        if (elementType.IsOfType(this)) {
                            matchingActions.Add(serviceAction);
                        }
                    }
                    else if (returnType != null && returnType.IsOfType(this)) {
                        matchingActions.Add(serviceAction);
                    }
                }

                if (matchingActions.Any()) {
                    var os = SimpleOrderSet<INakedObjectActionPeer>.CreateOrderSet("", matchingActions.ToArray());

                    RelatedActions.Add(serviceSpecification.Identifier.ClassName, os);
                }
            }
        }

        public virtual bool IsCollection {
            get { return ContainsFacet(typeof(ICollectionFacet)); }
        }

        public virtual bool IsParseable {
            get { return ContainsFacet(typeof(IParseableFacet)); }
        }

        public virtual bool IsObject {
            get { return !IsCollection; }
        }

        public bool IsOfType(IIntrospectableSpecification specification) {

            if (specification == this) {
                return true;
            }
            if (Interfaces.Any(interfaceSpec => interfaceSpec.IsOfType(specification))) {
                return true;
            }

            // match covariant generic types 
            if (Type.IsGenericType && IsCollection) {
                Type otherType = specification.Type;
                if (otherType.IsGenericType && Type.GetGenericArguments().Count() == 1 && otherType.GetGenericArguments().Count() == 1) {
                    if (Type.GetGenericTypeDefinition() == (typeof(IQueryable<>)) && Type.GetGenericTypeDefinition() == otherType.GetGenericTypeDefinition()) {
                        Type genericArgument = Type.GetGenericArguments().Single();
                        Type otherGenericArgument = otherType.GetGenericArguments().Single();
                        Type otherGenericParameter = otherType.GetGenericTypeDefinition().GetGenericArguments().Single();
                        if ((otherGenericParameter.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0) {
                            if (otherGenericArgument.IsAssignableFrom(genericArgument)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return Superclass != null && Superclass.IsOfType(specification);
        }

        public string GetIconName(INakedObject forObject) {
            var iconFacet = GetFacet<IIconFacet>();
            string iconName = null;
            if (iconFacet != null) {
                iconName = forObject == null ? iconFacet.GetIconName() : iconFacet.GetIconName(forObject);
            }
            else if (IsCollection) {
                iconName = GetFacet<ITypeOfFacet>().ValueSpec.GetIconName(null);
            }

            return string.IsNullOrEmpty(iconName) ? "Default" : iconName;
        }
    }
}
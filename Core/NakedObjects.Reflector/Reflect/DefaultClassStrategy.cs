// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Configuration;
using NakedObjects.Core.Util;
using NakedObjects.Util;

namespace NakedObjects.Reflect {
    /// <summary>
    ///     Standard way of determining which fields are to be exposed in a Naked Objects system.
    /// </summary>
    [Serializable]
    public class DefaultClassStrategy : IClassStrategy {
        private readonly IReflectorConfiguration config;
        private static readonly ILog Log = LogManager.GetLogger(typeof (DefaultClassStrategy));

        public DefaultClassStrategy(IReflectorConfiguration config) {
            this.config = config;
        }

        private bool IsTypeIgnored(Type type) {
            return type.GetCustomAttribute<NakedObjectsIgnoreAttribute>() != null;
        }

        // only intended for use during initial reflection
        private IImmutableDictionary<Type, bool> namespaceScratchPad = ImmutableDictionary<Type, bool>.Empty;

        private bool IsNamespaceMatch(Type type) {

            if (!namespaceScratchPad.ContainsKey(type)) {
                var ns = type.Namespace ?? "";
                var match = config.SupportedNamespaces.Any(ns.StartsWith);
                namespaceScratchPad = namespaceScratchPad.Add(type, match);
            }

            return namespaceScratchPad[type];
        }

        private bool IsTypeWhiteListed(Type type) {
            return IsTypeSupportedSystemType(type) || IsNamespaceMatch(type) || IsTypeExplicitlyRequested(type);
        }

        private bool IsTypeExplicitlyRequested(Type type) {
            IEnumerable<Type> services = config.MenuServices.Union(config.ContributedActions).Union(config.SystemServices);
            return config.TypesToIntrospect.Any(t => t == type) || services.Any(t => t == type) || type.IsGenericType && config.TypesToIntrospect.Any(t => t == type.GetGenericTypeDefinition());
        }

        private Type ToMatch(Type type) {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        private bool IsTypeSupportedSystemType(Type type) {
            return config.SupportedSystemTypes.Any(t => t == ToMatch(type));
        }

        public virtual bool IsTypeToBeIntrospected(Type type) {
            Type returnType = FilterNullableAndProxies(type);
            return !IsTypeIgnored(returnType) && !IsTypeUnsupportedByReflector(returnType) && IsTypeWhiteListed(returnType);
        }

        public virtual Type GetType(Type type) {
            Type returnType = FilterNullableAndProxies(type);
            return IsTypeToBeIntrospected(returnType) ? returnType : null;
        }

        public  Type FilterNullableAndProxies(Type type) {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>)) {
                // use type inside nullable wrapper
                Log.DebugFormat("Using wrapped type instead of {0}", type);
                return type.GetGenericArguments()[0];
            }
            if (TypeUtils.IsProxy(type)) {
                Log.DebugFormat("Using proxied type instead of {0}", type);
                return type.BaseType;
            }
            return type;
        }

        public bool IsSystemClass(Type introspectedType) {
            return introspectedType.FullName.StartsWith("System.");
        }

        private bool IsTypeUnsupportedByReflector(Type type) {
            return type.IsPointer ||
                   type.IsByRef ||
                   CollectionUtils.IsDictionary(type) ||
                   type.IsGenericParameter ||
                   type.ContainsGenericParameters;
        }

        public string GetKeyForType(Type type) {
            if (IsGenericCollection(type)) {
                return type.Namespace + "." + type.Name;
            }

            if (type.IsArray && !(type.GetElementType().IsValueType || type.GetElementType() == typeof (string))) {
                return "System.Array";
            }

            return type.GetProxiedTypeFullName();
        }

        // because Sets don't implement IEnumerable<>
        private static bool IsGenericCollection(Type type) {
            return CollectionUtils.IsGenericType(type, typeof (IEnumerable<>)) ||
                   CollectionUtils.IsGenericType(type, typeof (ISet<>));
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}
﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;

namespace NakedObjects {
    /// <summary>
    ///     Never allow this action to be contributed to an object menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    [Obsolete("This attribute is no longer recognised as of NOF 7. Contributed Actions must be explicitly annotated with the ContributedAction attribute.")]
    public class NotContributedActionAttribute : Attribute {
        public NotContributedActionAttribute(params Type[] notContributedToTypes) {
            NotContributedToTypes = notContributedToTypes ?? new Type[] {};
        }

        public Type[] NotContributedToTypes { get; private set; }
    }
}
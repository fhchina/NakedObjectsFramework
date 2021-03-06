// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Interactions;

namespace NakedObjects.Architecture {
    public abstract class InteractionException : Exception {
        private readonly IIdentifier identifier;
        private readonly InteractionType interactionType;
        private readonly INakedObject target;

        protected InteractionException(InteractionContext ic)
            : this(ic, null) {}

        protected InteractionException(InteractionContext ic, string message)
            : base(message) {
            interactionType = ic.InteractionType;
            identifier = ic.Id;
            target = ic.Target;
        }

        /// <summary>
        ///     The type of interaction that caused this exception to be raised
        /// </summary>
        public virtual InteractionType InteractionType {
            get { return interactionType; }
        }

        /// <summary>
        ///     The identifier of the feature (object or member) being interacted with
        /// </summary>
        public virtual IIdentifier Identifier {
            get { return identifier; }
        }

        /// <summary>
        ///     The object being interacted with
        /// </summary>
        public virtual INakedObject Target {
            get { return target; }
        }
    }
}
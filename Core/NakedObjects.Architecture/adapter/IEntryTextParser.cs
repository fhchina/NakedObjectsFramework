// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

namespace NakedObjects.Architecture.Adapter {
    public interface IEntryTextParser {
        /// <summary>
        ///     Parses a text entry made by a user and sets the domain object's value
        /// </summary>
        /// <exception cref="InvalidEntryException" />
        INakedObject ParseTextEntry(INakedObject original, string text);

        /// <summary>
        ///     Returns the title to display this object with, which is usually got from the
        ///     wrapped <see cref="INakedObject.Object" /> domain object
        /// </summary>
        string TitleString(INakedObject nakedObject);
    }

    // Copyright (c) Naked Objects Group Ltd.
}
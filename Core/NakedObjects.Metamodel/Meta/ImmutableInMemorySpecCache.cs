// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Menu;
using NakedObjects.Architecture.SpecImmutable;

namespace NakedObjects.Meta {
    [Serializable]
    internal class SerializedData {
        public IList<string> Keys { get; set; }
        public IList<IObjectSpecImmutable> Values { get; set; }
    }

    public class ImmutableInMemorySpecCache : ISpecificationCache {
        private ImmutableList<IMenuImmutable> mainMenus = ImmutableList<IMenuImmutable>.Empty;

        private ImmutableDictionary<string, IObjectSpecImmutable> specs =
            ImmutableDictionary<string, IObjectSpecImmutable>.Empty;

        // constructor to use when reflecting
        public ImmutableInMemorySpecCache() {}

        // constructor to use when loading metadata from file
        public ImmutableInMemorySpecCache(string file) {
            using (FileStream fs = File.Open(file, FileMode.Open)) {
                IFormatter formatter = new BinaryFormatter();
                var data = (SerializedData) formatter.Deserialize(fs);
                specs = data.Keys.Zip(data.Values, (k, v) => new {k, v}).ToDictionary(a => a.k, a => a.v).ToImmutableDictionary();
            }
        }

        #region ISpecificationCache Members

        public void Serialize(string file) {
            using (FileStream fs = File.Open(file, FileMode.OpenOrCreate)) {
                IFormatter formatter = new BinaryFormatter();
                var data = new SerializedData {Keys = specs.Keys.ToList(), Values = specs.Values.ToList()};
                formatter.Serialize(fs, data);
            }
        }

        public virtual IObjectSpecImmutable GetSpecification(string key) {
            return specs.ContainsKey(key) ? specs[key] : null;
        }

        public virtual void Cache(string key, IObjectSpecImmutable spec) {
            specs = specs.Add(key, spec);
        }

        public virtual void Clear() {
            specs = specs.Clear();
        }

        public virtual IObjectSpecImmutable[] AllSpecifications() {
            return specs.Values.ToArray();
        }

        public void Cache(IMenuImmutable mainMenu) {
            mainMenus = mainMenus.Add(mainMenu);
        }

        public IMenuImmutable[] MainMenus() {
            return mainMenus.ToArray();
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}
﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Configuration;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Architecture.Component {
    /// <summary>
    /// Provides access to the domain services that have been registered as part of the application. 
    /// </summary>
    public interface IServicesManager {
      
        INakedObject GetService(string id);

        INakedObject GetService(IServiceSpec spec);

        ServiceType GetServiceType(IServiceSpec spec);

        INakedObject[] GetServices();

        INakedObject[] GetServices(ServiceType serviceType);

        INakedObject[] GetServicesWithVisibleActions(ServiceType serviceType, ILifecycleManager lifecycleManager);
    }
}
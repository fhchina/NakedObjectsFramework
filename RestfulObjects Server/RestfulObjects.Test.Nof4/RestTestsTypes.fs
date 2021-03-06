﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
module NakedObjects.Rest.Test.Nof4Types

open NUnit.Framework
open RestfulObjects.Mvc
open RestfulObjects.Mvc.Media
open System
open RestfulObjects.Snapshot.Utility
open System.Web.Http
open Microsoft.Practices.Unity
open RestfulObjects.Test.Data
open NakedObjects.Surface.Nof4.Implementation
open NakedObjects.Surface.Nof4.Utility
open NakedObjects.Surface
open MvcTestApp.Controllers
open NakedObjects.Rest.Test.RestTestsHelpers
open NakedObjects.Architecture.Configuration
open NakedObjects.Core.Configuration
open NakedObjects.Persistor.Entity.Configuration
open NakedObjects.Persistor.Entity
open System.Data.Entity.Core.Objects

[<TestFixture>]
type Nof4TestsTypes() = 
    class
        inherit NakedObjects.Xat.AcceptanceTestCase()
        
        override x.RegisterTypes(container) = 
            base.RegisterTypes(container)
            let config = new EntityObjectStoreConfiguration()
            let f = (fun () -> new CodeFirstContext("RestTest") :> Data.Entity.DbContext)
            config.UsingCodeFirstContext(Func<Data.Entity.DbContext>(f)) |> ignore
            container.RegisterInstance(typeof<IEntityObjectStoreConfiguration>, null, config, (new ContainerControlledLifetimeManager())) |> ignore
            container.RegisterType(typeof<IOidStrategy>, typeof<ExternalOid>, null, (new PerResolveLifetimeManager())) |> ignore
            container.RegisterType(typeof<INakedObjectsSurface>, typeof<NakedObjectsSurface>, null, (new PerResolveLifetimeManager())) |> ignore
            let types = 
                [| typeof<Immutable>
                   typeof<WithActionViewModel>
                   typeof<WithCollectionViewModel>
                   typeof<WithValueViewModel>
                   typeof<WithNestedViewModel>
                   typeof<RedirectedObject>
                   typeof<WithScalars>
                   typeof<VerySimple>
                   typeof<VerySimpleEager>
                   typeof<WithAction>
                   typeof<WithActionObject>
                   typeof<WithAttachments>
                   typeof<WithCollection>
                   typeof<WithDateTimeKey>
                   typeof<WithError>
                   typeof<WithGetError>
                   typeof<WithNestedViewModel>
                   typeof<WithReference>
                   typeof<WithReferenceViewModel>
                   typeof<MostSimple>
                   typeof<MostSimpleViewModel>
                   typeof<WithValue>
                   typeof<TestEnum>                   
                   typeof<MostSimple[]>                 
                   typeof<SetWrapper<MostSimple>> |]
            let ms = [| typeof<RestDataRepository>;  typeof<WithActionService> |]
            let ca = [| typeof<ContributorService> |]
            let reflectorConfig = new ReflectorConfiguration(types, ms, ca, [||], [|"RestfulObjects.Test.Data"|])
            container.RegisterInstance(typeof<IReflectorConfiguration>, null, reflectorConfig, (new ContainerControlledLifetimeManager())) |> ignore
            ()
        
        [<TestFixtureSetUp>]
        member x.FixtureSetup() = 
            CodeFirstSetup()
            NakedObjects.Xat.AcceptanceTestCase.InitializeNakedObjectsFramework(x)
        
        [<SetUp>]
        member x.Setup() = 
            x.StartTest()
            UriMtHelper.GetApplicationPath <- Func<string>(fun () -> "")
            RestfulObjectsControllerBase.IsReadOnly <- false
            GlobalConfiguration.Configuration.Formatters.[0] <- new JsonNetFormatter(null)
        
        [<TearDown>]
        member x.TearDown() = 
            RestfulObjectsControllerBase.DomainModel <- RestControlFlags.DomainModelType.Selectable
            RestfulObjectsControllerBase.ConcurrencyChecking <- false
            RestfulObjectsControllerBase.CacheSettings <- (0, 3600, 86400)
        
        [<TestFixtureTearDown>]
        member x.FixtureTearDown() = NakedObjects.Xat.AcceptanceTestCase.CleanupNakedObjectsFramework(x)
        
        override x.MenuServices  = 
           [| box (new RestDataRepository())
              box (new WithActionService()) |]
        
        override x.ContributedActions  = [| box (new ContributorService()) |]
        member x.api = x.GetConfiguredContainer().Resolve<RestfulObjectsController>()
        
        [<Test>]
        [<Ignore>] // temp ignore
        member x.GetDomainTypes() = DomainTypes20.GetDomainTypes x.api
        
        [<Test>]
        [<Ignore>] // temp ignore
        member x.GetDomainTypesWithMediaType() = DomainTypes20.GetDomainTypesWithMediaType x.api
        
        [<Test>]
        member x.NotAcceptableGetDomainTypes() = DomainTypes20.NotAcceptableGetDomainTypes x.api
    end

// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NakedObjects;

namespace AdventureWorksModel {
    // ReSharper disable once PartialTypeWithSinglePart
    // ReSharper disable InconsistentNaming

    public partial class Department {
        #region Primitive Properties

        #region DepartmentID (Int16)

        [MemberOrder(100)]
        public virtual short DepartmentID { get; set; }

        #endregion

        #region Name (String)

        [MemberOrder(110), StringLength(50)]
        public virtual string Name { get; set; }

        #endregion

        #region GroupName (String)

        [MemberOrder(120), StringLength(50)]
        public virtual string GroupName { get; set; }

        #endregion

        #region ModifiedDate (DateTime)

        [MemberOrder(130), Mask("d")]
        public virtual DateTime ModifiedDate { get; set; }

        #endregion

        #endregion

        #region Navigation Properties

        #region EmployeeDepartmentHistories (Collection of EmployeeDepartmentHistory)

        private ICollection<EmployeeDepartmentHistory> _employeeDepartmentHistories = new List<EmployeeDepartmentHistory>();

        [MemberOrder(140), Disabled]
        public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistories {
            get { return _employeeDepartmentHistories; }
            set { _employeeDepartmentHistories = value; }
        }

        #endregion

        #endregion
    }
}
// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using Expenses.ExpenseClaims;
using Expenses.ExpenseEmployees;

namespace Expenses.Fixtures {
    public class SvenClaim2Submitted : AbstractClaimFixture {
        public static Employee DICK;
        public static Employee SVEN;
        public static Claim SVEN_CLAIM_2;

        public  void Install() {
            SVEN = EmployeeFixture.SVEN;
            DICK = EmployeeFixture.DICK;

            SVEN_CLAIM_2 = CreateNewClaim(SVEN, null, "15th Mar - Sales call, London", ProjectCodeFixture.CODE1, new DateTime(2007, 4, 3));
            var mar14th = new DateTime(2007, 3, 14);
            AddTaxi(SVEN_CLAIM_2, mar14th, null, 18M, "Euston", "City", true);
            AddMeal(SVEN_CLAIM_2, mar14th, "Lunch with client", 50M);
            SVEN_CLAIM_2.Submit(DICK, false);
        }
    }
}
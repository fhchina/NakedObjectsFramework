// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System.ComponentModel;
using System.Linq;
using NakedObjects;
using NakedObjects.Services;

namespace AdventureWorksModel {
    public enum Ordering {
        Ascending,
        Descending
    };


    [DisplayName("Orders")]
    public class OrderRepository : AbstractFactoryAndRepository {
        #region FindOrder

        [FinderAction]
        [MemberOrder(10)]
        public SalesOrderHeader FindOrder([DefaultValue("SO")] string orderNumber) {
            IQueryable<SalesOrderHeader> query = from obj in Instances<SalesOrderHeader>()
                where obj.SalesOrderNumber == orderNumber
                select obj;

            return SingleObjectWarnIfNoMatch(query);
        }

        #region HighestValueOrders

        [FinderAction]
        [MemberOrder(90)]
        [TableView(true, "TotalDue", "Customer", "OrderDate", "SalesPerson", "Comment")]
        public IQueryable<SalesOrderHeader> HighestValueOrders() {
            return OrdersByValue(Ordering.Descending);
        }

        [FinderAction]
        [MemberOrder(91)]
        [TableView(true, "TotalDue", "Customer", "OrderDate", "SalesPerson")]
        public IQueryable<SalesOrderHeader> OrdersByValue(Ordering ordering) {
            return ordering == Ordering.Descending ? Instances<SalesOrderHeader>().OrderByDescending(obj => obj.TotalDue) :
                Instances<SalesOrderHeader>().OrderBy(obj => obj.TotalDue);
        }

        #endregion

        #endregion

        [FinderAction]
        [MemberOrder(99)]
        public SalesOrderHeader RandomOrder() {
            return Random<SalesOrderHeader>();
        }

        #region OrdersInProcess

        [FinderAction]
        [MemberOrder(5)]
        [TableView(true, "OrderDate", "DueDate")]
        public IQueryable<SalesOrderHeader> OrdersInProcess() {
            return from obj in Instances<SalesOrderHeader>()
                where obj.Status == 1
                select obj;
        }

        #endregion
    }
}
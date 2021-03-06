// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Transaction;
using NakedObjects.Core.Transaction;
using NakedObjects.Core.Util;

namespace NakedObjects.Core.Component {
    public class TransactionManager : ITransactionManager {
        private static readonly ILog Log = LogManager.GetLogger(typeof (TransactionManager));
        private readonly IObjectStore objectStore;
        private ITransaction transaction;
        private int transactionLevel;
        private bool userAborted;

        public TransactionManager(IObjectStore objectStore) {
            Assert.AssertNotNull(objectStore);
            this.objectStore = objectStore;
        }

        private ITransaction Transaction {
            get {
                if (transaction == null) {
                    Log.Info("Creating new transaction");
                    return new NestedTransaction(objectStore);
                }
                return transaction;
            }
        }

        #region ITransactionManager Members

        public virtual void StartTransaction() {
            if (transaction == null) {
                transaction = new NestedTransaction(objectStore);
                transactionLevel = 0;
                userAborted = false;
                objectStore.StartTransaction();
            }
            transactionLevel++;
        }

        public virtual bool FlushTransaction() {
            if (transaction != null) {
                return transaction.Flush();
            }
            return false;
        }

        public virtual void AbortTransaction() {
            if (transaction != null) {
                transaction.Abort();
                transaction = null;
                transactionLevel = 0;
                objectStore.AbortTransaction();
            }
        }

        public virtual void UserAbortTransaction() {
            AbortTransaction();
            userAborted = true;
        }

        public virtual void EndTransaction() {
            transactionLevel--;
            if (transactionLevel == 0) {
                Transaction.Commit();
                transaction = null;
            }
            else if (transactionLevel < 0) {
                transactionLevel = 0;
                if (!userAborted) {
                    Log.ErrorFormat("End transaction with level {0}", transactionLevel);
                    throw new TransactionException("No transaction running to end");
                }
            }
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}
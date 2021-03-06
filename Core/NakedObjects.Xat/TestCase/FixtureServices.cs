// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Threading;
using NakedObjects.Core.Adapter;

namespace NakedObjects.Core.Fixture {
    public class FixtureServices {
        public FixtureServices() {
            Clock = FixtureClock.Initialize();
        }

        protected FixtureClock Clock { get; set; }

        public void EarlierDate(int years, int months, int days) {
            Clock.AddDate(-years, -months, -days);
        }

        public void EarlierTime(int hours, int minutes) {
            Clock.AddTime(-hours, -minutes);
        }

        public void LaterDate(int years, int months, int days) {
            Clock.AddDate(years, months, days);
        }

        public void LaterTime(int hours, int minutes) {
            Clock.AddTime(hours, minutes);
        }

        public void ResetClock() {
            Clock.Reset();
        }

        public void SetDate(int year, int month, int day) {
            Clock.SetDate(year, month, day);
        }

        public void SetTime(int hour, int minute) {
            Clock.SetTime(hour, minute);
        }

        //public void SetUser(string username, string[] roles) {
        //    var staticContext = (StaticContext) NakedObjectsContext.Instance;

        //    IIdentity identity = new GenericIdentity(username);
        //    IPrincipal principal = new GenericPrincipal(identity, roles);

        //    ISession session = new SimpleSession(principal);
        //    staticContext.SetSession(session);
        //}

        #region Nested type: FixtureClock

        protected class FixtureClock {
            private DateTime time;

            /// <summary>
            ///     Access via <see cref="IClock.Ticks" />
            /// </summary>
            protected long Ticks {
                get { return time.Ticks; }
            }

            /// <summary>
            ///     Create a return new FixtureClock
            /// </summary>
            public static FixtureClock Initialize() {
                return new FixtureClock();
            }

            /// <summary>
            ///     Set time - leaving date unchanged
            /// </summary>
            public void SetTime(int hour, int min) {
                time = new DateTime(time.Year,
                    time.Month,
                    time.Day,
                    hour,
                    min,
                    0);
            }

            /// <summary>
            ///     Set date
            /// </summary>
            public void SetDate(int year, int month, int day) {
                time = new DateTime(year, month, day);
            }

            /// <summary>
            ///     Add time to current datetime
            /// </summary>
            public void AddTime(int hours, int minutes) {
                time = time.AddHours(hours);
                time = time.AddMinutes(minutes);
            }

            /// <summary>
            ///     Add years/month/days to current time
            /// </summary>
            public void AddDate(int years, int months, int days) {
                time = time.AddYears(years);
                time = time.AddMonths(months);
                time = time.AddDays(days);
            }

            /// <summary>
            ///     Reset time to now
            /// </summary>
            public void Reset() {
                time = DateTime.Now;
            }

            public override string ToString() {
                return time.ToString(Thread.CurrentThread.CurrentCulture);
            }
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}
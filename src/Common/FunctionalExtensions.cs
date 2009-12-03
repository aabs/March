using System;
using System.Collections.Generic;

namespace Common
{
    public static class FunctionalExtensions
    {
        private static readonly Logger Logger = new Logger(typeof (FunctionalExtensions));

        public static IEnumerable<R> Map<T,R>(this IEnumerable<T> seq, Func<T,R> job)
        {
            if (seq == null)
                yield break;
            foreach (T item in seq)
            {
                yield return job(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> seq, Action<T> job)
        {
            if (seq == null)
                return;
            foreach (T item in seq)
            {
                job(item);
            }
        }

        /// <summary>
        /// Attempts to run the <see cref="job"/> on <see cref="me"/> (retrying at most <see cref="tries"/> times, in the event of failure).
        /// </summary>
        /// <typeparam name="T">The type of the object that contains the data and methods that the job will invoke</typeparam>
        /// <param name="me">The object that contains the state data and methods for the job.</param>
        /// <param name="job">The task to be performed.</param>
        /// <param name="compensatingAction">What to do if an attempt is not successful</param>
        /// <param name="tries">The maximum number of times that <see cref="job"/> should be run before giving up.</param>
        /// <param name="acceptableExceptionTypes">a list of types (as optional params) that are caught and swallowed in favour of the <see cref="compensatingAction"/></param>
        /// <returns>true if the last attempt was successful, false otherwise</returns>
        public static bool Attempt<T>(this T me, Func<T, bool> job, Func<T, bool> compensatingAction, int tries,
                                      params Type[] acceptableExceptionTypes)
            where T : class
        {
            return Attempt(me, job, compensatingAction, tries,
                           (t, e) =>
                               {
                                   var okTypes = new List<Type>(acceptableExceptionTypes);
                                   return okTypes.Contains(t);
                               });
        }

        /// <summary>
        /// Attempts to run the <see cref="job"/> on <see cref="me"/> (retrying at most <see cref="tries"/> times, in the event of failure).
        /// </summary>
        /// <typeparam name="T">The type of the object that contains the data and methods that the job will invoke</typeparam>
        /// <param name="me">The object that contains the state data and methods for the job.</param>
        /// <param name="job">The task to be performed.</param>
        /// <param name="compensatingAction">What to do if an attempt is not successful</param>
        /// <param name="tries">The maximum number of times that <see cref="job"/> should be run before giving up.</param>
        /// <param name="attemptCompensationPred">A function to calculate whether to go ahead with the compensating activity <see cref="compensatingAction"/></param>
        /// <returns>true if the last attempt was successful, false otherwise</returns>
        public static bool Attempt<T>(this T me, Func<T, bool> job, Func<T, bool> compensatingAction, int tries,
                                      Func<Type, Exception, bool> attemptCompensationPred)
            where T : class
        {
            if (me == null)
                throw new ArgumentNullException("me");
            if (job == null)
                throw new ArgumentNullException("job");
            if (tries < 1)
                throw new ArgumentException("retry attempts must be a positive integer");

            bool success = false;
            int tmpTries = tries;

            while (!success && --tmpTries > 0)
            {
                try
                {
                    success = job(me);
                }
                catch (Exception e)
                {
                    if (attemptCompensationPred(e.GetType(), e))
                    {
                        Logger.Warn("Retrying {1}/{2} due to {0}.", e.Message, tries - tmpTries, tries);
                        Logger.Debug("performing compensating activity.");

                        compensatingAction(me);
                    }
                    else
                        throw;
                }
            }

            return success;
        }

        public static void WaitTill<S>(S s, Func<S, bool> pred, Action<S> loop)
        {
            while (!pred(s))
                loop(s);
        }

        public static Func<A, bool> Match<A, T>(Func<A, T> propSelect, T match)
        {
            return a => propSelect(a).Equals(match);
        }
    }
}
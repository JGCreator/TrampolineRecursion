using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDT.Abrams.Common.Patterns.Recursion
{
    public static class LambdaActionRecursion
    {
        public delegate RecursiveAction RecursiveAction();
        public delegate RecursiveAction<T> RecursiveAction<in T>(T arg);
        public delegate RecursiveAction<T1, T2> RecursiveAction<in T1, in T2>(T1 arg1, T2 arg2);
        public delegate RecursiveAction<T1, T2, T3> RecursiveAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

        public delegate Func<RecursiveAction> Trampoline(RecursiveAction recursiveAction);
        public delegate Func<T, RecursiveAction<T>> Trampoline<T>(RecursiveAction<T> recursiveAction);
        public delegate Func<T1, T2, RecursiveAction<T1, T2>> Trampoline<T1, T2>(RecursiveAction<T1, T2> recursiveAction);
        public delegate Func<T1, T2, T3, RecursiveAction<T1, T2, T3>> Trampoline<T1, T2, T3>(RecursiveAction<T1, T2, T3> recursiveAction);

        public static RecursiveAction Break(this RecursiveAction recursiveAction) { return null; }
        public static RecursiveAction<T> Break<T>(this RecursiveAction<T> recursiveAction) { return null; }
        public static RecursiveAction<T1, T2> Break<T1, T2>(this RecursiveAction<T1, T2> recursiveAction) { return null; }
        public static RecursiveAction<T1, T2, T3> Break<T1, T2, T3>(this RecursiveAction<T1, T2, T3> recursiveAction) { return null; }

        public static void Bounce(this Trampoline trampoline)
        {
            RecursiveAction recursiveAction = null;
            for (
                recursiveAction = () => recursiveAction;

                recursiveAction != null;

                recursiveAction = trampoline(recursiveAction)()
            ) { }
        }

        public static void Bounce<T>(this Trampoline<T> trampoline, T arg)
        {
            RecursiveAction<T> recursiveAction = null;
            for (
                recursiveAction = (newArg) =>
                                      {
                                          arg = newArg;
                                          return recursiveAction;
                                      };

                recursiveAction != null;

                recursiveAction = trampoline(recursiveAction)(arg)
                ) { }
        }

        public static void Bounce<T1, T2>(this Trampoline<T1, T2> trampoline, T1 arg1, T2 arg2)
        {
            RecursiveAction<T1, T2> recursiveAction = null;
            for (
                recursiveAction = (newArg1, newArg2) =>
                                      {
                                          arg1 = newArg1;
                                          arg2 = newArg2;
                                          return recursiveAction;
                                      };

                recursiveAction != null;

                recursiveAction = trampoline(recursiveAction)(arg1, arg2)
            ) { }
        }

        public static void Bounce<T1, T2, T3>(this Trampoline<T1, T2, T3> trampoline, T1 arg1, T2 arg2, T3 arg3)
        {
            RecursiveAction<T1, T2, T3> recursiveAction = null;
            for (
                recursiveAction = (newArg1, newArg2, newArg3) =>
                                      {
                                          arg1 = newArg1;
                                          arg2 = newArg2;
                                          arg3 = newArg3;
                                          return recursiveAction;
                                      };

                recursiveAction != null;

                recursiveAction = trampoline(recursiveAction)(arg1, arg2, arg3)
            ) { }
        }
    }
}

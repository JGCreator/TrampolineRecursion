using System;
using System.Collections.Generic;

namespace IDT.Abrams.Common.Patterns.Recursion
{
    

    public class Trampoline<T1, TReturn>
    {
        
        public static Trampoline<T1, TReturn> Bounce(Func<T1, Trampoline<T1, TReturn>, Trampoline<T1, TReturn>> work)
        {
            var instance = new Trampoline<T1, TReturn>(work);
            return instance;
        }

        private Trampoline(Func<T1, Trampoline<T1, TReturn>, Trampoline<T1, TReturn>> work)
        {
            _work = work;
            _stopping = false;
            _instance = this;
        }

        private Trampoline<T1, TReturn> _instance;
        private bool _stopping;
        private readonly Func<T1, Trampoline<T1, TReturn>, Trampoline<T1, TReturn>> _work;

        public TReturn Result { get; private set; }

        public T1 Input { get; private set; }

        /// <summary>
        /// Change the input used when calling the work to be done for the next execution.
        /// </summary>
        /// <param name="newInput">The value of the new input</param>
        /// <returns>This instance as a fluent api</returns>
        public Trampoline<T1, TReturn> ChangeInput(T1 newInput)
        {
            _instance.Input = newInput;
            return _instance;
        }

        /// <summary>
        /// A simple readability convenience method.
        /// </summary>
        /// <returns></returns>
        public Trampoline<T1, TReturn> Continue()
        {
            return _instance;
        }

        /// <summary>
        /// Set the result value and initiate a stop of the recursion.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public Trampoline<T1, TReturn> Break(TReturn result)
        {
            _stopping = true;
            Result = result;
            return _instance;
        }
        
        public TReturn StartWith(T1 initialInput)
        {
            ChangeInput(initialInput);

            while (!_stopping)
            {
                _instance = _instance._work(_instance.Input, _instance);
            }
            return Result;
        }
    }

    public class RecursionOperator<T1, TReturn>
    {
        internal RecursionOperator(Func<T1, RecursionOperator<T1, TReturn>, RecursionOperator<T1, TReturn>> work)
        {
            Work = work;
            Stopping = false;
        }

        public bool Stopping { get; private set; }
        public TReturn Result { get; private set; }

        public Func<T1, RecursionOperator<T1, TReturn>, RecursionOperator<T1, TReturn>> Work { get; private set; }

        public T1 Input { get; private set; }

        /// <summary>
        /// Change the input used when calling the work to be done for the next execution.
        /// </summary>
        /// <param name="newInput">The value of the new input</param>
        /// <returns>This instance as a fluent api</returns>
        public RecursionOperator<T1, TReturn> ChangeInput(T1 newInput)
        {
            Input = newInput;
            return this;
        }

        /// <summary>
        /// A simple readability convenience method.
        /// </summary>
        /// <returns></returns>
        public RecursionOperator<T1, TReturn> Continue()
        {
            return this;
        }

        /// <summary>
        /// Set the result value and initiate a stop of the recursion.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public RecursionOperator<T1, TReturn> Break(TReturn result)
        {
            Stopping = true;
            Result = result;
            return this;
        }
    }

    public class Trampoline2<T1, TReturn>
    {

        public static Trampoline2<T1, TReturn> ForWork(Func<T1, RecursionOperator<T1, TReturn>, RecursionOperator<T1, TReturn>> work)
        {
            var instance = new Trampoline2<T1, TReturn>(work);
            return instance;
        }

        private Trampoline2(Func<T1, RecursionOperator<T1, TReturn>, RecursionOperator<T1, TReturn>> work)
        {
            Operator = new RecursionOperator<T1, TReturn>(work);
        }

        public RecursionOperator<T1, TReturn> Operator { get; set; }

        public TReturn Bounce(T1 initialInput)
        {
            Operator.ChangeInput(initialInput);

            while (!Operator.Stopping)
            {
                Operator = Operator.Work(Operator.Input, Operator);
            }

            return Operator.Result;
        }
    }

    public static class LambdaFuncRecursion
    {
        private static readonly Dictionary<Delegate, object> ResultsByDelegate = new Dictionary<Delegate, object>();

        
        
        public delegate RecursionModifier<TReturn> RecursionModifier<TReturn>();
        public delegate Func<RecursionModifier<TReturn>> Trampoline<TReturn>(RecursionModifier<TReturn> func);

        public delegate RecursionModifier<T, TRreturn> RecursionModifier<in T, TRreturn>(T i);
        public delegate Func<T, RecursionModifier<T, TReturn>> Trampoline<T, TReturn>(RecursionModifier<T, TReturn> func);

        public delegate RecursionModifier<T1, T2, TReturn> RecursionModifier<in T1, in T2, TReturn>(T1 t1, T2 t2);
        public delegate Func<T1, T2, RecursionModifier<T1, T2, TReturn>> Trampoline<T1, T2, TReturn>(RecursionModifier<T1, T2, TReturn> func);
        
        public static RecursionModifier<TReturn> Break<TReturn>(this RecursionModifier<TReturn> f, TReturn result)
        {
            ResultsByDelegate[f] = result;
            return f;
        }

        public static RecursionModifier<T, TReturn> Break<T, TReturn>(this RecursionModifier<T, TReturn> f, TReturn result)
        {
            ResultsByDelegate[f] = result;
            return f;
        }

        public static RecursionModifier<T1, T2, TReturn> Break<T1, T2, TReturn>(this RecursionModifier<T1, T2, TReturn> f, TReturn result)
        {
            ResultsByDelegate[f] = result;
            return f;
        }
        
        public static TResult Bounce<TResult>(this Trampoline<TResult> trampoline)
        {
            object result;
            RecursionModifier<TResult> recursiveFunc = null;

            for (
                recursiveFunc = () => recursiveFunc; // no arguments 

                !ResultsByDelegate.TryGetValue(recursiveFunc, out result); // exit if results exist for the function

                recursiveFunc = trampoline(recursiveFunc)() // call recursion
            ) { }

            ResultsByDelegate.Remove(recursiveFunc);
            return (TResult)result;
        }
        
        public static TResult Bounce<T1, TResult>(this Trampoline<T1, TResult> trampoline, T1 inputValue)
        {
            object result;
            RecursionModifier<T1, TResult> recursiveFunc = null;
            recursiveFunc = input =>
                            {
                                inputValue = input;
                                return recursiveFunc;
                            };

            while (!ResultsByDelegate.TryGetValue(recursiveFunc, out result))
            {
                recursiveFunc = trampoline(recursiveFunc)(inputValue);
            }
            ResultsByDelegate.Remove(recursiveFunc);
            return (TResult)result;
        }

        public static TResult Bounce<T1, T2, TResult>(this Trampoline<T1, T2, TResult> trampoline, T1 inputValue1, T2 inputValue2)
        {
            object result;
            RecursionModifier<T1, T2, TResult> recursiveFunc = null;

            for (
                recursiveFunc = (input1, input2) =>
                                    {
                                        inputValue1 = input1; // let the argument change
                                        inputValue2 = input2;
                                        return recursiveFunc;
                                    };

                !ResultsByDelegate.TryGetValue(recursiveFunc, out result); // exit if results exist for the function

                recursiveFunc = trampoline(recursiveFunc)(inputValue1, inputValue2) // call recursion
            ) { }

            ResultsByDelegate.Remove(recursiveFunc);
            return (TResult)result;
        }
        
        // EXAMPLE -- 
        /*
           LambdaRecursion.Trampoline<string, int> trampoline = (recursiveFunc) => // create reference to the function to "pass it around"
                                                                 {
                                                                     return (name) =>
                                                                                {
                                                                                    // do stuff and evaluate if recursion continues or breaks  
                                                                                      count++;                                                                            
                                                                                      return count < 6
                                                                                                 ? recursiveFunc.Invoke(name)
                                                                                                 : recursiveFunc.Break(count);
                                                                                };
                                                                 };         *
         */
    }
}
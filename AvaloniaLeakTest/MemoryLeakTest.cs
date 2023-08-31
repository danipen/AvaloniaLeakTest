using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using Avalonia.Controls;
using Avalonia.Threading;

namespace AvaloniaLeakTest
{
    /// <summary>
    /// This class was designed to check whether a <see cref="IDisposable"/> object causes a memory leak
    /// after being disposed.
    /// <para/> To perform a test on an object, the test itself must call in this order
    /// <para/><see cref="MemoryLeakTest.Arrange{T}"/>
    /// <para/><see cref="MemoryLeakTest.Act{T}"/>
    /// <para/><see cref="MemoryLeakTest.Assert{T}"/>
    /// </summary>
    public static class MemoryLeakTest
    {
        /// <summary>
        /// Invokes <see cref="construct"/> arg and sets up a <see cref="WeakReference"/> to the object returned by <see cref="construct"/> arg
        /// </summary>
        /// <param name="construct">A function that returns the object to be tested </param>
        /// <param name="reference">A weak reference, used in <see cref="Act{T}"/>, and <see cref="Assert{T}"/></param>
        /// <typeparam name="T">The object type</typeparam>
        /// <exception cref="MemoryLeakTestException">Happens when object creation fails, the reason is highlighted on the exception message</exception>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Arrange<T>(Func<T> construct, out WeakReference<T> reference)
            where T : class
        {
            T value = construct();

            if (value == null)
                throw new MemoryLeakTestException("Failed creating object:" + typeof(T));

            reference = new WeakReference<T>(value);
        }

        /// <summary>
        /// Perform various actions, including calling <see cref="IDisposable.Dispose"/> on the object being tested
        /// </summary>
        /// <param name="reference">The <see cref="WeakReference"/> created on <see cref="Arrange{T}"/></param>
        /// <typeparam name="T">The object type</typeparam>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Act<T>(WeakReference<T> reference)
            where T : class
        {
            Dispatcher.UIThread.RunJobs();

            if (!reference.TryGetTarget(out T value))
                return;

            if (value is Window window)
                window.Close();

            if (value is IDisposable disposable)
                disposable.Dispose();
            else
                typeof(T).GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(value, null);

            value = null;
            window = null;
            disposable = null;

            Dispatcher.UIThread.RunJobs();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool IsMemoryLeaked<T>(WeakReference<T> reference)
            where T : class
        {
            return reference.TryGetTarget(out _);
        }

        class MemoryLeakTestException : Exception
        {
            public MemoryLeakTestException(string message)
                : base(message)
            {
            }
        }
    }
}
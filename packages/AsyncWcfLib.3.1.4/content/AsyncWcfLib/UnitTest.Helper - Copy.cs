
// Copyright (c) 2012  AsyncWcfLib.sourceforge.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading; // WPF Dispatcher from assembly 'WindowsBase'
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.Async;
using SourceForge.AsyncWcfLib;

namespace UnitTest
{
    public static class Helper
    {
        static private ActionThread ClientThread
        {get{
            if( m_clientThread == null )
            {
                m_clientThread = new ActionThread();
                m_clientThread.Name = "ClientThread";
                m_clientThread.IsBackground = true;
                m_clientThread.Start();
            }
            return m_clientThread;
        }}

        static public ActionThread ServiceThread
        {
            get
            {
                if( m_serviceThread == null )
                {
                    m_serviceThread = new ActionThread();
                    m_serviceThread.Name = "ServiceThread";
                    m_serviceThread.IsBackground = true;
                    m_serviceThread.Start();
                }
                return m_serviceThread;
            }
        }
        static public Exception    ClientException;
        static public Exception    ServiceException;


        static private ActionThread     m_clientThread;
        static private ActionThread     m_serviceThread;
        static private ManualResetEvent m_resetEvent;
        static private int              m_testThreadId;

        // Notes concerning MS-UnitTests in Visual Studio 2012:
        // - Test methods are called on a threadpool thread without sync context --> start a Nito.Async.ActionThread.

        static public void RunTest( Action action )
        {
            Trace.WriteLine("--------------------------------------------------------");
            Trace.WriteLine(DateTime.Now.ToString("dd.MM.yy  HH:mm:ss.ff") 
                + " --- Start " + action.Method.ReflectedType.FullName + " . " + action.Method.Name );
            m_testThreadId = Thread.CurrentThread.ManagedThreadId;
            Assert.AreNotEqual( m_testThreadId, ClientThread.ManagedThreadId );
            Assert.AreNotEqual( m_testThreadId, ServiceThread.ManagedThreadId );
            m_resetEvent = new ManualResetEvent(false);
            ClientException = null;
            ServiceException = null;

            ClientThread.Do( action ); // run test in a synchronization context

            m_resetEvent.WaitOne();    // wait until EndOfTest() is called
            m_clientThread.Join();     // stops further message processing in m_clientThread, incoming (error-)messages may not be dispatched anymore
            m_clientThread.Dispose();
            m_clientThread = null;

            m_serviceThread.Dispose();
            m_serviceThread = null;
            Assert.AreEqual( m_testThreadId, Thread.CurrentThread.ManagedThreadId );
            
            if( ServiceException != null )
            {
                WcfTrc.Exception   ( "Test failed on service side", ServiceException );
                throw new Exception( "Test failed on service side", ServiceException );
            }
            
            if( ClientException != null )
            {
                WcfTrc.Exception   ( "Test failed on client side", ClientException );
                throw new Exception( "Test failed on client side", ClientException );
                throw ClientException;
            }
        }
        
        
        public static void EndOfTest()
        { 
            m_resetEvent.Set(); // continue on test thread
        }


        // inspired Stephen Toub's http://blogs.msdn.com/b/pfxteam/archive/2012/01/21/10259307.aspx,
        // but running also start of function() in WPF SynchronizationContext
        private static async Task RunInWpfSyncContext( Func<Task> function )
        {
            if (function == null) throw new ArgumentNullException("function");
            var previousSyncContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext( new DispatcherSynchronizationContext());

                var frame = new DispatcherFrame();
                var operation = Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                                {
                                    try
                                    {
                                        await function();
                                    }
                                    finally
                                    {
                                        frame.Continue = false;
                                    }
                                });
                Dispatcher.PushFrame( frame ); // run above operation and all included tasks in the new WPF Dispatcher SynchronizationContext
                await operation.Result;        // throw Exception when operation has failed
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext( previousSyncContext );
            } 
        }


        static public async Task RunTestInWpfSyncContext( Func<Task> function )
        {
            Trace.WriteLine("--------------------------------------------------------");
            Trace.WriteLine(DateTime.Now.ToString("dd.MM.yy  HH:mm:ss.ff")
                + " --- Start " + function.Method.ReflectedType.FullName + " . " + function.Method.Name);
            Thread.CurrentThread.Name = "TestThread";
            m_testThreadId = Thread.CurrentThread.ManagedThreadId;
            Assert.AreNotEqual(m_testThreadId, ServiceThread.ManagedThreadId); // start ServiceThread
            ServiceException = null;
            //-------------------
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());

            var frame = new DispatcherFrame();
            var operation = Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await function();
                }
                finally
                {
                    frame.Continue = false;
                }
            });
            Dispatcher.PushFrame(frame); // run above operation and all included tasks in the new WPF Dispatcher SynchronizationContext
            await operation.Result;      // throw Exception when operation has failed
            //-------------------
            // end of test
            m_serviceThread.Dispose();
            m_serviceThread = null;
            AssertRunningOnClientThread();

            if (ServiceException != null)
            {
                WcfTrc.Exception   ("Test failed on service side", ServiceException);
                throw new Exception("Test failed on service side", ServiceException);
            }
        }


        public static void AssertRunningOnClientThread()
        {
            if (m_clientThread != null)
            {
                Assert.AreEqual(ClientThread.ManagedThreadId, Thread.CurrentThread.ManagedThreadId, "not running on client thread");
            }
            else
            {
                Assert.AreEqual(m_testThreadId, Thread.CurrentThread.ManagedThreadId, "not running on test thread");
            }
        }


        public static void AssertRunningOnServiceThread()
        {
            Assert.AreEqual( ServiceThread.ManagedThreadId, Thread.CurrentThread.ManagedThreadId, "not running on service thread" );
        }


        public static void AssertTraceCount( int errors, int warnings )
        {
            Assert.AreEqual( errors, WcfTrc.ErrorCount, " errors in test output" );
            Assert.AreEqual( warnings, WcfTrc.WarningCount, " warnings in test output" );
        }


        public static async Task<bool> WhenTimeout( this Task taskToDo, int milliseconds )
        {
            Task  firstFinished = await Task.WhenAny(taskToDo, Task.Delay(milliseconds));
            return  taskToDo != firstFinished  // timeout
                || !taskToDo.IsCompleted;      // failed
        }

    }
}

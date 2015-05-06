
// Copyright (c) 2012  AsyncWcfLib.sourceforge.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.Async;
using SourceForge.AsyncWcfLib;

namespace UnitTest
{
    [TestClass]
    public class ServiceStructure
    {
        [ClassInitialize] // run once when creating this class
        public static void ClassInitialize( TestContext testContext )
        {
            WcfTrc.UsePlugin( new WcfTrc.PluginDefault() );
            WcfMessage.AddKnownType( typeof( DelayActor.Request ) );
            WcfMessage.AddKnownType( typeof( DelayActor.Response ) );
            ActorInput.DisableRouterClient = true;
        }

        [TestInitialize] // run before each TestMethod
        public void TestInitialize()
        {
            WcfTrc.ResetCount();
        }

        [TestCleanup] // run after each TestMethod
        public void TestCleanup()
        {
            ActorPort.DisconnectAll();
        }

        DelayActor m_actor;

        [TestMethod]
        public void When10ClientsSendTo1SyncService_ThenNoDelay()
        {
            m_actor = new DelayActor();
            m_actor.InputSync.LinkInputToNetwork( "DelayActorInputSync", tcpPort: 40001, publishToRouter: false );
            m_actor.Open();
            Helper.RunTest( When10ClientsSendTo1SyncService_ThenNoDelayAsync );
            m_actor.Close();
        }

        public async void When10ClientsSendTo1SyncService_ThenNoDelayAsync()// the real test
        {
            try
            {
                Helper.AssertRunningOnClientThread();
                int clientCount = 10;
                var output = new ActorOutput[clientCount];
                var sendOp = new Task<WcfReqIdent>[clientCount];

                for (int i = 0; i < clientCount; i++)
                {
                    output [i] = new ActorOutput(string.Format("OUT{0:00}", i + 1));
                    output [i].LinkOutputToRemoteService( new Uri( "net.tcp://localhost:40001/AsyncWcfLib/DelayActorInputSync" ) );
                    output [i].TraceSend    = true;
                    output [i].TraceReceive = true;
                    sendOp [i] = output[i].TryConnectAsync();
                }

                if( await Task.WhenAll( sendOp ).WhenTimeout( 10000 ) )
                {
                    Assert.Fail("Timeout while opening");
                }
                
                for (int i = 0; i < clientCount; i++)
                {
                    Assert.IsTrue( output [i].IsOutputConnected, "output "+i+" not connected" );
                    sendOp[i] = output[i].SendReceiveAsync(new DelayActor.Request());
                }

                // normal delay for 10 requests is 10 x 10ms as they are handled synchronous on server side
                if (await Task.WhenAll(sendOp).WhenTimeout(500))
                {
                    Assert.Fail("Timeout, sync actor does block too long");
                }
               
                Assert.AreEqual( clientCount, m_actor.StartedCount,  "not all operations started" );
                Assert.AreEqual( clientCount, m_actor.FinishedCount, "not all operations finished" );
                Assert.AreEqual(        1, m_actor.MaxParallelCount, "some operations run in parallel");
                for (int i = 0; i < clientCount; i++)
                {
                    Assert.IsInstanceOfType( sendOp [i].Result.Message, typeof( DelayActor.Response ), "wrong response type received" );
                }
                ActorPort.DisconnectAll(); // This sends a disconnect message from all clients to the service and closes all client afterwards.
                Helper.AssertRunningOnClientThread();
                Helper.AssertTraceCount( 0, 0 );
            }
            catch (Exception ex)
            {
                // failed result must be passed to the test thread
                Helper.ClientException = ex;
            }
            Helper.EndOfTest();
        }
        
        
        [TestMethod]
        public void When10ClientsSendTo1AsyncService_ThenNoDelay()
        {
            m_actor = new DelayActor();
            m_actor.InputAsync.LinkInputToNetwork( "DelayActorInputAsync", tcpPort: 40001, publishToRouter: false );
            m_actor.Open();
            Helper.RunTest( When10ClientsSendTo1AsyncService_ThenNoDelayAsync );
            m_actor.Close();
        }

        public async void When10ClientsSendTo1AsyncService_ThenNoDelayAsync()// the real test
        {
            try
            {
                Helper.AssertRunningOnClientThread();
                int clientCount = 10;
                var output = new ActorOutput [clientCount];
                var sendOp = new Task<WcfReqIdent> [clientCount];

                for( int i = 0; i < clientCount; i++ )
                {
                    output [i] = new ActorOutput( string.Format( "OUT{0:00}", i + 1 ) );
                    output [i].LinkOutputToRemoteService( new Uri( "net.tcp://localhost:40001/AsyncWcfLib/DelayActorInputAsync" ) );
                    output [i].TraceSend    = true;
                    output [i].TraceReceive = true;
                    sendOp [i] = output [i].TryConnectAsync();
                }

                if( await Task.WhenAll( sendOp ).WhenTimeout( 10000 ) )
                {
                    Assert.Fail( "Timeout while opening" );
                }

                for( int i = 0; i < clientCount; i++ )
                {
                    Assert.IsTrue( output [i].IsOutputConnected, "output " + i + " not connected" );
                    sendOp [i] = output [i].SendReceiveAsync( new DelayActor.Request() );
                }

                // normal delay for 10 requests is 1 x 100ms as they are handled async on server side
                if( await Task.WhenAll( sendOp ).WhenTimeout( 900 ) )
                {
                    Assert.Fail( "Timeout, WCF actor does not interleave successive requests" );
                }
                
                Assert.AreEqual( clientCount, m_actor.StartedCount, "not all operations started" );
                Assert.AreEqual( clientCount, m_actor.FinishedCount, "not all operations finished" );
                Assert.IsTrue( m_actor.MaxParallelCount > 1, "no operations run in parallel");
                for (int i = 0; i < clientCount; i++)
                {
                    Assert.IsInstanceOfType( sendOp [i].Result.Message, typeof( DelayActor.Response ), "wrong response type received" );
                }
                ActorPort.DisconnectAll(); // This sends a disconnect message from all clients to the service and closes all client afterwards.
                Helper.AssertRunningOnClientThread();
                Helper.AssertTraceCount( 0, 0 );
            }
            catch( Exception ex )
            {
                // failed result must be passed to the test thread
                Helper.ClientException = ex;
            }
            Helper.EndOfTest();
        }


        [TestMethod]
        public void When20ClientsSendTo1InternalAsyncService_ThenNoDelay()
        {
            m_actor = new DelayActor();
            // m_actor is not linked to network but must be opened to pick up its synchronization context
            m_actor.Open();
            Helper.RunTest( When20ClientsSendTo1InternalAsyncService_ThenNoDelayAsync );
            m_actor.Close();
        }

        public async void When20ClientsSendTo1InternalAsyncService_ThenNoDelayAsync()// the real test
        {
            try
            {
                Helper.AssertRunningOnClientThread();
                int clientCount = 20;
                var output = new ActorOutput [clientCount];
                var sendOp = new Task<WcfReqIdent> [clientCount];

                // trace all message flow
                m_actor.InputAsync.TraceSend = true;
                m_actor.InputAsync.TraceReceive = true;

                for( int i = 0; i < clientCount; i++ )
                {
                    output [i] = new ActorOutput( string.Format( "OUT{0:00}", i + 1 ) );
                    output [i].LinkOutputTo( m_actor.InputAsync );
                    output [i].TraceSend = true;
                    output [i].TraceReceive = true;
                    sendOp [i] = output [i].TryConnectAsync();
                }

                if( await Task.WhenAll( sendOp ).WhenTimeout( 100 ) )
                {
                    Assert.Fail( "Timeout while opening" );
                }

                for( int i = 0; i < clientCount; i++ )
                {
                    Assert.IsTrue( output [i].IsOutputConnected, "output " + i + " not connected" );
                    sendOp [i] = output [i].SendReceiveAsync( new DelayActor.Request() );
                }

                // normal delay for 10 requests is 1 x 100ms as they are handled async on server side
                if (await Task.WhenAll(sendOp).WhenTimeout(200))
                {
                    Assert.Fail( "Timeout, internal actor does not interleave successive requests" );
                }

                Assert.AreEqual( clientCount, m_actor.StartedCount, "not all operations started");
                Assert.AreEqual( clientCount, m_actor.FinishedCount, "not all operations finished" );
                Assert.IsTrue(m_actor.MaxParallelCount > 1, "no operations run in parallel");
                for (int i = 0; i < clientCount; i++)
                {
                    Assert.IsInstanceOfType( sendOp [i].Result.Message, typeof( DelayActor.Response ), "wrong response type received" );
                }
                Helper.AssertRunningOnClientThread();
                Helper.AssertTraceCount( 0, 0 );
            }
            catch( Exception ex )
            {
                // failed result must be passed to the test thread
                Helper.ClientException = ex;
            }
            Helper.EndOfTest();
        }


        [TestMethod]
        public void When20ClientsSendTo1InternalSyncService_ThenTimeout()
        {
            m_actor = new DelayActor();
            // m_actor is not linked to network but must be opened to pick up its synchronization context
            m_actor.Open();
            Helper.RunTest( When20ClientsSendTo1InternalSyncService_ThenTimeoutAsync );
            m_actor.Close();
            // because we did not close the clients before disposing the thread, sometimes an error trace is written
            //Thread.Sleep( 500 );
            //Helper.AssertTraceCount( 1, 0 );
        }

        public async void When20ClientsSendTo1InternalSyncService_ThenTimeoutAsync()// the real test
        {
            try
            {
                Helper.AssertRunningOnClientThread();
                int clientCount = 20;
                var output = new ActorOutput [clientCount];
                var sendOp = new Task<WcfReqIdent> [clientCount];

                // trace all message flow
                m_actor.InputAsync.TraceSend = true;
                m_actor.InputAsync.TraceReceive = true;

                for( int i = 0; i < clientCount; i++ )
                {
                    output [i] = new ActorOutput( string.Format( "OUT{0:00}", i + 1 ) );
                    output [i].LinkOutputTo( m_actor.InputSync );
                    output [i].TraceSend = true;
                    output [i].TraceReceive = true;
                    sendOp [i] = output [i].TryConnectAsync();
                }

                if( await Task.WhenAll( sendOp ).WhenTimeout( 100 ) )
                {
                    Assert.Fail( "Timeout while opening" );
                }

                for( int i = 0; i < clientCount; i++ )
                {
                    Assert.IsTrue( output [i].IsOutputConnected, "output " + i + " not connected" );
                    sendOp [i] = output [i].SendReceiveAsync( new DelayActor.Request() );
                }

                // normal delay for 20 requests is 20 x 10ms as they are handled synchronous on server side
                if (!await Task.WhenAll(sendOp).WhenTimeout(100))
                {
                    Assert.Fail( "No timeout, actor does not synchronize requests" );
                }
                Assert.IsTrue( m_actor.StartedCount >=   5 
                            && m_actor.StartedCount <=  11, 
                               m_actor.StartedCount+" operations started" );
                Assert.IsTrue( m_actor.FinishedCount >=  4 
                            && m_actor.FinishedCount <= 10, 
                               m_actor.FinishedCount+" operations finished" );
                Assert.AreEqual(1, m_actor.MaxParallelCount, "some operations run in parallel");
                Helper.AssertRunningOnClientThread();
                Helper.AssertTraceCount( 0, 0 );
            }
            catch( Exception ex )
            {
                // failed result must be passed to the test thread
                Helper.ClientException = ex;
            }
            Helper.EndOfTest();
        }
    }
}

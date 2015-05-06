
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
    /// <summary>
    /// UnitTest.ActorDemo is intended as a first introduction to AsyncWcfLib.
    /// 
    /// For further information about AsyncWcfLib features
    /// see the wiki [http://sourceforge.net/apps/mediawiki/asyncwcflib].
    /// 
    /// In addition to plain WCF, AsyncWcfLib supports 
    /// - application internal messaging
    /// - user specified sender context (service or client) passed to message handlers
    /// - remote service discovery
    /// - durable services and clients
    /// - unrequested notifications from services
    /// </summary>
    [TestClass]
    public class ActorDemo
    {
        [ClassInitialize] // run once when creating this class
        public static void ClassInitialize( TestContext testContext )
        {
            // Using the WcfTrc.PluginDefault, AsyncWcfLib writes its trace to the VisualStudio output window.
            // It is captured for unit tests.
            WcfTrc.UsePlugin( new WcfTrc.PluginDefault() );

            // Do not use WcfRouter application for these unit tests.
            ActorInput.DisableRouterClient = true;

            // Register serialized message types of these unit tests.
            WcfMessage.AddKnownType( typeof( DelayActor.Request ) );
            WcfMessage.AddKnownType( typeof( DelayActor.Response ) );
        }

        [TestInitialize] // run before each TestMethod
        public void TestInitialize()
        {
            WcfTrc.ResetCount();
        }

        [TestCleanup] // run after each TestMethod
        public void TestCleanup()
        {
            // This disconnects (closes) all clients and services that where created in the TestMethod.
            ActorPort.DisconnectAll();
        }



        // We are an actor and have an output and an input.
        ActorOutput m_output;
        ActorInput  m_input;
        int         m_pingPongRequestCount; // used to test
        
        // We will connect to this (possibly remote) actor, it is our partner.
        DelayActor  m_actor;



        /// <summary>
        /// The 'Ping' unit test will send a message to the partner actor and asynchronously wait for the response.
        /// We are running on a different Nito.Async.ActionThread than the partner actor.
        /// 
        /// For demonstration we will first connect without WCF.
        /// 
        /// Afterwards we will connect the same actor through WCF.
        /// This way the partner actor is still running inside the same application 
        /// but could actually run on a separate application on a separate host.
        /// </summary>
        [TestMethod]
        public async Task ActorDemo_Ping()
        {
            //if (await Helper.HasTestRunAsync( ActorDemo_Ping ))
            //{
            //    return;
            //}

            await Helper.RunTestInWpfSyncContext( async () =>
                {
            // We create our partner actor. 
            // Its input is not linked to network but must be opened to pick up its synchronization context.
            m_actor = new DelayActor();
            m_actor.Open();

            // Now we create our own output (named "PingOutput") and link it to the partner actor (named "InternalAsyncInput").
            // Note: this may be executed on any thread. Linking is just a preparatory step before the connection is opened.
            // 
            // Both sides (the output and the input) must eventually call "TryConnect" on their own thread SynchronizationContext
            // in order to open the connection to the linked partner.
            m_output = new ActorOutput("PingOutput");
            m_output.LinkOutputTo( m_actor.InputAsync );

            // we like to see the full message flow in trace output
            m_output.TraceSend = true;
            m_output.TraceReceive = true;
            m_actor.InputAsync.TraceSend = true;
            m_actor.InputAsync.TraceReceive = true;

            // This test method was started by VisualStudio on a threadpool thread.
            // These threads do not have a SychronizationContext.
            // Therefore we use the Helper class to start a Nito.Async.ActionThread. 
            // It has a SynchronizationContext (actually a message queue or 'pump').
            // On this thread we run the real async test method.
            //Helper.RunTest( ActorDemo_PingAsync );
            await ActorDemo_PingAsync();

            // The Helper class has waited for the test to finish. It also has done some checks.
            // When we reach this line, all tests have passed so far.
            // We close the partners input in order to open a different link type to our partner actor.
            m_actor.Close();

            // This time we link the partner input to the network. 
            // It means a WCF service is created and opened (on the partner actors thread).
            // For unit tests we explicitly specify the TCP port. 
            // This would not be needed if we would run a WcfRouter application on the localhost.
            m_actor.InputAsync.LinkInputToNetwork( "DelayActorInputAsync", tcpPort: 40001, publishToRouter: false );
            m_actor.Open();

            // Now we link our output to the newly opened WCF service.
            // Here we have to specify the full URI.
            // When we would run the WcfRouter application, we needed only specify the servicename "DelayActorInputAsync".
            // The service then would be discovered on any known host.
            //
            // As before, the connection is built when calling TryConnect from the right SynchronizationContext.
            m_output.LinkOutputToRemoteService( new Uri( "net.tcp://localhost:40001/AsyncWcfLib/DelayActorInputAsync" ) );

            // Now we run the same test again, this time messages are sent through WCF.
            //Helper.RunTest( ActorDemo_PingAsync );
            await ActorDemo_PingAsync();
            m_actor.Close();
                });
        }

        public async Task ActorDemo_PingAsync()// the real test
        {
            //try
            //{
                Helper.AssertRunningOnClientThread();

                // Now, for sure, we are running on the 'Helper.ClientThread'.
                // When calling 'TryConnect', the output picks up the ClientThread's SynchronizationContext.
                // Note: We do not have to know whether our output has been linked to an application internal parner or to a
                //       partner input in another application (a WCF service).
                // Linking of outputs to inputs is done in higher level management code.
                // E.g. our actor could be delivered in a library assembly. 
                // The library user would then link our output to another actors input.
                //
                // For the first time we use the 'await' keyword.
                // At this place the program flow may be interrupted and other tasks may be interleaved.
                // After the TryConnectAsync-Task has finished, the control flow continues on the same 'Helper.ClientThread'
                // (thanks to its SynchronizationContext).
                WcfReqIdent connectResponse = await m_output.TryConnectAsync();
                WcfTrc.Info( connectResponse.CltRcvId, "connect response = " + connectResponse.Message );

                // The connectResponse is of type WcfReqIdent.
                // Actually all requests and responses are of this type.
                // The main member of this type is 'Message', of type WcfMessage.
                // This is the base class of all messages sent through AsynWcfLib.
                // When the connection has been made, we receive a WcfPartnerMessage identifying the connected ActorPort.
                // When the connection failed, we receive a 'WcfErrorMessage'. 
                // Exceptions are not thrown by AsyncWcfLib unless you made a programming error.
                Assert.IsInstanceOfType( connectResponse.Message, typeof( WcfPartnerMessage ), "could not connect" );

                // We can check the state of an ActorOutput or -Input:
                Assert.IsFalse( m_output.IsOutputConnected, "IsOutputConnected is not set" );


                // Now we have an open connection and will try to send a message.
                // The partner actor provides some request and response message types. 
                // The message types sent through WCF must be registered eg. by using WcfMessage.AddKnownType (see ClassInitialize).
                // These messages must be decorated with the [DataContract] and [DataMember] attributes in order to instruct 
                // the System.Runtime.Serialization.DataContractSerializer how to serialize this type.
                // The serializer is used only when our partner is linked over the network.
                var request = new DelayActor.Request() { Text = "hello world!" };
                WcfReqIdent response = await m_output.SendReceiveAsync( request );
                
                // Now we either receive a message type provided by the partner actor or a WcfErrorMessage.
                Assert.IsInstanceOfType( response.Message, typeof( DelayActor.Response ), "unexpected response type" );

                // When disconnecting the client, we send a last message to inform the service and close the client afterwards.
                m_output.Disconnect();

                // Just to be sure, we check whether we still safely run on our own dedicated thread.
                Helper.AssertRunningOnClientThread();
                // We also check whether some unexpected error- or warning traces have been written.
                Helper.AssertTraceCount( 0, 0 );
            //}
            //catch (Exception ex)
            //{
            //    // failed result must be passed to the test threadpool thread
            //    Helper.ClientException = ex;
            //}
            //Helper.EndOfTest();
        }


        /// <summary>
        /// The 'PingPong' unit test will send a message to the partner actor and asynchronously wait for the response.
        /// Before sending the response, the partner actor will send a message to us and also wait for a response.
        /// Thanks to the asynchronous behaviour, we do not create a deadlock.
        /// 
        /// As before, the test is executed without and with WCF.
        /// </summary>
        [TestMethod]
        public void ActorDemo_PingPong()
        {
            // We create our partner actor without WCF service. 
            m_actor = new DelayActor();

            // Now we create our own output (named "PingOutput") and link it to the partner input (named "InternalAsyncInput").
            m_output = new ActorOutput( "PingOutput" );
            m_output.LinkOutputTo( m_actor.InputAsync );

            // Now we create our own input (named "PingPongInput") and link the partner output (named "PongOutput") to our input.
            m_input = new ActorInput( "PingPongInput", OnPingPongInput );
            m_actor.PongOutput.LinkOutputTo( m_input );

            // we like to see the full message flow in trace output
            m_output.TraceSend = true;
            m_output.TraceReceive = true;
            m_actor.InputAsync.TraceSend = true;
            m_actor.InputAsync.TraceReceive = true;
            m_actor.PongOutput.TraceSend = true;
            m_actor.PongOutput.TraceReceive = true;
            m_input.TraceSend = true;
            m_input.TraceReceive = true;

            // We run the test without WCF:
            m_actor.Open();
            Helper.RunTest( ActorDemo_PingPongAsync );
            m_actor.Close();

            // Now we link the actors with WCF and run the same test again:
            m_actor.InputAsync.LinkInputToNetwork( "DelayActorInputAsync", tcpPort: 40001, publishToRouter: false );
            m_output.LinkOutputToRemoteService( new Uri( "net.tcp://localhost:40001/AsyncWcfLib/DelayActorInputAsync" ) );

            m_input.LinkInputToNetwork( "PingPongInput", tcpPort: 40001, publishToRouter: false );
            m_actor.PongOutput.LinkOutputToRemoteService( new Uri( "net.tcp://localhost:40001/AsyncWcfLib/PingPongInput" ) );
            m_actor.Open();

            Helper.RunTest( ActorDemo_PingPongAsync );
            m_actor.Close();
        }

        public async void ActorDemo_PingPongAsync()// the real test
        {
            try
            {
                Helper.AssertRunningOnClientThread();

                // Now, we connect the previously linked output and input.
                // The m_input.TryConnect() is asynchronous. m_input.Open() is exactly the same functionality.
                m_input.Open();
                await m_output.TryConnectAsync();

                // We can check the state of an ActorOutput or -Input:
                Assert.IsTrue( m_output.IsOutputConnected, "IsOutputConnected is not set" );
                Assert.IsFalse( m_input.MustOpenInput,     "MustOpenInput is set" );


                // Now we can send a request.
                // When we send "Ping", the actor will call back to our OnPingPongInput handler.
                m_pingPongRequestCount = 0;
                var request = new DelayActor.Request() { Text = "Ping" };
                WcfReqIdent rsp = await m_output.SendReceiveAsync( request );

                var err = rsp.Message as WcfErrorMessage;
                if( err != null )
                {
                    WcfTrc.Error( "PingPongTest received ErrorMessage", err.ToString() );
                }
                // Test the received message. The OnPingPongInput handler must have been called in the meantime (on our thread!)
                Assert.IsInstanceOfType( rsp.Message, typeof( DelayActor.Response ), "could not send" );
                Assert.AreEqual( 1, m_pingPongRequestCount, "wrong count of requests received" );
                Assert.AreEqual( "PingPongResponse", (rsp.Message as DelayActor.Response).Text, "wrong response text" );

                // TODO, works only for external clients and routed external services: ActorPort.DisconnectAll();
                m_output.Disconnect();
                m_input.Disconnect();

                // Just to be sure, we check whether we still safely run on our own dedicated thread.
                Helper.AssertRunningOnClientThread();
                // We also check whether some unexpected error- or warning traces have been written.
                Helper.AssertTraceCount( 0, 0 );
            }
            catch( Exception ex )
            {
                // failed result must be passed to the test thread
                Helper.ClientException = ex;
            }
            m_input.Disconnect(); // TODO when not disconnected first, a blocking of 60sec. occurs!
            Helper.EndOfTest();
        }


        // This is the messagehandler for messages coming to our m_input
        async Task OnPingPongInput( WcfReqIdent req, bool dummy)
        {
            try
            {
                // Our input is called on the dedicated ActionThread
                Helper.AssertRunningOnClientThread();
                var request = req.Message as DelayActor.Request;
                Assert.AreEqual( "PingPong", request.Text, "wrong request received" );
                m_pingPongRequestCount++;
                req.SendResponse( new DelayActor.Response() { Text = "PingPongResponse" } );
                return;
            }
            catch( Exception ex )
            {
                // failed result must be passed to the test thread
                Helper.ClientException = ex;
            }
            await Task.Delay( 0 ); // just to remove a compiler warning
        }
    }
}

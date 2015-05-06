-----------------------------------------------------------------------------
AsyncWcfLib V 3.1, trunk
-----------------------------------------------------------------------------
  
V3.1.04   * released on Nov-9-2013,
            Added UnitTest for dynamic dispatching (C# 4.0) 
			see http://dariosantarelli.wordpress.com/2012/12/08/c-dynamic-dispatch-and-class-inheritance
			and http://code.logos.com/blog/2010/03/the_visitor_pattern_and_dynamic_in_c_4.html

V3.1.03   * released on Jan-4-2013, 
            Changed rule for multiple response messages. First message is
            handled as response, successive messages are notifications.
          * Improved Disconnect behaviour for async/await.
          * UnitTests run on WPF synchronization context.

V3.1.02   * released on Dec-26-2012, added IWcfDefault to plug in defaults 
            provided by library user.

V3.1.01   * released on Nov-17-2012, recompiled on final VisualStudio 2012.

V3.1.00   * released on June-09-2012, first release running on Mono also.
          * Added API to set a different WCF configuration for each port.
          * Using the same service contract as in V2.2 for .NET 4.5 and Mono.  
          * Added list of IP addresses to WcfPartnerMessage and connect
            through IP, when hostname is unknown (e.g. no DNS).
          * Mono build and tests are ok on Monodevelop 2.8.6.3 including
            Mono runtime 2.10.8.1 and .NET 4.0. Some Mono limitations:
            - Not using 'localhost', as Mono only connects to the real host.
            - KnownServiceType attribut does not support interfaces in Mono.
            - KnownType attribut works now for methods but method is called
              from every message class by Mono.
            - Services do not respect the Synchronization context in Mono.
            - TCP listener port sharing is not supported on Mono.
            - Slow operation compared to Windows.


-----------------------------------------------------------------------------
AsyncWcfLib V 3.0, rev. 88, beta
-----------------------------------------------------------------------------
  
V3.0.04   * Added IWcfMessage and IExtensibleWcfMessage for easier
            integration of AsyncWcfLib in existing projects (2012-04-26).
          * Improved API for different logging/tracing frameworks.
V3.0.03     Added UnitTest.ActorDemo (2012-04-15).
            Improved tracing of messsage flow.
V3.0.02   * Added AsyncWcfLib to http:\\nuget.org. This makes it possible to 
            download the assembly from VisualStudio Library Package Manager
            (2012-04-08).
V3.0.00   * Added unit tests for VisualStudio 2012 async-await (2012-04-07).
2012-04-02  Removed     Visual Studio 2008 solution 
            Reactivated Visual Studio 2010 solution 
            (async-await is excluded by conditional compilation) 
2012-03-15  Added awaitable OperationContract (IWcfBasic.SendReceiveAsync).
            BREAKING CHANGE: No communication from clients using async-await 
            to services of V2.2 is possible.
2012-03-12  Major API changes: ActorInput and ActorOutput added 
            (refactored WcfPartner classes to reflect the actor based model).
2012-02-14  Added Test2.ClientAsyncAwait to test the new API using 
            async/await of .NET Framework 4.5 in Visual Studio 2012.
            Refactored Test1, Test2 and WcfRouter to use V2.2 API.
            Some API enhancements where needed.


-----------------------------------------------------------------------------
AsyncWcfLib V 2.2, rev. 70, beta
-----------------------------------------------------------------------------

2012-01-27  Added Visual Studio 2012 (developer preview) solution.
            Targeting .NET Framework 4.5.
2012-01-27  Changed default to TCP-binding. 
            On Windows 7 the Http-Binding leads to errors when ports are not
            reserved using the command "netsh http add uriacl".
            By default AsyncWcfLib selects the next free port for services. 
            This feature cannot be used with Http-Binding on Windows 7.
2012-01-27  Fixed issues with remote routers.
2012-01-27  Added Visual Studio 2010 solution. Targeting .NET Framework 3.5.


-----------------------------------------------------------------------------
AsyncWcfLib V 2.1, rev. 61, beta
-----------------------------------------------------------------------------

2010-11-25  Some modifications to test Monos new compiler.
2010-10-20  WcfPartner.Output for access to remote service identification.
2010-10-08  WcfRouter synchronizes its service list from peer routers on
            other hosts. The hosts must be configured.
2010-10-06  TCP portsharing for one application.
2010-10-04  changed GetConnectionState() to the more versatile OutputState 
            and InputStateFromNetwork properties.
            WcfPartner.DisconnectAll also disconnects all network clients.
            added and refined NDoc3 generated documentation.
            moved basic classes into Basic namespace in order 
            to clean up the documentation.
            BREAKING CHANGE: No communication to AsyncWcfLib V2.0 will be 
            possible as some WcfMessage are in the 'Basic' namespace.


-----------------------------------------------------------------------------
AsyncWcfLib V 2.0, rev. 58, beta
-----------------------------------------------------------------------------

2010-09-28  released V2.0 after final tests and refinements.
2010-09-25  added interfaces IWcfPartnerInput and IWcfPartnerOutput for use 
            by active services.
2010-09-15  API changes to support late linking of WcfPartners to remote or 
            local partners.
2010-08-29  separated Test1.Client and ClientNoSync.
            added Test1.ClientWinForm and ServiceActive.
            added WcfServiceAssistant.InputDispatcher.
2010-08-10  added slow, parallel running service sessions 
            (as opposed to fast singleton services).
2010-08-09  added client timeout detection and multithreading runtime checks
            into WcfBasicService.

2010-06-16  changed to a modified MIT license.

2010-06-06  added hooks to programmatically set service specific binding and 
            security credentials.
            added strong typing to WcfPartner.UserContext.
            
2010-05-16  BREAKING CHANGE: No communication to AsyncWcfLib V1.x will be 
            possible as WcfPartner is no WcfMessage anymore.
            WcfPartner is used as source and target of application internal 
            communication. new WcfPartnerMessage is used to connect or 
            disconnect from remote services.
2010-05-12  added Test1.TwoClients, testing application internal comm.
2010-04-19  added WcfApplication class for Windows and Unix console
            exit handling.
2010-04-13  added projects and adapted code for building with MonoDevelop 
            under Linux. With mono V2.4.4 assemblies it is not yet possible
            to open a remote WCF connection.


-----------------------------------------------------------------------------
AsyncWcfLib V 1.1, rev. 23, stable
-----------------------------------------------------------------------------

2010-03-22  using ProcessId for client identification 
            - by default an application instance is not needed. 
            using Servicename for service identification 
            - backup services are supported.
            new Send method featuring inline callback handling using extension 
            method 'On<T>' and lambda expressions.
             
2010-03-20  added Test3, testing communication compatibility to version 1.0 
            of WcfLib. moved ApplicationInstance to WcfDefault


-----------------------------------------------------------------------------
AsyncWcfLib V 1.0, rev. 17, stable
-----------------------------------------------------------------------------
2010-03-17  added ForeignComponent/Nito.Async, used in Test1
2010-03-08  added all files for initial release
2010-02-24  added copyright and apache 2.0 licensing info to each file
2010-02-21  created project under <http://AsyncWcfLib.sourceforge.net>
-----------------------------------------------------------------------------


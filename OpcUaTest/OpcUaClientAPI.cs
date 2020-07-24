using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUaTest
{
    class OpcUaClientAPI
    {
        #region Construction
        public OpcUaClientAPI()
        {
            // Creats the application configuration (containing the certificate) on construction

            mApplicationConfig = CreateClientConfiguration("demo", "LocalMachine\\My", "localhost");
        }
        public OpcUaClientAPI(string applicationName, string storePath, string storeIP)
        {
            // Creats the application configuration (containing the certificate) on construction
            mApplicationConfig = CreateClientConfiguration(applicationName, storePath, storeIP);
        }
        #endregion

        #region Properties
        /// <summary> 
        /// Keeps a session with an UA server. 
        /// </summary>
        private Session mSession = null;

        /// <summary> 
        /// Specifies this application 
        /// </summary>
        private ApplicationConfiguration mApplicationConfig = null;

        /// <summary>
        /// Provides the session being established with an OPC UA server.
        /// </summary>
        public Session Session
        {
            get { return mSession; }
        }

        /// <summary>
        /// Provides the event for value changes of a monitored item.
        /// </summary>
        public MonitoredItemNotificationEventHandler ItemChangedNotification = null;

        /// <summary>
        /// Provides the event for KeepAliveNotifications.
        /// </summary>
        public KeepAliveEventHandler KeepAliveNotification = null;

        #endregion

        #region Discovery
        /// <summary>Finds Servers based on a discovery url</summary>
        /// <param name="discoveryUrl">The discovery url</param>
        /// <returns>ApplicationDescriptionCollection containing found servers</returns>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public ApplicationDescriptionCollection FindServers(string discoveryUrl)
        {
            //Create a URI using the discovery URL
            Uri uri = new Uri(discoveryUrl);
            try
            {
                //Ceate a DiscoveryClient
                DiscoveryClient client = DiscoveryClient.Create(uri);
                //Find servers
                ApplicationDescriptionCollection servers = client.FindServers(null);
                return servers;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        /// <summary>Finds Endpoints based on a server's url</summary>
        /// <param name="discoveryUrl">The server's url</param>
        /// <returns>EndpointDescriptionCollection containing found Endpoints</returns>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public EndpointDescriptionCollection GetEndpoints(string serverUrl)
        {
            //Create a URI using the server's URL
            Uri uri = new Uri(serverUrl);
            try
            {
                //Create a DiscoveryClient
                DiscoveryClient client = DiscoveryClient.Create(uri);
                //Search for available endpoints
                EndpointDescriptionCollection endpoints = client.GetEndpoints(null);
                return endpoints;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }
        #endregion

        #region Connect/Disconnect
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="url"></param>
        /// <param name="security"></param>
        /// <param name="userIdentity"></param>
        public void Connect(string url, bool security, UserIdentity userIdentity)
        {
            try
            {
                //Secify application configuration
                ApplicationConfiguration ApplicationConfig = mApplicationConfig;

                //Hook up a validator function for a CertificateValidation event
                mApplicationConfig.CertificateValidator.CertificateValidation += Validator_CertificateValidation;

                //Create EndPoint description
                EndpointDescription EndpointDescription = CreateEndpointDescription(url, security);

                //Create EndPoint configuration
                EndpointConfiguration EndpointConfiguration = EndpointConfiguration.Create(ApplicationConfig);

                //Create an Endpoint object to connect to server
                ConfiguredEndpoint Endpoint = new ConfiguredEndpoint(null, EndpointDescription, EndpointConfiguration);

                //Create anonymous user identity
                //UserIdentity UserIdentity = new UserIdentity();                


                //Create and connect session
                mSession = Session.Create(
                    ApplicationConfig,
                    Endpoint,
                    true,
                    "MySession",
                      60000,
                    userIdentity,
                    null
                    );

                mSession.KeepAlive += new KeepAliveEventHandler(Notification_KeepAlive);

            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="endpointDescription"></param>
        /// <param name="userIdentity"></param>
        public void Connect(EndpointDescription endpointDescription, UserIdentity userIdentity)
        {
            try
            {
                //Secify application configuration
                ApplicationConfiguration ApplicationConfig = mApplicationConfig;

                //ApplicationConfig.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;

                //Hook up a validator function for a CertificateValidation event
                ApplicationConfig.CertificateValidator.CertificateValidation += Validator_CertificateValidation;

                //Create EndPoint configuration
                EndpointConfiguration EndpointConfiguration = EndpointConfiguration.Create(ApplicationConfig);

                //Connect to server and get endpoints
                ConfiguredEndpoint mEndpoint = new ConfiguredEndpoint(null, endpointDescription, EndpointConfiguration);

                //Create the binding factory.
                BindingFactory bindingFactory = BindingFactory.Create(mApplicationConfig, ServiceMessageContext.GlobalContext);

                //Create anonymous user identity
                //UserIdentity UserIdentity = new UserIdentity();

                //Create and connect session
                mSession = Session.Create(
                    ApplicationConfig,
                    mEndpoint,
                    true,
                    "MySession",
                    60000,
                    userIdentity,
                    null
                    );

                mSession.KeepAlive += new KeepAliveEventHandler(Notification_KeepAlive);
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }


        /// <summary>
        /// 连接,mqg于20180914增加，增加PLC会话超时时间
        /// </summary>
        /// <param name="url"></param>
        /// <param name="security"></param>
        /// <param name="userIdentity"></param>
        /// <param name="timeOverMinutes">会话超时分钟数,单位分钟</param>
        public void Connect(string url, bool security, UserIdentity userIdentity, int timeOverMinutes)
        {
            try
            {
                //Secify application configuration
                ApplicationConfiguration ApplicationConfig = mApplicationConfig;

                //Hook up a validator function for a CertificateValidation event
                mApplicationConfig.CertificateValidator.CertificateValidation += Validator_CertificateValidation;

                //Create EndPoint description
                EndpointDescription EndpointDescription = CreateEndpointDescription(url, security);

                //Create EndPoint configuration
                EndpointConfiguration EndpointConfiguration = EndpointConfiguration.Create(ApplicationConfig);

                //Create an Endpoint object to connect to server
                ConfiguredEndpoint Endpoint = new ConfiguredEndpoint(null, EndpointDescription, EndpointConfiguration);

                //Create anonymous user identity
                //UserIdentity UserIdentity = new UserIdentity();                


                //Create and connect session             
                mSession = Session.Create(
                    ApplicationConfig,
                    Endpoint,
                    true,
                    "MySession",
                     Convert.ToUInt32(timeOverMinutes * 60000),
                    userIdentity,
                    null
                    );

                mSession.KeepAlive += new KeepAliveEventHandler(Notification_KeepAlive);

            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        /// <summary>
        /// 连接, mqg于20180914增加，增加PLC会话超时时间
        /// </summary>
        /// <param name="endpointDescription"></param>
        /// <param name="userIdentity"></param>
        /// <param name="timeOverMinutes"></param>
        public void Connect(EndpointDescription endpointDescription, UserIdentity userIdentity, int timeOverMinutes)
        {
            try
            {
                //Secify application configuration
                ApplicationConfiguration ApplicationConfig = mApplicationConfig;

                //ApplicationConfig.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;

                //Hook up a validator function for a CertificateValidation event
                ApplicationConfig.CertificateValidator.CertificateValidation += Validator_CertificateValidation;

                //Create EndPoint configuration
                EndpointConfiguration EndpointConfiguration = EndpointConfiguration.Create(ApplicationConfig);

                //Connect to server and get endpoints
                ConfiguredEndpoint mEndpoint = new ConfiguredEndpoint(null, endpointDescription, EndpointConfiguration);

                //Create the binding factory.
                BindingFactory bindingFactory = BindingFactory.Create(mApplicationConfig, ServiceMessageContext.GlobalContext);

                //Create anonymous user identity
                //UserIdentity UserIdentity = new UserIdentity();

                //Create and connect session              
                mSession = Session.Create(
                    ApplicationConfig,
                    mEndpoint,
                    true,
                    "MySession",
                    Convert.ToUInt32(timeOverMinutes * 60000),
                    userIdentity,
                    null
                    );

                mSession.KeepAlive += new KeepAliveEventHandler(Notification_KeepAlive);
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }


        /// <summary>Closes an existing session and disconnects from the server.</summary>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public void Disconnect()
        {
            // Close the session.
            try
            {
                mSession.Close(10000);
                mSession.Dispose();
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Autofac;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using MMO.Framework;
using MMO.PhotonFramework.Server;
using Photon.SocketServer;
using log4net;
using log4net.Config;
using LogManager = ExitGames.Logging.LogManager;
using MMO.PhotonFramework.Client;
using Photon.SocketServer.ServerToServer;

namespace MMO.PhotonFramework.Application
{
    public abstract class PhotonApplication : ApplicationBase
    {
        public abstract byte SubCodeParameterKey { get; }
        public PhotonConnectionCollection ConnectionCollection { get; private set; }

        public static readonly Guid ServerId = Guid.NewGuid();
        protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public abstract IPEndPoint MasterEndPoint { get; }
        public abstract int? TcpPoint { get; }
        public abstract int? UdpPort { get; }
        public abstract IPAddress PublicIpAdress { get; }
        public abstract int ServerType { get; }

        protected abstract int ConnectRetryIntervalSeconds { get; }
        protected abstract bool ConnectsToMaster { get; }

        private static PhotonServerPeer _masterPeer;
        private byte _isReconnecting;
        private Timer _retry;

        private PhotonPeerFactory _factory;
        private IEnumerable<IBackgroundThread> _backgroundThreads;


        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return _factory.CreatePeer(initRequest);
        }

        protected override void Setup()
        {
            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["LogFileName"] = ApplicationName;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(BinaryPath, "log4net.config")));

            var builder = new ContainerBuilder();

            Initialize(builder);

            var container = builder.Build();

            _factory = container.Resolve<PhotonPeerFactory>();
            ConnectionCollection = container.Resolve<PhotonConnectionCollection>();
            _backgroundThreads = container.Resolve<IEnumerable<IBackgroundThread>>();

            ResolveParameters(container);

            foreach (var backgroundthread in _backgroundThreads)
            {
                backgroundthread.Setup();
                ThreadPool.QueueUserWorkItem(backgroundthread.Run);
            }

            if (ConnectsToMaster)
            {
                ConnectToMaster();
            }
        }

        protected void Initialize(ContainerBuilder builder)
        {
            builder.RegisterType<PhotonPeerFactory>();
            builder.RegisterType<PhotonServerPeer>();
            builder.RegisterType<PhotonClientPeer>();
            builder.RegisterType<PhotonClientHandlerList>();
            builder.RegisterType<PhotonServerHandlerList>();

            RegisterContainerObjects(builder);
        }

        protected override void TearDown()
        {
        }

        protected override void OnStopRequested()
        {
            foreach (var backgroundThread in _backgroundThreads)
            {
                backgroundThread.Stop();
            }

            foreach (KeyValuePair<Guid, PhotonServerPeer> photonServerPeer in ConnectionCollection.Servers)
            {
                photonServerPeer.Value.Disconnect();
            }

            foreach (KeyValuePair<Guid, PhotonClientPeer> photonClientPeer in ConnectionCollection.Clients)
            {
                photonClientPeer.Value.Disconnect();
            }

            base.OnStopRequested();
        }

        public void ConnectToMaster()
        {
            if (ConnectToServer(MasterEndPoint, "Master", "Master") == false)
            {
                Log.Warn("master connection refused");
                return;
            }

            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat(_isReconnecting == 0 ? "Connection to master at {0}" : "Reconnected to master at {0}", MasterEndPoint);
            }
        }

        protected override void OnServerConnectionFailed(int errorCode, string errorMessage, object state)
        {
            if (_isReconnecting == 0)
            {
                Log.ErrorFormat("Master connection failed with error {0} : {1}", errorCode, errorMessage);
            }
            else if (Log.IsDebugEnabled)
            {
                Log.ErrorFormat("Master connection failed with error {0} : {1}", errorCode, errorMessage);
            }

            string stateString = state as string;
            if (state != null && stateString.Equals("Master"))
            {
                ReconnectToMaster();
            }
        }

        public void ReconnectToMaster()
        {
            Thread.VolatileWrite(ref _isReconnecting, 1);
            _retry = new Timer(o => ConnectToMaster(), null, ConnectRetryIntervalSeconds * 1000, 0);
        }

        protected override ServerPeerBase CreateServerPeer(InitResponse initResponse, object state)
        {
            return _factory.CreatePeer(initResponse);
        }

        protected abstract void RegisterContainerObjects(ContainerBuilder builder);
        protected abstract void ResolveParameters(IContainer container);

        public abstract void Register(PhotonServerPeer peer);
    }
}

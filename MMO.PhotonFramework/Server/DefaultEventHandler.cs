using MMO.PhotonFramework.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.PhotonFramework.Server
{
    public abstract class DefaultEventHandler : PhotonServerHandler
    {
        public DefaultEventHandler(PhotonApplication application) : base(application)
        {
        }

    }
}

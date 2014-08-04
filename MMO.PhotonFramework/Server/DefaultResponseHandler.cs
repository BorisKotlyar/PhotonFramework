using MMO.PhotonFramework.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.PhotonFramework.Server
{
    public abstract class DefaultResponseHandler : PhotonServerHandler
    {
        protected DefaultResponseHandler(PhotonApplication application) : base (application)
        {

        }
    }
}

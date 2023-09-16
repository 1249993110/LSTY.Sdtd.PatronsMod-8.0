using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace LSTY.Sdtd.PatronsMod.SignalR
{
    public class ApiKeyAuthorizeHubConnection : IAuthorizeHubConnection
    {
        public bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            var token = request.Headers.Get("access-token");
            if (token == AppSettings.AccessToken)
            {
                return true;
            }

            token = request.QueryString.Get("access-token");
            if (token == AppSettings.AccessToken)
            {
                return true;
            }

            return false;
        }
    }
}
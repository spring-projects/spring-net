using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.ServiceModel.Activation;

namespace Spring.RestSilverlightQuickStart.Services
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service // : IService
    {
        private IDictionary<string, string> users;

        public Service()
        {
            users = new Dictionary<string, string>();
            users.Add("1", "Bruno Baïa");
            users.Add("2", "Marie Baia");
        }

        [OperationContract]
        [WebGet(UriTemplate = "user/{id}")]
        public Message GetUser(string id)
        {
            WebOperationContext context = WebOperationContext.Current;

            if (!users.ContainsKey(id))
            {
                context.OutgoingResponse.SetStatusAsNotFound(String.Format("User with id '{0}' not found", id));
                return context.CreateTextResponse(null);
            }

            return context.CreateTextResponse(users[id]);
        }

        [OperationContract]
        [WebGet(UriTemplate = "users")]
        public Message GetUsersCount()
        {
            WebOperationContext context = WebOperationContext.Current;

            return context.CreateTextResponse(users.Count.ToString());
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "user", Method = "POST")]
        public Message Post(Stream stream)
        {
            WebOperationContext context = WebOperationContext.Current;

            UriTemplateMatch match = context.IncomingRequest.UriTemplateMatch;
            UriTemplate template = new UriTemplate("/user/{id}");

            string id = (users.Count + 1).ToString(); // generate new ID
            string name;
            using (StreamReader reader = new StreamReader(stream))
            {
                name = reader.ReadToEnd();
            }

            if (String.IsNullOrEmpty(name))
            {
                context.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                context.OutgoingResponse.StatusDescription = "Content cannot be null or empty";
                return WebOperationContext.Current.CreateTextResponse("");
            }

            users.Add(id, name);

            Uri uri = template.BindByPosition(match.BaseUri, id);
            context.OutgoingResponse.SetStatusAsCreated(uri);
            context.OutgoingResponse.StatusDescription = String.Format("User id '{0}' created with '{1}'", id, name);

            return WebOperationContext.Current.CreateTextResponse(id);
        }
    }
}

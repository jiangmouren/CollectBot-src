using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System.Net.Http;
using System.Web.Http.Description;
using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System;
using System.Collections.Generic;
using ResolverLibrary.Interface.Request;
using Newtonsoft.Json;
using System.Reflection;
using FormBot.Models;
using System.Threading;
using System.Collections.Concurrent;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Microsoft.Bot.Sample.FormBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        delegate void Del(GMSContext context);

        internal static IDialog<SandwichOrder> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(SandwichOrder.BuildForm));
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                //ConcurrentQueue<Vector> optionVectors = new ConcurrentQueue<Vector>();
                //await preProcessOptions(optionVectors);
                //Dictionary<string, double> inputVector = await preProcessInput(activity);
                //List<Vector> optionVectorList = new List<Vector>();
                //optionVectorList.AddRange(optionVectors);
                //int index = FindMax(optionVectorList, inputVector);
                //SandwichOptions result = (SandwichOptions)Enum.GetValues(typeof(SandwichOptions)).GetValue(index);
                // one of these will have an interface and process it
                await CallGMS1(activity);
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, MakeRootDialog);
                        break;

                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private async Task CallGMS1(Activity activity)
        {
            if (activity.Text.ToLower()!="hi" && activity.Text.ToLower()!="hello" && activity.Text.ToLower()!="yes" && activity.Text.ToLower()!="no")
            {
                Dictionary<string, double> inputVector = await preProcessInput(activity);
                var list = inputVector.ToList();
                list.Sort((entry0, entry1) => {
                    if ((entry0.Value - entry1.Value) == 0)
                    {
                        return 0;
                    }
                    else if ((entry0.Value - entry1.Value) > 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });
                activity.Text = list[0].Key;
            }
        }

        private async Task CallGMS2(Activity activity)
        {

        }

        private int FindMax(List<Vector> optionVectors, Vector inputVector)
        {
            double similarity = double.MinValue;
            int index = 0;
            for (int i=0; i<optionVectors.Count; i++)
            {
                double current = Vector.GetCosineSimilarity(optionVectors[i], inputVector);
                if (current > similarity)
                {
                    similarity = current;
                    index = i;
                }
            }
            return index;
        }

        /// <summary>
        /// A bridge method for testing purpose to call GMS resolvers to pre-process user input. 
        /// </summary>
        /// <param name="activity"></param>
        private async Task preProcessOptions(ConcurrentQueue<Vector> optionVectors)
        {
            List<string> categories = new List<string> { "cold", "hot", "chicken", "beef", "fish", "onion" };
            string api = "http://generic-resolver-prod-flight.trafficmanager.net/api/servicebotlet/semantic-categorization";
            //string api = "http://c113e841.ngrok.io/api/servicebotlet/semantic-categorization";
            //ThreadPool.SetMaxThreads(1, 1);
            foreach (ProductCategories option in Enum.GetValues(typeof(ProductCategories)))
            {
                //iterate through all sandwichoptions and get score vector for each.
                string description = option.Description().ToLower();
                var request = new SemanticCategorizationRequest(description, categories);
                string content = JsonConvert.SerializeObject(request);
                //WaitCallback callBack = gmsPost;
                //TODO: thinking about build wrapper void method to wrap around gmsPost
                //string content = "{ \"categories\": [ \"cold sandwich\", \"hot sandwich\", \"chicken sandwich\", \"beef sandwich\", \"fish sandwich\", \"Onion sandwich\" ], \"query\": \"Ham\" }";
                GMSContext context = new GMSContext(api, null, content, optionVectors);
                //bool status = ThreadPool.QueueUserWorkItem(callBack, context);
                //string result = await gmsPost(context);
                await gmsOptionsPost(context);
            }
        }

        private async Task<Dictionary<string, double>> preProcessInput(Activity activity)
        {
            //List<string> categories = new List<string> { "cold", "hot", "chicken", "beef", "fish", "onion" };
            List<string> categories = new List<string> { "Auto And Tires" , "Baby" , "Books" , "Cell Phones" , "Clothing" , "Electronics" , "Home Improvement" , "Jewelry" , "Office" , "Pets" , "Pharmacy" , "Sports And Outdoors" };
            string api = "http://generic-resolver-prod-flight.trafficmanager.net/api/servicebotlet/semantic-categorization";
            //ThreadPool.SetMaxThreads(1, 1);
                //iterate through all sandwichoptions and get score vector for each.
            var request = new SemanticCategorizationRequest(activity.Text, categories);
            string content = JsonConvert.SerializeObject(request);
            string response = await gmsPost(api, null, content);
            if (response != null)
            {
                return GetVector(response);
            }
            return null;
        }


        private async Task gmsOptionsPost(object context)
        {
            if (!context.GetType().Equals(typeof(GMSContext)))
            {
                throw new Exception();
            }

            string endpoint = ((GMSContext)context).endpoint;
            string action = ((GMSContext)context).action;
            string requestContent = ((GMSContext)context).requestContent;

            string response = await gmsPost(endpoint, action, requestContent);
            if (response != null)
            {
                //((GMSContext)context).optionVectors.Enqueue(GetVector(response));
            }

        }

        private Dictionary<string, double> GetVector(string response)
        {
            JObject jObject = JObject.Parse(response);
            var result = jObject.GetValue("CategorizationResults");
            Dictionary<string, double> scores = new Dictionary<string, double>();
            //List<double> scores = new List<double>();
            foreach (var token in result)
            {
                double score = (double)token.Last.Last;
                string name = (string)token.First.Last;
                scores.Add(name, score);
            }
            return scores;
        }

        private async Task<string> gmsPost(string endpoint, string action, string requestContent)
        {
            AuthenticationResult auth = GetAuthentication(); 
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Set(HttpRequestHeader.ContentType, "application/json");
            client.Headers.Add(HttpRequestHeader.Authorization, auth.CreateAuthorizationHeader());
            //client.Headers.Add("client-request-id", Guid.NewGuid().ToString());
            string response = null;
            try
            {
                response = await client.UploadStringTaskAsync(endpoint, "POST", requestContent);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

        private static AuthenticationResult GetAuthentication()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var authority = appSettings["ResolverService_Auth"];
            var resource = appSettings["ResolverService_Resource"];
            var clientId = appSettings["ResolverService_ClientId"];
            var clientSecret = appSettings["ResolverService_ClientSecret"];
            var clientCredential = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(authority, false);
            return context.AcquireTokenAsync(resource, clientCredential).Result;
        }
    }
}
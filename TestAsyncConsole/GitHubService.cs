using Newtonsoft.Json.Linq;
using Octokit;
using System.Runtime.CompilerServices;

namespace TestAsyncConsole
{
    public class GitHubService
    {
        // https://docs.github.com/en/graphql/reference/objects#pullrequestconnection
        private const string PagedPrQuery =
            @"query ($owner: String!, $repo_name: String!, $start_cursor:String) {
              repository(owner: $owner, name: $repo_name) {
                pullRequests(last: 1, before: $start_cursor)
                 {
                    totalCount
                    pageInfo {
                      hasPreviousPage
                      startCursor
                    }
                    nodes {
                      title
                      number
                      createdAt
                    }
                  }
                }
              }
            ";

        private readonly string _token;
        private readonly GitHubClient _githubClient;

        public GitHubService(string token)
        {
            _token = token;
            _githubClient = new GitHubClient(new Octokit.ProductHeaderValue("PrQueryDemo"))
            {
                Credentials = new Octokit.Credentials(_token)
            };
        }

        public async IAsyncEnumerable<JToken> GetLatestPrsAsync(int count, string repoOwner, string repoName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Console.WriteLine("GetLatestPrsAsync() - Start");
            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
            Console.WriteLine();

            var prQuery = new GraphQLRequest
            {
                Query = PagedPrQuery
            };
            prQuery.Variables["owner"] = repoOwner;
            prQuery.Variables["repo_name"] = repoName;

            bool hasMorePages = true;
            int pagesReturned = 0;
            int prsReturned = 0;

            while (hasMorePages && (pagesReturned++ < count))
            {
                var postBody = prQuery.ToJsonText();

                Console.WriteLine("Calling \"_githubClient.Connection.Post()\"");
                Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);

                var responseTask = _githubClient.Connection.Post<string>(new Uri("https://api.github.com/graphql"), postBody, "application/json", "application/json", cancellationToken: cancellationToken);
                var response = await responseTask;
                //var response = responseTask.Result;

                Console.WriteLine("Finished \"_githubClient.Connection.Post()\"");
                Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
                Console.WriteLine();

                JObject results = JObject.Parse(response.HttpResponse.Body.ToString()!);

                int totalCount = (int)pullRequests(results)["totalCount"]!;
                hasMorePages = (bool)pageInfo(results)["hasPreviousPage"]!;
                prQuery.Variables["start_cursor"] = pageInfo(results)["startCursor"]!.ToString();
                prsReturned += pullRequests(results)["nodes"]!.Count();

                foreach (JObject pr in pullRequests(results)["nodes"]!.Cast<JObject>())
                {
                    yield return pr;
                }   
            }

            JObject pullRequests(JObject result) => (JObject)result["data"]!["repository"]!["pullRequests"]!;
            JObject pageInfo(JObject result) => (JObject)pullRequests(result)["pageInfo"]!;

            Console.WriteLine("Thread Id: {0}", Environment.CurrentManagedThreadId);
            Console.WriteLine("GetLatestPrsAsync() - End");
            Console.WriteLine();
        }
    }
}

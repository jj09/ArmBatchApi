using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ArmBatchApiClient.Models;
using ArmBatchApiClient.Services;

namespace ArmBatchApiClient
{
    class Program
    {
        private static string[] _resourceIds = {
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestUS/providers/microsoft.insights/components/azuremobileportal",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/azureportal/providers/microsoft.insights/components/azuretipsandtricks",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/azureportal/providers/Microsoft.Web/sites/azuretipsandtricks",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Storage-WestUS/providers/Microsoft.ClassicStorage/storageAccounts/bitnamiwestus5214430498",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestUS/providers/microsoft.insights/components/bookslib",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestUS/providers/Microsoft.Web/sites/BooksLib",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestUS/providers/Microsoft.Web/sites/BooksLib/providers/Microsoft.ResourceHealth/availabilityStatuses/current",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/cloud-shell-storage-southcentralus/providers/Microsoft.Storage/storageAccounts/cs785f2f4351b0fx431bxa81",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestEurope/providers/Microsoft.Web/serverFarms/Default1",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestUS/providers/Microsoft.Web/serverFarms/Default1",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestUS/providers/Microsoft.Web/serverFarms/Default2",
            "/subscriptions/FAKE_SUBSRIPTION_ID/resourceGroups/Default-Web-WestEurope/providers/microsoft.insights/components/dotnetconf"
        };

        private const string ArmToken = "Bearer FAKETOKEN";

        private static ArmService _armService;

        private const int BatchSize = 20;
        private const int BatchDelayMs = 50;

        static void Main(string[] args)
        {
            _armService = new ArmService(ArmToken, BatchSize, BatchDelayMs);

            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var resources = new ConcurrentBag<Resource>();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"Start requesting resource details...");
            Console.WriteLine($"Number of requests to be sent: {_resourceIds.Count()}");
            Console.WriteLine("--------------------------------------");

            var tasks = _resourceIds.Select(async resourceId =>
            {
                await Task.Delay(new Random().Next() % 5000);   // simulate calling GetResource from different parts of UI
                var response = await _armService.GetResource(resourceId);
                resources.Add(response);
            });

            await Task.WhenAll(tasks);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"Responses received: {resources.Count}");
            Console.WriteLine($"Number of batch requests sent: {_armService.BatchRequestsCount}");
            Console.WriteLine("--------------------------------------");
        }
    }
}

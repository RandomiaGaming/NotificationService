using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace NotificationServiceExample
{
    public static class Program
    {
        private sealed class StatusJsonSchema
        {
            public string status;
        }
        private sealed class ShowNotificationJsonSchema
        {
            public string title = "";
            public string message = "";
            public string iconPath = "";
        }
        public static void Main(string[] args)
        {
            // Start NotificationService on a random port
            Process notificationService = Process.Start("D:\\ImportantData\\School\\NotificationService\\NotificationService\\bin\\Release\\NotificationService.exe");
            notificationService.WaitForExit();
            int port = notificationService.ExitCode;
            // Prepare input for the request
            ShowNotificationJsonSchema input = new ShowNotificationJsonSchema();
            input.title = "Test Notification";
            input.message = "Hello world this is a test notification.";
            input.iconPath = "C:\\Users\\RandomiaGaming\\Desktop\\Icon.png";
            string inputJson = JsonConvert.SerializeObject(input);
            // Send the request
            HttpClient httpClient = new HttpClient();
            string url = $"http://localhost:{port}/ShowNotification";
            HttpContent content = new StringContent(inputJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = httpClient.PostAsync(url, content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            string outputJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            // Parse output and ensure status OK
            StatusJsonSchema output = JsonConvert.DeserializeObject<StatusJsonSchema>(outputJson);
            if (output.status.ToLower() != "OK".ToLower())
            {
                throw new Exception(output.status);
            }
        }
    }
}
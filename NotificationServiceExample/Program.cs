using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace NotificationServiceExample
{
    public sealed class NotificationServiceClient
    {
        // Private Variables
        private int Port = -1;
        private HttpClient httpClient = null;
        // Constructor/Destructor
        public NotificationServiceClient(int port)
        {
            Port = port;
            httpClient = new HttpClient();
        }
        ~NotificationServiceClient()
        {
            httpClient.Dispose();
        }
        // Internal HTTP post helper method
        private string SendRequest(string endpoint, string inputJson)
        {
            string url = $"http://localhost:{Port}/{endpoint}";
            HttpContent content = new StringContent(inputJson, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = httpClient.PostAsync(url, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                return "{\"status\":\"" + ex.Message + "\"}";
            }
        }
        // Json Schemas
        private sealed class BlankJsonSchema
        {

        }
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
        // Functions
        public void Check()
        {
            BlankJsonSchema input = new BlankJsonSchema();
            string inputJson = JsonConvert.SerializeObject(input);
            string outputJson = SendRequest("Check", inputJson);
            StatusJsonSchema output = JsonConvert.DeserializeObject<StatusJsonSchema>(outputJson);
            if (output.status.ToLower() != "OK".ToLower())
            {
                throw new Exception(output.status);
            }
        }
        public void Exit()
        {
            BlankJsonSchema input = new BlankJsonSchema();
            string inputJson = JsonConvert.SerializeObject(input);
            string outputJson = SendRequest("Exit", inputJson);
            StatusJsonSchema output = JsonConvert.DeserializeObject<StatusJsonSchema>(outputJson);
            if (output.status.ToLower() != "OK".ToLower())
            {
                throw new Exception(output.status);
            }
        }
        public void ShowNotification(string title, string message, string iconPath)
        {
            ShowNotificationJsonSchema input = new ShowNotificationJsonSchema();
            input.title = title;
            input.message = message;
            input.iconPath = iconPath;
            string inputJson = JsonConvert.SerializeObject(input);
            string outputJson = SendRequest("ShowNotification", inputJson);
            StatusJsonSchema output = JsonConvert.DeserializeObject<StatusJsonSchema>(outputJson);
            if (output.status.ToLower() != "OK".ToLower())
            {
                throw new Exception(output.status);
            }
        }

        // Constant path to NotificationService.exe
        private const string NotificationServicePath = "D:\\ImportantData\\School\\NotificationService\\bin\\Debug\\NotificationService.exe";
        // Static methods for launching NotificationService.exe
        public static int Launch()
        {
            Process notificationService = Process.Start(NotificationServicePath);
            notificationService.WaitForExit(15000);
            if (!notificationService.HasExited)
            {
                throw new Exception("Timeout while waiting for NotificationService.exe.");
            }
            if (notificationService.ExitCode <= 0)
            {
                throw new Exception("Failed to launch NotificationService.exe.");
            }
            else
            {
                return notificationService.ExitCode;
            }
        }
        public static void Launch(int port)
        {
            Process.Start(NotificationServicePath, "/port " + port.ToString());
        }
    }
    public static class Program
    {
        public static void Main(string[] args)
        {
            int port = NotificationServiceClient.Launch();
            NotificationServiceClient notificationService = new NotificationServiceClient(port);
            notificationService.Check();
            notificationService.ShowNotification("Test", "Hello World", "C:\\Users\\RandomiaGaming\\Desktop\\Fox.jpg");
            notificationService.Exit();
        }
    }
}
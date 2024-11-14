﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;

namespace NotificationService
{
    public static class Program
    {
        // Json Schemas
        public sealed class BlankJsonSchema
        {

        }
        public sealed class StatusJsonSchema
        {
            public string status;
        }
        public sealed class ShowNotificationJsonSchema
        {
            public string title = "";
            public string message = "";
            public string iconPath = "";
        }
        // Functions
        public static string Check(string inputJson)
        {
            BlankJsonSchema input = JsonConvert.DeserializeObject<BlankJsonSchema>(inputJson);
            StatusJsonSchema output = new StatusJsonSchema();
            output.status = "OK";
            return JsonConvert.SerializeObject(output);
        }
        public static string Exit(string inputJson)
        {
            BlankJsonSchema input = JsonConvert.DeserializeObject<BlankJsonSchema>(inputJson);
            ExitRequested = true;
            StatusJsonSchema output = new StatusJsonSchema();
            output.status = "OK";
            return JsonConvert.SerializeObject(output);
        }
        public static string ShowNotification(string inputJson)
        {
            ShowNotificationJsonSchema input = JsonConvert.DeserializeObject<ShowNotificationJsonSchema>(inputJson);
            ToastContentBuilder toastContentBuilder = new ToastContentBuilder();
            if (input.title != null)
            {
                toastContentBuilder.AddText(input.title);
            }
            if (input.message != null)
            {
                toastContentBuilder.AddText(input.message);
            }
            if (input.iconPath != null)
            {
                toastContentBuilder.AddAppLogoOverride(new Uri(input.iconPath));
            }
            toastContentBuilder.Show();
            StatusJsonSchema output = new StatusJsonSchema();
            output.status = "OK";
            return JsonConvert.SerializeObject(output);
        }
        // Status Variables
        public static int Port = -1;
        public static bool ExitRequested = false;
        // Main Helper Functions
        public static int PortFromArgs(string[] args)
        {
            int output = -1;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "/port")
                {
                    if (args.Length == i + 1 || !int.TryParse(args[i + 1], out output) || output < -1)
                    {
                        throw new Exception($"Invalid port specified.");
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    throw new Exception($"Invalid argument \"{args[i]}\".");
                }
            }
            return output;
        }
        public static int GetFreePort()
        {
            int output = -1;
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            output = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return output;
        }
        public static string ProcessMessage(string endpoint, string inputJson)
        {
            endpoint = endpoint.ToLower();
            if (endpoint == "/Check".ToLower())
            {
                return Check(inputJson);
            }
            else if (endpoint == "/Exit".ToLower())
            {
                return Exit(inputJson);
            }
            else if (endpoint == "/ShowNotification".ToLower())
            {
                return ShowNotification(inputJson);
            }
            else
            {
                return "{\"status\":\"Bad endpoint.\"}";
            }
        }
        public static void RunOnPort(int port)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            Console.WriteLine($"Started server on port {Port}...");

            while (!ExitRequested)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    if (request.HttpMethod == "POST")
                    {
                        string outputJson;
                        try
                        {
                            StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
                            string inputJson = reader.ReadToEnd();
                            reader.Dispose();

                            string endpoint = request.Url.AbsolutePath;

                            outputJson = ProcessMessage(endpoint, inputJson);
                        }
                        catch (Exception ex)
                        {
                            StatusJsonSchema output = new StatusJsonSchema();
                            output.status = ex.Message;
                            outputJson = JsonConvert.SerializeObject(output);
                        }

                        HttpListenerResponse response = context.Response;
                        byte[] buffer = Encoding.UTF8.GetBytes(outputJson);
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = "application/json";
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        public static int Main(string[] args)
        {
            try
            {
                Port = PortFromArgs(args);
                if (Port == -1)
                {
                    Console.WriteLine($"No port given. Selecting random...");
                    Port = GetFreePort();
                    Console.WriteLine($"Selected port {Port}. Relaunching...");
                    Process.Start(typeof(Program).Assembly.Location, $"/port {Port}");
                    return Port;
                }
                else
                {
                    Console.WriteLine($"Starting server on port {Port}...");
                    RunOnPort(Port);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }
    }
}

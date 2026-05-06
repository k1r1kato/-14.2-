using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SimpleApiServer
{
    class Program
    {
        private static List<TaskItem> _tasks = new List<TaskItem>();
        private static int _nextId = 1;
        private static AppConfig _config;

        static void Main(string[] args)
        {
            // Чтение конфигурации
            try
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                _config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(configPath));
                Logger.LogFilePath = _config.LogFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка конфига. Завершение.");
                return;
            }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(_config.ListenUrl);

            try
            {
                listener.Start();
                Logger.LogInfo($"Сервер запущен: {_config.ListenUrl}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка запуска", ex);
                return;
            }

            while (true)
            {
                var context = listener.GetContext();
                var req = context.Request;
                var res = context.Response;

                Logger.LogInfo($"{req.HttpMethod} {req.Url.LocalPath}"); 

                try
                {
                    if (req.Url.LocalPath.TrimEnd('/') == "/api/tasks" && req.HttpMethod == "POST")
                    {
                        HandlePost(req, res);
                    }
                    else
                    {
                        SendJson(res, new { error = "Not Found" }, 404);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Ошибка сервера", ex);
                    SendJson(res, new { error = "Internal Error" }, 500);
                }
            }
        }

        static void HandlePost(HttpListenerRequest req, HttpListenerResponse res)
        {
            using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
            var data = JsonSerializer.Deserialize<TaskItem>(reader.ReadToEnd());

            if (string.IsNullOrEmpty(data?.Title))
            {
                Logger.LogError("Валидация провалена: пустое название");
                SendJson(res, new { error = "Title is required" }, 400);
                return;
            }

            data.Id = _nextId++;
            _tasks.Add(data);
            Logger.LogInfo($"Задача {data.Id} создана");
            SendJson(res, data, 201);
        }

        static void SendJson(HttpListenerResponse res, object data, int code)
        {
            res.StatusCode = code;
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }
    }
}
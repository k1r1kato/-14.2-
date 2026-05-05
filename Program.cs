using System.Net;
using System.Text;
using System.Text.Json;

List<TaskItem> tasks = new List<TaskItem>();
int nextId = 1;

HttpListener listener = new HttpListener();
listener.Prefixes.Add("http://localhost:5000/");
listener.Start();

Console.WriteLine("Server started on http://localhost:5000");

while (true)
{
    var context = listener.GetContext();
    var request = context.Request;
    var response = context.Response;

    if (request.HttpMethod == "GET" && request.Url!.AbsolutePath == "/api/tasks")
    {
        string json = JsonSerializer.Serialize(tasks);
        WriteJson(response, json, 200);
    }

    else if (request.HttpMethod == "POST" && request.Url!.AbsolutePath == "/api/tasks")
    {
        string body;

        using (var reader = new StreamReader(request.InputStream))
        {
            body = reader.ReadToEnd();
        }

        var data = JsonSerializer.Deserialize<CreateTaskRequest>(body);

        if (data == null || string.IsNullOrWhiteSpace(data.Title))
        {
            WriteJson(response, "{\"error\":\"Title required\"}", 400);
            return;
        }

        var task = new TaskItem
        {
            Id = nextId++,
            Title = data.Title,
            Description = data.Description,
            Priority = data.Priority ?? 1,
            IsCompleted = data.IsCompleted ?? false
        };

        tasks.Add(task);

        WriteJson(response, JsonSerializer.Serialize(task), 201);
    }

    else
    {
        WriteJson(response, "{\"error\":\"Not found\"}", 404);
    }
}

void WriteJson(HttpListenerResponse response, string json, int status)
{
    byte[] buffer = Encoding.UTF8.GetBytes(json);

    response.StatusCode = status;
    response.ContentType = "application/json";
    response.ContentLength64 = buffer.Length;

    using var stream = response.OutputStream;
    stream.Write(buffer, 0, buffer.Length);
}
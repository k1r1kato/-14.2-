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

    try
    {
        if (request.HttpMethod == "GET" && request.Url!.AbsolutePath == "/api/tasks")
        {
            WriteJson(response, JsonSerializer.Serialize(tasks), 200);
        }

        else if (request.HttpMethod == "POST" && request.Url!.AbsolutePath == "/api/tasks")
        {
            HandleCreateTask(request, response);
        }

        else if (request.HttpMethod == "PUT" && request.Url!.AbsolutePath.StartsWith("/api/tasks/"))
        {
            HandleUpdateTask(request, response);
        }

        else
        {
            WriteJson(response, "{\"error\":\"Not found\"}", 404);
        }
    }
    catch (Exception ex)
    {
        WriteJson(response, JsonSerializer.Serialize(new { error = ex.Message }), 500);
    }
}

void HandleCreateTask(HttpListenerRequest request, HttpListenerResponse response)
{
    string body;

    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
    {
        body = reader.ReadToEnd();
    }

    CreateTaskRequest? data;

    try
    {
        data = JsonSerializer.Deserialize<CreateTaskRequest>(body);
    }
    catch
    {
        WriteJson(response, "{\"error\":\"Некорректный JSON\"}", 400);
        return;
    }

    var errors = new List<object>();

    if (string.IsNullOrWhiteSpace(data?.Title))
        errors.Add(new { field = "Title", message = "Название обязательно" });

    else if (data.Title.Length > 200)
        errors.Add(new { field = "Title", message = "Название не более 200 символов" });

    if (data?.Priority is int p && (p < 1 || p > 5))
        errors.Add(new { field = "Priority", message = "Приоритет от 1 до 5" });

    if (data?.Description?.Length > 1000)
        errors.Add(new { field = "Description", message = "Описание не более 1000 символов" });

    if (errors.Count > 0)
    {
        var errorResponse = new
        {
            error = "Ошибка валидации",
            errors
        };

        WriteJson(response, JsonSerializer.Serialize(errorResponse), 400);
        return;
    }

    var task = new TaskItem
    {
        Id = nextId++,
        Title = data!.Title!,
        Description = data.Description,
        Priority = data.Priority ?? 1,
        IsCompleted = data.IsCompleted ?? false
    };

    tasks.Add(task);

    WriteJson(response, JsonSerializer.Serialize(task), 201);
}

void HandleUpdateTask(HttpListenerRequest request, HttpListenerResponse response)
{
    var parts = request.Url!.AbsolutePath.Split('/');
    int id = int.Parse(parts[3]);

    var task = tasks.FirstOrDefault(t => t.Id == id);

    if (task == null)
    {
        WriteJson(response, "{\"error\":\"Task not found\"}", 404);
        return;
    }

    string body;

    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
    {
        body = reader.ReadToEnd();
    }

    CreateTaskRequest? data;

    try
    {
        data = JsonSerializer.Deserialize<CreateTaskRequest>(body);
    }
    catch
    {
        WriteJson(response, "{\"error\":\"Некорректный JSON\"}", 400);
        return;
    }

    var errors = new List<object>();

    if (string.IsNullOrWhiteSpace(data?.Title))
        errors.Add(new { field = "Title", message = "Название обязательно" });

    if (data?.Priority is int p && (p < 1 || p > 5))
        errors.Add(new { field = "Priority", message = "Приоритет от 1 до 5" });

    if (errors.Count > 0)
    {
        var errorResponse = new
        {
            error = "Ошибка валидации",
            errors
        };

        WriteJson(response, JsonSerializer.Serialize(errorResponse), 400);
        return;
    }

    task.Title = data!.Title!;
    task.Description = data.Description;
    task.Priority = data.Priority ?? task.Priority;

    WriteJson(response, JsonSerializer.Serialize(task), 200);
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
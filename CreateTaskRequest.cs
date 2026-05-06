using System;

public class CreateTaskRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? Priority { get; set; }

    public bool? IsCompleted { get; set; }
}
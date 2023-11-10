namespace EventSourcingTaskApp.Core.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException() : base("Task not found.") { }
    }
}

namespace EventSourcingTaskApp.Core.Exceptions
{
    public class TaskAlreadyCreatedException : Exception
    {
        public TaskAlreadyCreatedException() : base("Task already created.") { }
    }
}

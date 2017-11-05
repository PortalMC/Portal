using System;
using System.Threading.Tasks;

namespace Portal.Extensions
{
    public static class TaskExtension
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x => { Console.WriteLine(x.Exception.ToString()); }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
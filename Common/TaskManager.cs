using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    public class TasksManager<TKey, TResult>
    {

        private readonly Dictionary<TKey, TaskCompletionSource<TResult>> _tasks = new Dictionary<TKey, TaskCompletionSource<TResult>>();

        public Task<TResult> Add(TKey key)
        {
            lock (_tasks)
            {
                var task = new TaskCompletionSource<TResult>(key);
                _tasks.Add(key, task);

                return task.Task;
            }
        }

        public void Compliete(TKey key, TResult result)
        {
            lock (_tasks)
            {
                var theTask = _tasks[key];
                _tasks.Remove(key);
                theTask.SetResult(result);
            }
        }

        private TaskCompletionSource<TResult>[] GetAndDeleteAll()
        {
            lock (_tasks)
            {
                var result = _tasks.Values.ToArray();
                _tasks.Clear();
                return result;
            }

        }

        public void SetExceptionsToAll(Exception exception)
        {
            var tasks = GetAndDeleteAll();

            foreach (var taskCompletionSource in tasks)
                taskCompletionSource.SetException(exception);

        }

    }




}

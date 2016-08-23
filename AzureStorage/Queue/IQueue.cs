using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureStorage.Queue
{
    public class QueueMessageToken<T>
    {
        public T Message { get; set; }
        public object Token { get; set; }

        public static QueueMessageToken<T> Create(T message, object token)
        {
            return new QueueMessageToken<T>
            {
                Message = message,
                Token = token
            };
        }
    }

    public interface IQueue<T>
    {
        // Кладем сообщение в очередь
        void PutMessage(T itm);
        Task PutMessageAsync(T itm);

        /// <summary>
        ///  Вынимаем сообщение из очереди. Если сообщения нет - получем null
        /// </summary>
        /// <returns></returns>

        T GetMessage();

        /// <summary>
        /// Получаем сообщение асинхронно. Если пусто, получаем null
        /// </summary>
        /// <returns>Сообщение</returns>
        Task<T> GetMessageAsync();

        /// <summary>
        /// Получить сообщение и сделать его невидимым на 30 секунд.
        /// За 30 секунд его необходимо обработать, и если все удачно, вызвать ProcessMessage, иначе через 30 секунд сообщение вернется в очередь
        /// </summary>
        /// <returns>Токен сообщения</returns>
        QueueMessageToken<T> GetMessageAndHide();

        /// <summary>
        /// Удаление сообщения из очереди, взятого ранее методом PeekMessage
        /// </summary>
        /// <param name="token">токен сообщения</param>
        void ProcessMessage(QueueMessageToken<T> token);

        /// <summary>
        /// Получить все сообщения не удаляя их из очереди
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> PeekAllMessages(int maxCount);
        Task<IEnumerable<T>> PeekAllMessagesAsync(int maxCount);


        void Clear();
        Task ClearAsync();

        /// <summary>
        /// Количество сообщений в очереди (approx)
        /// </summary>
        int Size { get; }

        Task<int> GetSizeAsync();
    }

}


namespace BlazorApp1.Helpers
{
    public interface IMessageService
    {
        event Action OnMessage;
        void SendMessage();
        void ClearMessages();
    }


    public class MessageService : IMessageService
    {
        public event Action OnMessage;

        public void SendMessage( )
        {
            OnMessage?.Invoke();
        }


        public void ClearMessages()
        {
            OnMessage?.Invoke();
        }

    }
}

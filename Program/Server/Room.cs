using Butterfly;

namespace Server
{
    public class Room : Controller.Board.LocalField<string>, Room.ICommunication
    {
        public readonly Dictionary<string, Client.ICommunication> _clients 
            = new Dictionary<string, Client.ICommunication>();

        public string CreatorClientName { get => Field; }

        public interface ICommunication
        {
        }
    }
}
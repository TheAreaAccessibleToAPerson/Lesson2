using Butterfly;

namespace Server
{
    public class Room : Controller.Board, Room.ICommunication
    {
        public const string DISCONNECT_CLIENT = "disconnect_client";

        public readonly Dictionary<string, Client.ICommunication> Clients 
            = new Dictionary<string, Client.ICommunication>();

        public string CreatorClientName {private set;get;} = "";
        public Client.ICommunication CreatorClientCommunication {private set;get;} = null;

        void Start() => SystemInformation("Create.", ConsoleColor.Green);
        void Stop() => SystemInformation("Destroying.", ConsoleColor.Green);

        public void SetCreatorInformation(string name, Client.ICommunication communication)
        {
            if (CreatorClientName == "" && CreatorClientCommunication == null)
            {
                CreatorClientName = name;
                CreatorClientCommunication = communication;
            }
            else throw new Exception();
        }

        private string _roomName = "";

        void Construction()
            => _roomName = GetKey();

        void ICommunication.Send(string clientName, string message)
        {
            foreach(Client.ICommunication client in Clients.Values)
                client.Send(_roomName, clientName, message);

            CreatorClientCommunication.Send(_roomName, clientName, message);
        }

        public void DisconnectAllClients()
        {
            foreach(Client.ICommunication client in Clients.Values)
                client.Send(_roomName, CreatorClientName, DISCONNECT_CLIENT);
        }


        public interface ICommunication
        {
            void Send(string clientName, string message);
        }
    }
}
using Butterfly;

namespace Server
{
    public sealed class RoomManager : Controller
    {
        public const string CREATING_ROOM = "Creating room";
        public const string CONNECT_TO_ROOM = "Connect to room.";
        public const string DISCONNECT_TO_ROOM = "Disconnet to room";

        private readonly Dictionary<string, Room> _rooms = new Dictionary<string, Room>();

        void Construction()
        {
            listen_echo_3_3<string, string, Client.ICommunication, string, bool, Room.ICommunication>
                (CREATING_ROOM)
                .output_to((roomName, clientName, clientCommunication, @return) =>
                {
                    if (try_obj<Room>(roomName, out Room room))
                    {
                        @return.To(roomName, false, null);
                    }
                    else
                    {
                        Room newRoom = obj<Room>(roomName, clientName);

                        _rooms.Add(roomName, newRoom);

                        newRoom._clients.Add(clientName, clientCommunication);

                        @return.To(roomName, true, newRoom);
                    }
                }, 
                Header.EVENT_2);

            listen_echo_3_3<string, string, Client.ICommunication, string, bool, Room.ICommunication>
                (CONNECT_TO_ROOM)
                .output_to((roomName, clientName, clientCommunication, @return) =>
                {
                    if (try_obj<Room>(roomName, out Room room))
                    {
                        room._clients.Add(clientName, clientCommunication);

                        @return.To(roomName, true, room);
                    }
                    else @return.To(roomName, false, null);
                }, 
                Header.EVENT_2);

            listen_echo_2_2<string, string, string, bool>
                (DISCONNECT_TO_ROOM)
                .output_to((roomName, clientName, @return) =>
                {
                    if (try_obj<Room>(roomName, out Room room))
                    {
                        if (room._clients.Remove(roomName))
                        {
                            @return.To(roomName, true);
                        }
                        else @return.To(roomName, false);
                    }
                    else @return.To(roomName, false);
                },
                Header.EVENT_2);
        }
    }
}

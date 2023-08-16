using Butterfly;

namespace Server
{
    public sealed class RoomManager : Controller
    {
        /// <summary>
        /// Создает(пытается) комнату новую комнату.
        /// in[string:roomName, string:clientName, Client.ICommunication] 
        /// out[string:roomName, bool:result, Room.ICommunication]
        /// </summary>
        public const string CREATING_ROOM = "creating_room";

        /// <summary>
        /// Подключается(пытается) к сущесвующей комнате.
        /// in[string:roomName, string:clientName, Client.ICommunication] 
        /// out[string:roomName, bool:result, Room.ICommunication]
        /// </summary>
        public const string CONNECTION_TO_ROOM = "connection_to_room";

        /// <summary>
        /// Отключается(пытается) от комнате.
        /// in[string:roomName, string:clientName] 
        /// out[string:roomName, string:error]
        /// </summary>
        public const string DISCONNECTING_ROOM = "disconnection_room";

        /// <summary>
        /// Отключается ото всех комнат и отключает свои комнаты.
        /// </summary>
        public const string DISCONNECTING_ROOMS = "disconnection_rooms";

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
                        Room newRoom = obj<Room>(roomName);

                        newRoom.SetCreatorInformation(clientName, clientCommunication);

                        _rooms.Add(roomName, newRoom);

                        @return.To(roomName, true, newRoom);
                    }
                },
                Header.SERVER_ROOM_EVENT);

            listen_echo_3_3<string, string, Client.ICommunication, string, string, Room.ICommunication>
                (CONNECTION_TO_ROOM)
                .output_to((roomName, clientName, clientCommunication, @return) =>
                {
                    if (_rooms.TryGetValue(roomName, out Room room))
                    {
                        if (room.CreatorClientName != clientName)
                        {
                            if (room.Clients.TryAdd(clientName, clientCommunication))
                            {
                                @return.To(roomName, "", room);
                            }
                            else @return.To(roomName, "Вы уже подключились к данной команте.", null);
                        }
                        else @return.To(roomName, "Вы пытаетесь подключиться к своей комнате.", null);
                    }
                    else @return.To(roomName, "Комнаты с данным именем не существует.", null);
                },
                Header.SERVER_ROOM_EVENT);

            listen_echo_2_2<string, string, string, string>
                (DISCONNECTING_ROOM)
                .output_to((roomName, clientName, @return) =>
                {
                    if (_rooms.TryGetValue(roomName, out Room room))
                    {
                        if (room.Clients.Remove(clientName))
                        {
                            @return.To(roomName, "");
                        }
                        else if (room.CreatorClientName == clientName)
                        {
                            if (room.CreatorClientName == clientName)
                                room.DisconnectAllClients();

                            _rooms.Remove(roomName);

                            room.destroy();

                            @return.To(roomName, "");
                        }
                        else @return.To(roomName, "Вы не подключены к данной комнате.");
                    }
                    else @return.To(roomName, "Комнаты с таким именем не сущесвует.");
                },
                Header.SERVER_ROOM_EVENT);

            listen_message<string, string[]>
                (DISCONNECTING_ROOMS)
                .output_to((clientName, roomsName) =>
                {
                    foreach (string roomName in roomsName)
                    {
                        if (_rooms.TryGetValue(roomName, out Room room))
                        {
                            if (room.CreatorClientName == clientName)
                            {
                                room.DisconnectAllClients();

                                _rooms.Remove(roomName);

                                room.destroy();
                            }
                        }
                    }
                });
        }
    }
}

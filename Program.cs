namespace Butterfly
{
    /*
    // -all - все созданые/доступные обьекты.
    // Command - пустой ввод выдаст все доступные команды.

    // Создаем сервер.
    Object name:Program
    Command:creating_server - Server_1

    // Включаем прослушку входящих клинтов.
    ObjectName:Server_1
    Command:creating_listen_clients - addr 127.0.0.1, port 111

    // Создаем клиeнта, подключаем его к серверу.
    ObjectName:Program
    Command:creating_client - Client_1, 222, 127.0.0.1, 111

    // Создаем клиeнта, подключаем его к серверу.
    ObjectName:Program
    Command:creating_client - Client_2, 333, 127.0.0.1, 111

    // Создаем комнату.
    ObjectName:Client_1
    Command:creating_room - Room_1

    // Подключаемся к комнате.
    ObjectName:Client_2
    Command:connecting_to_room - Room_1

    // Отправляем сообщение всем подключеным клиентам.
    ObjectName:Client_1
    Command:send - Room_1, Hello    

    // Отключаемся от комнаты.
    ObjectName:Client_2
    Command:disconnecting_room - Room_1

    // Отключаемся из ранее созданой комнаты Room_1, комната уничтожится.
    ObjectName:Client_1
    Command:disconnecting_room - Room_1

    // Отключаем клиетов
    ObjectName:Client_1
    Command:exit

    ObjectName:Client_2
    Command:exit

    // Отключаем сервер
    ObjectName:Server_1
    Command:exit

    // Завершаем программу
    ObjectName:Program
    Command:exit

    */
    public sealed class Program 
    {
        public static void Main(string[] args)  
        {
            Butterfly.fly<Header>(new Butterfly.Settings() 
            {
                Name = "Program",
                SystemEvent = new EventSetting(Header.WORK_WITCH_OBJECTS_EVENT, 10),

                EventsSetting = new EventSetting[] 
                {
                    new EventSetting(Header.SERVER_RECEIVE_NETWORK_EVENT, 10),
                    new EventSetting(Header.SERVER_SEND_NETWORK_EVENT, 10),
                    new EventSetting(Header.SERVER_ROOM_EVENT, 10),

                    new EventSetting(Header.CLIENT_RECEIVE_NETWORK_EVENT, 10),
                    new EventSetting(Header.CLIENT_SEND_NETWORK_EVENT, 10),
                }
            });
        }
    }
}
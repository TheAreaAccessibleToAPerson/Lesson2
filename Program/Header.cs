using Butterfly;

public struct _
{
    public const string END_MESSAGE = "&&01";
    public const string SPLIT_MESSAGE = "&&02";
}

public class Header : Controller, ReadLine.IInformation
{
    /// <summary>
    /// Системное событие. Отвечает за создание обьектов и любых операций
    /// связаных с ними. При подключении нового клинта к серверу
    /// будет изменятся timeDelay на 0, после создания обьекта и выполнения
    /// всех операций timeDelay вернется в 10.
    /// </summary>
    public const string WORK_WITCH_OBJECTS_EVENT = "WorkWithObjectsEvent";

    /// <summary>
    /// Событие обрабатывающее входящие сообщение на сервер от клинтов.
    /// </summary>
    public const string SERVER_RECEIVE_NETWORK_EVENT = "ServerReceiveNetworkEvent";

    /// <summary>
    /// Событие обрабатывающее исходящие сообщения с сервера клиeнтам.
    /// </summary>
    public const string SERVER_SEND_NETWORK_EVENT = "ServerSendNetworkEvent";

    /// <summary>
    /// Событие выполняющее все работу с комнатами.
    /// </summary>
    public const string SERVER_ROOM_EVENT = "ServerRoomEvent";

    /// <summary>
    /// Событие обрабатывающее входящие сообщения книентам с сервера. 
    /// </summary>
    public const string CLIENT_RECEIVE_NETWORK_EVENT = "ClientReceiveNetworkEvent";

    /// <summary>
    /// Событие обрабатывающее исходящие сообщения от клинтов на сервер.
    /// </summary>
    public const string CLIENT_SEND_NETWORK_EVENT = "ClientSendNetworkEvent";

    void Construction()
    {
        // Создаем прослушку для входящих операций, которые будут выполнены
        // запущеным событием. 

        listen_events(WORK_WITCH_OBJECTS_EVENT, WORK_WITCH_OBJECTS_EVENT);
        //listen_events(SERVER_RECEIVE_NETWORK_EVENT, SERVER_RECEIVE_NETWORK_EVENT);
        listen_events(SERVER_SEND_NETWORK_EVENT, SERVER_SEND_NETWORK_EVENT);
        listen_events(CLIENT_SEND_NETWORK_EVENT, CLIENT_SEND_NETWORK_EVENT);
        //listen_events(CLIENT_RECEIVE_NETWORK_EVENT, CLIENT_RECEIVE_NETWORK_EVENT);
        listen_events(SERVER_ROOM_EVENT, SERVER_ROOM_EVENT);
    }

    void Start()
    {
        SystemInformation("Start program.", ConsoleColor.Green);

        // Реализуем работу с данным обьектов из кансоли.
        ReadLine.Start(this);
    }

    // Реализуем команды для консоли.
    void ReadLine.IInformation.Command(string command)
    {
        switch (command)
        {
            case "creating_server":

                ConsoleLine("Введите имя севера:");

                string creatingServerName = System.Console.ReadLine();
                if (creatingServerName == "") return;

                if (try_obj(creatingServerName, out Server.Main serverObject))
                {
                    ConsoleLine($"Сервер с именем {creatingServerName} уже сущесвует, " +
                        "приостановить его роботу и создать новый? yes/no:");
                    string isStoppingServerName = System.Console.ReadLine();

                    if (isStoppingServerName == "yes") serverObject.destroy();
                    else return;
                }

                obj<Server.Main>(creatingServerName);

                break;

            case "creating_client":

                ConsoleLine("Введите имя нового клинта:");
                string creatingClientName = System.Console.ReadLine();
                if (creatingClientName == "") return;

                ConsoleLine("Введите локальный порт для нового клинта:");
                string creatingClientLocalPort = System.Console.ReadLine();
                if (creatingClientLocalPort == "") return;

                ConsoleLine("Введите удаленный адрес для подключения:");
                string creatingClientRemoteAddress = System.Console.ReadLine();
                if (creatingClientRemoteAddress == "") return;

                ConsoleLine("Введите удаленный порт для подключения:");
                string creatingClientRemotePort = System.Console.ReadLine();
                if (creatingClientRemoteAddress == "") return;

                if (try_obj(creatingClientName, out Client.Main client))
                {
                    if (creatingClientLocalPort == client.Port)
                    {
                        ConsoleLine("Уже создан клиент с таким же именем и портом," +
                            " введите другой порт.");

                        creatingClientLocalPort = System.Console.ReadLine();
                        if (creatingClientRemotePort == client.Port)
                        {
                            SystemInformation("Вы опять ввели тот же номер порта.", ConsoleColor.Red);
                            return;
                        }
                    }

                    client.destroy();
                }

                // Создаем обьект, передаем в него локальные данные.
                obj<Client.Main>(creatingClientName,
                    new string[]
                    {
                        creatingClientLocalPort,
                        creatingClientRemoteAddress,
                        creatingClientRemotePort
                    });

                break;

            case "exit":

                destroy();

                break;

            default:
                Console("creating_server - создание сервера.");
                Console("creating_client - создать клиeнта.");
                Console("exit - остановка программы.");
                break;
        }
    }

    void Stop()
    {
        // Если небыл запущен метод Start() где данный обьект
        // был подключан к консоли, то и отключать его не неужно.
        if (StateInformation.IsCallStart)
            ReadLine.Stop(this); // Исключаем данный обьект из кансоли.

        SystemInformation("StopProgram", ConsoleColor.Green);
    }
}

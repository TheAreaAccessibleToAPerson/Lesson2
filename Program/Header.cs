using Butterfly;

public struct _
{
    public const string END_MESSAGE = "&&01";
    public const string SPLIT_MESSAGE = "&&02";
}

public class Header : Controller, ReadLine.IInformation
{
    public const string SYSTEM_EVENT = "SystemEvent";
    public const string EVENT_1 = "Event_1";
    public const string EVENT_2 = "Event_2";

    void Construction()
    {
        listen_events(SYSTEM_EVENT, SYSTEM_EVENT);
        listen_events(EVENT_1, EVENT_1);
        listen_events(EVENT_2, EVENT_2);
    }

    void Start()
    {
        SystemInformation("Start program.", ConsoleColor.Green);
        ReadLine.Start(this);
    }

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
                Console("creating_client - создать клинта.");
                Console("exit - остановка программы.");
                break;
        }
    }

    void Stop()
    {
        if (StateInformation.IsCallStart) ReadLine.Stop(this);

        SystemInformation("StopProgram", ConsoleColor.Green);
    }
}

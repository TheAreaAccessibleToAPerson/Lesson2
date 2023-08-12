namespace Butterfly
{
    public sealed class Program 
    {
        public static void Main(string[] args)  
        {
            Butterfly.fly<Header>(new Butterfly.Settings() 
            {
                Name = "Program",
                SystemEvent = new EventSetting(Header.SYSTEM_EVENT, 10),

                EventsSetting = new EventSetting[] 
                {
                    new EventSetting(Header.EVENT_1, 10),
                    new EventSetting(Header.EVENT_2, 10),
                }
            });
        }
    }
}
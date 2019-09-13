using System;

namespace HxngryCONSOLE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(200, 50);
            User user = new User();
            user.InitUser();
            App app = new App();
            app.InitApp(user);
            app.MainLoop();
        }
    }
}

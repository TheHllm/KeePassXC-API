using KeePassXC_API;
using System;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("\t -lock \t lock the database");
                    Console.WriteLine("\t -logins [url] \t get all logins for a url");
                    Console.WriteLine("\t -generate \t generates a password");
                    Environment.Exit(0);
                }
             
                KeepassXCApi api = new();
                await api.AssociateIfNeeded();

                switch (args[0])
                {
                    case "-lock":
                        await api.LockDatabase();
                        break;

                    case "-logins":
                        AccountInformation[] logins = await api.GetLogins(args[1]);
                        foreach (var login in logins)
                        {
                            Console.WriteLine("{0}\n{1}", login.Username, login.Password);
                        }
                        break;

                    case "-generate":
                        Console.WriteLine(await api.GeneratePassword());
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}

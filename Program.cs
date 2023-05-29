
namespace BTM
{
    class Program
    {
        static void Main(string[] args)
        {
            BTMSystem.GetInstance().SetBaseExample();

            Terminal terminal = new Terminal();
            terminal.Run(true);
        }
    }
}
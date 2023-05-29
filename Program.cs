
namespace BTM
{
    class Program
    {
        static void Main(string[] args)
        {
            BTMSystem.GetInstance().SetTextExample();

            Terminal terminal = new Terminal();
            terminal.Run(true);
        }
    }
}
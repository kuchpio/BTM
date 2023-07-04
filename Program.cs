
namespace BTM
{
    class Program
    {
        static void Main(string[] args)
        {
            BTM.GetInstance().SetTextExample();

            Terminal terminal = new Terminal();
            terminal.Run(false);
        }
    }
}

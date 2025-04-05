using PressPattern;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter username: ");
        string user = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(user))
        {
            Console.WriteLine("Invalid username. Exiting.");
            return;
        }

        var recorder = new KeystrokeRecorder(user);
        recorder.Start();
    }
}
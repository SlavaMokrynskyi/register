using Microsoft.Win32;
class Program
{
    static bool LogEnabled = false;
    static bool ParallelEnabled = false;
    static int MonitoringInterval = 1000;

    static async Task Main(string[] args)
    {
        ReadSettings();

        Console.WriteLine("SystemMonitor запущено...");
        Console.WriteLine($"LogEnabled: {LogEnabled}");
        Console.WriteLine($"ParallelEnabled: {ParallelEnabled}");
        Console.WriteLine($"MonitoringInterval: {MonitoringInterval} ms");

        while (true)
        {
            int[] data = GenerateLoad(1_000_000);

            double avg = 0;

            if (ParallelEnabled)
            {
                avg = ProcessDataParallel(data);
            }
            else
            {
                avg = ProcessDataSequential(data);
            }

            Console.WriteLine($"Середнє значення: {avg}");

            if (LogEnabled)
            {
                string logLine = $"{DateTime.Now}: Average = {avg}\n";
                await File.AppendAllTextAsync("monitor_log.txt", logLine);
            }

            Thread.Sleep(MonitoringInterval);
        }
    }

    static void ReadSettings()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SystemMonitor"))
        {
            if (key != null)
            {
                LogEnabled = Convert.ToInt32(key.GetValue("LogEnabled", 0)) == 1;
                ParallelEnabled = Convert.ToInt32(key.GetValue("ParallelEnabled", 0)) == 1;
                MonitoringInterval = Convert.ToInt32(key.GetValue("MonitoringInterval", 1000));
            }
            else
            {
                Console.WriteLine("Реєстр не знайдено, використовується стандартна конфігурація.");
            }
        }
    }

    static int[] GenerateLoad(int size)
    {
        Random rand = new Random();
        int[] data = new int[size];
        for (int i = 0; i < size; i++)
        {
            data[i] = rand.Next(1, 100);
        }
        return data;
    }

    static double ProcessDataSequential(int[] data)
    {
        long sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i];
        }

        return (double)sum / data.Length;
    }

    static double ProcessDataParallel(int[] data)
    {
        long sum = 0;
        object locker = new object();

        Parallel.For(0, data.Length, i =>
        {
            lock (locker)
            {
                sum += data[i];
            }
        });

        return (double)sum / data.Length;
    }
}

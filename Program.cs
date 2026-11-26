using WaveGen;

int durationSeconds = 5;
string fileName = "GENERATE.TXT";

if (args.Length > 0)
{
    if (int.TryParse(args[0], out int parsedDuration) && parsedDuration > 0)
    {
        durationSeconds = parsedDuration;
    }
    else
    {
        Console.WriteLine("Использование: WaveGen [длительность_в_секундах] [имя_файла]");
        return;
    }
}

if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
{
    fileName = args[1];
}

SyntheticSignalGenerator.GenerateTestFile(fileName, durationSeconds);
Console.WriteLine($"Файл {fileName} успешно сгенерирован (длительность: {durationSeconds} сек, отсчётов: {durationSeconds * 125}).");

using System;
using System.Collections.Generic;
using System.Text;

namespace WaveGen
{
    public static class SyntheticSignalGenerator
    {
        public static void GenerateTestFile(string filePath, int durationSeconds = 5)
        {
            const int sampleRate = 125;
            int totalSamples = durationSeconds * sampleRate;
            var sb = new StringBuilder();
            var rnd = new Random(42);
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine("Length : \t" + totalSamples);
            sb.AppendLine("Sex : \tMale");
            sb.AppendLine("Age : \t44");
            sb.AppendLine("Height : \t180");
            sb.AppendLine("Weight : \t89");
            sb.AppendLine("ABP1 : \t128/78");
            sb.AppendLine("ABP2 : \t190/110");
            sb.AppendLine("ABP3 : \t115/69");
            sb.AppendLine("ABP4 : \t180/105");
            sb.AppendLine("Pulse : \t78");

            // Заголовок в точном соответствии с вашим форматом (разделитель - табуляция)
            sb.AppendLine("Date/time\tECG1\tECG2\tReo 1\tReo 2\tSphigmo 1\tSphigmo 2\tSphigmo 3\tSphigmo 4\tPPG\tApex\tSound\t");

            DateTime baseTime = new DateTime(2026, 11, 21, 20, 48, 9);

            // Параметры сердечного ритма и задержек:
            // R-R = 100 отсчётов (0.8 сек = 75 уд/мин)
            // Приход волны на руку: через 18 отсчётов (144 мс) после R-пика
            // Приход волны на ногу: через 33 отсчёта (264 мс) после R-пика -> задержка рука-нога = 15 отсчётов (120 мс)
            const int rrPeriod = 100;
            const int armFootOffset = 18;
            const int legFootOffset = 33;

            for (int i = 0; i < totalSamples; i++)
            {
                long elapsedMicroseconds = i * 8;
                DateTime currentTime = baseTime.AddSeconds(i / sampleRate);
                int phase = i % rrPeriod; // Позиция внутри текущего сердечного цикла (0..99)

                // 1. ЭКГ1: базовая линия ~233000 + узкий R-пик в начале цикла (phase == 5)
                int ecg1 = 233000 + rnd.Next(-180, 180);
                if (phase == 4 || phase == 6) ecg1 += 7500;
                else if (phase == 5) ecg1 += 17500; // Пик ~250500
                else if (phase == 7) ecg1 -= 1200; // S-зубец

                // 2. Сфигмограмма руки (Sphigmo 1 и Sphigmo 3): базовая линия ~190000, амплитуда ~8000
                double armWave = GeneratePulseWaveform(phase - armFootOffset, rrPeriod);
                int sphigmoArm1 = (int)(190000 + armWave * 8000 + rnd.Next(-40, 40));
                int sphigmoArm2 = (int)(250000 + armWave * 8000 + rnd.Next(-40, 40));

                // 3. Сфигмограмма ноги (Sphigmo 2 и Sphigmo 4): базовая линия ~0 и ~172000, амплитуда ~8500
                double legWave = GeneratePulseWaveform(phase - legFootOffset, rrPeriod);
                int sphigmoLeg1 = (int)(0 + legWave * 8500 + rnd.Next(-40, 40));
                int sphigmoLeg2 = (int)(172000 + legWave * 8500 + rnd.Next(-40, 40));

                // 4. Заглушки для неиспользуемых каналов (ECG2, Reo1, Reo2, PPG, Apex, 8 отсчётов Sound)
                int ecg2 = -156200 + rnd.Next(-100, 100);
                int reo1 = -1156290 + rnd.Next(-20, 20);
                int reo2 = 1386590 + rnd.Next(-20, 20);
                int ppg = -230400 + rnd.Next(-150, 150);
                int apex = -377500 + rnd.Next(-150, 150);
                string sound8 = "-700\t-750\t-800\t-720\t-690\t-740\t-780\t-710";

                // 5. Манжеты давления (NIBP1: 125/80, NIBP2: 140/90, NIBP3: 125/80, NIBP4: 140/90) + DeviceStatus
                string nibpAndStatus = "125\t80\t50\t140\t90\t50\t125\t80\t50\t140\t90\t50\t35466";

                sb.AppendLine(
                    $"{elapsedMicroseconds}\t{ecg1}\t{ecg2}\t{reo1}\t{reo2}\t" +
                    $"{sphigmoArm1}\t{sphigmoLeg1}\t{sphigmoArm2}\t{sphigmoLeg2}\t" +
                    $"{ppg}\t{apex}\t{sound8}\t{nibpAndStatus}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Формирует реалистичную форму пульсовой волны (крутой подъём + дикротическая волна).
        /// </summary>
        private static double GeneratePulseWaveform(int t, int period)
        {
            if (t < 0) t += period;

            // Систолический пик (максимум через ~14 отсчётов / 112 мс от начала подъёма)
            double systolic = 0;
            if (t >= 0 && t < 45)
            {
                double x = t / 14.0;
                systolic = x * Math.Exp(1.0 - x);
            }

            // Дикротический зубец на спаде (через ~32 отсчёта от начала волны)
            double dicrotic = 0.28 * Math.Exp(-Math.Pow((t - 32.0) / 7.0, 2));

            // Плавный диастолический спад к концу цикла
            double baselineDecay = 0.15 * Math.Exp(-t / 35.0);

            return Math.Max(0, systolic + dicrotic + baselineDecay - 0.05);
        }
    }
}

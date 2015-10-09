using System;
using System.IO;
using NAudio.Wave;
using SoundTouchNet;
using System.Diagnostics;
using SermonProcessor.Properties;

namespace SoundTouchExample
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ParseArguments(args);

            //Directory.SetCurrentDirectory(Settings.Default.BaseDirectory);
            
            //foreach (string File in Directory.GetFiles(".", "*.wav"))
            //{
            //    string OutFile = SoxEffects(File);
            //    TrimAudioLength(OutFile);
            //    //AddIntroOutro(TrimAudioLength(OutFile));
            //}
            //Console.WriteLine("Finished Processing Files");
            //Console.ReadLine();
        }

        private static void ParseArguments(string[] args)
        {
            /*
             *  SermonProcessor <command> <input file> <output file>    
             *      <command>       Function to perform to tranform input file to output file
             *                      
             *                      CleanSpeech     Cleans the audio speech using sox.
             *                      
             *                      TrimLength      Trims the audio to a given length in minutes.
             *                      
             *                      AddIntroOutro   Adds intro and outro clips to input file
             *                      
             *      <input file>    File being tranformed
             *      <output file>   File being produced after tranformation.  This can be a path instead.
             */
            switch (args[0].ToLower())
            {
                case "cleanspeech":
                    break;
                case "trimlength":
                    break;
                case "addintrooutro":
                    AddIntroOutro(args[1], args[2], args[3], 
                        double.Parse(args[4]), 
                        double.Parse(args[5]), 
                        args[6]);
                    break;
                default:
                    Console.WriteLine("Invalid Arguments...");
                    break;
            }
        }

        private static string TrimAudioLength(string File)
        {
            TimeSpan TargetDuration = new TimeSpan(0, 29, 0); // 29 minutes

            WaveFileReader reader = new WaveFileReader(File);
            //Calculate Tempo
            float PercentChange = CalculateTempo(reader.TotalTime, TargetDuration);
            
            string FileOut = File.Replace(Settings.Default.SpeechCleanedDirectory, Settings.Default.TrimmedDirectory);
            processWave(File, FileOut, 1 + PercentChange * 0.01f, 1.0f, 1.0f);
            
            //AddIntroOutro(FileOut);

            reader.Close();
            
            return FileOut;
        }

        private static string SoxEffects(string File)
        {
            string FileOut = File.Replace(@".\", Settings.Default.SpeechCleanedDirectory);
            
            
            Process p = Process.Start(Settings.Default.SoxEffectBatchFile, string.Format("\"{0}\" \"{1}\"", File, FileOut));

            if (p.WaitForExit(10 * 60 * 1000)) //10 minutes;
            {
                Console.WriteLine("File {0} updated and copied to {1}", File, FileOut);
            }
            else
            {
                Console.WriteLine("File {0} failed to apply sox effects in a timely manner.", File, FileOut);
            }

            return FileOut;
        }

        private static void AddIntroOutro(string SermonFileName = "",
            string IntroFileName = "",
            string OutroFileName = "",
            double StartSermonTime = 0,
            double StartOutroTime = 0,
            string ResultingFile = "")
        {
            if (String.IsNullOrWhiteSpace(SermonFileName)) throw new ArgumentException("SermonFileName must reference a valid file.");

            if (String.IsNullOrWhiteSpace(IntroFileName)) throw new ArgumentException("IntroFileName must reference a valid file.");

            if (String.IsNullOrWhiteSpace(OutroFileName)) throw new ArgumentException("OutroFileName must reference a valid file.");

            if (String.IsNullOrWhiteSpace(ResultingFile)) ResultingFile = Settings.Default.IntroOutroDirectory;


            WaveFileReader intro = new WaveFileReader(IntroFileName);
            WaveFileReader outro = new WaveFileReader(OutroFileName);
            WaveFileReader audio = new WaveFileReader(SermonFileName);

            WaveMixerStream32 mixer = new WaveMixerStream32();
            //mixer.AutoStop;

            WaveOffsetStream audioOffsetted = new WaveOffsetStream(
                audio,
                TimeSpan.FromSeconds(StartSermonTime), //N seconds after start of intro.
                TimeSpan.Zero,
                audio.TotalTime);

            TimeSpan outroOffset = TimeSpan.FromSeconds(StartSermonTime) + audio.TotalTime - TimeSpan.FromSeconds(StartOutroTime);
            
            WaveOffsetStream outroOffsetted = new WaveOffsetStream(
               outro,
               outroOffset,
               TimeSpan.Zero,
               outro.TotalTime);

            WaveChannel32 intro32 = new WaveChannel32(intro);
            intro32.PadWithZeroes = false;
            mixer.AddInputStream(intro32);

            WaveChannel32 outro32 = new WaveChannel32(outroOffsetted);
            outro32.PadWithZeroes = false;
            mixer.AddInputStream(outro32);

            WaveChannel32 audio32 = new WaveChannel32(audioOffsetted);
            audio32.PadWithZeroes = false;
            mixer.AddInputStream(audio32);


            FileInfo file = new FileInfo(SermonFileName);
            if (!Directory.Exists(ResultingFile)) Directory.CreateDirectory(ResultingFile);

            string FileOut = String.Format(@"{0}\{1}", ResultingFile, file.Name);

            WaveFileWriter.CreateWaveFile(FileOut, new Wave32To16Stream(mixer));

        }


        //private static void AddIntroOutro(string SermonFileName = "",
        //    string IntroFileName = "",
        //    string OutroFileName = "",
        //    double StartSermonTime = 0,
        //    double StartOutroTime = 0,
        //    string ResultingFile = "")
        //{
        //    if (!String.IsNullOrWhiteSpace(SermonFileName)) throw new ArgumentException("SermonFileName must reference a valid file.");

        //    if (!String.IsNullOrWhiteSpace(IntroFileName)) throw new ArgumentException("IntroFileName must reference a valid file.");
            
        //    if (!String.IsNullOrWhiteSpace(OutroFileName)) throw new ArgumentException("OutroFileName must reference a valid file.");
            
        //    if (!String.IsNullOrWhiteSpace(ResultingFile)) ResultingFile = Settings.Default.IntroOutroDirectory;

        //    WaveFileReader intro = new WaveFileReader(IntroFileName);
        //    WaveFileReader outro = new WaveFileReader(OutroFileName);
        //    WaveFileReader audio = new WaveFileReader(SermonFileName);

        //    WaveMixerStream32 mixer = new WaveMixerStream32();
        //    //mixer.AutoStop;

        //    WaveOffsetStream audioOffsetted = new WaveOffsetStream(
        //        audio,
        //        TimeSpan.FromSeconds(StartSermonTime), //22.5 seconds after start of intro.
        //        TimeSpan.Zero, 
        //        audio.TotalTime);

        //    TimeSpan outroOffset = TimeSpan.FromSeconds(StartSermonTime) + audio.TotalTime +
        //       TimeSpan.FromSeconds(StartOutroTime) - outro.TotalTime;

        //    WaveOffsetStream outroOffsetted = new WaveOffsetStream(
        //       outro, 
        //       outroOffset, 
        //       TimeSpan.Zero, 
        //       outro.TotalTime);

        //    WaveChannel32 intro32 = new WaveChannel32(intro);
        //    intro32.PadWithZeroes = false;
        //    mixer.AddInputStream(intro32);

        //    WaveChannel32 outro32 = new WaveChannel32(outroOffsetted);
        //    outro32.PadWithZeroes = false;
        //    mixer.AddInputStream(outro32);

        //    WaveChannel32 audio32 = new WaveChannel32(audioOffsetted);
        //    audio32.PadWithZeroes = false;
        //    mixer.AddInputStream(audio32);

        //    //string FileOut = OriginalAudioFileName.Replace(@".\Trimmed\", @".\IntroOutroAdded\");
        //    FileInfo file = new FileInfo(SermonFileName);
        //    if (!Directory.Exists(ResultingFile)) Directory.CreateDirectory(ResultingFile);
            
        //    // Currently ResultingFile should point to a folder where the resulting file will be created
        //    string FileOut = String.Format(@"{0}\{1}", ResultingFile, file.Name);
           
        //    WaveFileWriter.CreateWaveFile(FileOut, new Wave32To16Stream(mixer));
        //}

        private static float CalculateTempo(TimeSpan CurrentDuration, TimeSpan TargetDuration)
        {
            return ((CurrentDuration.Ticks * 100.0f) / TargetDuration.Ticks) - 100.0f;
        }

        /// <summary>    
        /// Load wave file and change its tempo, pitch and rate and save it to another file    
        /// </summary>    
        static void processWave(string fileIn, string fileOut, float newTempo, float newPitch, float newRate)
        {
            WaveFileReader reader = new WaveFileReader(fileIn);
            int numChannels = reader.WaveFormat.Channels;
            if (numChannels > 2)
                throw new Exception("SoundTouch supports only mono or stereo.");
            int sampleRate = reader.WaveFormat.SampleRate;
            int bitPerSample = reader.WaveFormat.BitsPerSample;
            const int BUFFER_SIZE = 1024 * 16;
            SoundStretcher stretcher = new SoundStretcher(sampleRate, numChannels);
            WaveFileWriter writer = new WaveFileWriter(fileOut, new WaveFormat(sampleRate, 16, numChannels));
            stretcher.Tempo = newTempo;
            stretcher.Pitch = newPitch;
            stretcher.Rate = newRate;
            byte[] buffer = new byte[BUFFER_SIZE];
            short[] buffer2 = null;

            if (bitPerSample != 16 && bitPerSample != 8)
            {
                throw new Exception("Not implemented yet.");
            }

            if (bitPerSample == 8)
            {
                buffer2 = new short[BUFFER_SIZE];
            }

            bool finished = false;
            while (true)
            {
                int bytesRead = 0;
                if (!finished)
                {
                    bytesRead = reader.Read(buffer, 0, BUFFER_SIZE);
                    if (bytesRead == 0)
                    {
                        finished = true;
                        stretcher.Flush();
                    }
                    else
                    {
                        if (bitPerSample == 16)
                        {
                            stretcher.PutSamplesFromBuffer(buffer, 0, bytesRead);
                        }
                        else if (bitPerSample == 8)
                        {
                            for (int i = 0; i < BUFFER_SIZE; i++)
                                buffer2[i] = (short)((buffer[i] - 128) * 256);
                            stretcher.PutSamples(buffer2);
                        }
                    }
                }

                bytesRead = stretcher.ReceiveSamplesToBuffer(buffer, 0, BUFFER_SIZE);
                writer.WriteData(buffer, 0, bytesRead);
                if (finished && bytesRead == 0)
                    break;
            }

            reader.Close();
            writer.Close();
        }
    }

}

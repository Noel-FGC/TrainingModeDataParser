using System;
using System.IO;
using System.CommandLine;

namespace TrainingModeDataParser {
  class Program {
    static void Main(string[] args) {
      var rootCommand = new RootCommand();
      var filePathArg = new Argument<string>(name: "path", description: "Path to the bin or txt file");
      var trialOpt = new Option<bool>(name: "--trial", description: "Read/Create a trial bin, with 2 00 bytes inbetween each frame", getDefaultValue: () => false);
      trialOpt.AddAlias("-t");
      rootCommand.Add(filePathArg);
      rootCommand.AddGlobalOption(trialOpt);

      rootCommand.SetHandler((filePath, isTrial) => {
        if (filePath.EndsWith(".bin")) {
          Parse(filePath, isTrial);  
        } else if (filePath.EndsWith(".txt")) {
          Rebuild(filePath, isTrial);
        }
      }, filePathArg, trialOpt);
      rootCommand.Invoke(args);
    }

    static void Rebuild(string filePath, bool isTrial) {
      string output = filePath.Substring(0, filePath.Length - 4) + ".bin";

      var lines = File.ReadAllLines(filePath);
      var writeStream = File.Open(output, FileMode.Create);
      var writer = new BinaryWriter(writeStream);

      foreach (string line in lines) {
        int Direction = 0;
        byte[] returnBytes = [0x00, 0x00];

        foreach(char character in line) {
          if (char.IsDigit(character)) {
            Direction = character - '0';
          } else {
            char lcharacter = char.ToLower(character);
            switch (lcharacter) {
              case 'a': returnBytes[0] |= 0b_00010000; break;
              case 'b': returnBytes[0] |= 0b_00100000; break;
              case 'c': returnBytes[0] |= 0b_01000000; break;
              case 'd': returnBytes[0] |= 0b_10000000; break;
              case 'e': returnBytes[1] |= 0b_00000001; break;
              case 'f': returnBytes[1] |= 0b_00000010; break;
              default: {
                Console.WriteLine("Unsupported Character " + character);
                Console.ReadKey();
                Environment.Exit(1);
                break;
              }
            }
          }
        }
        if (Direction == 0) {
          Console.WriteLine("No (Valid) Direction Found In: " + line);
          Console.ReadKey();
          Environment.Exit(1);
        }
        returnBytes[0] |= (byte)(Direction & 0x0F);
        writer.Write(returnBytes);
        if (isTrial) writer.Write((byte[])[0x00, 0x00]);
      }
      writer.Close();
    }

    static void Parse(string filePath, bool isTrial) {
      string output = filePath.Substring(0, filePath.Length - 4) + ".txt";

      var readStream = File.Open(filePath, FileMode.Open);
      var reader = new BinaryReader(readStream);
      var writeStream = File.Open(output, FileMode.Create);
      var writer = new StreamWriter(writeStream);

      while (readStream.Position < readStream.Length) {
        var frame = reader.ReadBytes(2);

        int Direction = frame[0] & 0b_00001111;
        bool a = (frame[0] & 0b_00010000) != 0;
        bool b = (frame[0] & 0b_00100000) != 0;
        bool c = (frame[0] & 0b_01000000) != 0;
        bool d = (frame[0] & 0b_10000000) != 0;

        bool e = (frame[1] & 0b_00000001) != 0;
        bool f = (frame[1] & 0b_00000010) != 0;

        string buttonStr = "";

        if (a) buttonStr += 'A';
        if (b) buttonStr += 'B';
        if (c) buttonStr += 'C';
        if (d) buttonStr += 'D';
        if (e) buttonStr += 'E';
        if (f) buttonStr += 'F';

        writer.WriteLine(Direction + buttonStr);

        if (isTrial) {
          readStream.Seek(2, SeekOrigin.Current);
        }
      }
      writer.Close();
    }
  }
}

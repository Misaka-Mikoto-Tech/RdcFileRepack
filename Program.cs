using Rdc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdcFileRepack
{
	class Program
	{
		static void ShowHelp()
        {
			Console.WriteLine("filepath mode");
			Console.WriteLine("mode:[repack] or [dump]");
        }

		static void Main(string[] args)
		{
			if(args.Length != 2)
            {
				ShowHelp();
				return;
            }

			//string rdcPath = @"H:\Temp\抓帧.rdc";
			//string rdcPath = @"H:\Temp\女仆奔跑.rdc";

			string rdcPath = args[0];
			string mode = args[1];

            RdcFile rdcFile = new RdcFile();
			rdcFile.LoadFromRdc(rdcPath);

            if (mode == "dump")
            {
                rdcFile.ExportTextures();
                Console.WriteLine($"已导出所有贴图");
            }
            else if (mode == "repack") // 重新生成
            {
				//rdcFile.SetDeviceName("NVIDIA GeForce GTX 8848 8GB");
				rdcFile.LoadTexturesFromFile();

				string rdcPathNew = Path.GetFileNameWithoutExtension(rdcPath);
				rdcPathNew = $"{Path.GetDirectoryName(rdcPath)}/{rdcPathNew}_repack.rdc";
                rdcFile.SaveToRdc(rdcPathNew);

				Console.WriteLine($"重新保存为 {rdcPathNew}");
            }

			Console.ReadKey();
		}
	}
}

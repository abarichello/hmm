using System;
using System.IO;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public static class MatchFile
	{
		public static string WriteFile(IMatchInformation match, string folder)
		{
			string filename = string.Format("{0}{1}{2}{3}", new object[]
			{
				MatchFile.FilePrefix,
				DateTime.Now.ToString(MatchFile.FileDateFormat),
				match.MatchId.ToString(),
				MatchFile.FileExtension
			});
			return MatchFile.WriteFile(match, folder, filename);
		}

		public static string WriteFile(IMatchInformation match, string folder, string filename)
		{
			if (string.IsNullOrEmpty(folder))
			{
				folder = MatchFile.DefaultFolder;
			}
			string text = Path.Combine(folder, filename);
			try
			{
				using (FileStream fileStream = File.Create(text, 2048))
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
					{
						binaryWriter.Write(1);
						binaryWriter.Write(match.MatchId.ToByteArray());
						char[] array = match.Version.ToCharArray();
						binaryWriter.Write(array.Length);
						binaryWriter.Write(array);
						BitStream bitStream = new BitStream();
						match.Data.WriteToBitStream(bitStream);
						binaryWriter.Write(bitStream.GetWrittenSize());
						binaryWriter.Write(bitStream.RawBuffer, 0, bitStream.GetWrittenSize());
						bitStream.ResetBitsWritten();
						match.States.SaveTo(binaryWriter);
						match.KeyFrames.SaveTo(binaryWriter);
						MatchFile.Log.InfoFormat("Match file saved to={0}", new object[]
						{
							text
						});
					}
				}
			}
			catch (DirectoryNotFoundException e)
			{
				MatchFile.Log.Fatal("Failed to find directory to save file, using app root. Original destination=" + text, e);
				return MatchFile.WriteFile(match, null);
			}
			catch (Exception e2)
			{
				MatchFile.Log.Fatal("Exception saving match file", e2);
				throw;
			}
			return text;
		}

		public static void ReadFile(string path, out IMatchInformation match)
		{
			try
			{
				MatchInformation matchInformation = new MatchInformation();
				using (FileStream fileStream = File.OpenRead(path))
				{
					using (BinaryReader binaryReader = new BinaryReader(fileStream))
					{
						int num = binaryReader.ReadInt32();
						if (num != 1)
						{
						}
						matchInformation.MatchId = new Guid(binaryReader.ReadBytes(16));
						int count = binaryReader.ReadInt32();
						matchInformation.Version = new string(binaryReader.ReadChars(count));
						int count2 = binaryReader.ReadInt32();
						byte[] buffer = binaryReader.ReadBytes(count2);
						BitStream bs = new BitStream(buffer);
						matchInformation.Data = new MatchData();
						matchInformation.Data.ReadFromBitStream(bs);
						matchInformation.States = new MemoryMatchBuffer(binaryReader);
						matchInformation.KeyFrames = new MemoryMatchBuffer(binaryReader);
					}
				}
				match = matchInformation;
			}
			catch (Exception e)
			{
				MatchFile.Log.Fatal("Exception loading match file", e);
				throw;
			}
		}

		private const int MatchFileVersion = 1;

		private static readonly BitLogger Log = new BitLogger(typeof(MatchFile));

		private static readonly string FilePrefix = "match";

		private static readonly string FileDateFormat = "-yyyyMMdd-HHmmss-";

		private static readonly string FileExtension = ".hmm";

		private static readonly string DefaultFolder = "./";
	}
}

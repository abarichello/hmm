using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace HeavyMetalMachines
{
	[Serializable]
	internal class HeatMap
	{
		public HeatMap(string mapVersion, float mapSize, string address, int port = 1298)
		{
			this.address = address;
			this.port = port;
			this.mapSize = mapSize;
			this.mapVersion = mapVersion;
			this.shouldStop = false;
			this.processThread = new Thread(new ThreadStart(this.ProcessThread))
			{
				Name = "HeatMap Processor"
			};
			this.processThread.Start();
			this.serviceRunning = true;
		}

		public void RegisterEvent(HeatMap.EventType type, Vector3 position)
		{
			if (!this.serviceRunning)
			{
				return;
			}
			this.eventQueue.Enqueue(new HeatMap.QueueEvent(type, position));
		}

		~HeatMap()
		{
			this.shouldStop = true;
		}

		private void ProcessThread()
		{
			byte[] array = new byte[256];
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(this.address, this.port);
			if (!socket.Connected)
			{
				return;
			}
			BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array));
			binaryWriter.Seek(0, SeekOrigin.Begin);
			binaryWriter.Write('S');
			binaryWriter.Write('R');
			binaryWriter.Write('V');
			binaryWriter.Write(130);
			binaryWriter.Write(this.mapVersion.Length);
			binaryWriter.Write(this.mapVersion.ToCharArray());
			binaryWriter.Write(this.mapSize * 2f);
			binaryWriter.Flush();
			socket.Send(array, 0, (int)binaryWriter.Seek(0, SeekOrigin.Current), SocketFlags.None);
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!this.shouldStop)
			{
				Thread.Sleep(32);
				if (!socket.Connected)
				{
					break;
				}
				if (stopwatch.ElapsedMilliseconds > 10000L)
				{
					stopwatch.Reset();
					stopwatch.Start();
					binaryWriter.Seek(0, SeekOrigin.Begin);
					binaryWriter.Write(129);
					binaryWriter.Flush();
					socket.Send(array, 0, (int)binaryWriter.Seek(0, SeekOrigin.Current), SocketFlags.None);
				}
				if (!this.eventQueue.IsEmpty)
				{
					HeatMap.QueueEvent queueEvent;
					if (this.eventQueue.Dequeue(out queueEvent))
					{
						binaryWriter.Seek(0, SeekOrigin.Begin);
						binaryWriter.Write(128);
						binaryWriter.Write((byte)queueEvent.type);
						binaryWriter.Write(queueEvent.position.x + this.mapSize);
						binaryWriter.Write(queueEvent.position.y + this.mapSize);
						binaryWriter.Write(queueEvent.position.z + this.mapSize);
						binaryWriter.Flush();
					}
					socket.Send(array, 0, (int)binaryWriter.Seek(0, SeekOrigin.Current), SocketFlags.None);
				}
			}
			this.serviceRunning = false;
			socket.Disconnect(false);
		}

		public HeatMap.CellData[] heatMapInfo;

		public readonly float mapSize;

		public string mapVersion = "Undefined";

		private volatile bool shouldStop;

		private volatile bool serviceRunning = true;

		private SafeQueue<HeatMap.QueueEvent> eventQueue = new SafeQueue<HeatMap.QueueEvent>();

		private readonly string address;

		private readonly int port;

		private Thread processThread;

		public enum EventType : byte
		{
			Death
		}

		private enum MessageType : byte
		{
			Event = 128,
			HeartBeat,
			MapVersion
		}

		internal struct CellData
		{
			public int deaths;
		}

		private struct QueueEvent : IDisposable
		{
			public QueueEvent(HeatMap.EventType type, Vector3 position)
			{
				this.type = type;
				this.position = position;
			}

			public void Dispose()
			{
			}

			public HeatMap.EventType type;

			public Vector3 position;
		}
	}
}

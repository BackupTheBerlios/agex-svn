//	mp3.NET MPEG Decoder
//	(c) 2005, mp3.NET Developers
//
//	This library is free software; you can redistribute 
//	it and/or modify it under the terms of the 
//	GNU Lesser General Public License as published by the 
//	Free Software Foundation; either version 2.1 of the License, 
//	or (at your option) any later version.
//
//	This library is distributed in the hope that it will be useful, 
//	but WITHOUT ANY WARRANTY; without even the implied warranty of 
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
//	GNU Lesser General Public License for more details.
//
//	You should have received a copy of the GNU Lesser General 
//	Public License along with this library; if not, write to the 
//	Free Software Foundation, Inc., 59 Temple Place, Suite 330, 
//	Boston, MA 02111-1307 USA

using System;
using System.IO;
using System.Collections;

namespace MP3Net
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class MP3Stream
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//HACK: test file
			MP3Stream mp3 = new MP3Stream(new FileStream("growl.mp3", FileMode.Open));
		}

		private Stream source;
		ArrayList headerstream = new ArrayList();
		byte[] comparebuffer = new byte[4];
		int currentframe = 0;

		public MP3Stream (Stream source)
		{
			source.Seek(0, SeekOrigin.Begin);
			this.source = source;
			SeekHeader();
			Frame firstframe = new Frame((Header)headerstream[0], source);
			Console.WriteLine("MDB " + firstframe.main_data_begin);
			firstframe = new Frame((Header)headerstream[1], source);
			Console.WriteLine("MDB " + firstframe.main_data_begin);
			firstframe = new Frame((Header)headerstream[2], source);
			Console.WriteLine("MDB " + firstframe.main_data_begin);
			firstframe = new Frame((Header)headerstream[3], source);
			Console.WriteLine("MDB " + firstframe.main_data_begin);
			firstframe = new Frame((Header)headerstream[4], source);
			Console.WriteLine("MDB " + firstframe.main_data_begin);
		}

		public int Frequency
		{
			get
			{
				return ((Header)headerstream[0]).frequency;
			}
		}

		public short Channels
		{
			get
			{
				return (((Header)headerstream[0]).channelmode == Header.MONO) ? (short)1 : (short)2;
			}
		}

		public int ReadFrame(byte[] buffer)
		{
			buffer = new byte[((Header)headerstream[currentframe]).framelength];
			Frame thisframe = new Frame((Header)headerstream[currentframe], source);
			//int read = thisframe.Decode(buffer);
			currentframe++;
			return 0;
		}

		private void SeekHeader()
		{
			byte[] buffer = new byte[4];
			do
			{
				// read 4 bytes (possible header)
				int read = source.Read(buffer, 0, 4);

				// less than 4 bytes read -> EOF
				if(read < 4)
				{
					break;
				}

				// check for typical header strucure
				if(SyncCheck(buffer))
				{
					// load the header bytes into the header class
					Header head = new Header(buffer, (source.Position-4));

					// extract header information and do another check for invalid values
					if(head.Init())
					{
						// if frame contains CRC bytes, copy them to the header
						if(head.crc)
						{
							source.Read(head.crcbytes, 0, 2);
							source.Seek(-2, SeekOrigin.Current);
						}

						// check if the frame length matches the header data
						byte[] syncbytes = new byte[4];
						long currentpos = source.Position;
						source.Seek(head.NextFrame(), SeekOrigin.Begin);
						source.Read(syncbytes, 0, 4);
						source.Seek(-1, SeekOrigin.Current);
						// do we have another header?
						if(SyncCheck(syncbytes))
						{
							// first header?
							if(headerstream.Count <= 0)
							{
								if(SyncCompare(buffer, syncbytes)) // check if MPEG Version / Layer matches those of the next header (to reduce chance to catch noise)
								{
									comparebuffer = (byte[])buffer.Clone();

									// we have found a frame!
									headerstream.Add(head);
									Console.WriteLine(head.ToString());
									//break;
								}  
								else 
								{
									// resume search where we left
									source.Seek(currentpos, SeekOrigin.Begin);
								}
							
							}   
							else  // not the first header, so we don't do more checks to save some time 
							{
								// we have found a frame!
								headerstream.Add(head);
								//Console.WriteLine(head.ToString());
							}  
						}

					}
				}  

				// move 1 byte per cycle (instead of 4)
				source.Seek(-3, SeekOrigin.Current);

			} while (true);
			
			Console.WriteLine("Frames: " + headerstream.Count.ToString());

			if(headerstream.Count <= 0)
			{
				throw new Exception("Could not find MPEG headers");
			}
		}

		private bool SyncCheck(byte[] buffer)
		{
			//      byte0 = 11111111      byte1 = 111xxxxx			    byte2 != 111111xx			  byte2 != 0000xxxx	  AND NOT	byte1 = xxx01xxx		   byte1 = xxxxx00x			 byte2 = xxxx11xx			 byte3 = xxxxxx10
			return (buffer[0] == 255 && ((buffer[1] & 224) == 224) && ((buffer[2] & 252) != 252) && ((buffer[2] & 240) != 0) && !(((buffer[1] & 24) == 8) || ((buffer[1] & 6) == 0) || ((buffer[2] & 12) == 12) || ((buffer[3] & 3) == 2)));
		}

		private bool SyncCompare(byte[] buffera, byte[] bufferb)
		{
			//  equal mpeg version / layer   equal mode, copyright etc.					equal frequency
			if( buffera[1] == bufferb[1] && (buffera[3] & 207) == (bufferb[3] & 207) && (buffera[2] & 12) == (bufferb[2] & 12) )
			{
				return true;
			}
			return false;
		}
	}
}

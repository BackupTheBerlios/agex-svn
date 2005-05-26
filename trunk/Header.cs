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
using System.Collections;

namespace MP3Net
{
	/// <summary>
	/// Summary description for Header.
	/// </summary>
	public class Header
	{
		public byte[] head;

		public long position;

		public short mpegversion;
		public const short MPEG_V25 = 3;
		public const short MPEG_V2 = 2;
		public const short MPEG_V1 = 1;

		public short mpeglayer;
		public const short LAYER3 = 3;
		public const short LAYER2 = 2;
		public const short LAYER1 = 1;

		public bool crc;
		public byte[] crcbytes = new byte[2];

		private int[][][] b_rates;
		public int bitrate;

		public bool padding;

		public int frequency;

		public short channelmode;
		public const short MONO = 1;
		public const short DUAL_CHANNEL = 2;
		public const short JOINT_STEREO = 3;
		public const short STEREO = 4;

		public short modeext;
		public const short BAND4TO31 = 1;
		public const short BAND8TO31 = 2;
		public const short BAND12TO31 = 3;
		public const short BAND16TO31 = 4;
		public bool stereoInt;
		public bool stereoMS;

		public int framelength;

		public Header (byte[] head, long position)
		{
			b_rates = new int[3][][];
			b_rates[0] = new int[3][];
			b_rates[1] = new int[3][];
			b_rates[0][0] = new int[]{0,32,64,96,128,160,192,224,256,288,320,352,384,416,448};
			b_rates[0][1] = new int[]{0,32,48,56,64,80,96,112,128,160,192,224,256,320,384};
			b_rates[0][2] = new int[]{0,32,40,48,56,64,80,96,112,128,160,192,224,256,320};
			b_rates[1][0] = new int[]{0,32,48,56,64,80,96,112,128,144,160,176,192,224,256};
			b_rates[1][1] = new int[]{0,8,16,24,32,40,48,56,64,80,96,112,128,144,160};
			b_rates[1][2] = b_rates[1][1];
			b_rates[2] = b_rates[1];

			this.head = head;
			this.position = position;
		}

		public long NextFrame()
		{
			long newpos = position + framelength;
			return newpos;
		}

		public override String ToString()
		{
			String output = "";
			switch(mpegversion)
			{
				case MPEG_V1:
					output += "MPEG 1 ";
					break;
				case MPEG_V2:
					output += "MPEG 2 ";
					break;
				case MPEG_V25:
					output += "MPEG 2.5 ";
					break;
			}
			switch(mpeglayer)
			{
				case LAYER1:
					output += "Layer I ";
					break;
				case LAYER2:
					output += "Layer II ";
					break;
				case LAYER3:
					output += "Layer III ";
					break;
			}
			if(crc)
				output += "CRC protected ";
			if(bitrate > 0)
				output += bitrate.ToString() + "kbps ";
			if(frequency > 0)
				output += frequency.ToString() + "Hz ";
			switch(channelmode)
			{
				case MONO:
					output += "Mono ";
					break;
				case DUAL_CHANNEL:
					output += "Dual Channel ";
					break;
				case JOINT_STEREO:
					output += "Joint Stereo ";
					break;
				case STEREO:
					output += "Stereo ";
					break;
			}
			if(padding)
				output += "Padded ";

			return output;
		}
		
		public bool Init()
		{
			// extract version
			switch(head[1] & 24)
			{
				case 0:
					mpegversion = MPEG_V25;
					break;
				case 16:
					mpegversion = MPEG_V2;
					break;
				case 24:
					mpegversion = MPEG_V1;
					break;
				default:
					return false;
			}

			// extract layer
			switch(head[1] & 6)
			{
				case 2:
					mpeglayer = LAYER3;
					break;
				case 4:
					mpeglayer = LAYER2;
					break;
				case 6:
					mpeglayer = LAYER1;
					break;
				default:
					return false;
			}

			// crc protection?
			switch(head[1] & 1)
			{
				case 1:
					crc = false;
					break;
				case 0:
					crc = true;
					break;
				default:
					return false;
			}

			// extract bitrate
			int bitrate_index = (head[2] >> 4) & 15;
			bitrate = b_rates[(mpegversion-1)][(mpeglayer-1)][bitrate_index];
			if(bitrate <= 0)
				return false;

			// extract sampling frequency
			switch(head[2] & 12)
			{
				case 0:
					if(mpegversion == MPEG_V1)
					{
						frequency = 44100;
					} 
					else if (mpegversion == MPEG_V2) 
					{
						frequency = 22050;
					} 
					else if (mpegversion == MPEG_V25)
					{
						frequency = 11025;
					}
					break;
				case 4:
					if(mpegversion == MPEG_V1)
					{
						frequency = 48000;
					} 
					else if (mpegversion == MPEG_V2) 
					{
						frequency = 24000;
					} 
					else if (mpegversion == MPEG_V25)
					{
						frequency = 12000;
					}
					break;
				case 8:
					if(mpegversion == MPEG_V1)
					{
						frequency = 32000;
					} 
					else if (mpegversion == MPEG_V2) 
					{
						frequency = 16000;
					} 
					else if (mpegversion == MPEG_V25)
					{
						frequency = 8000;
					}
					break;
				default:
					return false;
			}

			// padded frame or not?
			switch(head[2] & 2)
			{
				case 2:
					padding = true;
					break;
				case 0:
					padding = false;
					break;
				default:
					return false;
			}

			// extract channel mode
			switch(head[3] & 192)
			{
				case 0:				  
					channelmode = STEREO;
					break;
				case 64:
					channelmode = JOINT_STEREO;
					break;
				case 128:
					channelmode = DUAL_CHANNEL;
					break;
				case 192:
					channelmode = MONO;
					break;
				default:
					return false;
			}

			// mode extension for joint stereo
			if( channelmode == JOINT_STEREO )
			{
				switch(head[3] & 48)
				{
					case 0:
						modeext = BAND4TO31;
						stereoInt = false;
						stereoMS = false;
						break;
					case 16:
						modeext = BAND8TO31;
						stereoInt = true;
						stereoMS = false;
						break;
					case 32:
						modeext = BAND12TO31;
						stereoInt = false;
						stereoMS = true;
						break;
					case 48:
						modeext = BAND16TO31;
						stereoInt = true;
						stereoMS = true;
						break;
					default:
						return false;
				}
			}

			// calc framelength
			int padding_int = (padding) ? 1 : 0;
			if(mpegversion == MPEG_V1)
			{
				
				framelength = ((1152*bitrate*1000)/frequency)/8 + padding_int;
			} 
			else 
			{
				framelength = ((576*bitrate*1000)/frequency)/8 + padding_int;
			}

			//throw new Exception();

			return true;
		}
	}
}
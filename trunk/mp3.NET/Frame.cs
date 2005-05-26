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

namespace MP3Net
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Frame
	{

		public class Granule
		{
			public Channel[] channels;
		}

		public class Channel
		{
			public int part_23_length;
			public int big_value;
			public int global_gain;
			public int scalefac_compress;
			public int window_switching_flag;
			public int block_type;
			public int mixed_block_flag;
			public Region[] regions;
			public Window[] windows;
			public int region0_count;
			public int region1_count;
			public int preflag;
			public int scalefac_scale;
			public int countltable_select;

			public int slen1;
			public int slen2;
			public int slen3;
			public int slen4;
			public int nrsfb1;
			public int nrsfb2;
			public int nrsfb3;
			public int nrsfb4;
			public short id;
		}

		public class Region
		{
			public int table_select;
		}

		public class Window
		{
			public int subblock_gain;
		}

		public Header header;
		private byte[] sidedata;
		private Stream maindata;
		private Stream source;
		public int main_data_begin;
		private bool[][] scfsi = new bool[2][];
		private Granule[] granules;
		private int[][] part_23_length = new int[2][];
		private int[][] big_values = new int[2][];
		private int[][] global_gains = new int[2][];
		private int[][] sf_compress = new int[2][];

		public Frame(Header frameheader, Stream sourceStream)
		{
			scfsi[0] = new bool[4];
			scfsi[1] = new bool[4];
			part_23_length[0] = new int[2];
			part_23_length[1] = new int[2];
			big_values[0] = new int[2];
			big_values[1] = new int[2];
			global_gains[0] = new int[2];
			global_gains[1] = new int[2];
			sf_compress[0] = new int[2];
			sf_compress[1] = new int[2];
			
			header = frameheader;
			source = sourceStream;
			
			// let's get our frame's data
			byte[] data = new byte[header.framelength];
			long org_pos = source.Position;
			int headlen = (header.crc) ? 6 : 4;
			source.Seek(header.position+headlen, SeekOrigin.Begin);
			source.Read(data, 0, header.framelength-headlen);
			source.Seek(org_pos, SeekOrigin.Begin);

			// split data

			// select sidedata length
			int sidedata_len = 0;
			if(header.mpegversion == Header.MPEG_V1)
			{
				if(header.channelmode == Header.MONO)
				{
					sidedata_len = 17;
				} 
				else 
				{
					sidedata_len = 32;
				}
			} 
			else 
			{
				if(header.channelmode == Header.MONO)
				{
					sidedata_len = 9;
				} 
				else 
				{
					sidedata_len = 17;
				}
			}

			// init buffers
			sidedata = new byte[sidedata_len];
			byte[] mdata = new byte[data.Length - sidedata_len];

			// write the data into the buffers
			MemoryStream audiodata = new MemoryStream(data);
			audiodata.Read(sidedata, 0, (int)sidedata.Length);
			audiodata.Read(mdata, 0, (int)mdata.Length);

			// create stream
			maindata = new MemoryStream(mdata);

			Parse_sidedata();
		}

		private void Parse_sidedata()
		{
			// retrieve main_data_begin
			BitReader reader = new BitReader(sidedata);
			if(header.mpegversion == Header.MPEG_V1)
				main_data_begin = reader.ReadBits(9);
			else
				main_data_begin = reader.ReadBits(8);

			// skip private bits
			if(header.channelmode == Header.MONO)
				reader.SeekF(6);
			else
				reader.SeekF(5);

			// fill scalefactors information array
			if(header.mpegversion == Header.MPEG_V1)
			{
				int i;
				for(i=0; i<4; i++)
				{
					scfsi[0][i] = (reader.ReadBits(1) == 0) ? false : true;
				}
				if(header.channelmode != Header.MONO)
				{
					for(i=0; i<4; i++)
					{
						scfsi[1][i] = (reader.ReadBits(1) == 0) ? false : true;
					}
				}
			}

			granules = (header.mpegversion == Header.MPEG_V1) ? new Granule[2] : new Granule[1];
			foreach(Granule g in granules)
			{
				short chid = 0;
				g.channels = (header.channelmode == Header.MONO) ? new Channel[1] : new Channel[2];
				foreach(Channel c in g.channels)
				{
					c.id = chid;
					chid++;

					c.part_23_length = reader.ReadBits(12);
					c.big_value = reader.ReadBits(9);
					c.global_gain = reader.ReadBits(8);

					int sfc_len = (header.mpegversion == Header.MPEG_V1) ? 4 : 9;
					c.scalefac_compress = reader.ReadBits(sfc_len);

					c.window_switching_flag = reader.ReadBits(1);
					if(c.window_switching_flag == 1)
					{
						c.block_type = reader.ReadBits(2);
						c.mixed_block_flag = reader.ReadBits(1);

						c.regions = new Region[2];
						foreach(Region r in c.regions)
						{
							r.table_select = reader.ReadBits(5);
						}

						c.windows = new Window[3];
						foreach(Window w in c.windows)
						{
							w.subblock_gain = reader.ReadBits(3);
						}
					}  
					else 
					{
						c.regions = new Region[3];
						foreach(Region r in c.regions)
						{
							r.table_select = reader.ReadBits(5);
						}

						c.region0_count = reader.ReadBits(4);
						c.region1_count = reader.ReadBits(3);
					}

					if(header.mpegversion == Header.MPEG_V1)
						c.preflag = reader.ReadBits(1);

					c.scalefac_scale = reader.ReadBits(1);
					c.countltable_select = reader.ReadBits(1);


					if(header.mpegversion == Header.MPEG_V1)
					{
						switch(c.scalefac_compress)
						{
							case 0:
							case 1:
							case 2:
							case 3:
								c.slen1 = 0;
								break;
							case 4:
							case 11:
							case 12:
							case 13:
								c.slen1 = 3;
								break;
							case 5:
							case 6:
							case 7:
								c.slen1 = 1;
								break;
							case 8:
							case 9:
							case 10:
								c.slen1 = 2;
								break;
							case 14:
							case 15:
								c.slen1 = 4;
								break;
						}
						switch(c.scalefac_compress)
						{
							case 0:
							case 4:
								c.slen2 = 0;
								break;
							case 1:
							case 5:
							case 8:
							case 11:
								c.slen2 = 1;
								break;
							case 2:
							case 6:
							case 9:
							case 12:
							case 14:
								c.slen2 = 1;
								break;
							case 3:
							case 7:
							case 10:
							case 13:
							case 15:
								c.slen2 = 3;
								break;
						}

					}	 
					else 
					{	 // MPEG2
						
						if(!( (header.modeext == Header.BAND8TO31 | header.modeext == Header.BAND16TO31) && (c.id == 1) ))
						{
							if(c.scalefac_compress < 400)
							{
								c.slen1 = (c.scalefac_compress >> 4) / 5;
								c.slen2 = (c.scalefac_compress >> 4) % 5;
								c.slen3 = (c.scalefac_compress % 16) >> 2;
								c.slen4 = (c.scalefac_compress % 4);
								c.preflag = 0;
								if(c.block_type != 10)
								{
									c.nrsfb1 = 6;
									c.nrsfb2 = 5;
									c.nrsfb3 = 5;
									c.nrsfb4 = 5;
								} 
								else 
								{
									if(c.mixed_block_flag == 0)
									{
										c.nrsfb1 = c.nrsfb2 = c.nrsfb3 = c.nrsfb4 = 9;
									} 
									else 
									{
										c.nrsfb1 = 6;
										c.nrsfb2 = c.nrsfb3 = c.nrsfb4 = 9;
									}
								}
							} 
							else if(c.scalefac_compress < 500) 
							{
								c.slen1 = ((c.scalefac_compress-400) >> 2)/5;
								c.slen2 = ((c.scalefac_compress-400) >> 2)%5;
								c.slen3 = (c.scalefac_compress-400)%4;
								c.slen4 = 0;
								c.preflag = 0;
								if(c.block_type != 10)
								{
									c.nrsfb1 = 6;
									c.nrsfb2 = 5;
									c.nrsfb3 = 7;
									c.nrsfb4 = 3;
								} 
								else 
								{
									if(c.mixed_block_flag == 0)
									{
										c.nrsfb1 = 9;
										c.nrsfb2 = 9;
										c.nrsfb3 = 12;
										c.nrsfb4 = 6;
									} 
									else 
									{
										c.nrsfb1 = 6;
										c.nrsfb2 = 9;
										c.nrsfb3 = 12;
										c.nrsfb4 = 6;
									}
								}
							} 
							else if(c.scalefac_compress < 512) 
							{
								c.slen1 = (c.scalefac_compress-500)/3;
								c.slen2 = (c.scalefac_compress-500)%3;
								c.slen3 = 0;
								c.slen4 = 0;
								c.preflag = 1;
								if(c.block_type != 10)
								{
									c.nrsfb1 = 11;
									c.nrsfb2 = 10;
									c.nrsfb3 = 0;
									c.nrsfb4 = 0;
								} 
								else 
								{
									c.nrsfb2 = 18;
									c.nrsfb3 = 0;
									c.nrsfb4 = 0;
									if(c.mixed_block_flag == 0)
									{
										c.nrsfb1 = 18;
									} 
									else 
									{
										c.nrsfb1 = 15;
									}
								}
							}
						}  
						else 
						{

						}
					}
				}
			}			
		}

		//TODO: CRC check
	}

	public class BitReader
	{
		private byte[] bytedata;
		private int pos;

		public BitReader (byte[] data)
		{
			this.bytedata = data;
			pos = 0;
		}

		public int Position
		{
			get
			{
				return pos;
			}
			set
			{
				pos = value;
			}
		}

		public void SeekF( int nr )
		{
			pos += nr;
		}

		public void SeekB( int nr )
		{
			pos -= nr;
			if(pos < 0)
			{
				pos = 0;
			}
		}

		public int ReadBits(int nr)
		{
			int bytenr = Convert.ToInt32(Math.Floor(pos / 8));
			int i;
			int bytepos = pos - (bytenr * 8);
			int returnval = 0;
			for(i = 0; i < nr; i++)
			{
				int bitselector = 0;
				switch(bytepos)
				{
					case 0:
						bitselector = 128;
						break;
					case 1:
						bitselector = 64;
						break;
					case 2:
						bitselector = 32;
						break;
					case 3:
						bitselector = 16;
						break;
					case 4:
						bitselector = 8;
						break;
					case 5:
						bitselector = 4;
						break;
					case 6:
						bitselector = 2;
						break;
					case 7:
						bitselector = 1;
						break;
					case 8:
						bytenr++;
						bytepos = 0;
						bitselector = 128;
						break;
				}
				if(bytenr >= bytedata.Length)
				{
					break;
				}
				returnval += ((bytedata[bytenr] & bitselector) == bitselector) ? Convert.ToInt32(Math.Pow(2, nr - (i+1))) : 0;
				bytepos++;
			}
			return returnval;
		}
	}
}

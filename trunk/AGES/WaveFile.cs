#region LGPL License
/*
Axiom Game Engine Sound Library
Copyright (C) 2005 AGES developers

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.IO;

namespace Axiom.SoundSystems
{

	public struct WaveFile
	{
		private int freq;
		private short bits;
		private short chan;
		private Stream data;
		private Stream file;
		
		public int Frequency
		{
			get{
				return freq;
			}
			set{
				freq = value;
			}
		}
		
		public short Bits
		{
			get{
				return bits;
			}
			set{
				bits = value;
			}
		}
		
		public short Channels
		{
			get{
				return chan;
			}
			set{
				chan = value;
			}
		}
		
		public Stream Data
		{
			get{
				return data;
			}
			set{
				data = value;
			}
		}
		
		public Stream WavFile
		{
			get{
				return file;
			}
			set{
				file = value;
			}
		}
	}
}

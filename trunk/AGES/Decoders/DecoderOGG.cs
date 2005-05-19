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
using csogg;
using csvorbis;

namespace Axiom.SoundSystems.Decoders
{
	/// <summary>
	/// The OGG-Vorbis decoder
	/// </summary>
	public class DecoderOGG : IDecoder
	{
		/// <summary>
		/// Decodes the ogg-vorbis file
		/// </summary>
		/// <param name="input">Stream of the ogg-vorbis file</param>
		/// <returns>PCM-Wave version of the input</returns>
		public WaveFile Decode(Stream input)
		{
			MemoryStream output = new MemoryStream();
			WaveFile wf = new WaveFile();
			
			VorbisFile vf = new VorbisFile((FileStream)input, null, 0);
			Info inf = vf.getInfo(-1);
			
			wf.Channels = (short)inf.channels;
			wf.Frequency = inf.rate;
			wf.Bits = 16;
			
			Axiom.Core.LogManager.Instance.Write("SoundSystem: File is Ogg Vorbis "+inf.version.ToString()+" "+inf.rate.ToString()+"Hz, "+inf.channels.ToString()+" channels");
			
			int bufferlen = 4096;
			int result = 1;
			byte[] buffer = new byte[bufferlen];
			int[] section = new int[1];
			while(result != 0)
			{
				result = vf.read(buffer, bufferlen, 0, 2, 1, section);
				output.Write(buffer, 0, result);
			}
			
			output.Seek(0, SeekOrigin.Begin);
			wf.Data = output;
			
			return wf;
		}
	}
}

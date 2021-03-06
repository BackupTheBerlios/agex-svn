#region GPL License
/*
Axiom Game Engine Sound Library
Copyright (C) 2005 AGES developers

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public
License as published by the Free Software Foundation; either
version 2 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

#region Info
// MPEG Layer 3 Decoder in seperate plugin because of possible license problems
// Read more on http://www.mp3licensing.com/help/developer.html
//
// Using Mp3Sharp - Managed Mp3 decoder
// Copyright by Robert Burke (rob@mle.ie)
#endregion

// NOTE: 	DUE TO THE USE OF MP3SHARP THIS PLUGIN IS LICENSED UNDER THE GPL
// 			UNLIKE THE REST OF AGES, WHICH IS AVAILABLE UNDER THE LGPL
//			(maybe someone could rewrite this to a LGPL compatible plugin)

using System;
using System.IO;
using Mp3Sharp;
using Axiom.SoundSystems;

namespace Axiom.SoundSystems.Decoders
{
	/// <summary>
	/// The Mp3 decoder
	/// (This assembly is GPL licensed)
	/// </summary>
	public class DecoderMP3 : IDecoder
	{
		/// <summary>
		/// Decodes the mp3 file
		/// </summary>
		public WaveFile Decode(Stream input)
		{
			//TODO: Still AWFUL playback quality
			
			Mp3Stream mp3 = new Mp3Stream(input);
			
			mp3.Seek(0, SeekOrigin.Begin);
			mp3.DecodeFrames(1);
			
			WaveFile wf = new WaveFile();
			wf.Bits = 16;
			wf.Channels = mp3.ChannelCount;
			wf.Frequency = mp3.Frequency;
			System.Console.WriteLine(mp3.Length);
			MemoryStream data = new MemoryStream();
			int buffersize = 4*4096;
			byte[] buffer = new byte[buffersize];
			int result = 1;
			while(true)
			{
				result = mp3.Read(buffer, 0, buffersize);
				data.Write(buffer, 0, result);
				if(result != buffersize)
				{
					break;
				}
			}
			
			data.Seek(0, SeekOrigin.Begin);
			wf.Data = data;
			
			Axiom.Core.LogManager.Instance.Write("SoundSystem: File is MPEG Layer III "+wf.Frequency+"Hz, "+wf.Channels+" channels");
			
			return wf;
		}
	}
}

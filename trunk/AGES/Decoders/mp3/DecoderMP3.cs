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

namespace Axiom.SoundSystems.Decoders
{
	/// <summary>
	/// The Mp3 decoder
	/// </summary>
	public class DecoderMP3 : IDecoder
	{
		/// <summary>
		/// Decodes the mp3 file
		/// </summary>
		public Stream Decode(Stream input)
		{
			Mp3Stream mp3 = new Mp3Stream(input);
			return mp3;
		}
	}
}

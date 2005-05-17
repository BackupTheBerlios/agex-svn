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

namespace Axiom.SoundSystems.Decoders
{
	/// <summary>
	/// The Wave decoder
	/// </summary>
	public class DecoderWAV : IDecoder
	{
		/// <summary>
		/// Decodes the Wave file (ie. just returns the input)
		/// </summary>
		/// <param name="input">The incoming stream</param>
		/// <returns>The outgoing stream</returns>
		public Stream Decode(Stream input)
		{
			return input;
		}
	}
}

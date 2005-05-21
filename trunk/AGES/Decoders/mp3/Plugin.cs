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

#region Using Statements

using System;
using System.Collections;
using Axiom.Core;
using Axiom.SoundSystems;

#endregion

namespace Axiom.SoundSystems.Decoders
{
	/// <summary>
	/// The Axiom plugin for the MP3 decoder
	/// </summary>
	public class MP3Plugin : IPlugin 
	{ 
		/// <summary>
		/// Register the MP3 decoder and the .mp3 extension with the <see cref="Axiom.SoundSystems.FileManager">FileManager</see>
		/// </summary>
		public void Start() 
		{ 
			if(FileManager.Instance == null)
			{
				new FileManager();
			}
			FileManager.Instance.Extensions.Add("mp3");
			FileManager.Instance.DecoderTypes.Add(typeof(Axiom.SoundSystems.Decoders.DecoderMP3));
		} 

		/// <summary>
		/// Remove registration of the MP3 decoder and .mp3 extension from the <see cref="Axiom.SoundSystems.FileManager">FileManager</see>
		/// </summary>
		public void Stop() 
		{ 
			FileManager.Instance.Extensions.Remove("mp3");
			FileManager.Instance.DecoderTypes.Remove(typeof(Axiom.SoundSystems.Decoders.DecoderMP3));
		} 
	}
}

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
using System.Collections;
using Axiom.Core;
using Axiom.SoundSystems.Decoders;

namespace Axiom.SoundSystems
{
	/// <summary>
	/// Class FileManager, takes care of the loading and decoding of audio files
	/// </summary>
	public class FileManager
	{
		private ArrayList extensionlist = new ArrayList();
		private ArrayList typelist = new ArrayList();
		
		private static FileManager instance;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public FileManager()
		{
			if(instance == null)
			{
				// load the built-in decoders
				extensionlist.Add("wav");
				typelist.Add(typeof(Axiom.SoundSystems.Decoders.DecoderWAV));
				extensionlist.Add("ogg");
				typelist.Add(typeof(Axiom.SoundSystems.Decoders.DecoderOGG));
	
				instance = this;
			} else {
				return;
			}
		}
		
		/// <summary>
		/// Loads a file with a given name from the Common Resources and decodes it
		/// </summary>
		/// <param name="filename">The name of the file to load</param>
		/// <returns>A stream with the decoded file contents</returns>
		public WaveFile Load(string filename)
		{
			// load the stream with the resourcemanager
			Stream originalFile = ResourceManager.FindCommonResourceData(filename);
			
			// extract the file extension
			string extension = "";
			Array parts = filename.Split(new char[]{'.'});
			foreach(string ext in parts)
			{
				extension = ext.ToLower();
			}
			
			// search through extensionlist for the current extension
			int i=0;
			for(i=0; i < extensionlist.Count; i++)
			{
				if( extension == (string)extensionlist[i] )
				{
					// if the current extension is found, create an instance of the decoder and return the decoded file
					IDecoder decoder = (IDecoder)Activator.CreateInstance((Type)typelist[i]);
					return decoder.Decode(originalFile);
				}
			}
			
			throw new AxiomException("Unkown or unsupported sound format " + filename + ", please check extension.");
			//return null;
		}
		
		/// <summary>
		/// Writes a line to the log with all loaded decoder types
		/// </summary>
		public void LogSupportedFiles()
		{
			string extensions = "";
			foreach(string ext in extensionlist)
			{
				extensions = extensions + " " + ext + ",";
			}
			LogManager.Instance.Write("SoundSystem: Support for file types {0} is loaded", extensions);
		}
		
		/// <summary>
		/// Singleton Instance
		/// </summary>
		public static FileManager Instance
		{
			get{
				return instance;
			}
		}
		
		/// <summary>
		/// The list of known extensions
		/// </summary>
		public ArrayList Extensions
		{
			get{
				return extensionlist;
			}
		}
		
		/// <summary>
		/// The list of decoders
		/// </summary>
		public ArrayList DecoderTypes
		{
			get{
				return typelist;
			}
		}
	}
}

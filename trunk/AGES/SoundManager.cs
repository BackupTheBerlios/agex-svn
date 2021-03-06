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

// Some general remarks and todos
// TODO: split buffer and source
// TODO: Music class for streaming long files

using System;
using System.Collections;
using System.Threading;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.SoundSystems
{

	/// <summary>
	/// Abstract Class SoundManager : IDisposable, Keeps track of loaded sounds and updates 
	/// sound and listener positions according to scene changes
	/// </summary>
	public abstract class SoundManager : IDisposable
	{
		/// <summary>
		/// Singleton instance
		/// </summary>
		protected static SoundManager instance;
		
		/// <summary>
		/// The last assigned sound id
		/// </summary>
		protected int lastid = 0;
		
		/// <summary>
		/// List of all loaded sounds
		/// </summary>
		protected ArrayList soundlist = new ArrayList();
		
		/// <summary>
		/// The currently active RenderWindow
		/// </summary>
		protected RenderWindow window;
		
		/// <summary>
		/// The camera associated with the listener position
		/// </summary>
		protected Camera cam;
		
		/// <summary>
		/// Last camera position, to check if it's changed
		/// </summary>
		protected Axiom.MathLib.Vector3 lastcamposition = Vector3.Zero;
		
		/// <summary>
		/// Last camera orientation, to check if it's changed
		/// </summary>
		protected Axiom.MathLib.Quaternion lastcamorientation = Quaternion.Zero;

		/// <summary>
		/// Constructor
		/// </summary>
		public SoundManager() {
			soundlist.Add(null);
			Root.Instance.FrameStarted += new FrameEvent(FrameUpdate);
		}
	
		/// <summary>
		/// Singleton Instance
		/// </summary>
		public static SoundManager Instance
		{
			get{
				return instance;	
			}
		}
		
		/// <summary>
		/// IDisposable implementation
		/// </summary>
		public virtual void Dispose()
		{
			Root.Instance.FrameStarted -= new FrameEvent(FrameUpdate);
			instance = null;
		}
		
		/// <summary>
		/// Load a sound from the common resource (defined in EngineConfig.xml)
		/// </summary>
		/// <param name="filename">The sound's filename, extension is used to determine the encoding</param>
		/// <param name="type">The sound type (Simple or 3D)</param>
		/// <returns>The loaded sound SceneObject</returns>
		public abstract Sound LoadSound(string filename, short type);
		
		/// <summary>
		/// Load a sound from the common resource (defined in EngineConfig.xml) in a new, paralel thread.
		/// (Currently only for DirectSound)
		/// </summary>
		/// <param name="filename">The sound's filename, extension is used to determine the encoding</param>
		/// <param name="type">The sound type (Simple or 3D)</param>
		/// <returns>The ID of the sound, so it can be called using <see cref="GetSound">GetSound()</see></returns>
		public virtual int PreLoadSound(string filename, short type)
		{
			Axiom.SoundSystems.LoadInfo info = new Axiom.SoundSystems.LoadInfo(filename, lastid, type);
			if(!ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadedLoadSound), info))
			{
				throw new AxiomException("Unable to queue thread for preloading");
			}
			
			// update the ID counter
			lastid++;
			
			return (lastid-1);			
		}
		
		/// <summary>
		/// Entry point for the loading thread
		/// </summary>
		/// <param name="stateInfo">State information, used to access the filename, id and type of the new sound</param>
		protected abstract void ThreadedLoadSound (object stateInfo);
		
		/// <summary>
		/// Get a previously loaded sound
		/// </summary>
		/// <param name="ID">The sound's ID</param>
		/// <returns>The sound SceneObject requested or null if it's not found</returns>
		public virtual Sound GetSound(int ID)
		{
			if(ID < (soundlist.Count-1))
			{
				Sound thissound;
				do
				{
					thissound = (Sound)soundlist[ID];
				} while(thissound == null );
				
				return thissound;
			} else {
				return null;
			}
		}
		
		/// <summary>
		/// Update the positions according to scene changes
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected virtual void FrameUpdate(Object source, FrameEventArgs e)
		{
			foreach(Sound sound in soundlist)
			{
				if(sound != null)
					sound.UpdatePosition();
			}
		}
		
		/// <summary>
		/// Do some final initialisations
		/// </summary>
		/// <param name="renderwindow">The target RenderWindow</param>
		/// <param name="camera">The 'listening' camera</param>
		public virtual void SetRenderWindow(RenderWindow renderwindow, Camera camera)
		{
			FileManager.Instance.LogSupportedFiles();
			this.window = renderwindow;
			this.cam = camera;
		}
		
		/// <summary>
		/// The camera's velocity for Doppler effect calculations
		/// </summary>
		public abstract Vector3 CameraVelocity
		{
			get;
			set;
		}
		
		/// <summary>
		/// Factor that sets the sound decay in the medium (default is 1)
		/// </summary>
		public abstract float RolloffFactor
		{
			get;
			set;
		}
		
	}
}

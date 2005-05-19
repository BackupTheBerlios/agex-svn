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
using Axiom.Core;
using Axiom.Graphics;
using Axiom.SoundSystems;
using Microsoft.DirectX.DirectSound;
using Microsoft.DirectX;
using Buffer = Microsoft.DirectX.DirectSound.Buffer;

namespace Axiom.SoundSystems.DirectSound
{

	public class DirectSoundManager : SoundManager
	{
		private Device device;
		private Listener3D listener;
		
		public DirectSoundManager () : base()
		{
			try{
				device = new Device();
			} catch (SoundException)
			{
				throw new AxiomException("Unable to load DirectSound", null);
			}

			instance = this;
		}
	
		public Device Device
		{
			get{
				return device;
			}
		}
		
		public override Axiom.SoundSystems.Sound LoadSound(string filename, short type)
		{
			// create the sound and add it to our list
			Axiom.SoundSystems.Sound sound = new Axiom.SoundSystems.DirectSound.Sound(filename, lastid, type);
			soundlist.Add(sound);
			
			// update the ID counter
			lastid++;
			
			return sound;
		}
		
		public override void SetRenderWindow(RenderWindow renderwindow, Camera camera)
		{
			this.window = renderwindow;
			this.cam = camera;
			
			// link the device to our current System.Windows.Form (since we need DirectX we're sure that we're in Windows)
			device.SetCooperativeLevel((System.Windows.Forms.Control)window.Handle, CooperativeLevel.Priority);
			
			// create a buffer for the listener
			BufferDescription desc = new BufferDescription();
			desc.Control3D = true;
			desc.PrimaryBuffer = true;
			Buffer lbuffer = new Buffer(desc, device);
			listener = new Listener3D(lbuffer);
			
			// let the log know that we're using DirectSound and it's set up
			LogManager.Instance.Write("DirectSound SoundSystem initialised");
		}
		
		protected override void FrameUpdate(Object source, FrameEventArgs e)
		{
			base.FrameUpdate(source, e);
			
			// if the camera moved
			if(cam.WorldPosition.x != lastcamposition.x || cam.WorldPosition.y != lastcamposition.y || cam.WorldPosition.z != lastcamposition.z)
			{
				listener.Position = new Vector3(cam.WorldPosition.x, cam.WorldPosition.y, cam.WorldPosition.z);
				lastcamposition = cam.WorldPosition;
			}
			
			// if the camera turned
			if(cam.WorldOrientation.w != lastcamorientation.w || cam.WorldOrientation.x != lastcamorientation.x || cam.WorldOrientation.y != lastcamorientation.y || cam.WorldOrientation.z != lastcamorientation.z)
			{
				Axiom.MathLib.Vector3 top = cam.WorldOrientation.YAxis;
				Axiom.MathLib.Vector3 front = cam.WorldOrientation.ZAxis;
				listener.Orientation = new Listener3DOrientation(new Vector3(front.x, front.y, front.z), new Vector3(top.x, top.y, top.z));	
				lastcamorientation = cam.WorldOrientation;
			}
			
			// we have updated all sound settings for this frame, so let DirectX recalculate the output sound with the new settings
			listener.CommitDeferredSettings();
			
		}
		
		public override Axiom.MathLib.Vector3 CameraVelocity
		{
			get{
				return new Axiom.MathLib.Vector3(listener.Velocity.X, listener.Velocity.Y, listener.Velocity.Z);
			}
			set{
				Axiom.MathLib.Vector3 speed = value;
				listener.Velocity = new Vector3(speed.x, speed.y, speed.z);
			}
		}
		
		public override float RolloffFactor
		{
			set{
				listener.RolloffFactor = value;
			}
			get{
				return listener.RolloffFactor;
			}
		}
		
		public override void Dispose()
		{
			base.Dispose();
			
			listener.Dispose();
			device.Dispose();
		}
	}

}

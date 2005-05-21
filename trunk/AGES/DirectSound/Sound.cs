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
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using Axiom.Core;

namespace Axiom.SoundSystems.DirectSound
{

	public class Sound : Axiom.SoundSystems.Sound
	{
		private SecondaryBuffer sound;
		private BufferDescription soundDesc;
		private Buffer3D threeDsound;
		
		public Sound(string filename, int ID, short type) : base(filename, ID)
		{
			// get the file data
			WaveFile wf = FileManager.Instance.Load(filename);
			
			if(wf.WavFile != null) // we have a wave file with headers
			{
				// set up the buffer properties
				soundDesc = new BufferDescription();
				soundDesc.GlobalFocus = false;
				soundDesc.ControlVolume = true;
				
				// enable 3D features for 3D sounds
				if(type == Sound.THREED_SOUND)
				{
					soundDesc.Control3D = true;
					soundDesc.Mute3DAtMaximumDistance = true;
				}
				
				// load the wave file from the stream into the buffer
				sound = new SecondaryBuffer(wf.WavFile, soundDesc, ((DirectSoundManager)SoundManager.Instance).Device);
				
			} else { // we have only raw PCM encoded sound data (usually from a decoder)
				
				// convert the format settings
				WaveFormat wfo = new WaveFormat();
				wfo.BitsPerSample = wf.Bits;
				wfo.Channels = wf.Channels;
				wfo.SamplesPerSecond = wf.Frequency;
				wfo.BlockAlign = (short)(wf.Bits*wf.Channels / 8);
				wfo.FormatTag = WaveFormatTag.Pcm;
				wfo.AverageBytesPerSecond = wf.Frequency * wfo.BlockAlign;
				
				// set up buffer properties
				soundDesc = new BufferDescription(wfo);
				soundDesc.GlobalFocus = false;
				soundDesc.ControlVolume = true;
				soundDesc.BufferBytes = (int)wf.Data.Length;
				
				// enable 3D features for 3D sounds
				if(type == Sound.THREED_SOUND)
				{
					soundDesc.Control3D = true;
					soundDesc.Mute3DAtMaximumDistance = true;
				}
				
				// initialise the buffer and copy the (raw data) stream into it
				sound = new SecondaryBuffer(soundDesc, ((DirectSoundManager)SoundManager.Instance).Device);	
				sound.Write(0, wf.Data, (int)wf.Data.Length, LockFlag.EntireBuffer);
			}
			
			// create a 3D buffer for 3D sounds
			if(type == Sound.THREED_SOUND)
			{
				threeDsound = new Buffer3D(sound);
				threeDsound.Mode = Mode3D.Normal;
				threeDsound.Deferred = true;
			}
		}
		
		public override void Play(bool loop)
		{

			if(loop)
			{
				sound.Play(0, BufferPlayFlags.Looping);	
			} else {
				sound.Play(0, BufferPlayFlags.Default);
			}
		}
		
		public override void Pause()
		{
			sound.Stop();
		}
		
		public override void Stop()
		{
			sound.Stop();
			sound.SetCurrentPosition(0);
		}
		public override void Dispose()
		{
			base.Dispose();
			
			sound.Dispose();
			if(threeDsound != null)
			{
				threeDsound.Dispose();
			}
		}
		
		public override Axiom.MathLib.Vector3 WorldPosition
		{
			get{
				return new Axiom.MathLib.Vector3(threeDsound.Position.X, threeDsound.Position.Y, threeDsound.Position.Z);
			}
		}
		
		protected override void SetPosition(Axiom.MathLib.Vector3 newposition)
		{
			threeDsound.Position = new Vector3(newposition.x, newposition.y, newposition.z);
		}
		
		public override int[] ConeAngles
		{
			get{
				return new int[]{threeDsound.ConeAngles.Inside, threeDsound.ConeAngles.Outside};
			}
			set{
				Angles angles = new Angles();
				angles.Inside = value[0];
				angles.Outside = value[1];
				threeDsound.ConeAngles = angles;
			}
		}
		
		protected override void SetConeDirection(Axiom.MathLib.Vector3 direction)
		{
			Axiom.MathLib.Vector3 vector = direction;
			threeDsound.ConeOrientation = new Vector3(vector.x, vector.y, vector.z);
		}
		
		public override int OutsideVolume
		{
			get{
				return threeDsound.ConeOutsideVolume;
			}
			set{
				threeDsound.ConeOutsideVolume = value;
			}
		}
		
		public override float MaxDistance
		{
			get{
				return threeDsound.MaxDistance;
			}
			set{
				threeDsound.MaxDistance = value;
			}
		}
		
		public override Axiom.MathLib.Vector3 Velocity
		{
			get{
				return new Axiom.MathLib.Vector3(threeDsound.Velocity.X, threeDsound.Velocity.Y, threeDsound.Velocity.Z);
			}
			set{
				Axiom.MathLib.Vector3 vector = value;
				threeDsound.Velocity = new Vector3(vector.x, vector.y, vector.z);
			}
		}
		
		public override int Volume
		{
			get{
				return sound.Volume;
			}
			set{
				sound.Volume = value;
			}
		}
	}
}

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
			soundDesc = new BufferDescription();
			soundDesc.GlobalFocus = false;
			soundDesc.ControlVolume = true;
			
			System.IO.Stream filestream = FileManager.Instance.Load(filename);
			int streamlen = (int) filestream.Length;
			
			switch(type)
			{
				case SIMPLE_SOUND:
					sound = new SecondaryBuffer(filestream, streamlen, soundDesc, ((DirectSoundManager)SoundManager.Instance).Device);
					break;
				case THREED_SOUND:
					soundDesc.Control3D = true;
					soundDesc.Mute3DAtMaximumDistance = true;
					sound = new SecondaryBuffer(filestream, streamlen, soundDesc, ((DirectSoundManager)SoundManager.Instance).Device);
					threeDsound = new Buffer3D(sound);
					threeDsound.Mode = Mode3D.Normal;
					threeDsound.Deferred = true;
					break;
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
		
		public override Axiom.MathLib.Vector3 ConeDirection
		{
			get{
				//TODO: Relative to SceneNode
				return new Axiom.MathLib.Vector3(threeDsound.ConeOrientation.X, threeDsound.ConeOrientation.Y, threeDsound.ConeOrientation.Z);
			}
			set{
				//TODO: Relative to SceneNode
				Axiom.MathLib.Vector3 vector = value;
				threeDsound.ConeOrientation = new Vector3(vector.x, vector.y, vector.z);
			}
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

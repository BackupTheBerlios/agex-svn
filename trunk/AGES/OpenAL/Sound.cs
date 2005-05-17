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
using Axiom.MathLib;
using Tao.OpenAl;

namespace Axiom.SoundSystems.OpenAL
{

	public class Sound : Axiom.SoundSystems.Sound
	{
		private int sound;
		private int source;
		private Vector3 worldposition = Vector3.Zero;
		
		public Sound(string filename, int ID, short type) : base(filename, ID)
		{
			switch(type)
			{
				case SIMPLE_SOUND:
					Al.alGenBuffers(1, out sound);
					if(Al.alGetError() != Al.AL_NO_ERROR) {
						throw new AxiomException("Unable to create buffer for file " + filename, null);
					}
					
					MemoryStream file = (MemoryStream)ResourceManager.FindCommonResourceData(filename);
					
					int format, size, frequency, loop;
					byte[] data;
					Alut.alutLoadWAVMemory(file.GetBuffer(), out format, out data, out size, out frequency, out loop);
					if(data == null)
					{
						throw new AxiomException("Unable to load " + filename + " into buffer");
					}
					
					Al.alBufferData(sound, format, data, size, frequency);
					Alut.alutUnloadWAV(format, out data, size, frequency);

					Al.alGenSources(1, out source);
					if(Al.alGetError() != Al.AL_NO_ERROR)
					{
						throw new AxiomException("Unable to create source for file " + filename, null);
					}
					
					Al.alSourcei(source, Al.AL_BUFFER, sound);
					
					break;
				case THREED_SOUND:
					break;
			}
		}
		
		public override void Play(bool loop)
		{
			if(loop)
			{
				Al.alSourcei(source, Al.AL_LOOPING, Al.AL_TRUE);
			} else {
				Al.alSourcei(source, Al.AL_LOOPING, Al.AL_FALSE);
			}
			Al.alSourcePlay(source);
		}
		
		public override void Dispose()
		{
			Al.alDeleteSources(1, ref source);
			Al.alDeleteBuffers(1, ref sound);
			base.Dispose();
		}
		
		protected override void SetPosition(Axiom.MathLib.Vector3 newposition)
		{
			//TODO: Implement
		}
		
		public override Vector3 WorldPosition
		{
			get{
				return worldposition;
			}
		}
		
		public override int[] ConeAngles
		{
			get{
				int inside = 0, outside = 0;
				Al.alSourcei(source, Al.AL_CONE_INNER_ANGLE, inside);
				Al.alSourcei(source, Al.AL_CONE_OUTER_ANGLE, outside);
				return new int[]{inside, outside};
			}
			set{
				Al.alSourcei(source, Al.AL_CONE_INNER_ANGLE, value[0]);
				Al.alSourcei(source, Al.AL_CONE_OUTER_ANGLE, value[1]);
			}
		}
		
		public override Vector3 ConeDirection
		{
			get{
				float[] vector = new float[]{0,0,0};
				Al.alGetSourcefv(source, Al.AL_DIRECTION, vector);
				return new Vector3(vector[0], vector[1], vector[2]);
			}
			set{
				float[] vector = new float[]{value.x, value.y, value.z};
				Al.alSourcefv(source, Al.AL_DIRECTION, vector);
			}
		}
		
		public override int OutsideVolume
		{
			get{
				float val;
				Al.alGetSourcef(source, Al.AL_CONE_OUTER_GAIN, out val);
				return GetDirectSoundVolume(val);
			}
			set{
				Al.alSourcef(source, Al.AL_CONE_OUTER_GAIN, GetOpenALVolume(value));
			}
		}
		
		public override float MaxDistance
		{
			get{
				float val;
				Al.alGetSourcef(source, Al.AL_MAX_DISTANCE, out val);
				return val;
			}
			set{
				Al.alSourcef(source, Al.AL_MAX_DISTANCE, value);
			}
		}
		
		public override Vector3 Velocity
		{
			get{
				float[] val = new float[]{0,0,0};
				Al.alGetSourcefv(source, Al.AL_VELOCITY, val);
				return new Vector3(val[0], val[1], val[2]);
			}
			set{
				float[] vector = new float[]{value.x, value.y, value.z};
				Al.alSourcefv(source, Al.AL_VELOCITY, vector);
			}
		}
		
		public override int Volume
		{
			get{
				float val;
				Al.alGetSourcef(source, Al.AL_GAIN, out val);
				return GetDirectSoundVolume(val);
			}
			set{
				Al.alSourcef(source, Al.AL_GAIN, GetOpenALVolume(value));
			}
		}
		
		protected float GetOpenALVolume(int directSoundVolume)
		{
			float newvalue = (float)directSoundVolume / 1000f + 1f;
			if(newvalue > 1f)
			{
				return 1f;
			}
			if(newvalue < 0f)
			{
				return 0f;
			}
			return newvalue;
		}
		
		protected int GetDirectSoundVolume(float openALVolume)
		{
			int newvalue = Int32.Parse(Math.Round((openALVolume - 1) * 1000).ToString());
			if(newvalue < -10000)
			{
				return -10000;
			}
			if(newvalue > 0)
			{
				return 0;
			}
			return newvalue;
		}
	}
}

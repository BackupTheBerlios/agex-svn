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
using Axiom.SoundSystems;
using Tao.OpenAl;

namespace Axiom.SoundSystems.OpenAL
{

	public class Sound : Axiom.SoundSystems.Sound
	{
		private int sound;
		private int source;
		private short soundtype;
		private float rolloff = 1;
		
		public Sound(string filename, int ID, short type) : base(filename, ID)
		{
			this.soundtype = type;
			
			// initialize the buffer
			Al.alGenBuffers(1, out sound);
			
			WaveFile file = FileManager.Instance.Load(filename);
			
			int format, size, frequency, loop;
			byte[] data;			

			if ( (file.WavFile) != null )
			{ 
				// we have a full wave file with headers
				// read the stream into a byte array
				byte[] buffer = new byte[file.WavFile.Length];
				file.WavFile.Read(buffer, 0, (int)file.WavFile.Length);
				
				Alut.alutLoadWAVMemory(buffer, out format, out data, out size, out frequency, out loop);
				
			} else { // we only have raw PCM encoded data
				
				// read the stream into a byte array
				data = new byte[file.Data.Length];
				file.Data.Read(data, 0, (int)file.Data.Length);
				size = (int) file.Data.Length;
				frequency = file.Frequency;
				format = 0;
				
				// get the data format from the Channels and Bits properties
				switch(file.Bits)
				{
					case 8:
						switch(file.Channels)
						{
							case 1:
								format = Al.AL_FORMAT_MONO8;
								break;
							case 2:
								format = Al.AL_FORMAT_STEREO8;
								break;
						}
						break;
					case 16:
						switch(file.Channels)
						{
							case 1:
								format = Al.AL_FORMAT_MONO16;
								break;
							case 2:
								format = Al.AL_FORMAT_STEREO16;
								break;
						}
						break;
				}
			}
			
			// fill the buffer
			Al.alBufferData(sound, format, data, size, frequency);
			
			if(file.WavFile != null) // if we loaded a Wave file we can unload it now
			{
				Alut.alutUnloadWAV(format, out data, size, frequency);
			}
			
			// create a sound source for the buffer
			Al.alGenSources(1, out source);

			// link the buffer and sound source
			Al.alSourcei(source, Al.AL_BUFFER, sound);
			
			if(type == Sound.SIMPLE_SOUND)
			{
				Al.alSourcei(source, Al.AL_DISTANCE_MODEL, Al.AL_NONE);
				rolloff = 0;
				Al.alSourcef(source, Al.AL_ROLLOFF_FACTOR, 0);
				Al.alSourcei(source, Al.AL_SOURCE_RELATIVE, Al.AL_TRUE);
				Al.alSourcefv(source, Al.AL_POSITION, new float[]{0,0,0});
				
			} else if (type == Sound.THREED_SOUND) {
				
				Al.alSourcei(source, Al.AL_DISTANCE_MODEL, Al.AL_INVERSE_DISTANCE);
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
		
		public override void Pause()
		{
			Al.alSourcePause(source);
		}
		
		public override void Stop()
		{
			Al.alSourceStop(source);
		}
		
		public override void Dispose()
		{
			Al.alDeleteSources(1, ref source);
			Al.alDeleteBuffers(1, ref sound);
			base.Dispose();
		}
		
		public override void UpdatePosition()
		{
			base.UpdatePosition();
			
			// check if the (global) rolloff factor changed and adapt to it
			if(SoundManager.Instance.RolloffFactor != rolloff && this.soundtype == Sound.THREED_SOUND)
			{
				rolloff = SoundManager.Instance.RolloffFactor;
				Al.alSourcef(source, Al.AL_ROLLOFF_FACTOR, rolloff);
			}
		}
		
		protected override void SetPosition(Axiom.MathLib.Vector3 newposition)
		{
			float[] vector = new float[]{newposition.x, newposition.y, newposition.z};
			Al.alSourcefv(source, Al.AL_POSITION, vector);
		}
		
		public override Vector3 WorldPosition
		{
			get{
				float[] vector = new float[3];
				Al.alGetSourcefv(source, Al.AL_POSITION, vector);
				return new Vector3(vector[0], vector[1], vector[2]);
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
		
		protected override void SetConeDirection(Axiom.MathLib.Vector3 direction)
		{
			Axiom.MathLib.Vector3 vector = direction;
			float[] vector2 = new float[]{vector.x, vector.y, vector.z};
			Al.alSourcefv(source, Al.AL_DIRECTION, vector2);			
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
			float newvalue = (float)directSoundVolume / 10000f + 1f;
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
			int newvalue = Convert.ToInt32((openALVolume - 1) * 10000);
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

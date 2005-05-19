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
using Axiom.MathLib;
using Axiom.SoundSystems;
using Tao.OpenAl;

namespace Axiom.SoundSystems.OpenAL
{

	public class OpenALSoundManager : SoundManager
	{
		private Vector3 listenerVelocity = Vector3.Zero;
		
		public OpenALSoundManager () : base()
		{
			Alut.alutInit();
			Al.alGetError();
						
			LogManager.Instance.Write("OpenAL SoundSystem initialised");
		}
	
		public override Axiom.SoundSystems.Sound LoadSound(string filename, short type)
		{
			return null;
		}
		
		public override Vector3 CameraVelocity
		{
			get{
				return listenerVelocity;
			}
			set{
				listenerVelocity = value;
				float[] vector = new float[]{listenerVelocity.x, listenerVelocity.y, listenerVelocity.z};
				Al.alListenerfv(Al.AL_VELOCITY, vector);
			}
		}

		public override float RolloffFactor
		{
			set{
				Al.alListenerf(Al.AL_ROLLOFF_FACTOR, value);
			}
			get{
				float val;
				Al.alGetListenerf(Al.AL_ROLLOFF_FACTOR, out val);
				return val;
			}
		}

		public override void Dispose()
		{
			Alut.alutExit();
			base.Dispose();
		}
		
	}

}

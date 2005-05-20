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
		private float rolloff = 1;
		
		public OpenALSoundManager () : base()
		{
			Alut.alutInit();
			Al.alGetError();
			
			instance = this;
			
			LogManager.Instance.Write("OpenAL SoundSystem initialised");
		}
		
		protected override void FrameUpdate(Object source, FrameEventArgs e)
		{
			base.FrameUpdate(source, e);
			
			// if the camera moved
			if(cam.WorldPosition.x != lastcamposition.x || cam.WorldPosition.y != lastcamposition.y || cam.WorldPosition.z != lastcamposition.z)
			{
				float[] vector = new float[]{cam.WorldPosition.x, cam.WorldPosition.y, cam.WorldPosition.z};
				Al.alListenerfv(Al.AL_POSITION, vector);
				lastcamposition = cam.WorldPosition;
			} 
			
			// if the camera turned
			if(cam.WorldOrientation.w != lastcamorientation.w || cam.WorldOrientation.x != lastcamorientation.x || cam.WorldOrientation.y != lastcamorientation.y || cam.WorldOrientation.z != lastcamorientation.z)
			{
				Axiom.MathLib.Vector3 top = cam.WorldOrientation.YAxis;
				Axiom.MathLib.Vector3 front = cam.WorldOrientation.ZAxis;
				float[] doublevector = new float[]{-front.x, -front.y, -front.z, top.x, top.y, top.z}; // negative front, to switch the left and right channel TODO: Test this with surround sound to see if back and forward is ok
				Al.alListenerfv(Al.AL_ORIENTATION, doublevector);
				lastcamorientation = cam.WorldOrientation;
			}
			
		}
	
		public override Axiom.SoundSystems.Sound LoadSound(string filename, short type)
		{
			// create the sound and add it to the list
			Sound thissound = new Axiom.SoundSystems.OpenAL.Sound(filename, lastid, type);
			soundlist.Add(thissound);
			
			// update the last id
			lastid++;
			
			return thissound;
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
				rolloff = value;
			}
			get{
				return rolloff;
			}
		}

		public override void Dispose()
		{
			Alut.alutExit();
			base.Dispose();
		}
		
	}

}

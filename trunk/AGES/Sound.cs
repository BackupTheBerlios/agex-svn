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
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.SoundSystems
{
	/// <summary>
	/// Class Sound : SceneObject, IDisposable, Implements and controls a sound buffer as SceneObject
	/// </summary>
	public abstract class Sound : SceneObject, IDisposable
	{
		/// <summary>
		/// This sound's ID
		/// </summary>
		protected int id;
		
		/// <summary>
		/// The last position of the attached SceneNode, to check if it's changed
		/// </summary>
		protected Axiom.MathLib.Vector3 lastnodeposition = Axiom.MathLib.Vector3.Zero;
		
		/// <summary>
		/// My current position (relative to the SceneNode)
		/// </summary>
		protected Axiom.MathLib.Vector3 position = Axiom.MathLib.Vector3.Zero;

		/// <summary>
		/// My last position (relative to the SceneNode)
		/// </summary>
		protected Axiom.MathLib.Vector3 lastposition = Axiom.MathLib.Vector3.Zero;
		
		/// <summary>
		/// SceneNode's last orientation, to check if it's changed
		/// </summary>
		protected Quaternion lastorientation = Quaternion.Zero;
		
		/// <summary>
		/// Last cone direction, to check if it's changed
		/// </summary>
		protected Vector3 lastdirection = Vector3.Zero;
		
		/// <summary>
		/// The direction of the sound cone
		/// </summary>
		protected Vector3 conedirection = Vector3.Zero;
		
		/// <summary>
		/// Type is a simple warning or background sound, without fancy 3D positioning etc.
		/// </summary>
		public const short SIMPLE_SOUND = 1;
		
		/// <summary>
		/// Type is a 3D positioned sound
		/// </summary>
		public const short THREED_SOUND = 2;
		
		/// <summary>
		/// Constructor, do not use this but initialise through <see cref="Axiom.SoundSystems.SoundManager.LoadSound">SoundManager.LoadSound()</see>
		/// </summary>
		/// <param name="filename">The filename of the wave file (searched in the common resource data, ie. in the paths set in EngineConfig.xml)</param>
		/// <param name="ID">This sound's ID given by the SoundManager</param>
		public Sound(string filename, int ID) : base()
		{
			if(ID >= 0)
			{
				id = ID;
				name = "RESERVED/Sounds/ID" + this.ID.ToString();
				LogManager.Instance.Write("SoundSystem: Loading sound {0}", filename);
			}
		}
		
		/// <summary>
		///	Start playing the file
		/// </summary>
		/// <param name="loop">Play in a loop or not</param>
		public abstract void Play(bool loop);
		
		/// <summary>
		/// Stops playing the file, calling <see cref="Axiom.SoundSystems.Sound.Play">Play()</see> again will start playing from the position where Pause() was called
		/// </summary>
		public abstract void Pause();
		
		/// <summary>
		/// Stops playing the file and set the play position to the start of the buffer
		/// </summary>
		public abstract void Stop();
		
		/// <summary>
		/// The ID of this sound given by the SoundManager
		/// </summary>
		public int ID
		{
			get{
				return id;
			}
		}
		
		/// <summary>
		/// IDisposable implementation
		/// </summary>
		public virtual void Dispose()
		{
			
		}
		
		/// <summary>
		/// Update the position of the sound source according to changes to the SceneNode and the Position property
		/// </summary>
		public virtual void UpdatePosition()
		{
			// if the position is changed
			if((parentNode != null) && ( (lastnodeposition.x != parentNode.WorldPosition.x || lastnodeposition.y != parentNode.WorldPosition.y || lastnodeposition.z != parentNode.WorldPosition.z) || (this.Position != lastposition)))
			{
				SetPosition(parentNode.WorldPosition + this.Position);
				lastnodeposition = parentNode.WorldPosition;
				lastposition = this.Position;
			}
			
			// if the orientation is changed
			if((parentNode != null) && ( (parentNode.WorldOrientation.w != lastorientation.w || parentNode.WorldOrientation.x != lastorientation.x || parentNode.WorldOrientation.y != lastorientation.y || parentNode.WorldOrientation.z != lastorientation.z) || (conedirection != lastdirection)))
			{
				SetConeDirection(Quaternion.Multiply(parentNode.WorldOrientation, conedirection));
				lastorientation = parentNode.WorldOrientation;
				lastdirection = conedirection;
			}
		}
		
		/// <summary>
		/// Set a new WorldPosition of the sound
		/// </summary>
		/// <param name="newposition">The position to set</param>
		protected abstract void SetPosition(Axiom.MathLib.Vector3 newposition);
		
		/// <summary>
		/// Set a new direction of the sound cone in World coordinates
		/// </summary>
		/// <param name="direction">The direction to set</param>
		protected abstract void SetConeDirection(Axiom.MathLib.Vector3 direction);
		
		/// <summary>
		/// Set the current camera - Unused
		/// </summary>
		/// <param name="camera">The current camera</param>
		public override void NotifyCurrentCamera(Camera camera)
		{
		}
		
		/// <summary>
		/// Null-implementation of SceneObject Member
		/// </summary>
		/// <param name="queue"></param>
		public override void UpdateRenderQueue(Axiom.Graphics.RenderQueue queue)
		{
			
		}
		
		/// <summary>
		/// The position of the sound source in the Axiom scene
		/// </summary>
		public abstract Axiom.MathLib.Vector3 WorldPosition
		{
			get;
		}
		
		/// <summary>
		/// The position of the sound source relative to the SceneNode
		/// </summary>
		public virtual Axiom.MathLib.Vector3 Position
		{
			get{
				return position;
			}
			set{
				position = value;
			}
		}
		
		/// <summary>
		/// An infinite small BoundingBox (as the sound source is a point) (inherited from SceneObject)
		/// </summary>
		public override AxisAlignedBox BoundingBox
		{
			get{
				return new AxisAlignedBox(Vector3.Zero, Vector3.Zero);
			}
		}
		
		/// <summary>
		/// BoundingRadius = NaN (inherited from SceneObject)
		/// </summary>
		public override float BoundingRadius
		{
			get{
				return float.NaN;
			}
		}
		
		/// <summary>
		/// The angle of the {inside, outside} cone. The volume will decrease from normal to <see cref="OutsideVolume">OutsideVolume</see> between the inside and outside cone
		/// </summary>
		public abstract int[] ConeAngles
		{
			get;
			set;
		}
		
		/// <summary>
		/// The direction of the sound cone (relative to the SceneNode orientation)
		/// </summary>
		public virtual Axiom.MathLib.Vector3 ConeDirection
		{
			get{
				return conedirection;
			}
			set{
				conedirection = value;
			}
		}
		
		/// <summary>
		/// The volume of the sound outside the outside cone. Value between 0 (normal volume) and -10000 (practically silent)
		/// </summary>
		public abstract int OutsideVolume
		{
			get;
			set;
		}
		
		/// <summary>
		/// If the listener is further away from a source than the maximum distance, the source is silenced. This also sets the transition area from 0 to MaxDistance where the sound will attenuate from normal to silent
		/// </summary>
		public abstract float MaxDistance
		{
			get;
			set;
		}
		
		/// <summary>
		/// The speed of the sound source, used for Doppler-effect calculation
		/// </summary>
		public abstract Axiom.MathLib.Vector3 Velocity
		{
			get;
			set;
		}
		
		/// <summary>
		/// The normal volume of the sound. Value between 0 (normal volume) and -10000 (practically silent)
		/// </summary>
		public abstract int Volume
		{
			get;
			set;
		}
	}
}

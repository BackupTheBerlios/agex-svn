#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003 Axiom Project Team

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
using System.Drawing;
using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.SoundSystems;

namespace Demo {
	/// <summary>
	/// Summary description for AGES.
	/// </summary>
	public class AGES : TechDemo {

        #region Private Fields

        private AnimationState animationState = null;
        private SceneNode headNode = null;

        #endregion Private Fields

        #region Protected Override Methods

        protected Sound growl;
        
        protected override void CreateScene() {
            // set some ambient light
            scene.AmbientLight = new ColorEx(1.0f, 0.2f, 0.2f, 0.2f);

            // create a skydome
            scene.SetSkyDome(true, "Examples/CloudySky", 5, 8);

            // create a simple default point light
            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);

            // create a plane for the plane mesh
            Plane plane = new Plane();
            plane.Normal = Vector3.UnitY;
            plane.D = 200;

            // create a plane mesh
            MeshManager.Instance.CreatePlane("FloorPlane", plane, 200000, 200000, 20, 20, true, 1, 50, 50, Vector3.UnitZ);

            // create an entity to reference this mesh
            Entity planeEntity = scene.CreateEntity("Floor", "FloorPlane");
            planeEntity.MaterialName = "Examples/RustySteel";
            scene.RootSceneNode.CreateChildSceneNode().AttachObject(planeEntity);

            // create an entity to have follow the path
            Entity ogreHead = scene.CreateEntity("OgreHead", "ogrehead.mesh");

            // create a scene node for the entity and attach the entity
            headNode = scene.RootSceneNode.CreateChildSceneNode("OgreHeadNode", Vector3.Zero, Quaternion.Identity);
            headNode.AttachObject(ogreHead);

            // make sure the camera tracks this node
            //camera.SetAutoTracking(true, headNode, Vector3.Zero);

            // create a scene node to attach the camera to
            SceneNode cameraNode = scene.RootSceneNode.CreateChildSceneNode("CameraNode");
            cameraNode.AttachObject(camera);
 
            /////////////////////////- AGE Sound Library Settings -//////////////////////////
            // initialise the SoundManager by setting the RenderWindow
            SoundManager.Instance.SetRenderWindow(this.window, this.camera);
            SoundManager.Instance.RolloffFactor = 0.01f; // we're working on a large scale
            
            // load a simple sound
            Sound back = SoundManager.Instance.LoadSound("background.wav", Sound.SIMPLE_SOUND);
            // set the volume low
           // back.Volume = -2000;
            // play the simple sound in a loop
            back.Play(true);
            
            // load a 3D sound
            growl = SoundManager.Instance.LoadSound("growl.ogg", Sound.THREED_SOUND);
            // attach the sound to the head
           	headNode.AttachObject(growl);
            // set the sound's properties
            //growl.ConeAngles = new int[]{90, 120};
           // growl.ConeDirection = Vector3.UnitZ;
      		//growl.MaxDistance = 5000;
           // growl.OutsideVolume = -10000;
            // play the 3D sound in a loop
            growl.Play(true);
            ////////////////////////////////////////////////////////////////////////////////
            
            // create new animation
            Animation animation = scene.CreateAnimation("OgreHeadAnimation", 10.0f);

            // nice smooth animation
            animation.InterpolationMode = InterpolationMode.Spline;

            // create the main animation track
            AnimationTrack track = animation.CreateTrack(0, cameraNode);

            // create a few keyframes to move the camera around
            KeyFrame frame = track.CreateKeyFrame(0.0f);

            frame = track.CreateKeyFrame(2.5f);
            frame.Translate = new Vector3(500, 500, -1000);

            frame = track.CreateKeyFrame(5.0f);
            frame.Translate = new Vector3(-1500, 1000, -600);

            frame = track.CreateKeyFrame(7.5f);
            frame.Translate = new Vector3(0, -100, 0);

            frame = track.CreateKeyFrame(10.0f);
            frame.Translate = Vector3.Zero;

            // create a new animation state to control the animation
            animationState = scene.CreateAnimationState("OgreHeadAnimation");

            // enable the animation
            animationState.IsEnabled = true;

            // turn on some fog
            scene.SetFog(FogMode.Exp, ColorEx.White, 0.0002f);
        }
        #endregion Protected Override Methods

        #region Protected Override Event Handlers

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            base.OnFrameStarted(source, e);

            // add time to the animation which is driven off of rendering time per frame
            animationState.AddTime(e.TimeSinceLastFrame);
         }

        #endregion Protected Override Event Handlers
	}
}

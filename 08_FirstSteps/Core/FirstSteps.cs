using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Tutorial.Core
{
    public class FirstSteps : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;

        private SceneNodeContainer[] _cubes;

        private float _camAngle = -25;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to pinkish light gray.
            RC.ClearColor = new float4(1, 0.90f, 0.90f, 1);

            // Create three cubes
            _cubes = new SceneNodeContainer[3];

            for (int i = 0; i < 3; i++)
            {
                // Transform of cube
                var cubeTransform = new TransformComponent
                {
                    Scale = new float3(1.5f * (i + 1), 1.5f * (i + 1), 1.5f * (i + 1)),
                    Translation = new float3(35.0f * (i/2.0f - 0.5f), 0, 0)
                };

                // Material of cube
                var cubeMaterial = new MaterialComponent
                {
                    Diffuse = new MatChannelContainer { Color = new float3(1, 0.5f, 0.5f) },
                    Specular = new SpecularChannelContainer { Color = float3.One, Shininess = 4 }
                };

                // Mesh of cube
                var cubeMesh = SimpleMeshes.CreateCuboid(new float3(7, 7, 7));

                var cubeNode = new SceneNodeContainer();
                cubeNode.Components = new List<SceneComponentContainer>();
                cubeNode.Components.Add(cubeTransform);
                cubeNode.Components.Add(cubeMaterial);
                cubeNode.Components.Add(cubeMesh);
                _cubes[i] = cubeNode;
            }

            // Create the scene
            _scene = new SceneContainer();
            _scene.Children = new List<SceneNodeContainer>();

            // Add all cubes into that scene
            foreach (SceneNodeContainer cube in _cubes)
            {
                _scene.Children.Add(cube);
            }

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Animate the cube
            for (int i = 0; i < 3; i++)
            {
                _cubes[i].GetTransform().Translation = new float3(35.0f * (i/2.0f - 0.5f), (5 + 5 * i) * Abs(M.Sin((2 * TimeSinceStart) + (M.Pi / 4))), 0);
                _cubes[i].GetTransform().Rotation = new float3(0, 2 * TimeSinceStart, 0);
                _cubes[i].GetTransform().Scale = new float3(1, 1 + 0.2f * Abs(M.Sin((2 * TimeSinceStart) + (M.Pi / 4))), 1);
            }

            // Animate the angle of the camera
            _camAngle = _camAngle + 5.0f * M.Pi/180.0f * DeltaTime;

            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, -10, 50) * float4x4.CreateRotationY(_camAngle);

            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}
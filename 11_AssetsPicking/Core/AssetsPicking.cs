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
    public class AssetsPicking : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private ScenePicker _scenePicker;

        private SceneNodeContainer _mainNode;
        private SceneNodeContainer _arm1Node;
        private SceneNodeContainer _arm2Node;
        private SceneNodeContainer _arm3Node;
        private SceneNodeContainer _shovelNode;
        private SceneNodeContainer _wheel1Node;
        private SceneNodeContainer _wheel2Node;
        private SceneNodeContainer _wheel3Node;
        private SceneNodeContainer _wheel4Node;

        private TransformComponent _mainTransform;
        private TransformComponent _arm1Transform;
        private TransformComponent _arm2Transform;
        private TransformComponent _arm3Transform;
        private TransformComponent _shovelTransform;
        private TransformComponent _wheel1Transform;
        private TransformComponent _wheel2Transform;
        private TransformComponent _wheel3Transform;
        private TransformComponent _wheel4Transform;

        private PickResult _currentPick;
        private float3 _oldColor;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("Bagger_Plain.fus");

            _mainNode = _scene.Children.FindNodes(node => node.Name == "Main")?.FirstOrDefault();
            _arm1Node = _scene.Children.FindNodes(node => node.Name == "Arm1")?.FirstOrDefault();
            _arm2Node = _scene.Children.FindNodes(node => node.Name == "Arm2")?.FirstOrDefault();
            _arm3Node = _scene.Children.FindNodes(node => node.Name == "Arm3")?.FirstOrDefault();
            _shovelNode = _scene.Children.FindNodes(node => node.Name == "Shovel")?.FirstOrDefault();
            _wheel1Node = _scene.Children.FindNodes(node => node.Name == "Wheel1")?.FirstOrDefault();
            _wheel1Node = _scene.Children.FindNodes(node => node.Name == "Wheel2")?.FirstOrDefault();
            _wheel1Node = _scene.Children.FindNodes(node => node.Name == "Wheel3")?.FirstOrDefault();
            _wheel1Node = _scene.Children.FindNodes(node => node.Name == "Wheel4")?.FirstOrDefault();

            _mainTransform = _mainNode?.GetTransform();
            _arm1Transform = _arm1Node?.GetTransform();
            _arm2Transform = _arm2Node?.GetTransform();
            _arm3Transform = _arm3Node?.GetTransform();
            _shovelTransform = _shovelNode?.GetTransform();
            _wheel1Transform = _wheel1Node?.GetTransform();
            _wheel1Transform = _wheel2Node?.GetTransform();
            _wheel1Transform = _wheel3Node?.GetTransform();
            _wheel1Transform = _wheel4Node?.GetTransform();

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
            _scenePicker = new ScenePicker(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, -1, 15) * float4x4.CreateRotationX(-(float)Atan(15.0 / 40.0)) * float4x4.CreateRotationY(-(float)Atan(15.0 / 40.0));

            if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);
                _scenePicker.View = RC.View;
                _scenePicker.Projection = RC.Projection;
                List<PickResult> pickResults = _scenePicker.Pick(pickPosClip).ToList();
                PickResult newPick = null;
                if (pickResults.Count > 0)
                {
                    pickResults.Sort((a, b) => Sign(a.ClipPos.z - b.ClipPos.z));
                    newPick = pickResults[0];
                }
                if (newPick?.Node != _currentPick?.Node)
                {
                    if (_currentPick != null)
                    {
                        _currentPick.Node.GetMaterial().Diffuse.Color = _oldColor;
                    }
                    if (newPick != null)
                    {
                        var mat = newPick.Node.GetMaterial();
                        _oldColor = mat.Diffuse.Color;
                        mat.Diffuse.Color = new float3(1, 0.4f, 0.4f);
                    }
                    _currentPick = newPick;
                }
            }

            // Change axis of _currentPick with A and D keys

            if (_currentPick?.Node != null) // make sure a node is selected
            {
                TransformComponent currentTransform = _currentPick.Node.GetTransform();
                string rotAxis = "x";
                float currentRot = currentTransform.Rotation.x;
                float minRot = 0;
                float maxRot = 0;

                if (_currentPick.Node == _mainNode)
                {
                    currentRot = currentTransform.Rotation.y;
                    rotAxis = "y";
                }
                if (_currentPick.Node == _arm1Node)
                {
                    currentRot = currentTransform.Rotation.y;
                    rotAxis = "y";
                }
                if (_currentPick.Node == _arm2Node)
                {
                    minRot = -0.01f;
                    maxRot = 2f;
                }
                if (_currentPick.Node == _arm3Node)
                {
                    minRot = -0.125f;
                    maxRot = 2f;
                }
                if (_currentPick.Node == _shovelNode)
                {
                    minRot = -1.5f;
                    maxRot = 1f;
                }

                currentRot += 3 * Keyboard.ADAxis * DeltaTime;
                if (minRot == 0 || (minRot <= currentRot && currentRot <= maxRot))
                {
                    if (rotAxis == "x")
                    {
                        currentTransform.Rotation = new float3(currentRot, 0, 0);
                    }
                    if (rotAxis == "y")
                    {
                        currentTransform.Rotation = new float3(0, currentRot, 0);
                    }
                }
            }

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

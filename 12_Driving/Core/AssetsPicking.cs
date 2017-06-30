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
        private SceneNodeContainer _wheelFrontNode;
        private SceneNodeContainer _wheelBackNode;

        private TransformComponent _mainTransform;
        private TransformComponent _arm1Transform;
        private TransformComponent _arm2Transform;
        private TransformComponent _arm3Transform;
        private TransformComponent _shovelTransform;
        private TransformComponent _wheelFrontTransform;
        private TransformComponent _wheelBackTransform;

        private PickResult _currentPick;
        private float3 _oldColor;

        private TransformComponent _cameraTransform;
        private float _d = 15;

        private SceneNodeContainer _cubeNode;
        private SceneNodeContainer _containerNode;
        private SceneNodeContainer _duneNode;

        float _baggerVelocity = 0;
        float _baggerDuneRot = 0;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("Bagger_Scene.fus");

            _mainNode = _scene.Children.FindNodes(node => node.Name == "Main")?.FirstOrDefault();
            _arm1Node = _scene.Children.FindNodes(node => node.Name == "Arm1")?.FirstOrDefault();
            _arm2Node = _scene.Children.FindNodes(node => node.Name == "Arm2")?.FirstOrDefault();
            _arm3Node = _scene.Children.FindNodes(node => node.Name == "Arm3")?.FirstOrDefault();
            _shovelNode = _scene.Children.FindNodes(node => node.Name == "Shovel")?.FirstOrDefault();
            _wheelFrontNode = _scene.Children.FindNodes(node => node.Name == "WheelFront")?.FirstOrDefault();
            _wheelBackNode = _scene.Children.FindNodes(node => node.Name == "WheelBack")?.FirstOrDefault();

            _containerNode = _scene.Children.FindNodes(node => node.Name == "Container")?.FirstOrDefault();
            _duneNode = _scene.Children.FindNodes(node => node.Name == "Icosphere")?.FirstOrDefault();

            _containerNode.GetTransform().Translation = new float3(-2, 0, 10);
            _duneNode.GetTransform().Translation = new float3(6, 0, 14);

            _mainTransform = _mainNode?.GetTransform();
            _arm1Transform = _arm1Node?.GetTransform();
            _arm2Transform = _arm2Node?.GetTransform();
            _arm3Transform = _arm3Node?.GetTransform();
            _shovelTransform = _shovelNode?.GetTransform();
            _wheelFrontTransform = _wheelFrontNode?.GetTransform();
            _wheelBackTransform = _wheelBackNode?.GetTransform();

            // ground
            _cubeNode = new SceneNodeContainer
            {
                Components = new List<SceneComponentContainer>
                {
                    // TRANSFROM COMPONENT
                    new TransformComponent
                    {
                        Translation = new float3(0,-1,0),
                        Rotation = new float3(0,0,0),
                        Scale = new float3(1,1,1)
                    },

                    // MATERIAL COMPONENT
                    new MaterialComponent
                    {
                        Diffuse = new MatChannelContainer { Color = new float3(0.8f, 0.8f, 0.8f) },
                        Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 2 }
                    },

                    // MESH COMPONENT
                    SimpleMeshes.CreateCuboid(new float3(400, 2, 400))
                }
            };
            _scene.Children.Add(_cubeNode);

            _cameraTransform = new TransformComponent { Rotation = new float3(-M.Pi / 5.7f, 0, 0), Scale = float3.One, Translation = new float3(0, 0, -10) };

            _scene.Children.Add(new SceneNodeContainer
            {
                Components = new List<SceneComponentContainer>
                {
                    _cameraTransform,
                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(0.7f, 0.7f, 0.7f) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                    SimpleMeshes.CreateCuboid(new float3(2, 2, 2))
                }
            });

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
            RC.View = float4x4.CreateRotationX(-M.Pi / 7.3f) * float4x4.CreateRotationY(M.Pi - _cameraTransform.Rotation.y) * float4x4.CreateTranslation(-_cameraTransform.Translation.x, -6, -_cameraTransform.Translation.z);

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
                if (newPick?.Node != _currentPick?.Node && newPick?.Node != _wheelFrontNode && newPick?.Node != _wheelBackNode && newPick?.Node != _cubeNode && newPick?.Node != _containerNode)
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
            if (_currentPick?.Node != null && _currentPick?.Node != _mainNode && _currentPick?.Node != _duneNode) // make sure a node is selected
            {
                TransformComponent currentTransform = _currentPick.Node.GetTransform();
                string rotAxis = "x";
                float currentRot = currentTransform.Rotation.x;
                float minRot = 0;
                float maxRot = 0;

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

            float3 dNT = _duneNode.GetTransform().Translation;
            float3 mNT = _mainTransform.Translation;

            _baggerDuneRot = M.MinAngle((float)System.Math.Atan2(float3.Normalize(mNT - dNT).x, float3.Normalize(mNT - dNT).z) + 2*M.Pi);
            
            if (_currentPick?.Node == _duneNode && _mainTransform.Rotation.y != _baggerDuneRot)
            {
                if (M.MinAngle(_mainTransform.Rotation.y) > M.MinAngle(_baggerDuneRot))
                {
                    _mainTransform.Rotation.y -= 0.025f;
                }
                if (M.MinAngle(_mainTransform.Rotation.y) < M.MinAngle(_baggerDuneRot))
                {
                    _mainTransform.Rotation.y += 0.025f;
                }
            }
            
            float posVel = 0;
            float newYRot = _mainTransform.Rotation.y;

            if (_currentPick?.Node == _mainNode)
            {
                float rotVel = Keyboard.ADAxis * DeltaTime;
                posVel = Keyboard.WSAxis * DeltaTime;
                newYRot = _mainTransform.Rotation.y + rotVel;
                float wheelRot = 0.75f * posVel * 2 * M.Pi;

                float3 newPos = _mainTransform.Translation;
                newPos.x -= posVel * M.Sin(newYRot) * 6;
                newPos.z -= posVel * M.Cos(newYRot) * 6;
                _mainTransform.Translation = newPos;

                _wheelFrontTransform.Rotation.x -= wheelRot;
                _wheelBackTransform.Rotation.x -= wheelRot;

                if (posVel > 0)
                {
                    _mainTransform.Rotation = new float3(0, _mainTransform.Rotation.y + rotVel, 0);
                }
                else if (posVel < 0)
                {
                    _mainTransform.Rotation = new float3(0, _mainTransform.Rotation.y - rotVel, 0);
                }

                if (rotVel != 0)
                {
                    _wheelFrontTransform.Rotation.y = 2 * rotVel * 2 * M.Pi;
                }
            }

            float3 pAalt = _mainTransform.Translation;
            float3 pBalt = _cameraTransform.Translation;
            float3 pAneu = _mainTransform.Translation + new float3(posVel * M.Sin(newYRot) * 10, 0, posVel * M.Cos(newYRot) * 10);
            float3 pBneu = pAneu + (float3.Normalize(pBalt - pAneu) * _d);

            _cameraTransform.Translation = pBneu;
            _cameraTransform.Rotation = new float3(0, (float)System.Math.Atan2(float3.Normalize(pBalt - pAneu).x, float3.Normalize(pBalt - pAneu).z), 0);
          
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

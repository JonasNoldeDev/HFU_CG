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
    public class HierarchyInput : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private float _camAngle = 0;
        private TransformComponent _baseTransform;
        private TransformComponent _bodyTransform;
        private TransformComponent _upperArmTransformPivot;
        private TransformComponent _upperArmTransform;
        private TransformComponent _lowerArmTransformPivot;
        private TransformComponent _lowerArmTransform;
        private TransformComponent _rightFingerTransformPivot;
        private TransformComponent _leftFingerTransformPivot;
        private TransformComponent _fingerTransform;
        private Boolean _fingersClosed = true;
        private Boolean _animateFingers = false;
        private float _mouseVelocity = 0;

        SceneContainer CreateScene()
        {
            // Initialize transform components that need to be changed inside "RenderAFrame"
            _baseTransform = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };
            _bodyTransform = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 6, 0)
            };
            _upperArmTransformPivot = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(2, 4, 0)
            };
            _upperArmTransform = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 4, 0)
            };
            _lowerArmTransformPivot = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(-2, 4, 0)
            };
            _lowerArmTransform = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 4, 0)
            };
            _rightFingerTransformPivot = new TransformComponent
            {
                Rotation = new float3(0, 0, 0.75f),
                Scale = new float3(1, 1, 1),
                Translation = new float3(-0.5f, 4.75f, 0)
            };
            _leftFingerTransformPivot = new TransformComponent
            {
                Rotation = new float3(0, 0, -0.75f),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0.5f, 4.75f, 0)
            };
            _fingerTransform = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 1.5f, 0)
            };

            // Setup the scene graph
            return new SceneContainer
            {
                Children = new List<SceneNodeContainer>
                {
                    // Base
                    new SceneNodeContainer
                    {
                        Components = new List<SceneComponentContainer>
                        {
                            // TRANSFROM COMPONENT
                            _baseTransform,

                            // MATERIAL COMPONENT
                            new MaterialComponent
                            {
                                Diffuse = new MatChannelContainer { Color = new float3(0.7f, 0.7f, 0.7f) },
                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                            },

                            // MESH COMPONENT
                            SimpleMeshes.CreateCuboid(new float3(10, 2, 10))
                        }
                    },
                    // Body
                    new SceneNodeContainer
                    {
                        Components = new List<SceneComponentContainer>
                        {
                            // TRANSFROM COMPONENT
                            _bodyTransform,

                            // MATERIAL COMPONENT
                            new MaterialComponent
                            {
                                Diffuse = new MatChannelContainer { Color = new float3(1, 0.5f, 0.5f) },
                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                            },

                            // MESH COMPONENT
                            SimpleMeshes.CreateCuboid(new float3(2, 10, 2))
                        },
                        Children = new List<SceneNodeContainer>
                        {
                            // Pivot UpperArm
                            new SceneNodeContainer
                            {
                                Components = new List<SceneComponentContainer>
                                {
                                    _upperArmTransformPivot,
                                },
                                Children = new List<SceneNodeContainer>
                                {
                                    // UpperArm
                                    new SceneNodeContainer
                                    {
                                        Components = new List<SceneComponentContainer>
                                        {
                                            // TRANSFROM COMPONENT
                                            _upperArmTransform,

                                            // MATERIAL COMPONENT
                                            new MaterialComponent
                                            {
                                                Diffuse = new MatChannelContainer { Color = new float3(0.5f, 1, 0.5f) },
                                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                                            },

                                            // MESH COMPONENT
                                            SimpleMeshes.CreateCuboid(new float3(2, 10, 2))
                                        },
                                        Children = new List<SceneNodeContainer>
                                        {
                                            // Pivot LowerArm
                                            new SceneNodeContainer
                                            {
                                                Components = new List<SceneComponentContainer>
                                                {
                                                    _lowerArmTransformPivot,
                                                },
                                                Children = new List<SceneNodeContainer>
                                                {
                                                    // LowerArm
                                                    new SceneNodeContainer
                                                    {
                                                        Components = new List<SceneComponentContainer>
                                                        {
                                                            // TRANSFROM COMPONENT
                                                            _lowerArmTransform,

                                                            // MATERIAL COMPONENT
                                                            new MaterialComponent
                                                            {
                                                                Diffuse = new MatChannelContainer { Color = new float3(0.5f, 0.5f, 1) },
                                                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                                                            },

                                                            // MESH COMPONENT
                                                            SimpleMeshes.CreateCuboid(new float3(2, 10, 2))
                                                        },
                                                        Children = new List<SceneNodeContainer>
                                                        {
                                                            // Pivot RightFinger
                                                            new SceneNodeContainer
                                                            {
                                                                Components = new List<SceneComponentContainer>
                                                                {
                                                                    _rightFingerTransformPivot,
                                                                },
                                                                Children = new List<SceneNodeContainer>
                                                                {
                                                                    // RightFinger
                                                                    new SceneNodeContainer
                                                                    {
                                                                        Components = new List<SceneComponentContainer>
                                                                        {
                                                                            // TRANSFROM COMPONENT
                                                                            _fingerTransform,

                                                                            // MATERIAL COMPONENT
                                                                            new MaterialComponent
                                                                            {
                                                                                Diffuse = new MatChannelContainer { Color = new float3(0.5f, 0.5f, 0.5f) },
                                                                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                                                                            },

                                                                            // MESH COMPONENT
                                                                            SimpleMeshes.CreateCuboid(new float3(0.5f, 3, 1))
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            // Pivot LeftFinger
                                                            new SceneNodeContainer
                                                            {
                                                                Components = new List<SceneComponentContainer>
                                                                {
                                                                    _leftFingerTransformPivot,
                                                                },
                                                                Children = new List<SceneNodeContainer>
                                                                {
                                                                    // LeftFinger
                                                                    new SceneNodeContainer
                                                                    {
                                                                        Components = new List<SceneComponentContainer>
                                                                        {
                                                                            // TRANSFROM COMPONENT
                                                                            _fingerTransform,

                                                                            // MATERIAL COMPONENT
                                                                            new MaterialComponent
                                                                            {
                                                                                Diffuse = new MatChannelContainer { Color = new float3(0.5f, 0.5f, 0.5f) },
                                                                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                                                                            },

                                                                            // MESH COMPONENT
                                                                            SimpleMeshes.CreateCuboid(new float3(0.5f, 3, 1))
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        // Init is called on startup.
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = CreateScene();

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Rotate body with left and right arrow keys
             float bodyRot = _bodyTransform.Rotation.y;
             bodyRot += 4 * Keyboard.LeftRightAxis * DeltaTime;
             _bodyTransform.Rotation = new float3(0, bodyRot, 0);

            // Rotate upperArm with up and down arrow keys
            float upperArmRot = _upperArmTransformPivot.Rotation.x;
            upperArmRot += 2 * Keyboard.UpDownAxis * DeltaTime;
            _upperArmTransformPivot.Rotation = new float3(upperArmRot, 0, 0);

            // Rotate lowerArm with w and s keys
            float lowerArmRot = _lowerArmTransformPivot.Rotation.x;
            lowerArmRot += 2 * Keyboard.WSAxis * DeltaTime;
            _lowerArmTransformPivot.Rotation = new float3(lowerArmRot, 0, 0);

            // Open and close fingers with a and d keys
            float rightFingerRot = _rightFingerTransformPivot.Rotation.z;
            rightFingerRot += 3 * Keyboard.ADAxis * DeltaTime;
            if (-0.1f <= rightFingerRot && rightFingerRot <= 0.75f) {
                _rightFingerTransformPivot.Rotation = new float3(0, 0, rightFingerRot);
                _leftFingerTransformPivot.Rotation = new float3(0, 0, -rightFingerRot);
            }

            // Open and close fingers by pressing the f key
            rightFingerRot = _rightFingerTransformPivot.Rotation.z;
            if (Keyboard.IsKeyDown(KeyCodes.F))
            {
                _fingersClosed = !_fingersClosed;
                _animateFingers = true;
            }

            if (_animateFingers)
            {
                if (_fingersClosed)
                {
                    rightFingerRot += 2 * DeltaTime;
                }
                else
                {
                    rightFingerRot -= 2 * DeltaTime;
                }
            }
            if (-0.1f <= rightFingerRot && rightFingerRot <= 0.75f)
            {
                _rightFingerTransformPivot.Rotation = new float3(0, 0, rightFingerRot);
                _leftFingerTransformPivot.Rotation = new float3(0, 0, -rightFingerRot);
            }
            else
            {
                _animateFingers = false;
            }

            //Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Setup the camera with swipe/grab functionality
            if (_mouseVelocity != 0)
            {
                if (_mouseVelocity > 0)
                {
                    _mouseVelocity -= 0.015f;
                    if (_mouseVelocity < 0) _mouseVelocity = 0;
                }
                if (_mouseVelocity < 0)
                {
                    _mouseVelocity += 0.015f;
                    if (_mouseVelocity > 0) _mouseVelocity = 0;
                }
            }
            if (Mouse.LeftButton)
            {
                _mouseVelocity = Mouse.Velocity.x / 7500;
            }
            _camAngle += _mouseVelocity;
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

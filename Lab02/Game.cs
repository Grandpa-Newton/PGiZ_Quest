using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectInput;
using SharpDX;
using SharpDX.Windows;
using Lab01;
using System.Drawing;
using System.IO;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System.Threading;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
using SharpDX.IO;
using SharpDX.WIC;
using System.IO.Packaging;
using ObjLoader.Loader.Loaders;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace QuestGame
{
    class Game : IDisposable
    {
        RenderForm _renderForm;

        const int NUM_LIGHTS = 4;

        Texture _tetrahedronTexture;
        Texture _plotTexture;
        private Texture _npcTexture;
        MeshObject _player;
        private MeshObject _firstNpcObject;
        MeshObject _plot;

        Inventory<MainInventoryItem> _mainInventory;
        
        Inventory<CollectibleItem> _collectiblesInventory;

        MeshObject[] _lights = new MeshObject[NUM_LIGHTS];
        Camera _camera;

        DirectX3DGraphics _directX3DGraphics;
        Renderer _renderer;

        private bool _isOpenCollectibleItems = false;
        
        SharpDX.Direct2D1.Bitmap _playerBitmap;

        SharpDX.Direct2D1.DeviceContext _d2dContext;

        private SharpDX.Direct2D1.Bitmap1 d2dTarget;

        SharpDX.Direct2D1.SolidColorBrush _greenBrush;

        SharpDX.Direct2D1.SolidColorBrush _redBrush;

        SharpDX.Direct2D1.SolidColorBrush _blueBrush;

        SharpDX.Direct2D1.SolidColorBrush _purpleBrush;

        SharpDX.Direct2D1.SolidColorBrush _whiteBrush;

        private BoundingBox _playerCollider;
        
        private BoundingBox _firstNpcCollider;
        
        private BoundingBox _treasureCollider;

        MaterialProperties _defaultMaterial;

        MaterialProperties _floorMaterial;

        LightProperties _light;

        bool _isMap = false;

        private SolidColorBrush _blackBrush;

        private Action OnPlayerFindingTreasure;

        private Action OnSecondNpcTaskSolved;

        private SequentialQuest _secondNpcSequentialQuest;

        private string textLayoutText = "";

        Vector4[] _lightColors = new Vector4[NUM_LIGHTS]
        {
            new Vector4(0f, 1f, 1f, 1f),
            new Vector4(0f, 1f, 0f, 1f),
            new Vector4(1f, 0f, 0f, 1f),
            new Vector4(0.5f, 0f, 0f, 1f)
        };

        int[] _lighTypes = new int[NUM_LIGHTS]
        {
            1,
            0,
            0,
            1,
        };

        int[] _lightEnabled = new int[NUM_LIGHTS]
        {
            1,
            1,
            1,
            1
        };

        private Dictionary<BoundingBox, Npc> npcColliders = new Dictionary<BoundingBox, Npc>();
        

        Vector2 _plotSize;


        TimeHelper _timeHelper;

        DXInput _dxInput;

        private Vector3 npcSpeed = Vector3.Zero;

        private Texture _stoneTexture;
        
        

        public Game()
        {
            _renderForm = new RenderForm();
            _renderForm.UserResized += RenderFormResizedCallback;
            _directX3DGraphics = new DirectX3DGraphics(_renderForm);
            _renderer = new Renderer(_directX3DGraphics);
            _renderer.CreateConstantBuffers();
            _defaultMaterial = new MaterialProperties
            {
                Material = new Material
                {
                    Emmisive = new Vector4(0f, 0.0f, 0.0f, 1f),
                    Ambient = new Vector4(0f, 0.1f, 0.06f, 1.0f),
                    Diffuse = new Vector4(0f, 0.50980392f, 0.50980392f, 1f),
                    Specular = new Vector4(0.50196078f, 0.50196078f, 0.50196078f, 1f),
                    SpecularPower = 32f,
                    UseTexture = 1
                }
            };

            _floorMaterial = new MaterialProperties
            {
                Material = new Material
                {
                    Emmisive = new Vector4(0.25f, 0.25f, 0.25f, 1f),
                    Ambient = new Vector4(0.05f, 0.05f, 0.05f, 1f),
                    Diffuse = new Vector4(0.5f, 0.5f, 0.4f, 1.0f),
                    Specular = new Vector4(0.7f, 0.7f, 0.04f, 1f),
                    SpecularPower = 10.0f,
                    UseTexture = 1
                }
            };
            
            var black = SharpDX.Color.Black;
            black.A = 100;
            
            _blackBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, black);


            /*SharpDX.DXGI.Device2 dxgiDevice2 = _directX3DGraphics.Device.QueryInterface<SharpDX.DXGI.Device2>();
            SharpDX.DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter;
            SharpDX.DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>();

            SharpDX.Direct2D1.Device d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice2);
            _d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);

            */
            _light.Lights = new Light[NUM_LIGHTS];
            for (int i = 0; i < NUM_LIGHTS; i++)
            {
                Light light = new Light();
                light.Enabled = _lightEnabled[i];
                light.LightType = _lighTypes[i];
                light.Color = _lightColors[i];
                light.SpotAngle = 0.785398f;
                light.ConstantAttenuation = 1.0f;
                light.LinearAttenuation = 0.08f;
                light.QuadraticAttenuation = 0.0f;
                light.Position = new Vector4((float)i * 5f - 5f, -9.5f, 0f, 1f);
                light.Direction = new Vector4(-light.Position.X, -light.Position.Y, -light.Position.Y, 0.0f);
                light.Direction.Normalize();

                _light.Lights[i] = light;
            }

            _plotSize = new Vector2(5.0f, 5.0f);

            _light.GlobalAmbient = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

            Loader loader = new Loader(_directX3DGraphics);
            _tetrahedronTexture = loader.LoadTextureFromFile("greyTexture.jpg", _renderer.AnisotropicSampler);
            _npcTexture = loader.LoadTextureFromFile("texture.bmp", _renderer.AnisotropicSampler);
            _plotTexture = loader.LoadTextureFromFile("edward-godlach-screenshot003.jpg", _renderer.AnisotropicSampler);
            _stoneTexture = loader.LoadTextureFromFile("stoneTexture.jpg", _renderer.AnisotropicSampler);
            
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();

            var fileStream = new FileStream("mainCharacter.obj", FileMode.Open);
            var result = objLoader.Load(fileStream);
            
            _player = loader.LoadMeshObjectFromObjFile(result, new Vector4(ToDecart(new Vector3(0f, 0f, 0f)), 1f), 0f, 0f, 0.0f, ref _plotTexture, _renderer.AnisotropicSampler);
            
            _playerCollider = new BoundingBox(new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y), result.Vertices.Min(v => v.Z)) + (Vector3)_player.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y), result.Vertices.Max(v => v.Z)) + (Vector3)_player.Position);


            _firstNpcObject = loader.LoadMeshObjectFromObjFile(result, new Vector4(ToDecart(new Vector3(-1f, -1f, 0f)), 1f), 0f,
                0f, 0f, ref _npcTexture, _renderer.AnisotropicSampler);
            
            _firstNpcCollider = new BoundingBox(new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y), result.Vertices.Min(v => v.Z)) + (Vector3)_firstNpcObject.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y), result.Vertices.Max(v => v.Z)) + (Vector3)_firstNpcObject.Position);

            _secondNpcObject = loader.LoadMeshObjectFromObjFile(result, new Vector4(ToDecart(new Vector3(0f, -3f, 0f)), 1f), 0f,
                0f, 0f, ref _npcTexture, _renderer.AnisotropicSampler);
            
            var secondNpcCollider = new BoundingBox(new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y), result.Vertices.Min(v => v.Z)) + (Vector3)_secondNpcObject.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y), result.Vertices.Max(v => v.Z)) + (Vector3)_secondNpcObject.Position);

           
            
            
            _treasureCollider = new BoundingBox(_firstNpcCollider.Minimum + ToDecart(new Vector3(1, 1, 0)), _firstNpcCollider.Maximum + ToDecart(new Vector3(1, 1, 0)));


            MainInventoryItem shovelItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "shovel.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.1f), ShovelInteraction);
            
            treasureItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "treasure.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.1f), () => true);
            
            CollectibleItem trophyItem = new CollectibleItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "trophy.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.08f), "Ну, трофей за пятое место за соревнования по мини-футболу в г. Ельск");
            
            var firstNpc = new Npc(_firstNpcObject,
                "Здарова, я, когда был маленьким, закопал сокровища у своего дома, \r\nно забыл где, осталась только карта, помоги, пожалуйста.",
                "Ну я ж тебе уже всё сказал, давай иди ищи, чувак.",
                "Блин, мужик, спасибо большое, на вот тебе за это кубок \r\nза победу в турнире по мини-футболу среди юношей!",
                ref OnPlayerFindingTreasure, new MainInventoryItem[]
                {
                    shovelItem
                }, new []
                {
                    trophyItem
                }, new []
                {
                    treasureItem
                });
            
            CollectibleItem bananaItem = new CollectibleItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "bananaPeel.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.05f), "Просто кожура от банана, я бы лучше её вообще выкинул, но ты как знаешь.");


            var secondNpc = new Npc(_secondNpcObject,
                "Привет, мне тут одну загадку дали, помоги-ка, я тебе подгончик сделаю",
                "Ну, я ж тебе уже записку с загадкой дал, давай вали отсюда",
                "Красава, не ожидал. Вот тебе кожура от банана!", ref OnSecondNpcTaskSolved, null, new []
                {
                    bananaItem
                });
            
            npcColliders.Add(_firstNpcCollider, firstNpc);
            npcColliders.Add(secondNpcCollider, secondNpc);

            _secondNpcSequentialQuest = new SequentialQuest(GenerateSecondNpcSlabs(loader), new List<int>()
            {
                0, 1, 2, 3
            }, secondNpc);
            
            _secondNpcSequentialQuest.OnRightPlayerSequence += OnRightPlayerSequenceSecondNpc;
            
            // МОДЕЛЬ
            // если хочу подвинуть на isoX вправо и на isoY влево, то нужно двигать:
            // по x: на (2 * isoY + isoX) / 2;
            // по y: на (2 * isoY - isoX) / 2;
            _plot = loader.MakePlot(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), 0.0f, 0.0f, 0.0f, _plotSize.X, _plotSize.Y, -1f);
            
            _camera = new Camera(new Vector4( -10.0f, 8.25f, -10.0f, 1.0f));
            _timeHelper = new TimeHelper();

            //_blackBrush = new SharpDX.Direct2D1.SolidColorBrush(_directX3DGraphics.D2dContext, SharpDX.Color.Black);

            var green = SharpDX.Color.Green;

            green.A = 100;

            var white = SharpDX.Color.WhiteSmoke;
            white.A = 100;

            _greenBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, green);
            _redBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, SharpDX.Color.Red);
            _blueBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, SharpDX.Color.Blue);
            _purpleBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, SharpDX.Color.Purple);
            _whiteBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, white);

            _playerBitmap = DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "textureTetrahedron.png");

            //var rectangleGeometry = new RoundedRectangleGeometry(d2dFactory, new RoundedRectangle() { RadiusX = 32, RadiusY = 32, Rect = new RectangleF(128, 128, width - 128 * 2, height - 128 * 2) });

            _camera.PitchBy(MathUtil.Pi / 6f);
            _camera.YawBy(MathUtil.Pi / 4f);


            //_tetrahedron.YawBy(MathUtil.Pi / 4f);
            //_tetrahedron.PitchBy(MathUtil.Pi / 6f);

            loader.Dispose();
            loader = null;
            _dxInput = new DXInput(_renderForm.Handle);

            InventoryItem<MainInventoryItem>[] mainInventoryStartItems = new InventoryItem<MainInventoryItem>[3];


            Vector2 itemCenter = new Vector2(100, 550);
            Vector2 defaultSize = new Vector2(800, 600);
            float angle = 0f;
            float defaultBoxScale = 1f;


            MainInventoryItem item = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "key.png"), itemCenter, angle, defaultSize, 0.05f), (() => true));
            
            
            mainInventoryStartItems[0] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, item, itemCenter, angle,
                defaultSize, defaultBoxScale);

            itemCenter = new Vector2(200, 550);
            
            item = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "loope.png"), itemCenter, angle, defaultSize, 0.02f), (() => true));
            
            
            mainInventoryStartItems[1] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, item, itemCenter, angle,
                defaultSize, defaultBoxScale);

            itemCenter = new Vector2(300, 550);
            
            /*item = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "loope.png"), itemCenter, angle, defaultSize, 0.02f), (() => true));
            */
            
            mainInventoryStartItems[2] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, null, itemCenter, angle,
                defaultSize, defaultBoxScale);
            
            _mainInventory = new Inventory<MainInventoryItem>(_directX3DGraphics, _dxInput, mainInventoryStartItems);

            InventoryItem<CollectibleItem>[] collectibleInventoryItems = new InventoryItem<CollectibleItem>[4];
            
            collectibleInventoryItems[0] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null, new SharpDX.Vector2(300, 450), 0f,
                new SharpDX.Vector2(800, 600), 2f, 2f);

            collectibleInventoryItems[1] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null,new SharpDX.Vector2(500, 450), 0f,
                new SharpDX.Vector2(800, 600),2f,  2f);

            collectibleInventoryItems[2] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null, new SharpDX.Vector2(300, 250), 0f,
                new SharpDX.Vector2(800, 600),2f, 2f);
            
            collectibleInventoryItems[3] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null, new SharpDX.Vector2(500, 250), 0f,
                new SharpDX.Vector2(800, 600),2f, 2f);
            
            _collectiblesInventory = new Inventory<CollectibleItem>(_directX3DGraphics, _dxInput, collectibleInventoryItems);

            
            xaudio2 = new XAudio2();
            
            var masteringVoice = new MasteringVoice(xaudio2);

            PLaySoundFile(xaudio2, "aa", "backgroundMusic.wav");

        }

        void PLaySoundFile(XAudio2 device, string text, string fileName)
        {
            var stream = new SoundStream(File.OpenRead(fileName));
            var waveFormat = stream.Format;
            audioBuffer = new AudioBuffer
            {
                Stream = stream.ToDataStream(),
                AudioBytes = (int) stream.Length,
                Flags = BufferFlags.EndOfStream,
                LoopCount = AudioBuffer.LoopInfinite
            };
            stream.Close();
            
            sourceVoice = new SourceVoice(device, waveFormat, true);
            // Adds a sample callback to check that they are working on source voices
            sourceVoice.BufferEnd += SourceVoiceOnBufferEnd;
            sourceVoice.SubmitSourceBuffer(audioBuffer, stream.DecodedPacketsInfo);
            sourceVoice.Start();
        }

        private void SourceVoiceOnBufferEnd(IntPtr obj)
        {
            //sourceVoice.Stop();
            //sourceVoice.Start();
            //PLaySoundFile(xaudio2, "aa", "backgroundMusic.wav");
        }

        private void OnRightPlayerSequenceSecondNpc()
        {
            OnSecondNpcTaskSolved?.Invoke();
            textLayoutText = "ПАБЕДА";
        }

        public List<InteractableObject> GenerateSecondNpcSlabs(Loader loader)
        {
            BoundingBox[] boundingBoxes = new BoundingBox[4];
            MeshObject[] meshObjects = new MeshObject[4];

            var slabSize = new Vector2(0.1f, 0.1f);
            
            meshObjects[0] = loader.MakePlot(new Vector4(ToDecart(new Vector3(3.0f, 0.0f, 0.0f)),0f), 0.0f, 0.0f, 0.0f, slabSize.X, slabSize.Y, 0f, ref boundingBoxes[0]);
            meshObjects[1] = loader.MakePlot(new Vector4(ToDecart(new Vector3(1.0f, 0.0f, 0.0f)),0f), 0.0f, 0.0f, 0.0f, slabSize.X, slabSize.Y, 0f, ref boundingBoxes[1]);
            meshObjects[2] = loader.MakePlot(new Vector4(ToDecart(new Vector3(2.0f, 1.0f, 0.0f)),0f), 0.0f, 0.0f, 0.0f, slabSize.X, slabSize.Y, 0f, ref boundingBoxes[2]);
            meshObjects[3] = loader.MakePlot(new Vector4(ToDecart(new Vector3(2.0f, -1.0f, 0.0f)),0f), 0.0f, 0.0f, 0.0f, slabSize.X, slabSize.Y, 0f, ref boundingBoxes[3]);

            
            
            List<InteractableObject> interactableObjects = new List<InteractableObject>();
            
            for (int i = 0; i < boundingBoxes.Length; i++)
            {
                boundingBoxes[i].Maximum += new Vector3(0f, 2f, 0f);
                interactableObjects.Add(new InteractableObject(meshObjects[i], boundingBoxes[i]));
            }

            return interactableObjects;

        }
        private bool ShovelInteraction()
        {
            if (_playerCollider.Intersects(_treasureCollider))
            {
                OnPlayerFindingTreasure?.Invoke();
                _mainInventory.ChangeItem(treasureItem, 2); //TODO Поменять на конкретный индекс лопаты
                textLayoutText = "УРА";
                return true;
            }
            else
            {
                
                textLayoutText = "Здесь ничо нет, мужик\r\n" + _player.Position + "\r\n" + _treasureCollider.Center;
                return false;
            }
        }

        private void RenderFormResizedCallback(object sender, EventArgs e)
        {
            _directX3DGraphics.Resize();
            _camera.Aspect = _renderForm.ClientSize.Width / (float)_renderForm.ClientSize.Height;
            _camera.Width = _renderForm.ClientSize.Width;
            _camera.Height = _renderForm.ClientSize.Height;
        }

        private bool _firstRun = true;
        private bool _isDisplayingText;
        
        private TextLayout _textLayout = null;
        private TextFormat testTextFormat;
        private readonly MainInventoryItem treasureItem;
        private readonly MeshObject _secondNpcObject;
        private XAudio2 xaudio2;
        private static SourceVoice sourceVoice;
        private static AudioBuffer audioBuffer;

        private void CheckSecondQuestCompleting()
        {
            if (_secondNpcSequentialQuest.IsStarting)
            {
                for (var index = 0; index < _secondNpcSequentialQuest.InteractableObjects.Count; index++)
                {
                    var interactableObject = _secondNpcSequentialQuest.InteractableObjects[index];
                    if (_playerCollider.Intersects(interactableObject.MeshCollider))
                    {
                        if (index == _secondNpcSequentialQuest.LastPlayerInteract)
                        {
                            textLayoutText = "Харош";
                            // может быть, просто идти дальше по циклу
                            return;
                        }
                        else
                        {
                            if (!_secondNpcSequentialQuest.AddToPlayerSequence(index))
                            {
                                textLayoutText = "Айа-айа, неправильно!";
                            }
                            else
                            {
                                textLayoutText = "Харош";
                                
                            }
                        }
                    }
                }
            }
        }

        public void RenderLoopCallBack()
        {
            if (_firstRun)
            {
                RenderFormResizedCallback(this, EventArgs.Empty);
                _firstRun = false;
            }
            
            _timeHelper.Update();
            _renderForm.Text = "FPS: " + _timeHelper.FPS.ToString();
            //_player.YawBy(_timeHelper.DeltaT * MathUtil.TwoPi * 0.1f);

            _dxInput.Update();

            /*_camera.YawBy(_dxInput.GetMouseDeltaX() * 0.0005f);
            _camera.PitchBy(_dxInput.GetMouseDeltaY() * 0.0001f);*/


            Vector3 playerMovement = Vector3.Zero;

            float deltaYaw = 0f;
            
            if (_dxInput.IsKeyPressed(Key.W))
            {
                _player.Yaw = MathUtil.Pi / 4f;
                playerMovement += new Vector3(0.0f, 1.0f, 0.0f);
            }
            if (_dxInput.IsKeyPressed(Key.S))
            { 
                _player.Yaw = MathUtil.Pi * 5f / 4f;
                playerMovement += new Vector3(0.0f, -1.0f, 0.0f);
            }
            if (_dxInput.IsKeyPressed(Key.A))
            {
                _player.Yaw = -MathUtil.Pi / 4f;
                playerMovement += new Vector3(-1.0f, 0.0f, 0.0f);
                //cameraMovement.X += .05f;
                //cameraMovement.Z -= .05f;
            }
            if (_dxInput.IsKeyPressed(Key.D))
            {
                _player.Yaw = MathUtil.Pi * 3 / 4f;
                playerMovement += new Vector3(1.0f, 0.0f, 0.0f);
            }
            /*if (_dxInput.IsKeyPressed(Key.Space))
            {
                playerMovement.Y += .1f;
            }
            if (_dxInput.IsKeyPressed(Key.LeftControl))
            {
                playerMovement.Y -= .1f;
            }*/
            
            playerMovement.Normalize();
            playerMovement = ToDecart(playerMovement);
            
            playerMovement *= _timeHelper.DeltaT;
            
            _player.MoveBy(playerMovement.X, playerMovement.Y, playerMovement.Z);
            _playerCollider.Minimum += playerMovement;
            _playerCollider.Maximum += playerMovement;
            _camera.MoveBy(playerMovement.X, playerMovement.Y, playerMovement.Z);
            
            
            
            PressKeyboard();
            Matrix viewMatrix = _camera.GetViewMatrix();
            Matrix projectionMatrix = _camera.GetProjectionMatrix();
            _light.EyePosition = _camera.Position;
            _renderer.BeginRender();


            //  _renderer.SetPerObjectConstantBuffer(_timeHelper.Time, 1);

            _renderer.SetLightConstantBuffer(_light);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_player.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_tetrahedronTexture);
            _renderer.RenderMeshObject(_player);
            
            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_firstNpcObject.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_npcTexture);
            _renderer.RenderMeshObject(_firstNpcObject);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            
            for (int i = 0; i < _secondNpcSequentialQuest.InteractableObjects.Count; i++)
            {
                _renderer.UpdatePerObjectConstantBuffers(_secondNpcSequentialQuest.InteractableObjects[i].MeshObject.GetWorldMatrix(), viewMatrix, projectionMatrix);
                _renderer.SetTexture(_stoneTexture);
                _renderer.RenderMeshObject(_secondNpcSequentialQuest.InteractableObjects[i].MeshObject);
            }
            
            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_secondNpcObject.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_npcTexture);
            _renderer.RenderMeshObject(_secondNpcObject);

            _renderer.SetPerObjectConstantBuffer(_floorMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_plot.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_plotTexture);
            _renderer.RenderMeshObject(_plot);

            _renderer.EndRender();

            if (_dxInput.IsKeyReleased(Key.M))
            {
                _isMap = !_isMap;
            }

            if (_dxInput.IsKeyReleased(Key.I))
            {
                _isOpenCollectibleItems = !_isOpenCollectibleItems;
            }

            if (_dxInput.IsKeyReleased(Key.Q))
            {
                var activeItem = _mainInventory.GetActiveItem();

                if (activeItem != null)
                {
                    if(activeItem.Item != null)
                        activeItem.Item.Interact();
                }
            }
            
            testTextFormat = new TextFormat(_directX3DGraphics.FactoryDWrite, "Calibri", 28)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center,
            };

            _directX3DGraphics.D2DRenderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
                 //= new TextLayout(_directX3DGraphics.FactoryDWrite, $"NPC:{_firstNpcCollider.Center},\n\r Player:{_playerCollider.Center}", testTextFormat, _renderForm.Width, _renderForm.Height);

            _directX3DGraphics.D2DRenderTarget.BeginDraw();

            CheckSecondQuestCompleting();
            
            if (_isDisplayingText && _dxInput.IsKeyReleased(Key.E))
            {
                _isDisplayingText = false;
            }

            if (!_isDisplayingText && _dxInput.IsKeyReleased(Key.E))
            {
                foreach (var npc in npcColliders)
                {
                    if (npc.Key.Intersects(_playerCollider))
                    {
                        _isDisplayingText = true;
                        NpcResponse npcResponse = npc.Value.Interact();
                        textLayoutText = npcResponse.ResponseText;
                        
                        if (npc.Value.NpcState == NpcStates.AfterQuestComplete)
                        {
                            var takenItems = npcResponse.Items;

                            if (takenItems != null)
                            {
                                for (int i = 0; i < takenItems.Length; i++)
                                {
                                    _mainInventory.RemoveItem(takenItems[i]);
                                }
                            }
                            
                            var collectibles = npc.Value.GetCollectibles();
                            if (collectibles != null)
                            {
                                for (int i = 0; i < collectibles.Length; i++)
                                {
                                    _collectiblesInventory.AddItem(collectibles[i]);
                                }
                            }

                            npcSpeed = ToDecart(new Vector3(0f, 0.0001f, 0f));
                        }
                        else
                        {
                            var items = npcResponse.Items;
                            
                            if (items != null)
                            {
                                for (int i = 0; i < items.Length; i++)
                                {
                                    _mainInventory.AddItem(items[i]);
                                }
                            }
                        }
                    }
                }
            }
            
            npcColliders[_firstNpcCollider].GameObject.MoveBy(npcSpeed.X, npcSpeed.Y, npcSpeed.Z);

            /*if (_firstNpcCollider.Intersects(_playerCollider) && _dxInput.IsKeyReleased(Key.E))
            {
                
                textLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, $"Здарова, мужик, вот те карта, найди там мой сундук, \r\nа я тебе трофейчик небольшой дам, давай, на связи", testTextFormat, _renderForm.Width, _renderForm.Height);
            }*/


            if (_isOpenCollectibleItems)
            {
                _directX3DGraphics.D2DRenderTarget.FillRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(0f, 0f, _renderForm.Width, _renderForm.Height), _blackBrush); // рамка карты
                _collectiblesInventory.DrawInventory();
            }
            else
            {
                _mainInventory.DrawInventory();
            }
            
            /*if (_isMap)
            {
                SharpDX.Direct2D1.PathGeometry triangleGeometry = new SharpDX.Direct2D1.PathGeometry(_directX3DGraphics.D2dFactory); // тетраэдр
                using (SharpDX.Direct2D1.GeometrySink geoSink = triangleGeometry.Open())
                {
                    Vector2 triangleCenter = new Vector2((_player.Position.X - _camera.Position.X) + (_renderForm.Width) / 2.0f, ((_renderForm.Height) / 2.0f) - (_player.Position.Z - _camera.Position.Z)); // 
                    geoSink.BeginFigure(new Vector2(triangleCenter.X, triangleCenter.Y - 15f), SharpDX.Direct2D1.FigureBegin.Filled); // верхняя вершина
                    geoSink.AddLine(new Vector2(triangleCenter.X + 15f, triangleCenter.Y + 15f)); // правая нижняя
                    geoSink.AddLine(new Vector2(triangleCenter.X - 15f, triangleCenter.Y + 15f)); // левая вершина
                    geoSink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
                    geoSink.Close();
                }

                SharpDX.Direct2D1.PathGeometry secondTriangleGeometry = new SharpDX.Direct2D1.PathGeometry(_directX3DGraphics.D2dFactory); // угол обзора наблюдателя
                using (SharpDX.Direct2D1.GeometrySink geoSink = secondTriangleGeometry.Open())
                {
                    float directionalLineRadius = 75;

                    var firstPoint = new RawVector2(_renderForm.Width / 2.0f + directionalLineRadius * (float)Math.Cos(Math.PI / 2.0f - _camera.Yaw - Math.PI / 3.0f),
                        _renderForm.Height / 2.0f - directionalLineRadius * (float)Math.Sin(Math.PI / 2.0f - _camera.Yaw - Math.PI / 3.0f));

                    var secondPoint = new RawVector2(_renderForm.Width / 2.0f + directionalLineRadius * (float)Math.Cos(Math.PI / 2.0f - _camera.Yaw + Math.PI / 3.0f),
                        _renderForm.Height / 2.0f - directionalLineRadius * (float)Math.Sin(Math.PI / 2.0f - _camera.Yaw + Math.PI / 3.0f));

                    geoSink.BeginFigure(new Vector2(_renderForm.Width / 2.0f, _renderForm.Height / 2.0f), SharpDX.Direct2D1.FigureBegin.Filled); // центр окружности
                    //geoSink.AddLine(new Vector2(_renderForm.Width / 2.0f - 15.0f, _renderForm.Height / 2.0f));
                    geoSink.AddLine(secondPoint); // левая вершина
                    geoSink.AddLine(firstPoint); // правая вершина
                    //geoSink.AddLine(new Vector2(_renderForm.Width / 2.0f + 15.0f, _renderForm.Height / 2.0f));
                    geoSink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
                    geoSink.Close();
                }


                Vector4 plotMapSize = new Vector4((_plot.Position.X - _plotSize.X) - _camera.Position.X, _camera.Position.Z - (_plot.Position.Z - _plotSize.Y),
                    (_plot.Position.X + _plotSize.X) - _camera.Position.X, _camera.Position.Z - (_plot.Position.Z + _plotSize.Y));



                //_directX3DGraphics.D2DRenderTarget.Clear(Color.Black);



                _directX3DGraphics.D2DRenderTarget.FillRectangle(new RawRectangleF(plotMapSize.X * 2.0f + (_renderForm.Width) / 2.0f, plotMapSize.Y * 2.0f + (_renderForm.Height) / 2.0f,
                    plotMapSize.Z * 2.0f + (_renderForm.Width) / 2.0f, plotMapSize.W * 2.0f + (_renderForm.Height) / 2.0f), _purpleBrush); // поверхность
                _directX3DGraphics.D2DRenderTarget.FillRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(100.0f, 50.0f, _renderForm.Width - 100.0f, _renderForm.Height - 100.0f), _greenBrush); // рамка карты




                /*_directX3DGraphics.D2DRenderTarget.DrawLine(new RawVector2(_renderForm.Width / 2.0f, _renderForm.Height / 2.0f),
                   firstPoint, _whiteBrush, 4.0f); // направление взгляда наблюдателя


                _directX3DGraphics.D2DRenderTarget.DrawLine(new RawVector2(_renderForm.Width / 2.0f, _renderForm.Height / 2.0f),
                   secondPoint, _whiteBrush, 4.0f); // направление взгляда наблюдателя




                //_directX3DGraphics.D2DRenderTarget.FillGeometry(triangleGeometry, _blueBrush); // тетраэдр

                ImagingFactory imagingFactory = new ImagingFactory();

                NativeFileStream fileStream = new NativeFileStream("tetrahedronchik.png",
                    NativeFileMode.Open, NativeFileAccess.Read);

                BitmapDecoder bitmapDecoder = new BitmapDecoder(imagingFactory, fileStream, DecodeOptions.CacheOnDemand);
                BitmapFrameDecode frame = bitmapDecoder.GetFrame(0);

                FormatConverter converter = new FormatConverter(imagingFactory);
                converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);

                var newBitmap = SharpDX.Direct2D1.Bitmap1.FromWicBitmap(_directX3DGraphics.D2DRenderTarget, converter);

                var bitmapBrush = new BitmapBrush(_directX3DGraphics.D2DRenderTarget, newBitmap);

                //_directX3DGraphics.D2DRenderTarget.FillRectangle(new RawRectangleF(100, 100, 200, 200), bitmapBrush);

                var transform = _directX3DGraphics.D2DRenderTarget.Transform;
                //new Vector2(, );

                _directX3DGraphics.D2DRenderTarget.Transform = Matrix3x2.Translation((_player.Position.X - _camera.Position.X) + (_renderForm.Width) / 2.0f - newBitmap.Size.Width/2.0f, ((_renderForm.Height) / 2.0f) - (_player.Position.Z - _camera.Position.Z) - newBitmap.Size.Height / 2.0f);

                _directX3DGraphics.D2DRenderTarget.DrawBitmap(newBitmap, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);

                _directX3DGraphics.D2DRenderTarget.Transform = transform;

                _directX3DGraphics.D2DRenderTarget.FillGeometry(secondTriangleGeometry, _whiteBrush); // угол камеры

                _directX3DGraphics.D2DRenderTarget.FillEllipse(new Ellipse(new SharpDX.Mathematics.Interop.RawVector2(_renderForm.Width / 2.0f, _renderForm.Height / 2.0f), 10.0f, 10.0f), _blueBrush); // игрок


            }*/

            if (_isDisplayingText)
            {
                //textLayoutText = (_secondNpcSequentialQuest.InteractableObjects[0].MeshCollider.Center).ToString() + _player.Position;
                _textLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, textLayoutText, testTextFormat, _renderForm.Width, _renderForm.Height);
                _directX3DGraphics.D2DRenderTarget.DrawTextLayout(new RawVector2(_renderForm.Width / 2 - 400, _renderForm.Height / 2 - 100), _textLayout, _whiteBrush, DrawTextOptions.None);
            }

            _directX3DGraphics.D2DRenderTarget.EndDraw();
            _directX3DGraphics.SwapChain.Present(0, PresentFlags.None);


            //_directX3DGraphics.D2dContext.DrawRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(
            //    _renderForm.Left + 100.0f, _renderForm.Top + 100.0f, _renderForm.Right - 100.0f, _renderForm.Bottom - 100.0f), _blackBrush);
            // _directX3DGraphics.D2dContext.EndDraw();
        }

        private void CheckPositions()
        {
            var a = _camera.Roll;
            var b = _camera.Pitch; // наклон по y (вокруг x)
            var c = _camera.Yaw; // поворот по x (вокруг y)
        }

        private void PressKeyboard()
        {
        }

        public static Vector3 ToDecart(Vector3 isometric)
        {
            Vector3 decart = new Vector3((2 * isometric.Y + isometric.X) / 2f,
                0f, (2 * isometric.Y - isometric.X) / 2f);

            return decart;
        }

        public void Run()
        {
            RenderLoop.Run(_renderForm, RenderLoopCallBack);
        }

        public void Dispose()
        {
            _player.Dispose();
            _tetrahedronTexture.Dispose();
            _plot.Dispose();
            _renderer.Dispose();
            _directX3DGraphics.Dispose();
        }
    }
}

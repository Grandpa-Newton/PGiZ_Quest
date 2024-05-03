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
using System.Windows.Forms;
using ObjLoader.Loader.Common;
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

        private Sprite _playerIcon;

        private DialogueVisual _mainDialogue;

        private Vector2 dialogueSpritePosition;

        SharpDX.Direct2D1.DeviceContext _d2dContext;

        private SharpDX.Direct2D1.Bitmap1 d2dTarget;

        SharpDX.Direct2D1.SolidColorBrush _greenBrush;

        SharpDX.Direct2D1.SolidColorBrush _redBrush;

        SharpDX.Direct2D1.SolidColorBrush _blueBrush;

        SharpDX.Direct2D1.SolidColorBrush _purpleBrush;

        SharpDX.Direct2D1.SolidColorBrush _whiteBrush;

        SharpDX.Direct2D1.SolidColorBrush _fullWhiteBrush;

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

        private Animation _playerMoveAnimation;

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


        private bool _firstRun = true;
        private bool _isDisplayingText;

        private TextLayout _textLayout = null;
        private TextFormat testTextFormat;
        private TextFormat collectibleTextFormat;
        private readonly MainInventoryItem treasureItem;
        private readonly MeshObject _secondNpcObject;
        private XAudio2 xaudio2;
        private static SourceVoice sourceVoice;
        private static AudioBuffer audioBuffer;
        private float _playerSpeed = 1f;
        private Texture _islandTexture;
        private MeshObject _island;
        private Texture _playerTexture;
        private readonly Animation _playerIdleAnimation;

        private BoundingSphere _islandBorder;

        private List<MeshObject> _palms = new List<MeshObject>();

        private void CreatingObjects()
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

            _mainDialogue = new DialogueVisual(_directX3DGraphics.D2DRenderTarget);

            _blackBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, black);

            dialogueSpritePosition = new Vector2(100f, 50f);
            collectibleTextFormat = new TextFormat(_directX3DGraphics.FactoryDWrite, "Calibri", 50)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center,
            };


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
        }

        private void LoadingObjectsAndColliders(Loader loader)
        {
        }

        public Game()
        {
            CreatingObjects();

            Loader loader = new Loader(_directX3DGraphics);


            // ***
            _tetrahedronTexture = loader.LoadTextureFromFile("greyTexture.jpg", _renderer.AnisotropicSampler);
            _npcTexture = loader.LoadTextureFromFile("texture.bmp", _renderer.AnisotropicSampler);
            _plotTexture = loader.LoadTextureFromFile("edward-godlach-screenshot003.jpg", _renderer.AnisotropicSampler);
            _stoneTexture = loader.LoadTextureFromFile("stoneTexture.jpg", _renderer.AnisotropicSampler);

            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();

            var fileStream = new FileStream("Boss.obj", FileMode.Open);
            var result = objLoader.Load(fileStream);

            fileStream.Close();

            _player = loader.LoadMeshObjectFromObjFile(result, new Vector4(ToDecart(new Vector3(0f, 0f, 0f)), 1f), 0f,
                0f, 0.0f, ref _playerTexture, _renderer.AnisotropicSampler);

            _playerMoveAnimation = new Animation();
            _playerMoveAnimation.Load("Animations/Walk/", loader, _renderer.AnisotropicSampler, 0.1f);

            _playerIdleAnimation = new Animation();
            _playerIdleAnimation.Load("Animations/Idle/", loader, _renderer.AnisotropicSampler, 0.1f);

            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("palm.obj", FileMode.Open);
            result = objLoader.Load(fileStream);
            fileStream.Close();

            _palm = loader.LoadMeshObjectFromObjFile(result, new Vector4(ToDecart(new Vector3(0f, 0f, 0f)), 1f), 0f,
                0f, 0.0f, ref _palmTexture, _renderer.AnisotropicSampler, 0.01f);

            int numberOfPalms = 30;

            for (int i = -3; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    _palms.Add(loader.LoadMeshObjectFromObjFile(result,
                        new Vector4(ToDecart(new Vector3((float)i, j, 0f)), 1f), 0f,
                        0f, 0.0f, ref _palmTexture, _renderer.AnisotropicSampler, 0.01f));
                }
            }

            _playerCollider = new BoundingBox(
                new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y),
                    result.Vertices.Min(v => v.Z)) * 0.01f + (Vector3)_player.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                    result.Vertices.Max(v => v.Z)) * 0.01f + (Vector3)_player.Position);


            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("Boss.obj", FileMode.Open);
            result = objLoader.Load(fileStream);

            fileStream.Close();

            _firstNpcObject = loader.LoadMeshObjectFromObjFile(result,
                new Vector4(ToDecart(new Vector3(-1.5f, -1.5f, 0f)), 1f), 0f,
                0f, 0f, ref _npcTexture, _renderer.AnisotropicSampler, 0.1f);

            _firstNpcCollider = new BoundingBox(
                new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y),
                    result.Vertices.Min(v => v.Z)) * 0.1f + (Vector3)_firstNpcObject.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                    result.Vertices.Max(v => v.Z)) * 0.1f + (Vector3)_firstNpcObject.Position);

            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("Boss.obj", FileMode.Open);
            result = objLoader.Load(fileStream);

            fileStream.Close();

            _secondNpcObject = loader.LoadMeshObjectFromObjFile(result,
                new Vector4(ToDecart(new Vector3(0f, -1.5f, 0f)), 1f), 0f,
                0f, 0f, ref _npcTexture, _renderer.AnisotropicSampler, 0.1f);

            var secondNpcCollider = new BoundingBox(
                new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y),
                    result.Vertices.Min(v => v.Z)) * 0.1f + (Vector3)_secondNpcObject.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                    result.Vertices.Max(v => v.Z)) * 0.1f + (Vector3)_secondNpcObject.Position);

            _treasureCollider = new BoundingBox(_firstNpcCollider.Minimum + ToDecart(new Vector3(1, 1, 0)),
                _firstNpcCollider.Maximum + ToDecart(new Vector3(1, 1, 0)));


            // ***

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
                "Мужик, ну я тебе самое ценное в своей жизни отдал уже!",
                ref OnPlayerFindingTreasure, new MainInventoryItem[]
                {
                    shovelItem
                }, new[]
                {
                    trophyItem
                }, new[]
                {
                    treasureItem
                }, new PlayerBoost(2f, 1));

            CollectibleItem bananaItem = new CollectibleItem(new Sprite(_directX3DGraphics,
                    DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "bananaPeel.png"), Vector2.Zero,
                    0f,
                    new Vector2(800, 600), 0.05f),
                "Просто кожура от банана, я бы лучше её вообще выкинул, но ты как знаешь.");

            _playerIcon = new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "playerIcon.png"), dialogueSpritePosition,
                0f, new Vector2(800, 600), 0.75f);
            
            var secondNpc = new Npc(_secondNpcObject,
                "Привет, мне тут одну загадку дали, помоги-ка, я тебе подгончик сделаю",
                "Ну, я ж тебе уже записку с загадкой дал, давай вали отсюда",
                "Красава, не ожидал. Вот тебе кожура от банана!",
                "*молчание*", ref OnSecondNpcTaskSolved, null, new[]
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
            //_plot = loader.MakePlot(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), 0.0f, 0.0f, 0.0f, _plotSize.X, _plotSize.Y, -1f);


            /*var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();

            var fileStream = new FileStream("mainCharacter.obj", FileMode.Open);
            var result = objLoader.Load(fileStream);*/

            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("water.obj", FileMode.Open);
            result = objLoader.Load(fileStream);


            _plot = loader.LoadMeshObjectFromObjFile(result, new Vector4(0.0f, -0.01f, 0.0f, 1.0f), 0.0f, 0.0f, 0.0f,
                ref _plotTexture, _renderer.AnisotropicSampler, 0.3f);

            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("island3.obj", FileMode.Open);
            result = objLoader.Load(fileStream);


            _island = loader.LoadMeshObjectFromObjFile(result, new Vector4(0.0f, -0.02f, 0.0f, 1.0f), 0.0f, 0.0f, 0.0f,
                ref _islandTexture, _renderer.AnisotropicSampler, 1.0f);
            _islandBorder = new BoundingSphere((Vector3)_island.Position, 3.07f);

            _camera = new Camera(new Vector4(-10.0f, 8.25f, -10.0f, 1.0f));
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
            _fullWhiteBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, SharpDX.Color.WhiteSmoke);

            _playerBitmap =
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "textureTetrahedron.png");

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
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "key.png"), itemCenter, angle,
                defaultSize, 0.05f), (() => true));


            mainInventoryStartItems[0] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, item, itemCenter,
                angle,
                defaultSize, defaultBoxScale);

            itemCenter = new Vector2(200, 550);

            item = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "loope.png"), itemCenter, angle,
                defaultSize, 0.02f), (() => true));


            mainInventoryStartItems[1] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, item, itemCenter,
                angle,
                defaultSize, defaultBoxScale);

            itemCenter = new Vector2(300, 550);

            /*item = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "loope.png"), itemCenter, angle, defaultSize, 0.02f), (() => true));
            */

            mainInventoryStartItems[2] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, null, itemCenter,
                angle,
                defaultSize, defaultBoxScale);

            _mainInventory = new Inventory<MainInventoryItem>(_directX3DGraphics, _dxInput, mainInventoryStartItems);

            InventoryItem<CollectibleItem>[] collectibleInventoryItems = new InventoryItem<CollectibleItem>[4];

            collectibleInventoryItems[0] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null,
                new SharpDX.Vector2(300, 450), 0f,
                new SharpDX.Vector2(800, 600), 2f, 2f);

            collectibleInventoryItems[1] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null,
                new SharpDX.Vector2(500, 450), 0f,
                new SharpDX.Vector2(800, 600), 2f, 2f);

            collectibleInventoryItems[2] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null,
                new SharpDX.Vector2(300, 250), 0f,
                new SharpDX.Vector2(800, 600), 2f, 2f);

            collectibleInventoryItems[3] = new InventoryItem<CollectibleItem>(_directX3DGraphics, null,
                new SharpDX.Vector2(500, 250), 0f,
                new SharpDX.Vector2(800, 600), 2f, 2f);

            _collectiblesInventory =
                new Inventory<CollectibleItem>(_directX3DGraphics, _dxInput, collectibleInventoryItems);


            xaudio2 = new XAudio2();

            var masteringVoice = new MasteringVoice(xaudio2);

            PLaySoundFile(xaudio2, "aa", "backgroundMusic.wav");

            _renderForm.WindowState = FormWindowState.Maximized;
        }


        void PLaySoundFile(XAudio2 device, string text, string fileName)
        {
            var stream = new SoundStream(File.OpenRead(fileName));
            var waveFormat = stream.Format;
            audioBuffer = new AudioBuffer
            {
                Stream = stream.ToDataStream(),
                AudioBytes = (int)stream.Length,
                Flags = BufferFlags.EndOfStream,
                LoopCount = AudioBuffer.LoopInfinite
            };
            stream.Close();

            sourceVoice = new SourceVoice(device, waveFormat, true);

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

            meshObjects[0] = loader.MakePlot(new Vector4(ToDecart(new Vector3(3.0f, 0.0f, 0.0f)), 0f), 0.0f, 0.0f, 0.0f,
                slabSize.X, slabSize.Y, 0f, ref boundingBoxes[0]);
            meshObjects[1] = loader.MakePlot(new Vector4(ToDecart(new Vector3(1.0f, 0.0f, 0.0f)), 0f), 0.0f, 0.0f, 0.0f,
                slabSize.X, slabSize.Y, 0f, ref boundingBoxes[1]);
            meshObjects[2] = loader.MakePlot(new Vector4(ToDecart(new Vector3(2.0f, 1.0f, 0.0f)), 0f), 0.0f, 0.0f, 0.0f,
                slabSize.X, slabSize.Y, 0f, ref boundingBoxes[2]);
            meshObjects[3] = loader.MakePlot(new Vector4(ToDecart(new Vector3(2.0f, -1.0f, 0.0f)), 0f), 0.0f, 0.0f,
                0.0f, slabSize.X, slabSize.Y, 0f, ref boundingBoxes[3]);


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

        private bool _isPlayerInCollider;
        private readonly MeshObject _palm;
        private Texture _palmTexture;

        private void UpdateInput()
        {
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

            playerMovement *= _timeHelper.DeltaT * _playerSpeed;


            _playerCollider.Minimum += playerMovement;
            _playerCollider.Maximum += playerMovement;

            if (!_playerCollider.Intersects(_islandBorder))
            {
                _playerCollider.Minimum -= playerMovement;
                _playerCollider.Maximum -= playerMovement;
                playerMovement = Vector3.Zero;
            }

            _player.MoveBy(playerMovement.X, playerMovement.Y, playerMovement.Z);

            MeshObject currentAnimMesh;
            if (playerMovement != Vector3.Zero)
            {
                _playerIdleAnimation.StopAnimation();
                _playerMoveAnimation.ContinueAnimation();
                currentAnimMesh = _playerMoveAnimation.GetCurrentMesh();
            }
            else
            {
                _playerMoveAnimation.StopAnimation();
                _playerIdleAnimation.ContinueAnimation();
                currentAnimMesh = _playerIdleAnimation.GetCurrentMesh();
            }

            currentAnimMesh.MoveTo(_player.Position.X, _player.Position.Y, _player.Position.Z);
            currentAnimMesh.Pitch = _player.Pitch;
            currentAnimMesh.Roll = _player.Roll;
            currentAnimMesh.Yaw = _player.Yaw;
            _player = currentAnimMesh;

            //_playerCollider.Minimum += playerMovement;
            //_playerCollider.Maximum += playerMovement;


            _camera.MoveBy(playerMovement.X, playerMovement.Y, playerMovement.Z);
        }

        private void RenderObjects()
        {
            Matrix viewMatrix = _camera.GetViewMatrix();
            Matrix projectionMatrix = _camera.GetProjectionMatrix();
            _light.EyePosition = _camera.Position;
            _renderer.BeginRender();


            //  _renderer.SetPerObjectConstantBuffer(_timeHelper.Time, 1);

            _renderer.SetLightConstantBuffer(_light);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_player.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_playerTexture);
            _renderer.RenderMeshObject(_player);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_firstNpcObject.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_playerTexture);
            _renderer.RenderMeshObject(_firstNpcObject);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);

            for (int i = 0; i < _secondNpcSequentialQuest.InteractableObjects.Count; i++)
            {
                _renderer.UpdatePerObjectConstantBuffers(
                    _secondNpcSequentialQuest.InteractableObjects[i].MeshObject.GetWorldMatrix(), viewMatrix,
                    projectionMatrix);
                _renderer.SetTexture(_stoneTexture);
                _renderer.RenderMeshObject(_secondNpcSequentialQuest.InteractableObjects[i].MeshObject);
            }

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_secondNpcObject.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_playerTexture);
            _renderer.RenderMeshObject(_secondNpcObject);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_island.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_islandTexture);
            _renderer.RenderMeshObject(_island);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.SetTexture(_palmTexture);
            for (int i = 0; i < _palms.Count(); i++)
            {
                _renderer.UpdatePerObjectConstantBuffers(_palms[i].GetWorldMatrix(), viewMatrix, projectionMatrix);
                _renderer.RenderMeshObject(_palms[i]);
            }


            _renderer.SetPerObjectConstantBuffer(_floorMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_plot.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_plotTexture);
            _renderer.RenderMeshObject(_plot);

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
                    if (activeItem.Item != null)
                        activeItem.Item.Interact();
                }
            }
        }

        private void DrawHUD()
        {
            testTextFormat = new TextFormat(_directX3DGraphics.FactoryDWrite, "Calibri", 28)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center,
            };


            _directX3DGraphics.D2DRenderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
            //= new TextLayout(_directX3DGraphics.FactoryDWrite, $"NPC:{_firstNpcCollider.Center},\n\r Player:{_playerCollider.Center}", testTextFormat, _renderForm.Width, _renderForm.Height);

            _directX3DGraphics.D2DRenderTarget.BeginDraw();

            if (_isOpenCollectibleItems)
            {
                _directX3DGraphics.D2DRenderTarget.FillRectangle(
                    new SharpDX.Mathematics.Interop.RawRectangleF(0f, 0f, _renderForm.Width, _renderForm.Height),
                    _blackBrush); // рамка карты
                var collectibleTextLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, "Коллекционные предметы",
                    collectibleTextFormat, _renderForm.Width,
                    _renderForm.Height);
                _directX3DGraphics.D2DRenderTarget.DrawTextLayout(
                    new RawVector2(0f, 50 - _renderForm.Height / 2f), collectibleTextLayout, _fullWhiteBrush,
                    DrawTextOptions.None);
                _textLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, "Описание", testTextFormat,
                    _renderForm.Width, _renderForm.Height);
                _directX3DGraphics.D2DRenderTarget.DrawTextLayout(
                    new RawVector2(0f, 300), _textLayout, _fullWhiteBrush, DrawTextOptions.None);
                var activeItem = _collectiblesInventory.GetActiveItem();
                if (activeItem != null && activeItem.Item != null)
                {
                    _textLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, activeItem.Item.Description,
                        testTextFormat,
                        _renderForm.Width, _renderForm.Height);
                    _directX3DGraphics.D2DRenderTarget.DrawTextLayout(
                        new RawVector2(0f, 350), _textLayout, _fullWhiteBrush, DrawTextOptions.None);
                }

                _collectiblesInventory.DrawInventory();
            }
            else
            {
                _mainInventory.DrawInventory();
            }

            if (_isDisplayingText)
            {
                //_directX3DGraphics.D2DRenderTarget.DrawRoundedRectangle(new RoundedRectangle());
                //textLayoutText = (_secondNpcSequentialQuest.InteractableObjects[0].MeshCollider.Center).ToString() + _player.Position;
                _textLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, textLayoutText, testTextFormat,
                    _renderForm.Width, _renderForm.Height);
                _directX3DGraphics.D2DRenderTarget.DrawTextLayout(
                    new RawVector2(_renderForm.Width / 2 - 400, _renderForm.Height / 2 - 100), _textLayout, _whiteBrush,
                    DrawTextOptions.None);
            }

            _playerIcon.DefaultSize = new Vector2(_renderForm.Width, _renderForm.Height);
            _playerIcon.CenterPosition = new Vector2(320f * _renderForm.Width / 1920f, 320f * _renderForm.Height/1080f);

            if(!textLayoutText.IsNullOrEmpty())
                _mainDialogue.Draw(_directX3DGraphics.D2DRenderTarget, _directX3DGraphics.FactoryDWrite, _playerIcon, "Ванёк: ", textLayoutText, _renderForm);

            _directX3DGraphics.D2DRenderTarget.EndDraw();

            _directX3DGraphics.SwapChain.Present(0, PresentFlags.None);
        }

        private void UsePlayerBoost(PlayerBoost playerBoost)
        {
            _playerSpeed *= playerBoost.SpeedMultiplier;
            for (int i = 0; i < playerBoost.InventoryExpansion; i++)
            {
                _mainInventory.ExpanseInventory();
            }
            //_mainInventory.AddItem();
        }

        private void InteractWithNpc(KeyValuePair<BoundingBox, Npc> npc)
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

                var playerBoost = npcResponse.PlayerBoost;

                if (playerBoost != null)
                {
                    UsePlayerBoost(playerBoost);
                }

                npcSpeed = ToDecart(new Vector3(0f, 0.0001f, 0f));

                npc.Value.TakePrizes();
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

        private void UpdateNPC()
        {
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
                        InteractWithNpc(npc);
                    }
                }
            }

            npcColliders[_firstNpcCollider].GameObject.MoveBy(npcSpeed.X, npcSpeed.Y, npcSpeed.Z);
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

            _dxInput.Update();


            UpdateInput();

            RenderObjects();

            CheckSecondQuestCompleting();

            UpdateNPC();

            DrawHUD();

            _renderer.EndRender();
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
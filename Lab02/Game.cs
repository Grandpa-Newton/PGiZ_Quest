using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectInput;
using SharpDX;
using SharpDX.Windows;
using Lab01;
using System.IO;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
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

        const int NumLights = 4;

        private Texture _plotTexture;
        private Texture _firstNpcTexture;
        private Texture _secondNpcTexture;
        private MeshObject _player;
        private MeshObject _firstNpcObject;
        private MeshObject _plot;

        private Inventory<MainInventoryItem> _mainInventory;

        private Inventory<CollectibleItem> _collectiblesInventory;

        private Camera _camera;

        private DirectX3DGraphics _directX3DGraphics;
        private Renderer _renderer;

        private bool _isOpenCollectibleItems;

        private Sprite _playerIcon;

        private DialogueVisual _mainDialogue;

        private Vector2 _dialogueSpritePosition;

        private SolidColorBrush _fullWhiteBrush;
        private SolidColorBrush _blackBrush;

        private BoundingBox _playerCollider;

        private BoundingBox _firstNpcCollider;

        private BoundingBox _treasureCollider;

        private string _currentSpeakerName;

        private Sprite _currentSpeakerIcon;

        private MaterialProperties _defaultMaterial;

        private MaterialProperties _floorMaterial;

        private LightProperties _light;

        private bool _isMap;

        private Action OnPlayerFindingTreasure;

        private Action OnSecondNpcTaskSolved;

        private Action OnThirdTaskFind;

        private SequentialQuest _secondNpcSequentialQuest;

        private string _textLayoutText = "";

        private Animation _playerMoveAnimation;

        Vector4[] _lightColors = new Vector4[]
        {
            new Vector4(0f, 1f, 1f, 1f),
            new Vector4(0f, 1f, 0f, 1f),
            new Vector4(1f, 0f, 0f, 1f),
            new Vector4(0.5f, 0f, 0f, 1f)
        };

        int[] _lighTypes = new int[]
        {
            1,
            0,
            0,
            1,
        };

        int[] _lightEnabled = new int[]
        {
            1,
            1,
            1,
            1
        };

        private Dictionary<BoundingBox, Npc> _npcColliders = new Dictionary<BoundingBox, Npc>();

        private TimeHelper _timeHelper;

        private DXInput _dxInput;

        private Vector3 _npcSpeed = Vector3.Zero;

        private Texture _stoneTexture;

        private bool _firstRun = true;
        private bool _isDisplayingText;

        private TextLayout _textLayout;
        private TextFormat _testTextFormat;
        private TextFormat _collectibleTextFormat;
        private MainInventoryItem _treasureItem;
        private MeshObject _secondNpcObject;
        private XAudio2 _xaudio2;
        private static SourceVoice _sourceVoice;
        private static AudioBuffer _audioBuffer;
        private float _playerSpeed = 1f;
        private Texture _islandTexture;
        private MeshObject _island;
        private Texture _playerTexture;
        private Animation _playerIdleAnimation;

        private bool _isOpeningImage;

        private BoundingSphere _islandBorder;

        private List<MeshObject> _palms = new List<MeshObject>();

        private List<BoundingBox> _palmsColliders = new List<BoundingBox>();
        
        private Texture _palmTexture;
        private MeshObject _thirdNpcObject;
        private Texture _thirdNpcTexture;
        private BoundingBox _secondTreasureCollider;
        private MainInventoryItem _secondTreasureItem;
        private MainInventoryItem _lastHintItem;
        private MainInventoryItem _firstQuestHintItem;
        private Sprite _currentOpeningImage;
        private CollectibleItem _bottleItem;
        private BoundingBox _bottleCollider;
        private MainInventoryItem _vaseItem;

        public Game()
        {
            CreatingInfrastructureObjects();

            Loader loader = new Loader(_directX3DGraphics);

            CreateObjectsAndColliders(loader);

            _playerIcon = new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "playerIcon.png"),
                _dialogueSpritePosition,
                0f, new Vector2(800, 600), 0.75f);
            
            _currentSpeakerName = "Ванёк";

            _currentSpeakerIcon = _playerIcon;


            _camera = new Camera(new Vector4(-10.0f, 8.25f, -10.0f, 1.0f));
            _timeHelper = new TimeHelper();

            _fullWhiteBrush = new SolidColorBrush(_directX3DGraphics.D2DRenderTarget, Color.WhiteSmoke);

            _camera.PitchBy(MathUtil.Pi / 6f);
            _camera.YawBy(MathUtil.Pi / 4f);

            loader.Dispose();
            loader = null;

            CreateInventories();

            _xaudio2 = new XAudio2();

            var masteringVoice = new MasteringVoice(_xaudio2);

            masteringVoice.SetVolume(0.05f);

            PLaySoundFile(_xaudio2, "newBackgroundMusic.wav", true);

            _renderForm.WindowState = FormWindowState.Maximized;
        }

        private void CreatingInfrastructureObjects()
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

            _dialogueSpritePosition = new Vector2(100f, 50f);
            _collectibleTextFormat = new TextFormat(_directX3DGraphics.FactoryDWrite, "Calibri", 50)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center,
            };

            _light.Lights = new Light[NumLights];
            for (int i = 0; i < NumLights; i++)
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

            _light.GlobalAmbient = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            
            _dxInput = new DXInput(_renderForm.Handle);
        }

        private void CreateObjectsAndColliders(Loader loader)
        {
            _stoneTexture = loader.LoadTextureFromFile("stoneTexture.jpg", _renderer.AnisotropicSampler);

            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();

            var fileStream = new FileStream("Boss.obj", FileMode.Open);
            var result = objLoader.Load(fileStream);

            fileStream.Close();

            _player = loader.LoadMeshObjectFromObjFile(result, new Vector4(ToDecart(new Vector3(0f, 0.0f, 0f)), 1f), 0f,
                0f, 0.0f, ref _playerTexture, _renderer.AnisotropicSampler);

            _playerMoveAnimation = new Animation();
            _playerMoveAnimation.Load("Animations/Walk/", loader, _renderer.AnisotropicSampler, 0.12f);

            _playerIdleAnimation = new Animation();
            _playerIdleAnimation.Load("Animations/Idle/", loader, _renderer.AnisotropicSampler, 0.12f);


            _playerCollider = new BoundingBox(
                new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y),
                    result.Vertices.Min(v => v.Z)) * 0.12f + (Vector3)_player.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                    result.Vertices.Max(v => v.Z)) * 0.12f + (Vector3)_player.Position);

            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("palm.obj", FileMode.Open);
            result = objLoader.Load(fileStream);
            fileStream.Close();

            int numberOfPalms = 30;

            for (int i = -3; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    float mult = 1f;
                    if (j == 0 && i == 0)
                    {
                        mult = 1.5f;
                    }

                    _palms.Add(loader.LoadMeshObjectFromObjFile(result,
                        new Vector4(ToDecart(new Vector3((float)i - 0.5f, j, 0f)), 1f), 0f,
                        0f, 0.0f, ref _palmTexture, _renderer.AnisotropicSampler, 0.012f * mult));
                    _palmsColliders.Add(new BoundingBox(new Vector3(result.Vertices.Min(v => v.X),
                            result.Vertices.Min(v => v.Y),
                            result.Vertices.Min(v => v.Z)) * 0.003f * mult + (Vector3)_palms[_palms.Count - 1].Position,
                        new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                            result.Vertices.Max(v => v.Z)) * 0.003f * mult +
                        (Vector3)_palms[_palms.Count - 1].Position));
                }
            }

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
            _islandBorder = new BoundingSphere((Vector3)_island.Position, 3.4f);

            CreateNpc(loader);
        }

        private void CreateNpc(Loader loader)
        {
            ObjLoaderFactory objLoaderFactory;
            IObjLoader objLoader;
            FileStream fileStream;
            LoadResult result;
            var firstNpcIcon = new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "firstNpcIcon.png"),
                _dialogueSpritePosition,
                0f, new Vector2(800, 600), 0.75f);

            var secondNpcIcon = new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "secondNpcIcon.png"),
                _dialogueSpritePosition,
                0f, new Vector2(800, 600), 0.75f);

            var thirdNpcIcon = new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "thirdNpcIcon.png"),
                _dialogueSpritePosition,
                0f, new Vector2(800, 600), 0.75f);

            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("secondNpcObject0001.obj", FileMode.Open);
            result = objLoader.Load(fileStream);

            fileStream.Close();

            _firstNpcObject = loader.LoadMeshObjectFromObjFile(result,
                new Vector4(ToDecart(new Vector3(-2.0f, -1.5f, 0f)), 1f), 0f,
                0f, 0f, ref _firstNpcTexture, _renderer.AnisotropicSampler, 0.14f);

            _firstNpcObject.YawBy(MathUtil.Pi / 2f);

            _firstNpcCollider = new BoundingBox(
                new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y),
                    result.Vertices.Min(v => v.Z)) * 0.14f + (Vector3)_firstNpcObject.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                    result.Vertices.Max(v => v.Z)) * 0.14f + (Vector3)_firstNpcObject.Position);

            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("newSecondNpcObject0001.obj", FileMode.Open);
            result = objLoader.Load(fileStream);

            fileStream.Close();

            _secondNpcObject = loader.LoadMeshObjectFromObjFile(result,
                new Vector4(ToDecart(new Vector3(1.0f, -1.5f, 0f)), 1f), 0f,
                0f, 0f, ref _secondNpcTexture, _renderer.AnisotropicSampler, 0.14f);

            var secondNpcCollider = new BoundingBox(
                new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y),
                    result.Vertices.Min(v => v.Z)) * 0.14f + (Vector3)_secondNpcObject.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                    result.Vertices.Max(v => v.Z)) * 0.14f + (Vector3)_secondNpcObject.Position);

            var treasureCenter = ToDecart(new Vector3(-2.5f, 1.0f, 0f));


            objLoaderFactory = new ObjLoaderFactory();
            objLoader = objLoaderFactory.Create();

            fileStream = new FileStream("thirdNpcObject0001.obj", FileMode.Open);
            result = objLoader.Load(fileStream);

            fileStream.Close();

            _thirdNpcObject = loader.LoadMeshObjectFromObjFile(result,
                new Vector4(ToDecart(new Vector3(0f, 1.5f, 0f)), 1f), 0f,
                0f, 0f, ref _thirdNpcTexture, _renderer.AnisotropicSampler, 0.14f);

            _thirdNpcObject.YawBy((5f * MathUtil.Pi) / 4f);

            var thirdNpcCollider = new BoundingBox(
                new Vector3(result.Vertices.Min(v => v.X), result.Vertices.Min(v => v.Y),
                    result.Vertices.Min(v => v.Z)) * 0.14f + (Vector3)_thirdNpcObject.Position,
                new Vector3(result.Vertices.Max(v => v.X), result.Vertices.Max(v => v.Y),
                    result.Vertices.Max(v => v.Z)) * 0.14f + (Vector3)_thirdNpcObject.Position);

            var treasureSize = new Vector3(0.5f, 2f, 0.5f);

            _treasureCollider = new BoundingBox(treasureCenter - treasureSize, treasureCenter + treasureSize);


            var secondTreasureCenter = ToDecart(new Vector3(-3f, 2f, 0f));

            _secondTreasureCollider =
                new BoundingBox(secondTreasureCenter - treasureSize, secondTreasureCenter + treasureSize);

            var bottleColliderCenter = ToDecart(new Vector3(3.5f, 0.5f, 0f));

            _bottleCollider = new BoundingBox(bottleColliderCenter - treasureSize, bottleColliderCenter + treasureSize);
            MainInventoryItem shovelItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "shovel.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.1f), ShovelInteraction);

            _treasureItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "treasure.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.1f), inventoryItem => true);

            _secondTreasureItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "treasure.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.1f), inventoryItem => true);

            _firstQuestHintItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "firstHint.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.03f), ShowImage);

            _lastHintItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "firstHint.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.03f), ShowImage);

            CollectibleItem trophyItem = new CollectibleItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "trophy.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.08f), "Ну, трофей за пятое место за соревнования по мини-футболу в г. Ельск");

            _bottleItem = new CollectibleItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "bottle.png"), Vector2.Zero, 0f,
                new Vector2(800, 600), 0.05f), "Какая-то бутылка с посланием, но её не открыть.");

            var firstNpc = new Npc(_firstNpcObject,
                "Здарова, я, когда был маленьким, закопал сокровища у своего дома, \r\nно забыл где, осталась только карта, помоги, пожалуйста.",
                "Ну я ж тебе уже всё сказал, давай иди ищи, чувак.",
                "Блин, мужик, спасибо большое, на вот тебе за это кубок \r\nза победу в турнире по мини-футболу среди юношей, ещё и \r\nкроссовки новые (+20% к скорости)!",
                "Мужик, ну я тебе самое ценное в своей жизни отдал уже!",
                ref OnPlayerFindingTreasure, firstNpcIcon, "Васёк", new MainInventoryItem[]
                {
                    _firstQuestHintItem,
                    shovelItem
                }, new[]
                {
                    trophyItem
                }, new[]
                {
                    _treasureItem
                }, new PlayerBoost(1.2f, 2));

            CollectibleItem bananaItem = new CollectibleItem(new Sprite(_directX3DGraphics,
                    DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "bananaPeel.png"), Vector2.Zero,
                    0f,
                    new Vector2(800, 600), 0.05f),
                "Просто кожура от банана, я бы лучше её вообще выкинул, но ты как знаешь.");
            
            _vaseItem = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "vase.png"), Vector2.Zero,
                0f,
                new Vector2(800, 600), 0.04f), VaseInteractFunction);
            
            var secondNpc = new Npc(_secondNpcObject,
                "Привет, мне тут одну загадку дали, помоги-ка, я тебе подгончик сделаю",
                "Ну, я ж тебе уже записку с загадкой дал, давай вали отсюда",
                "Красава, не ожидал. Вот тебе кожура от банана!",
                "*молчание*", ref OnSecondNpcTaskSolved, secondNpcIcon, "Виталя", null, new[]
                {
                    bananaItem
                }, new []
                {
                    _vaseItem
                });

            CollectibleItem moneyItem = new CollectibleItem(new Sprite(_directX3DGraphics,
                    DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "ancientCoin.png"), Vector2.Zero,
                    0f,
                    new Vector2(800, 600), 0.05f),
                "Какая-то древняя монета, может даже дорого стоить!");
            

            var thirdNpc = new Npc(_thirdNpcObject,
                "Здарова, тут нужно сокровища найти, я тебе взамен \r\nна него отдам старинную монету. Но сначала найди лопату, вот карта.",
                "Давай, мужик, монета не ждёт!",
                "Красава, не ожидал от тебя, держи, вот монетка!",
                "Ещё раз тебе спасибо, вот реально!", ref OnThirdTaskFind, thirdNpcIcon, "Колян", new[]
                {
                    _lastHintItem
                },
                new[]
                {
                    moneyItem
                }, new[]
                {
                    _secondTreasureItem
                });

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


            _npcColliders.Add(_firstNpcCollider, firstNpc);
            _npcColliders.Add(secondNpcCollider, secondNpc);
            _npcColliders.Add(thirdNpcCollider, thirdNpc);
        }

        private bool VaseInteractFunction(MainInventoryItem arg)
        {
            _textLayoutText = "Какая-то древняя ваза, хрупкая. Главное - не разбить!";
            _currentSpeakerIcon = _playerIcon;
            return true;
        }

        private void CreateInventories()
        {
            InventoryItem<MainInventoryItem>[] mainInventoryStartItems = new InventoryItem<MainInventoryItem>[3];


            Vector2 itemCenter = new Vector2(100, 550);
            Vector2 defaultSize = new Vector2(800, 600);
            float angle = 0f;
            float defaultBoxScale = 1f;


            MainInventoryItem item = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "key.png"), itemCenter, angle,
                defaultSize, 0.05f), (inventoryItem =>
            {
                _textLayoutText = "Ключи от дома, главное их не потерять, хотя зачем они мне здесь?";
                return true;
            }));


            mainInventoryStartItems[0] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, null, itemCenter,
                angle,
                defaultSize, defaultBoxScale);

            itemCenter = new Vector2(200, 550);

            item = new MainInventoryItem(new Sprite(_directX3DGraphics,
                DirectX3DGraphics.LoadFromFile(_directX3DGraphics.D2DRenderTarget, "loope.png"), itemCenter, angle,
                defaultSize, 0.02f), (inventoryItem => true));


            mainInventoryStartItems[1] = new InventoryItem<MainInventoryItem>(_directX3DGraphics, null, itemCenter,
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

            _collectiblesInventory.OnInventoryFull += OnInventoryFull;
        }

        private void OnInventoryFull()
        {
            _textLayoutText = "УРА! Я собрал все предметы!!!";
            _currentSpeakerIcon = _playerIcon;
            PLaySoundFile(_xaudio2, "victorySound.wav", false);
        }

        private bool ShowImage(MainInventoryItem item)
        {
            if (!_isOpeningImage)
            {
                var newItem = item.Sprite.Clone();
                newItem.DefaultScale *= 15f;
                newItem.DefaultSize = new Vector2(_renderForm.Width, _renderForm.Height);
                newItem.CenterPosition = new Vector2(_renderForm.Width / 2f, _renderForm.Height / 2f);
                _currentOpeningImage = newItem;
                _textLayoutText = null;
                _isOpeningImage = true;
            }
            else
            {
                _isOpeningImage = false;
            }

            return true;
        }


        void PLaySoundFile(XAudio2 device, string fileName, bool isLooping)
        {
            var stream = new SoundStream(File.OpenRead(fileName));
            var waveFormat = stream.Format;
            _audioBuffer = new AudioBuffer
            {
                Stream = stream.ToDataStream(),
                AudioBytes = (int)stream.Length,
                Flags = BufferFlags.EndOfStream,
            };
            if (isLooping)
            {
                _audioBuffer.LoopCount = AudioBuffer.LoopInfinite;
            }

            stream.Close();

            _sourceVoice = new SourceVoice(device, waveFormat, true);

            _sourceVoice.SubmitSourceBuffer(_audioBuffer, stream.DecodedPacketsInfo);
            _sourceVoice.Start();
        }
        

        private void OnRightPlayerSequenceSecondNpc()
        {
            OnSecondNpcTaskSolved?.Invoke();
            _mainInventory.AddItem(_vaseItem);
            _textLayoutText = "Угадал!";
        }

        private List<InteractableObject> GenerateSecondNpcSlabs(Loader loader)
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

        private bool ShovelInteraction(MainInventoryItem item)
        {
            if (_playerCollider.Intersects(_treasureCollider))
            {
                OnPlayerFindingTreasure?.Invoke();
                if (_mainInventory.ChangeItem(_treasureItem,
                        _firstQuestHintItem))
                {
                    _currentSpeakerName = "Ванёк";
                    _currentSpeakerIcon = _playerIcon;
                    _textLayoutText = "УРА! Шота нашёл!";
                    PLaySoundFile(_xaudio2, "victorySound.wav", false);
                    _treasureCollider.Minimum -= new Vector3(0, -100f, 0f);
                    _treasureCollider.Maximum -= new Vector3(0, -100f, 0f);
                    return true;
                }

                return false;
            }
            else if (_playerCollider.Intersects(_secondTreasureCollider))
            {
                if (_mainInventory.ChangeItem(_secondTreasureItem, _lastHintItem))
                {
                    OnThirdTaskFind?.Invoke();
                    _currentSpeakerName = "Ванёк";
                    _currentSpeakerIcon = _playerIcon;
                    _textLayoutText = "УРА! Шота нашёл!";
                    PLaySoundFile(_xaudio2, "victorySound.wav", false);
                    _secondTreasureCollider.Minimum -= new Vector3(0, -100f, 0f);
                    _secondTreasureCollider.Maximum -= new Vector3(0, -100f, 0f);
                    return true;
                }

                return false;
            }
            else if (_playerCollider.Intersects(_bottleCollider))
            {
                if (_collectiblesInventory.AddItem(_bottleItem))
                {
                    if (_collectiblesInventory.IsFull)
                    {
                        return true;
                    }

                    _currentSpeakerName = "Ванёк";
                    _currentSpeakerIcon = _playerIcon;
                    _textLayoutText = "УРА! Шота нашёл!";
                    PLaySoundFile(_xaudio2, "victorySound.wav", false);
                    
                    _bottleCollider.Minimum -= new Vector3(0, -100f, 0f);
                    _bottleCollider.Maximum -= new Vector3(0, -100f, 0f);
                    return true;
                }
                else
                {
                    _textLayoutText = "Что-то нашёл, но не влезает в карманы!";
                    _currentSpeakerIcon = _playerIcon;
                }

                return false;
            }
            else
            {
                _currentSpeakerName = "Ванёк";
                _currentSpeakerIcon = _playerIcon;
                _textLayoutText = "Здесь ничо нет, мужик\r\n" + _treasureCollider.Center + "\r\n" +
                                  _playerCollider.Center;
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
            if (!_secondNpcSequentialQuest.IsStarting) return;
            for (var index = 0; index < _secondNpcSequentialQuest.InteractableObjects.Count; index++)
            {
                var interactableObject = _secondNpcSequentialQuest.InteractableObjects[index];
                if (!_playerCollider.Intersects(interactableObject.MeshCollider)) continue;
                if (index == _secondNpcSequentialQuest.LastPlayerInteract)
                {
                    _textLayoutText = "Харош";
                    // может быть, просто идти дальше по циклу
                    return;
                }
                else
                {
                    if (!_secondNpcSequentialQuest.AddToPlayerSequence(index))
                    {
                        _textLayoutText = "Айа-айа, неправильно!";
                    }
                    else
                    {
                        _textLayoutText = "Харош";
                    }
                }
            }
        }


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
            }

            if (_dxInput.IsKeyPressed(Key.D))
            {
                _player.Yaw = MathUtil.Pi * 3 / 4f;
                playerMovement += new Vector3(1.0f, 0.0f, 0.0f);
            }

            if (_dxInput.IsKeyPressed(Key.Space))
            {
                _textLayoutText = null;
            }

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
            else
            {
                if (_palmsColliders.Any(palmCollider => _playerCollider.Intersects(palmCollider)))
                {
                    _playerCollider.Minimum -= playerMovement;
                    _playerCollider.Maximum -= playerMovement;
                    playerMovement = Vector3.Zero;
                }
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
            
            
            if (_dxInput.IsKeyReleased(Key.M))
            {
                _isMap = !_isMap;
            }

            if (_dxInput.IsKeyReleased(Key.I))
            {
                _isOpenCollectibleItems = !_isOpenCollectibleItems;
                if (_isOpenCollectibleItems)
                {
                    _textLayoutText = null;
                }
            }

            if (!_dxInput.IsKeyReleased(Key.Q)) return;
            var activeItem = _mainInventory.GetActiveItem();

            if (activeItem == null) return;
            if (activeItem.Item != null)
                activeItem.Item.Interact();
        }
        private void RenderObjects()
        {
            Matrix viewMatrix = _camera.GetViewMatrix();
            Matrix projectionMatrix = _camera.GetProjectionMatrix();
            _light.EyePosition = _camera.Position;
            
            _renderer.BeginRender();
            _renderer.SetLightConstantBuffer(_light);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_player.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_playerTexture);
            _renderer.RenderMeshObject(_player);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_firstNpcObject.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_firstNpcTexture);
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
            _renderer.SetTexture(_secondNpcTexture);
            _renderer.RenderMeshObject(_secondNpcObject);

            _renderer.SetPerObjectConstantBuffer(_defaultMaterial);
            _renderer.UpdatePerObjectConstantBuffers(_thirdNpcObject.GetWorldMatrix(), viewMatrix, projectionMatrix);
            _renderer.SetTexture(_thirdNpcTexture);
            _renderer.RenderMeshObject(_thirdNpcObject);

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
        }

        private void DrawHUD()
        {
            _testTextFormat = new TextFormat(_directX3DGraphics.FactoryDWrite, "Calibri", 28)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center,
            };

            _directX3DGraphics.D2DRenderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
            _directX3DGraphics.D2DRenderTarget.BeginDraw();

            if (_isOpenCollectibleItems)
            {
                _directX3DGraphics.D2DRenderTarget.FillRectangle(
                    new RawRectangleF(0f, 0f, _renderForm.Width, _renderForm.Height),
                    _blackBrush); // рамка карты
                var collectibleTextLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, "Коллекционные предметы",
                    _collectibleTextFormat, _renderForm.Width,
                    _renderForm.Height);
                _directX3DGraphics.D2DRenderTarget.DrawTextLayout(
                    new RawVector2(0f, 50 - _renderForm.Height / 2f), collectibleTextLayout, _fullWhiteBrush,
                    DrawTextOptions.None);
                _textLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, "Описание", _testTextFormat,
                    _renderForm.Width, _renderForm.Height);
                _directX3DGraphics.D2DRenderTarget.DrawTextLayout(
                    new RawVector2(0f, 300), _textLayout, _fullWhiteBrush, DrawTextOptions.None);
                var activeItem = _collectiblesInventory.GetActiveItem();
                if (activeItem != null && activeItem.Item != null)
                {
                    _textLayout = new TextLayout(_directX3DGraphics.FactoryDWrite, activeItem.Item.Description,
                        _testTextFormat,
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

            _currentSpeakerIcon.DefaultSize = new Vector2(_renderForm.Width, _renderForm.Height);
            _currentSpeakerIcon.CenterPosition =
                new Vector2(320f * _renderForm.Width / 1920f, 320f * _renderForm.Height / 1080f);

            if (!_textLayoutText.IsNullOrEmpty())
                _mainDialogue.Draw(_directX3DGraphics.D2DRenderTarget, _directX3DGraphics.FactoryDWrite,
                    _currentSpeakerIcon, _currentSpeakerName, _textLayoutText, _renderForm);

            if (_isOpeningImage)
            {
                _currentOpeningImage.Draw();
            }

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
        }

        private void InteractWithNpc(KeyValuePair<BoundingBox, Npc> npc)
        {
            _isDisplayingText = true;
            NpcResponse npcResponse = npc.Value.Interact();
            _currentSpeakerName = npc.Value.Name;
            _currentSpeakerIcon = npc.Value.IconSprite;
            _textLayoutText = npcResponse.ResponseText;

            if (npc.Value.NpcState == NpcStates.AfterQuestComplete)
            {
                var takenItems = npcResponse.Items;

                if (takenItems != null)
                {
                    foreach (var takenItem in takenItems)
                    {
                        _mainInventory.RemoveItem(takenItem);
                    }
                }

                var collectibles = npc.Value.GetCollectibles();
                if (collectibles != null)
                {
                    foreach (var collectible in collectibles)
                    {
                        _collectiblesInventory.AddItem(collectible);
                    }
                }

                var playerBoost = npcResponse.PlayerBoost;

                if (playerBoost != null)
                {
                    UsePlayerBoost(playerBoost);
                }

                _npcSpeed = ToDecart(new Vector3(0f, 0.0001f, 0f));

                npc.Value.TakePrizes();
            }
            else
            {
                var items = npcResponse.Items;

                if (items == null) return;
                foreach (var item in items)
                {
                    if (!_mainInventory.AddItem(item))
                    {
                        _textLayoutText = "Не влезает в карманы! Приходи позже!";
                        npc.Value.NpcState = NpcStates.BeforeGivingQuest;
                        break;
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
                foreach (var npc in _npcColliders)
                {
                    if (npc.Key.Intersects(_playerCollider))
                    {
                        InteractWithNpc(npc);
                    }
                }
            }

            _npcColliders[_firstNpcCollider].GameObject.MoveBy(_npcSpeed.X, _npcSpeed.Y, _npcSpeed.Z);
        }

        private void RenderLoopCallBack()
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

        private static Vector3 ToDecart(Vector3 isometric)
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
            _plot.Dispose();
            _renderer.Dispose();
            _directX3DGraphics.Dispose();
        }
    }
}
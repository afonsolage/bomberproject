using System;
using CommonLib.GridEngine;
using CommonLib.Util.Math;
using UnityEngine;
using CommonLib.Util;
using System.Collections;
using CommonLib.Messaging;

namespace Assets.Scripts.Engine.Logic.Object
{
    public class PlayerRoom : GridObject
    {
        private readonly ObjectManager _objectManager;
        public ObjectManager ObjectManager
        {
            get
            {
                return _objectManager;
            }
        }

        private GameObject _gameObject;
        public GameObject GameObject
        {
            get
            {
                return _gameObject;
            }
        }

        private bool _isMainPlayer;
        public bool IsMainPlayer
        {
            get
            {
                return _isMainPlayer;
            }
        }

        private GridObjectInstance _instance;
        protected GridObjectInstance Instance
        {
            get
            {
                if (_instance == null)
                    _instance = _gameObject.GetComponent<GridObjectInstance>();

                return _instance;
            }
        }

        private PlayerController _controller;

        private GridRemotePosSync _remoteSyncPos;

        protected PlayerAttributes _attr;
        public PlayerAttributes Attr
        {
            get
            {
                return _attr;
            }
        }

        public IEnumerator FlashingFx { get; set; }

        public PlayerRoom(uint uid, ObjectManager objectManager, PlayerAttributes attributes) : base(uid, (int)ObjectType.PLAYER, true, objectManager.Stage.GridEngine.Map)
        {
            _objectManager = objectManager;
            _attr = attributes;
        }

        internal void Instanciate(bool isMainPlayer, string nick, PlayerGender gender)
        {
            _isMainPlayer = isMainPlayer;

            //TODO: Instanciate others models based on gender and on future infos.
            var name = (isMainPlayer) ? "Main Player" : "Player " + _uid;

            if (isMainPlayer)
            {
                _gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
            }
            else
            {
                var prefab = "Prefabs/OtherPlayer";

                _gameObject = UnityEngine.Object.Instantiate(Resources.Load(prefab)) as GameObject;
            }

            _gameObject.name = name;
            _gameObject.transform.parent = _objectManager.gameObject.transform;
            _controller = _gameObject.GetComponent<PlayerController>();

            //TODO: Find a better way of set nick and others info (like guild and title, maybe?)
            SetPlayerName(nick);

            Instance.Setup(this);

            _remoteSyncPos = _gameObject.GetComponent<GridRemotePosSync>();

            UpdateAttributes(_attr);
        }

        private void SetPlayerName(string nick)
        {
            var uiManager = _objectManager.Stage.UIManager;

            var hud = uiManager.FindInstance(WindowType.HUD_ROOT).gameObject;
            var hudText = NGUITools.AddChild(hud, uiManager.GetWindowGameObject(WindowType.HUD_TEXT));

            hudText.name = "HUD - " + nick;

            var follow = hudText.GetComponent<UIFollowTarget>();
            follow.target = _gameObject.transform.Find("PivotName");

            var text = hudText.GetComponent<HUDText>();
            text.Add(nick, Color.white, 0f);
        }

        internal void Destroy()
        {
            UnityEngine.Object.Destroy(_gameObject);
        }

        internal void AddMoveDest(Vec2f worldPos)
        {
            if (_remoteSyncPos == null)
            {
                CLog.F("Trying to sync position of non remote object.");
                return;
            }

            _remoteSyncPos.AddMoveDest(worldPos);
        }

        internal void OnHit(uint hitter)
        {
            //Do something...
        }

        internal void UpdateAttributes(PlayerAttributes newAttributes = null)
        {
            if (newAttributes != null)
                _attr = newAttributes;

            _remoteSyncPos.moveSpeed = _attr.moveSpeed;

            if (IsMainPlayer)
            {
                _controller.moveSpeed = _attr.moveSpeed;
            }
        }

        internal void Died(uint killer)
        {
            _gameObject.SetActive(false);
        }

        internal void ForceMoveTo(Vec2f newWorldPos)
        {
            var force = newWorldPos - WorldPos;
            ForceMove(force.x, force.y);
        }
    }
}
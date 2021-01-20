
using CommonLib.GridEngine;
using CommonLib.Util.Math;
using UnityEngine;
using CommonLib.Util;
using System.Collections.Generic;
using CommonLib.Messaging.Client;

namespace Assets.Scripts.Engine.Logic.Object
{
    public class Bomb : GridObject
    {
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

        private readonly ObjectManager _objectManager;
        private GridRemotePosSync _remoteSyncPos;

        private GameObject _gameObject;
        public GameObject GameObject
        {
            get
            {
                return _gameObject;
            }
        }

        //private List<GameObject> _smokes;

        private bool _exploded;

        public Bomb(ObjectManager objManager, uint uid, GridMap map, Vec2 gridPos, uint moveSpeed) : base(uid, ObjectType.BOMB, false, map)
        {
            _objectManager = objManager;
            var app = objManager.Stage;

            var worldPos = map.GridToWorld(gridPos);
            Wrap(worldPos);

            _gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Bomb"), new Vector3(worldPos.x, 0 /*worldPos.y*/, worldPos.y), Quaternion.identity) as GameObject;
            _gameObject.name = "Bomb " + UID;
            _gameObject.transform.parent = _objectManager.gameObject.transform;

            Instance.Setup(this);

            _remoteSyncPos = _gameObject.GetComponent<GridRemotePosSync>();
            _remoteSyncPos.moveSpeed = moveSpeed;

            _exploded = false;
            //_smokes = new List<GameObject>();
        }

        internal void Explode(List<Vec2> explosionArea)
        {
            _exploded = true;

            // Test
            //var camera = Camera.main.gameObject.GetComponent<CameraManager>();
            //camera?.BeginShake(.2f, .3f);

            // Play animation.
            //_animator?.Play("Boom", false);

            // Play sound effect.
            AudioClip clip = Resources.Load("Sound/Explode") as AudioClip;
            SoundManager.PlaySound(clip);

            foreach (var gridPos in explosionArea)
            {
                var worldPos = Map.GridToWorld(gridPos);

                var go = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Explosion"), new Vector3(worldPos.x, 0/*worldPos.y*/, worldPos.y), Quaternion.identity) as GameObject;
                go.transform.parent = _objectManager.gameObject.transform;

                GameObject.Destroy(go, 0.5f);

                //_smokes.Add(go);
            }
        }

        public void Kick(uint playerUID, GridDir direction)
        {
            _objectManager.Stage.ServerConnection.Send(new CR_BOMB_KICK_REQ()
            {
                uid = playerUID,
                uidBomb = this.UID,
                dir = (byte)direction
            });
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (!_exploded /*|| (_animator != null && _animator.playing)*/)
                return;

            LeaveMap();

            //foreach (var go in _smokes)
            //{
            //    UnityEngine.Object.Destroy(go);
            //}

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
    }
}
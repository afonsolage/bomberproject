using CommonLib.GridEngine;
using CommonLib.Util.Math;
using UnityEngine;

namespace Assets.Scripts.Engine.Logic.Object
{
    class PowerUp : GridObject
    {
        private GridObjectInstance _instance;
        private GameObject _gameObject;

        public GameObject GameObject
        {
            get
            {
                return _gameObject;
            }
        }

        public PowerUp(uint uid, uint icon, Vec2 pos, GridMap map) : base(uid, ObjectType.POWERUP, false, map)
        {
            var prefab = Resources.Load(string.Format("Prefabs/PowerUp/{0}", icon));
            _gameObject = UnityEngine.Object.Instantiate(prefab) as GameObject;
            _gameObject.name = string.Format("PowerUp {0}", uid);

            _instance = _gameObject.GetComponent<GridObjectInstance>();
            _instance.Setup(this);

            Wrap(map.GridToWorld(pos));
            EnterMap();
        }

        public void Destroy(bool collected)
        {
            LeaveMap();

            if (collected)
            {
                UnityEngine.Object.Destroy(_gameObject);
            }
            else
            {
                _instance.StartCoroutine(SpriteFx.FlashSprites(_gameObject, 5, 0.1f, false, true));
            }
        }
    }
}

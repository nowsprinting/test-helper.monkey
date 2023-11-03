// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Text;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Component to make operation positions visible
    /// </summary>
    public class InteractiveComponentHint : MonoBehaviour
    {
        /// <summary>
        /// Color for interactive components that users can operate
        /// </summary>
        [SerializeField]
        public Color reachableColor = Color.yellow;

        /// <summary>
        /// Color for interactive components that users cannot operate
        /// </summary>
        [SerializeField]
        public Color unreachableColor = new Color(0xef, 0x81, 0x0f);

        /// <summary>
        /// Color for points that is the origin of position annotations
        /// </summary>
        [SerializeField]
        public Color originalPointColor = Color.gray;

        /// <summary>
        /// Number of frames per refresh hints
        /// </summary>
        [SerializeField]
        public int framesPerRefresh = 100;


        /// <summary>
        /// Screen point strategy. You should use a same strategy that monkey operators use.
        /// </summary>
        /// <param name="go">GameObject where monkey operators operate on</param>
        /// <returns></returns>
        public virtual Vector2 GetScreenPoint(GameObject go) => DefaultScreenPointStrategy.GetScreenPoint(go);


        /// <summary>
        /// Operation point and the hint texts for interactive components that users cannot operate
        /// </summary>
        public IReadOnlyDictionary<(Vector3, Vector3), string> NotReallyInteractives => _notReallyInteractives;

        /// <summary>
        /// Operation point and the hint texts for interactive components that users can operate
        /// </summary>
        public IReadOnlyDictionary<(Vector3, Vector3), string> ReallyInteractives => _reallyInteractives;

        /// <summary>
        /// Relation between an origin point for annotations and the operation point.
        /// </summary>
        public IReadOnlyList<(Vector3, Vector3, Vector3)> OriginalRelation => _originalRel;

        private int _count;

        private readonly StringBuilder _sb = new StringBuilder();

        private readonly Dictionary<(Vector3, Vector3), string> _notReallyInteractives =
            new Dictionary<(Vector3, Vector3), string>();

        private readonly Dictionary<(Vector3, Vector3), string> _reallyInteractives =
            new Dictionary<(Vector3, Vector3), string>();

        /// <summary>
        /// Triples of an original world point and a camera normal vector and the modified world point
        /// </summary>
        private readonly List<(Vector3, Vector3, Vector3)> _originalRel = new List<(Vector3, Vector3, Vector3)>();

        private readonly Dictionary<(Vector3, Vector3), HashSet<GameObject>> _tmpNotReallyInteractives =
            new Dictionary<(Vector3, Vector3), HashSet<GameObject>>();

        private readonly Dictionary<(Vector3, Vector3), HashSet<GameObject>> _tmpReallyInteractives =
            new Dictionary<(Vector3, Vector3), HashSet<GameObject>>();


        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            if (Time.frameCount % framesPerRefresh == 0)
            {
                Refresh();
            }
        }


        private void Refresh()
        {
            Clear();

            foreach (var component in InteractiveComponentCollector.FindInteractiveComponents())
            {
                var dst = component.IsReallyInteractiveFromUser(GetScreenPoint)
                    ? _tmpReallyInteractives
                    : _tmpNotReallyInteractives;

                var screenPoint = GetScreenPoint(component.gameObject);
                var originalPos = component.transform.position;
                var modifiedPos = InteractiveComponentHintPosition.GetWorldPoint(
                    CameraSelector.SelectBy(component.gameObject),
                    screenPoint,
                    originalPos
                );

                var cameraNorm = CameraSelector.SelectBy(component.gameObject)?.transform.forward * -1 ?? Vector3.back;

                if (!dst.TryGetValue((modifiedPos, cameraNorm), out var gameObjects))
                {
                    gameObjects = dst[(modifiedPos, cameraNorm)] = new HashSet<GameObject>();
                }

                gameObjects.Add(component.gameObject);

                _originalRel.Add((originalPos, cameraNorm, modifiedPos));
            }

            RefreshLabels();
        }


        private void Clear()
        {
            _reallyInteractives.Clear();
            _notReallyInteractives.Clear();
            _originalRel.Clear();

            foreach (var worldPointAndGameObjects in _tmpReallyInteractives)
            {
                worldPointAndGameObjects.Value.Clear();
            }

            foreach (var worldPointAndGameObjects in _tmpNotReallyInteractives)
            {
                worldPointAndGameObjects.Value.Clear();
            }
        }


        private void RefreshLabels()
        {
            foreach (var worldPointAndGameObjects in _tmpReallyInteractives)
            {
                var worldPoint = worldPointAndGameObjects.Key;
                var gameObjects = worldPointAndGameObjects.Value;
                if (gameObjects.Count == 0)
                {
                    continue;
                }

                _sb.Clear();
                foreach (var go in gameObjects)
                {
                    _sb.AppendLine(go.name);
                }

                _reallyInteractives[worldPoint] = _sb.ToString();
            }

            foreach (var worldPointAndGameObjects in _tmpNotReallyInteractives)
            {
                var worldPoint = worldPointAndGameObjects.Key;
                var gameObjects = worldPointAndGameObjects.Value;
                if (gameObjects.Count == 0)
                {
                    continue;
                }

                _sb.Clear();
                foreach (var go in gameObjects)
                {
                    _sb.AppendLine(go.name);
                }

                _notReallyInteractives[worldPoint] = _sb.ToString();
            }
        }
    }
}

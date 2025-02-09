using System;
using UnityEditor;

namespace RuntimeSounds.Editor.Utils
{
    public class Debouncer
    {
        private float _delay;

        private Action _action;

        private double _lastTimeTriggered;

        public Debouncer(float delay)
        {
            _delay = delay;
        }

        public void Execute(Action action)
        {
            _action = action;
            _lastTimeTriggered = EditorApplication.timeSinceStartup;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private void Update()
        {
            if (EditorApplication.timeSinceStartup - _lastTimeTriggered >= _delay)
            {
                EditorApplication.update -= Update;
                _action?.Invoke();
            }
        }
    }
}
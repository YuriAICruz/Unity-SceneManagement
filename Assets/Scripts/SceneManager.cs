using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScript.Steps;
using Object = UnityEngine.Object;

namespace SceneManagement
{
    public static class SceneManager
    {
        private static List<GameObject> _managers = new List<GameObject>();
        private static List<GameObject> _exceptions = new List<GameObject>();
        private static MonoBehaviour _mono;

        public static event Action OnStartLoadingScene, OnEndLoadingScene;
        public static event Action<float> OnLoading;

        public static void EnableProgressReporting(MonoBehaviour mono)
        {
            _mono = mono;
        }

        public static void AssignManager(GameObject man)
        {
            if (_managers.Contains(man)) return;

            _managers.Add(man);
        }

        public static void ClearManagers()
        {
            DeleteObjects(_managers);
        }

        public static void AssigException(GameObject gameObject)
        {
            if (_exceptions.Contains(gameObject)) return;

            _exceptions.Add(gameObject);
        }

        public static void ClearExceptions()
        {
            DeleteObjects(_exceptions);
        }

        private static void DeleteObjects(List<GameObject> objtsToDelete)
        {
            foreach (var gameObject in objtsToDelete)
            {
                if (gameObject != null)
                    Object.Destroy(gameObject);
            }
        }

        public static void LoadScene(int index, Action onComplete = null, Action<float> progress = null)
        {
            var objtsToDelete = GetObjtsToDelete();
            
            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);

            DelegateData(onComplete, progress, operation, objtsToDelete);
        }

        public static void LoadScene(string name, Action onComplete = null, Action<float> progress = null)
        {
            var objtsToDelete = GetObjtsToDelete();

            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

            DelegateData(onComplete, progress, operation, objtsToDelete);
        }

        private static void DelegateData(Action onComplete, Action<float> progress, AsyncOperation operation, List<GameObject> objtsToDelete)
        {
            if (_mono != null)
                _mono.StartCoroutine(ProgressReporting(operation, progress));

            operation.completed += (a) =>
            {
                if (onComplete != null) onComplete();
                if (OnEndLoadingScene != null) OnEndLoadingScene();
            };

            DeleteObjects(objtsToDelete);
        }

        private static List<GameObject> GetObjtsToDelete()
        {
            if (OnStartLoadingScene != null) OnStartLoadingScene();
            var objtsToDelete = Object.FindObjectsOfType<GameObject>().ToList();

            objtsToDelete = RemoveFromList(objtsToDelete, _managers);
            objtsToDelete = RemoveFromList(objtsToDelete, _exceptions);
            return objtsToDelete;
        }

        private static List<GameObject> RemoveFromList(List<GameObject> gameObjects, List<GameObject> gameObjectsToRemove)
        {
            var result = new List<GameObject>(gameObjects);
            foreach (var gameObject in gameObjectsToRemove)
            {
                var go = result.Find(gameObjectsToRemove.Contains);
                if (go!=null)
                {
                    var childs = go.transform.GetComponentsInChildren<Transform>().Select(x=>x.gameObject).ToList();
                    childs.Add(go);
                    result.RemoveAll(x => childs.Contains(x));
                }
            }

            return result;
        }
        

        static IEnumerator ProgressReporting(AsyncOperation operation, Action<float> progress)
        {
            while (operation.progress < 1)
            {
                if (progress != null) progress(operation.progress);
                if (OnLoading != null) OnLoading(operation.progress);

                yield return new WaitForChangedResult();
            }
            if (progress != null) progress(1);
            if (OnLoading != null) OnLoading(1);
        }
    }
}
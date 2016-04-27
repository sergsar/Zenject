#if !ZEN_NOT_UNITY3D

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using ModestTree;
using ModestTree.Util;

#if !ZEN_NOT_UNITY3D
using UnityEngine.SceneManagement;
using UnityEngine;
#endif

namespace Zenject
{
    public enum LoadSceneContainerMode
    {
        // This will use the ProjectCompositionRoot container as parent for the new scene
        // This is similar to just running the new scene normally
        None,
        // This will use current scene as parent for the new scene
        // This will allow the new scene to refer to dependencies in the current scene
        Child,
        // This will use the parent of the current scene as the parent for the next scene
        // In most cases this will be the same as None
        Sibling,
    }

    public class ZenjectSceneLoader
    {
        readonly DiContainer _sceneContainer;

        public ZenjectSceneLoader(SceneCompositionRoot sceneRoot)
        {
            _sceneContainer = sceneRoot.Container;
        }

        public void LoadScene(string sceneName)
        {
            LoadScene(sceneName, null);
        }

        public void LoadScene(string sceneName, Action<DiContainer> preBindings)
        {
            LoadScene(sceneName, preBindings, null);
        }

        public void LoadScene(string sceneName, Action<DiContainer> preBindings, Action<DiContainer> postBindings)
        {
            LoadSceneInternal(sceneName, LoadSceneMode.Single, LoadSceneContainerMode.None, preBindings, postBindings);
        }

        public void LoadSceneAdditive(
            string sceneName)
        {
            LoadSceneAdditive(sceneName, LoadSceneContainerMode.None);
        }

        public void LoadSceneAdditive(
            string sceneName, LoadSceneContainerMode containerMode)
        {
            LoadSceneAdditive(sceneName, containerMode, null);
        }

        public void LoadSceneAdditive(
            string sceneName, LoadSceneContainerMode containerMode, Action<DiContainer> preBindings)
        {
            LoadSceneAdditive(sceneName, containerMode, preBindings, null);
        }

        public void LoadSceneAdditive(
            string sceneName, LoadSceneContainerMode containerMode, Action<DiContainer> preBindings, Action<DiContainer> postBindings)
        {
            LoadSceneInternal(sceneName, LoadSceneMode.Additive, containerMode, preBindings, postBindings);
        }

        void LoadSceneInternal(
            string sceneName,
            LoadSceneMode loadMode,
            LoadSceneContainerMode containerMode,
            Action<DiContainer> preBindings,
            Action<DiContainer> postBindings)
        {
            if (loadMode == LoadSceneMode.Single)
            {
                Assert.IsEqual(containerMode, LoadSceneContainerMode.None);
            }

            if (containerMode == LoadSceneContainerMode.None)
            {
                SceneCompositionRoot.ParentContainer = null;
            }
            else if (containerMode == LoadSceneContainerMode.Child)
            {
                SceneCompositionRoot.ParentContainer = _sceneContainer;
            }
            else
            {
                Assert.IsEqual(containerMode, LoadSceneContainerMode.Sibling);
                SceneCompositionRoot.ParentContainer = _sceneContainer.ParentContainer;
            }

            SceneCompositionRoot.BeforeInstallHooks = preBindings;
            SceneCompositionRoot.AfterInstallHooks = postBindings;

            var scene = SceneManager.GetSceneByName(sceneName);

            Assert.That(Application.CanStreamedLevelBeLoaded(sceneName),
                "Unable to load scene '{0}'", sceneName);

            SceneManager.LoadScene(sceneName, loadMode);

            // It would be nice here to actually verify that the new scene has a SceneCompositionRoot
            // if we have extra binding hooks, or LoadSceneContainerMode != None, but
            // it doesn't seem like we can do that immediately after calling SceneManager.LoadScene
        }
    }
}

#endif
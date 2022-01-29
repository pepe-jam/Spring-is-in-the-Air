using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Helper.StringComparer;
namespace Helper
{
    /*
     * Enum of all Scene Names in the project.
     * Please update upon adding or renaming scenes in the Editor to ensure that scene loading actually works.
     */
    public enum SceneNames
    {
        Introduction,
        MainMenu,
        SewerLevel,
        WarehouseLevel
    }
    public class SceneLoader : MonoBehaviour
    {
        //private IMixedRealitySceneSystem sceneSystem;
        private String[] scenesInBuild;
        
        // Start is called before the first frame update
        void Awake()
        {
            //sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
            // making a list of all scenes in the build for later use
            scenesInBuild = new string[SceneManager.sceneCountInBuildSettings];
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                scenesInBuild[i] = System.IO.Path.GetFileNameWithoutExtension( UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex( i ));   
            }
        }

       public void LoadScene(string sceneToLoad)
       {
           // SceneManager.LoadScene(sceneToLoad);
           ///* All of the shitty code below is useless thanks to the weird way Unity's SceneManager works
     
            // SceneManager's loadScene() method does not throw an Exception if the sceneToLoad is invalid and instead just returns,
            // so we must force name validity ourselves.
            // We do this by fuzzy searching through the ScenesInBuild and loading the one with the best fitting name.
            String bestMatch = scenesInBuild[0];
            int lowestDistance = int.MaxValue;
            foreach (string sceneNameInBuild in scenesInBuild)
            {
                // compare each sceneNameInBuild to sceneToLoad using Levensthein Distance, pick the one with the lowest distance 
                int differenceScore = int.MaxValue;
                try
                {
                     differenceScore = LevenshteinDistance(sceneToLoad, sceneNameInBuild);
                }
                catch (NullReferenceException e)
                {
                    // in Edit mode 
                }
                
                if (differenceScore < lowestDistance)
                {
                    bestMatch = sceneNameInBuild;
                    lowestDistance = differenceScore;
                }
            } 
            Debug.Log("loading scene named "+bestMatch+", whose name is closest to the requested "+sceneToLoad);
            //sceneSystem.LoadContent(bestMatch, LoadSceneMode.Single);
             SceneManager.LoadScene(bestMatch); // incompatible with MRTK?
            // That's quite a hack but still better than the application breaking whenever someone renames a scene in the Editor
       }

        public void LoadScene(SceneNames sceneName)
        {
            SceneManager.LoadScene(sceneName.ToString());
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Bewildered
{
    // https://github.com/Unity-Technologies/guid-based-reference/blob/master/Assets/CrossSceneReference/Runtime/GuidComponent.cs
    public class UniqueObjectId : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private UniqueId _id;

        public UniqueId Id
        {
            get { return _id; }
        }

        private void CreateId()
        {

        }

#if UNITY_EDITOR
        private bool IsAssetOnDisk()
        {
            return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
        }

        private bool IsEditingInPrefabMode()
        {
            if (EditorUtility.IsPersistent(this))
            {
                // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
                return true;
            }
            else
            {
                // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
                StageHandle mainStage = StageUtility.GetMainStageHandle();
                StageHandle currentStage = StageUtility.GetStageHandle(gameObject);
                if (currentStage != mainStage)
                {
                    PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                    if (prefabStage != null)
                        return true;
                }
            }

            return false;
        }
#endif

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // This lets us detect if we are a prefab instance or a prefab asset.
            // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
            if (IsAssetOnDisk())
                _id = UniqueId.Empty;
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {

        }
    } 
}

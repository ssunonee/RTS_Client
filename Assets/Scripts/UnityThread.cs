using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityThread : MonoBehaviour
{
    public static UnityThread instance = null;

    //Holds actions received from another Thread. Will be coped to actionCopiedQueueFixedUpdateFunc then executed from there
    private static List<Action> actionQueuesFixedUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesFixedUpdateFunc to be executed
    List<Action> actionCopiedQueueFixedUpdateFunc = new List<Action>();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteFixedUpdateFunc = true;


    // Used to initialize UnityThread. Call once before any function here
    public static void initUnityThread(bool visible = false)
    {
        if (instance != null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            // add an invisible game object to the scene
            GameObject obj = new GameObject("MainThreadExecuter");
            if (visible == false)
            {
                obj.hideFlags = HideFlags.HideAndDontSave;
            }

            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<UnityThread>();
        }
    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    public static void executeInFixedUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesFixedUpdateFunc)
        {
            actionQueuesFixedUpdateFunc.Add(action);
            noActionQueueToExecuteFixedUpdateFunc = false;
        }
    }

    public void FixedUpdate()
    {
        if (noActionQueueToExecuteFixedUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueFixedUpdateFunc queue
        actionCopiedQueueFixedUpdateFunc.Clear();
        lock (actionQueuesFixedUpdateFunc)
        {
            //Copy actionQueuesFixedUpdateFunc to the actionCopiedQueueFixedUpdateFunc variable
            actionCopiedQueueFixedUpdateFunc.AddRange(actionQueuesFixedUpdateFunc);
            //Now clear the actionQueuesFixedUpdateFunc since we've done copying it
            actionQueuesFixedUpdateFunc.Clear();
            noActionQueueToExecuteFixedUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueFixedUpdateFunc
        for (int i = 0; i < actionCopiedQueueFixedUpdateFunc.Count; i++)
        {
            actionCopiedQueueFixedUpdateFunc[i].Invoke();
        }
    }

    public void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

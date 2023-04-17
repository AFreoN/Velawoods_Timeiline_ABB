using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionCaller
{
    public static ActionCaller CreateAction(Func<bool> condition, Action action, string callerName = "Action Caller")
    {
        GameObject g = new GameObject(callerName, typeof(Hook));

        ActionCaller actionCaller = new ActionCaller(condition, action,g, callerName);
        g.GetComponent<Hook>().updateAction = actionCaller.Update;

        return actionCaller;
    }

    Func<bool> condition;
    Action action;
    GameObject currentHook;

    private ActionCaller(Func<bool> _condition, Action _action,GameObject _hook, string callerName = "Action Caller")
    {
        condition = _condition;
        action = _action;
        currentHook = _hook;
    }

    void Update()
    {
        if(condition())
        {
            action?.Invoke();
            destroySelf();
        }
    }

    public void CallAction()
    {
        action?.Invoke();
        destroySelf();
    }

    public void destroySelf()
    {
        UnityEngine.Object.Destroy(currentHook);
    }

    public class Hook : MonoBehaviour
    {
        public Action updateAction;

        private void Update()
        {
            updateAction();
        }
    }
}

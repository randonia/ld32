using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyController))]
public class NodeTransformer : Editor
{
    void OnSceneGUI()
    {
        EnemyController enemyController = (EnemyController)target;
        GameObject gameObject = enemyController.gameObject;
        if (enemyController.PatrolPoints != null)
        {
            for (int i = 0; i < enemyController.PatrolPoints.Length; ++i)
            {
                Vector3 currPoint = enemyController.PatrolPoints[i];
                enemyController.PatrolPoints[i] = Handles.PositionHandle(currPoint, gameObject.transform.rotation);
                Handles.color = Color.white;
                Handles.Label(currPoint, string.Format("{0}:path[{1}]", gameObject.name, i));
            }
        }
    }
}
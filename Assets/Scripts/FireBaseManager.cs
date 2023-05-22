using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System;

public class FireBaseManager : MonoBehaviour
{
    public static void GetData(string document, Action<Dictionary<string, object>> callback)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Dictionary<string, object> data = new Dictionary<string, object>();

        db.Collection("HyperCasual").Document(document).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                data = snapshot.ToDictionary();
                Debug.Log("Load Complete");

                callback(data);
            }
            else
                Debug.Log("Load Error");
        });
    }

    public static void SaveData(Dictionary<string, object> data)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("HyperCasual").Document("RytjcXeghx0H4WdsLJL9").SetAsync(data).ContinueWith(task =>
        {
            if (task.IsCompleted)
                Debug.Log("Save Complete");
            else
                Debug.Log("Save Error");
        });
    }
}
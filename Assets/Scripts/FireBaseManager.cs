using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

public class FireBaseManager : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        CollectionReference collref = db.Collection("HyperCasual");
        QuerySnapshot snapshot = await collref.GetSnapshotAsync();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Dictionary<string, object> documentDictionary = document.ToDictionary();
            Debug.Log("score:  " + documentDictionary["score"] as string);
            Debug.Log("time: " + documentDictionary["time"] as string);
        }
    }

    public static void SaveData(Dictionary<string, object> data)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("HyperCasual").Document("RytjcXeghx0H4WdsLJL9").SetAsync(data).ContinueWith(task =>
        {
            if (task.IsCompleted)
                Debug.Log("Complete");
            else
                Debug.Log("Error");
        });
    }
}
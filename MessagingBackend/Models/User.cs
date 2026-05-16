using Google.Cloud.Firestore;

[FirestoreData]
public class User
{
    [FirestoreDocumentId]
    public string uid { get; set; } = "";

    [FirestoreProperty]
    public string name { get; set; } = "";

    [FirestoreProperty]
    public string email { get; set; } = "";

    [FirestoreProperty]
    public string? profileImageUrl { get; set; }

    [FirestoreProperty]
    public Timestamp lastActive { get; set; }

    [FirestoreProperty]
    public Timestamp createdAt { get; set; }
}
using Google.Cloud.Firestore;

[FirestoreData]
public class Chat
{
    [FirestoreDocumentId]
    public string chatId { get; set; } = "";

    [FirestoreProperty]
    public string type { get; set; } = "individual"; // e.g., "individual" or "group"

    [FirestoreProperty]
    public List<string> memberIds { get; set; } = new();

    [FirestoreProperty]
    public string? title { get; set; }

    [FirestoreProperty]
    public string? lastMessageId { get; set; }

    [FirestoreProperty]
    public Timestamp createdAt { get; set; }
}
using Google.Cloud.Firestore;

[FirestoreData]
public class Message
{
    [FirestoreDocumentId]
    public string messageId { get; set; } = "";

    [FirestoreProperty]
    public string chatId { get; set; } = "";

    [FirestoreProperty]
    public string senderId { get; set; } = "";

    [FirestoreProperty]
    public string senderName { get; set; } = "";

    [FirestoreProperty]
    public string? imageUrl { get; set; }

    [FirestoreProperty]
    public string messageText { get; set; } = "";

    [FirestoreProperty]
    public Timestamp timestamp { get; set; }

    [FirestoreProperty]
    public string type { get; set; } = "text";

    [FirestoreProperty]
    public List<string> readBy { get; set; } = new();

    [FirestoreProperty]
    public Timestamp? updatedAt { get; set; }
}
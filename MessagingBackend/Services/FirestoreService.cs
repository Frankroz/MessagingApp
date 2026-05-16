using Google.Cloud.Firestore;
using DotNetEnv;

public class FirestoreService
{
    private readonly FirestoreDb _db;

    public FirestoreService()
    {
        Env.Load(); // Load variables from .env
        string projectId = Env.GetString("FIREBASE_PROJECT_ID");

        // The SDK automatically looks for the GOOGLE_APPLICATION_CREDENTIALS env var
        _db = FirestoreDb.Create(projectId);
    }

    // USER QUERIES
    public async Task<List<User>> GetAllUsersAsync()
    {
        CollectionReference usersRef = _db.Collection("users");
        QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

        return snapshot.Documents.Select(doc => doc.ConvertTo<User>()).ToList();
    }

    public async Task SaveUserAsync(User user)
    {
        // Access document by the specific UID provided
        DocumentReference docRef = _db.Collection("users").Document(user.uid);

        // SetAsync creates the document if it doesn't exist, or overwrites if it does
        // MergeAll ensures we don't accidentally delete fields not in the User object
        await docRef.SetAsync(user, SetOptions.MergeAll);
    }

    public async Task<User?> GetUserByIdAsync(string uid)
    {
        DocumentReference docRef = _db.Collection("users").Document(uid);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<User>() : null;
    }

    public async Task DeleteUserCompletelyAsync(string uid)
    {
        // 1. Find all chats the user is in
        var userChats = await _db.Collection("chats")
                                 .WhereArrayContains("memberIds", uid)
                                 .GetSnapshotAsync();

        // 2. Remove the user from each chat's member list
        foreach (var doc in userChats.Documents)
        {
            await doc.Reference.UpdateAsync("memberIds", FieldValue.ArrayRemove(uid));
        }

        // 3. Finally delete the user document
        await _db.Collection("users").Document(uid).DeleteAsync();
    }

    // CHAT QUERIES

    public async Task<string> CreateChatAsync(Chat chat)
    {
        CollectionReference collection = _db.Collection("chats");
        // Ensure timestamp is set
        chat.createdAt = Timestamp.FromDateTime(DateTime.UtcNow);

        DocumentReference doc = await collection.AddAsync(chat);
        return doc.Id;
    }

    public async Task<Chat?> GetChatAsync(string chatId)
    {
        DocumentReference docRef = _db.Collection("chats").Document(chatId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<Chat>();
        }
        return null;
    }

    public async Task UpdateChatAsync(Chat chat)
    {
        DocumentReference docRef = _db.Collection("chats").Document(chat.chatId);
        // Use SetAsync with SetOptions.MergeAll to only update fields that are present
        await docRef.SetAsync(chat, SetOptions.MergeAll);
    }

    public async Task DeleteChatAsync(string chatId)
    {
        DocumentReference docRef = _db.Collection("chats").Document(chatId);
        await docRef.DeleteAsync();
    }

    public async Task CreateChatWithIdAsync(string customChatId, Chat chat)
    {
        DocumentReference docRef = _db.Collection("chats").Document(customChatId);
        chat.chatId = customChatId; // Sync the field with the Doc ID
        await docRef.SetAsync(chat);
    }

    public async Task AddMemberToChatAsync(string chatId, string newUserUid)
    {
        DocumentReference chatRef = _db.Collection("chats").Document(chatId);

        // FieldValue.ArrayUnion is the correct way to add to a list in Firestore
        await chatRef.UpdateAsync("memberIds", FieldValue.ArrayUnion(newUserUid));
    }

    // --- MESSAGE CRUD ---

    public async Task SendMessageAsync(string chatId, Message msg)
    {
        // 1. Get a reference to the top-level "messages" collection
        CollectionReference msgCollection = _db.Collection("messages");

        // 2. Generate a new document reference to get the random ID
        DocumentReference newMsgRef = msgCollection.Document();

        // 3. Sync the generated ID and passed chatId into the message object
        msg.messageId = newMsgRef.Id;
        msg.chatId = chatId;
        msg.timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
        msg.updatedAt = Timestamp.FromDateTime(DateTime.UtcNow);

        // 4. Save the message to the top-level messages collection
        await newMsgRef.SetAsync(msg);

        // 5. Update the corresponding chat document's lastMessageId (Crucial for your UI!)
        DocumentReference chatRef = _db.Collection("chats").Document(chatId);
        await chatRef.UpdateAsync("lastMessageId", msg.messageId);
    }

    public async Task<List<Message>> GetChatMessagesAsync(string chatId)
    {
        // 1. Target the top-level root "messages" collection
        Query query = _db.Collection("messages")
                         .WhereEqualTo("chatId", chatId) // Filter messages for this specific conversation
                         .OrderByDescending("timestamp"); // Order them newest first (uses your new composite index)

        // 2. Fetch the data from Firestore
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        // 3. Map the documents into C# Message objects
        return snapshot.Documents.Select(doc => doc.ConvertTo<Message>()).ToList();
    }

    public async Task UpdateMessageAsync(Message msg)
    {
        // Go directly to the top-level "messages" collection using the messageId
        DocumentReference docRef = _db.Collection("messages").Document(msg.messageId);

        // MergeAll ensures we only update fields that are explicitly set in the 'msg' object
        await docRef.SetAsync(msg, SetOptions.MergeAll);
    }

    public async Task DeleteMessageAsync(string chatId, string messageId)
    {
        // 1. Get references to the target message and the parent chat
        DocumentReference msgRef = _db.Collection("messages").Document(messageId);
        DocumentReference chatRef = _db.Collection("chats").Document(chatId);

        // 2. Check if this message happens to be the 'lastMessageId' for the chat
        DocumentSnapshot chatSnapshot = await chatRef.GetSnapshotAsync();
        string currentLastMessageId = chatSnapshot.GetValue<string>("lastMessageId");

        // 3. Delete the message document
        await msgRef.DeleteAsync();

        // 4. If we just deleted the latest message, find the new "last message"
        if (currentLastMessageId == messageId)
        {
            // Query the messages collection for the most recent remaining message in this chat
            Query nextLastMsgQuery = _db.Collection("messages")
                                        .WhereEqualTo("chatId", chatId)
                                        .OrderByDescending("timestamp")
                                        .Limit(1);

            QuerySnapshot querySnapshot = await nextLastMsgQuery.GetSnapshotAsync();

            if (querySnapshot.Documents.Count > 0)
            {
                // Update parent chat to point to the previous message
                string newLastMessageId = querySnapshot.Documents[0].Id;
                await chatRef.UpdateAsync("lastMessageId", newLastMessageId);
            }
            else
            {
                // No messages left in the chat, clear the field
                await chatRef.UpdateAsync("lastMessageId", "");
            }
        }
    }

    // --- MEMBER QUERIES ---

    public async Task<List<Chat>> GetUserChatsAsync(string userId)
    {
        // Firestore "ArrayContains" is used to find documents where the array contains a specific value
        Query query = _db.Collection("chats").WhereArrayContains("memberIds", userId);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(doc => doc.ConvertTo<Chat>()).ToList();
    }
}
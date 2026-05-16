# Overview

For this project, I'm implementing Firebase Database to create a simple messaging app, so users can talk with each other, and such

# Cloud Database

Firebase Database

- User squema
 {
  "uid": "asdfasdf1",
  "name": "Bob",
  "email": "bob@asdf.com",
  "profileImageUrl": "https://example.com/bob.jpg",
  "password": "asdf", <=== It will be hashed in the real database
  "lastActive": "2026-05-14T15:00:00Z",
  "createdAt": "2026-05-10T10:00:00Z"
 }

- Chat squema
 {
  "chatId": "asdfasdf3",
  "type": "individual", <== Or "group"
  "memberIds": ["asdfasdf1", "asdfasdf2"],
  "title":"Chat",
  "lastMessageId": "Kg6WXUn223X46rIKLTmU",
  "createdAt": "2026-05-12T09:00:00Z"
 }

- Message squema
{
  "messageId": "asdfasdf4",
  "chatId": "asdfasdf3"
  "senderId": "asdfasdf1",
  "senderName": "Bob",
  "imageUrl":"",
  "messageText": "Hello!",
  "timestamp": "2026-05-14T15:05:30Z",
  "updatedAt": "2026-05-14T15:05:30Z"
  "type": "text",
  "readBy": ["asdfasdf2"]
}

# Development Environment

For the backend I user .NET framework, to make it more stable and easy to code

# Useful Websites

{Make a list of websites that you found helpful in this project}

- [Firebase Docs](https://firebase.google.com/docs)
- [Firebase Indexes Docs](https://firebase.google.com/docs/firestore/query-data/indexing)

# Future Work

- Create Login and register
- Implement Authentication
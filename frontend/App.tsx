import React, { useEffect, useState } from "react";
import {
  StyleSheet,
  Text,
  View,
  FlatList,
  TouchableOpacity,
  TextInput,
  Button,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { SafeAreaProvider, SafeAreaView } from "react-native-safe-area-context";
import { getUserChats, getChatMessages, sendMessage } from "./utils/api";
import AsyncStorage from "@react-native-async-storage/async-storage";

// Simple interface mappings matching our backend models
interface Chat {
  chatId: string;
  type: string;
  title?: string;
  lastMessageId?: string;
  memberIds: string[];
}

interface Message {
  messageId: string;
  chatId: string;
  senderId: string;
  senderName: string;
  messageText: string;
}

const CURRENT_USER_ID = "YuAGsGrf5HP0bxO7S3Fp";
const CURRENT_USER_NAME = "Joe";

export default function App() {
  const [chats, setChats] = useState<Chat[]>([]);
  const [activeChat, setActiveChat] = useState<Chat | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputText, setInputText] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let intervalId: any = null;

    if (activeChat) {
      intervalId = setInterval(async () => {
        try {
          const updatedMsgs = await getChatMessages(activeChat.chatId);
          setMessages(updatedMsgs);

          // Keep the local storage up to date with the polling entries
          const localCacheKey = `@messages_${activeChat.chatId}`;
          await AsyncStorage.setItem(
            localCacheKey,
            JSON.stringify(updatedMsgs),
          );
        } catch (error) {
          console.error("Polling sync error:", error);
        }
      }, 1000);
    }

    return () => {
      if (intervalId) clearInterval(intervalId);
    };
  }, [activeChat]);

  useEffect(() => {
    loadUserChats();
  }, []);

  const loadUserChats = async () => {
    try {
      setLoading(true);
      const data = await getUserChats(CURRENT_USER_ID);
      setChats(data);
    } catch (error) {
      console.error("Failed to load user conversations", error);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectChat = async (chat: Chat) => {
    try {
      setLoading(true);
      setActiveChat(chat);
      setMessages([]);

      const localCacheKey = `@messages_${chat.chatId}`;
      const cachedData = await AsyncStorage.getItem(localCacheKey);

      if (cachedData !== null) {
        setMessages(JSON.parse(cachedData));
      }

      const liveMsgs = await getChatMessages(chat.chatId);
      setMessages(liveMsgs);

      await AsyncStorage.setItem(localCacheKey, JSON.stringify(liveMsgs));
    } catch (error) {
      console.error("Failed to execute data sync pipeline", error);
    } finally {
      setLoading(false);
    }
  };

  const handleSendMessage = async () => {
    if (!inputText.trim() || !activeChat) return;

    const payload = {
      senderId: CURRENT_USER_ID,
      senderName: CURRENT_USER_NAME,
      messageText: inputText.trim(),
      type: "text",
      readBy: [CURRENT_USER_ID],
    };

    try {
      const temporaryMessageId = `temp_${Date.now()}`;
      const optimizedMessageItem = {
        ...payload,
        messageId: temporaryMessageId,
        chatId: activeChat.chatId,
      };

      const newHistoryList = [optimizedMessageItem, ...messages];
      setMessages(newHistoryList);
      setInputText("");

      const localCacheKey = `@messages_${activeChat.chatId}`;
      await AsyncStorage.setItem(localCacheKey, JSON.stringify(newHistoryList));

      await sendMessage(activeChat.chatId, payload);
    } catch (error) {
      console.error("Failed sending message item payload", error);
    }
  };

  return (
    <SafeAreaProvider>
      <SafeAreaView style={styles.safeContainer}>
        {loading && (
          <View style={styles.loader}>
            <ActivityIndicator size="large" color="#0000ff" />
          </View>
        )}

        {/* View Layout Toggle logic based on selected activeChat value */}
        {!activeChat ? (
          <View style={styles.container}>
            <Text style={styles.headerTitle}>Conversations</Text>
            <FlatList
              data={chats}
              keyExtractor={(item) => item.chatId}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={styles.chatCard}
                  onPress={() => handleSelectChat(item)}
                >
                  <Text style={styles.chatTitle}>
                    {item.title ||
                      `Chat with ${item.memberIds.find((id) => id !== CURRENT_USER_ID)}`}
                  </Text>
                  <Text style={styles.chatTypeSub}>{item.type}</Text>
                </TouchableOpacity>
              )}
              ListEmptyComponent={
                <Text style={styles.emptyText}>No available chats found.</Text>
              }
            />
          </View>
        ) : (
          <KeyboardAvoidingView
            behavior={Platform.OS === "ios" ? "padding" : "height"}
            style={styles.container}
            keyboardVerticalOffset={Platform.OS === "ios" ? 90 : 0} // Adjusts alignment compensation
          >
            {/* Header Toolbar containing back routing logic */}
            <View style={styles.chatRoomHeader}>
              <Button title="Back" onPress={() => setActiveChat(null)} />
              <Text style={styles.roomTitle}>
                {activeChat.title || "Active Chat"}
              </Text>
            </View>

            {/* Main scroll list window (Inverted) */}
            <FlatList
              data={messages}
              keyExtractor={(item) => item.messageId}
              inverted
              renderItem={({ item }) => {
                const isMe = item.senderId === CURRENT_USER_ID;
                return (
                  <View
                    style={[
                      styles.bubbleWrapper,
                      isMe ? styles.myBubblePos : styles.theirBubblePos,
                    ]}
                  >
                    <Text style={styles.senderLabel}>{item.senderName}</Text>
                    <View
                      style={[
                        styles.bubble,
                        isMe ? styles.myBubbleBg : styles.theirBubbleBg,
                      ]}
                    >
                      <Text style={isMe ? styles.myText : styles.theirText}>
                        {item.messageText}
                      </Text>
                    </View>
                  </View>
                );
              }}
            />

            {/* Dedicated Bottom input interface segment */}
            <View style={styles.inputContainer}>
              <TextInput
                style={styles.inputField}
                placeholder="Type a message..."
                value={inputText}
                onChangeText={setInputText}
              />
              <Button title="Send" onPress={handleSendMessage} />
            </View>
          </KeyboardAvoidingView>
        )}
      </SafeAreaView>
    </SafeAreaProvider>
  );
}

const styles = StyleSheet.create({
  safeContainer: {
    flex: 1,
    backgroundColor: "#f5f5f5",
  },
  container: {
    flex: 1,
    paddingHorizontal: 16,
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: "bold",
    marginVertical: 16,
  },
  chatCard: {
    backgroundColor: "#fff",
    padding: 16,
    borderRadius: 8,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#e0e0e0",
  },
  chatTitle: {
    fontSize: 16,
    fontWeight: "600",
  },
  chatTypeSub: {
    fontSize: 12,
    color: "#888",
    marginTop: 4,
  },
  chatRoomHeader: {
    flexDirection: "row",
    alignItems: "center",
    marginVertical: 12,
  },
  roomTitle: {
    fontSize: 18,
    fontWeight: "bold",
    marginLeft: 16,
  },
  bubbleWrapper: {
    marginVertical: 4,
    maxWidth: "75%",
  },
  myBubblePos: {
    alignSelf: "flex-end",
  },
  theirBubblePos: {
    alignSelf: "flex-start",
  },
  senderLabel: {
    fontSize: 11,
    color: "#666",
    marginBottom: 2,
    marginHorizontal: 4,
  },
  bubble: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 16,
  },
  myBubbleBg: {
    backgroundColor: "#007aff",
  },
  theirBubbleBg: {
    backgroundColor: "#e5e5ea",
  },
  myText: {
    color: "#fff",
  },
  theirText: {
    color: "#000",
  },
  inputContainer: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 12,
    borderTopWidth: 1,
    borderTopColor: "#e0e0e0",
    backgroundColor: "#f5f5f5",
  },
  inputField: {
    flex: 1,
    backgroundColor: "#fff",
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: "#ccc",
    marginRight: 12,
  },
  emptyText: {
    textAlign: "center",
    color: "#888",
    marginTop: 40,
  },
  loader: {
    ...StyleSheet.absoluteFill,
    backgroundColor: "rgba(255,255,255,0.7)",
    justifyContent: "center",
    alignItems: "center",
    zIndex: 999,
  },
});

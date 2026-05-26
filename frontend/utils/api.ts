import axios from "axios";
import { Platform } from "react-native";

const DEFAULT_URL =
  Platform.OS === "android" ? "http://10.0.2.2:5292" : "http://localhost:5292";
const API_URL = process.env.EXPO_PUBLIC_API_URL || DEFAULT_URL;

const client = axios.create({
  baseURL: API_URL,
  headers: { "Content-Type": "application/json" },
});

// Fetch all chats containing the target user id
export const getUserChats = async (userId: string) => {
  const response = await client.get(`/api/chats/user/${userId}`);
  return response.data;
};

// Fetch messages belonging to a targeted chat ID
export const getChatMessages = async (chatId: string) => {
  const response = await client.get(`/api/chats/${chatId}/messages`);
  return response.data;
};

// Send a message instance up to the top-level collection
export const sendMessage = async (chatId: string, messageData: any) => {
  const response = await client.post(
    `/api/chats/${chatId}/messages`,
    messageData,
  );
  return response.data;
};

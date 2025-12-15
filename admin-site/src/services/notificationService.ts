import { API_BASE_URL } from '../config'

export interface CreateNotificationRequest {
  title: string;
  message: string;
  type: 'info' | 'warning' | 'maintenance' | 'success' | 'error';
  userIds?: string[]; // null hoặc undefined = gửi cho tất cả players
  userEmails?: string[]; // có thể dùng emails thay vì IDs
  sendEmail: boolean;
}

export interface NotificationResponse {
  id: string;
  title: string;
  message: string;
  type: string;
  createdAt: string;
  recipientCount: number;
  emailSent: boolean;
}

export interface NotificationStats {
  totalEmailsSent: number;
  successCount: number;
  failedCount: number;
  failedEmails: string[];
}

export interface SendNotificationResponse {
  message: string;
  notification: NotificationResponse;
  stats: NotificationStats;
}

const getAuthHeaders = (): HeadersInit => {
  const token = localStorage.getItem('authToken');
  return {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` })
  };
};

export const notificationService = {
  async sendNotification(data: CreateNotificationRequest): Promise<SendNotificationResponse> {
    const response = await fetch(`${API_BASE_URL}/notifications/send`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to send notification');
    }

    return response.json();
  },

  async sendTestNotification(userId?: string, title?: string, message?: string, type?: string): Promise<SendNotificationResponse> {
    const response = await fetch(`${API_BASE_URL}/notifications/send-test`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ userId, title, message, type })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to send test notification');
    }

    return response.json();
  }
};

export default notificationService;

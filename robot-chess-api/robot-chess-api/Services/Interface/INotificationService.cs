using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface;

public interface INotificationService
{
    Task<(NotificationResponseDto notification, NotificationStatsDto stats)> SendNotificationAsync(CreateNotificationDto dto);
}

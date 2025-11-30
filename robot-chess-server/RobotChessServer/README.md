# Robot Chess Integrated Server

## Cấu trúc Project

```
RobotChessServer/
├── Program.cs                          # Entry point của ứng dụng
├── RobotChessServer.csproj            # Project file
├── .env                               # Environment variables
│
├── Configuration/                     # Quản lý cấu hình
│   ├── ServerConfig.cs               # Lớp cấu hình chính
│   └── ConfigurationManager.cs       # Quản lý cấu hình từ environment
│
├── Models/                            # Mô hình dữ liệu
│   └── ClientInfo.cs                 # Thông tin client (TCP)
│
├── Services/                          # Business logic
│   ├── ITcpService.cs                # Interface TCP
│   ├── TcpService.cs                 # Dịch vụ TCP
│   ├── IWebSocketService.cs          # Interface WebSocket
│   ├── WebSocketService.cs           # Dịch vụ WebSocket
│   ├── IMessageProcessor.cs          # Interface xử lý message
│   └── MessageProcessor.cs           # Xử lý message từ TCP/WS
│
├── Utilities/                         # Các hàm tiện ích
│   ├── NetworkUtils.cs               # Utilities mạng (IP, Port)
│   └── LoggerHelper.cs               # Logging helper
│
└── bin/obj/                          # Build output
```

## Mô tả các folder

### Configuration/
- **ServerConfig.cs**: Chứa các cấu hình server (IP, ports, timeout)
- **ConfigurationManager.cs**: Tải cấu hình từ environment variables

### Models/
- **ClientInfo.cs**: Lưu thông tin về mỗi client TCP (Robot/AI)

### Services/
- **TcpService**: Quản lý kết nối TCP, xử lý client connections
- **WebSocketService**: Quản lý kết nối WebSocket
- **MessageProcessor**: Xử lý các loại message từ clients khác nhau

### Utilities/
- **NetworkUtils.cs**: Các hàm giúp với IP, port (GetAvailableIP, FindAvailablePort)
- **LoggerHelper.cs**: Logging với màu sắc (Info, Warning, Error, Debug)

## Quy trình hoạt động

1. **Program.cs** tải cấu hình → khởi tạo services
2. **TcpService** lắng nghe các kết nối TCP
3. **WebSocketService** lắng nghe các kết nối WebSocket
4. **MessageProcessor** xử lý các message từ cả hai loại client
5. Message được broadcast hoặc gửi đến đích thích hợp

## Environment Variables

```
PRIMARY_IP=localhost
FALLBACK_IP=localhost
TCP_PORT=8080
WS_PORT=8081
CONNECTION_TIMEOUT=3000
ALT_TCP_PORTS=8083,8084
ALT_WS_PORTS=8085,8086,8087
```

## Lợi ích của kiến trúc này

✅ **Separation of Concerns**: Mỗi folder có trách nhiệm riêng
✅ **Reusability**: Dễ tái sử dụng code giữa các modules
✅ **Testability**: Dễ dàng viết unit tests
✅ **Scalability**: Dễ mở rộng thêm features mới
✅ **Maintainability**: Dễ bảo trì và debug
✅ **Dependency Injection**: Services có thể được inject
